using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{

    public struct SpawnableItem
    {
        public string PrefabName;
        public ItemObject SpawnerItem;
        public SpawnableItem(string prefabName, string spawnerItemId)
        {
            this.PrefabName = prefabName;
            this.SpawnerItem = MBObjectManager.Instance.GetObject<ItemObject>(spawnerItemId);
        }
    }
    public class PE_PrefabSpawner : PE_UsableFromDistance
    {
        public string SpawnPointTag = "spawn_point";
        public string SpawningPrefabsXml = "SiegeUnits";
        public Vec3 SpawnOffset = new Vec3();
        public string PrefabSpawnerName = "Siege Unit Deployer";
        public string SpawnerCategoryName = "Siege Units";
        public float DespawnArea = 5f;
        private GameEntity SpawningPoint;
        public int MaxSpawnAmount = 30;
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement() => GameNetwork.IsServer ? base.GetTickRequirement() : ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel;
        public List<SpawnableItem> SpawnableItems { get; private set; }
        public List<GameEntity> SpawnedPrefabs { get; private set; }
        public Dictionary<GameEntity, IStray> StrayEntity { get; private set; }
        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (!GameNetwork.IsServer) return;
            foreach (GameEntity spawnedEntity in this.StrayEntity.Keys.ToList())
            {
                if (spawnedEntity == null)
                {
                    this.StrayEntity.Remove(spawnedEntity);
                    this.SpawnedPrefabs.Remove(spawnedEntity);
                    continue;
                }

                if (this.StrayEntity[spawnedEntity].IsStray())
                {
                    this.DespawnSpawnedPrefab(spawnedEntity);
                }
            }
        }

        protected void LoadSpawnableItems()
        {
            string SpawnPath = ModuleHelper.GetXmlPath("PersistentEmpires", "PrefabSpawner/" + this.SpawningPrefabsXml);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(SpawnPath);
            foreach (XmlNode node in xmlDocument.SelectNodes("/SpawnItems/SpawnItem"))
            {
                string itemId = node["ItemId"].InnerText;
                string prefabName = node["PrefabName"].InnerText;
                SpawnableItem spawnableItem = new SpawnableItem(prefabName, itemId);
                if (spawnableItem.SpawnerItem != null)
                {
                    this.SpawnableItems.Add(spawnableItem);
                }
            }
        }
        protected override void OnInit()
        {
            base.OnInit();
            TextObject actionMessage = new TextObject("Use {PrefabSpawnerName} To Spawn {SpawnerCategoryName}");
            actionMessage.SetTextVariable("PrefabSpawnerName", this.PrefabSpawnerName);
            actionMessage.SetTextVariable("SpawnerCategoryName", this.SpawnerCategoryName);
            base.ActionMessage = actionMessage;
            TextObject descriptionMessage = new TextObject("Press {KEY} To Interact");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
            this.SpawnableItems = new List<SpawnableItem>();
            this.LoadSpawnableItems();
            this.SpawningPoint = base.GameEntity.GetFirstChildEntityWithTag(this.SpawnPointTag);
            this.SpawnedPrefabs = new List<GameEntity>();
            this.StrayEntity = new Dictionary<GameEntity, IStray>();
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Siege Workshop";
        }

        public override void OnUse(Agent userAgent)
        {
            base.OnUse(userAgent);
            userAgent.StopUsingGameObjectMT(true);
            if (GameNetwork.IsServer)
            {
                Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);

                EquipmentIndex equipmentIndex = userAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                if (equipmentIndex == EquipmentIndex.None)
                {
                    this.DespawnNearest(userAgent);
                }
                else
                {
                    MissionWeapon missionWeapon = userAgent.Equipment[equipmentIndex];
                    SpawnableItem spawnableItem = this.SpawnableItems.FirstOrDefault((s) => s.SpawnerItem.Id == missionWeapon.Item.Id);
                    if (spawnableItem.SpawnerItem == null) this.DespawnNearest(userAgent);
                    else
                    {
                        if (this.SpawnedPrefabs.Count < this.MaxSpawnAmount)
                        {
                            this.SpawnSpawnableItem(userAgent, spawnableItem);
                        }
                        else
                        {
                            InformationComponent.Instance.SendMessage("This spawner reached it's limit. Please wait for one of them to de-spawn", new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                        }
                    }
                }
            }
        }

        private void SpawnSpawnableItem(Agent userAgent, SpawnableItem spawnableItem)
        {
            EquipmentIndex equipmentIndex = userAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            userAgent.RemoveEquippedWeapon(equipmentIndex);
            MatrixFrame spawnFrame = this.SpawningPoint.GetGlobalFrame();
            MissionObject mObject = Mission.Current.CreateMissionObjectFromPrefab(spawnableItem.PrefabName, spawnFrame);
            this.SpawnedPrefabs.Add(mObject.GameEntity);

            LoggerHelper.LogAnAction(userAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerSpawnedPrefab, null, new object[] {
                spawnableItem
            });

            // Initiate all mObject childrens
            List<GameEntity> childrens = new List<GameEntity>();

            ScriptComponentBehavior[] spawnablesRoot = mObject.GameEntity.GetScriptComponents().Where(s => s is ISpawnable).ToArray();
            foreach (ISpawnable spawnable in spawnablesRoot) spawnable.OnSpawnedByPrefab(this);

            mObject.GameEntity.GetChildrenRecursive(ref childrens);
            foreach (GameEntity child in childrens)
            {
                ScriptComponentBehavior[] spawnables = child.GetScriptComponents().Where(s => s is ISpawnable).ToArray();
                foreach (ISpawnable spawnable in spawnables) spawnable.OnSpawnedByPrefab(this);

                ScriptComponentBehavior[] strayScripts = child.GetScriptComponents().Where(s => s is IStray).ToArray();
                foreach (IStray stray in strayScripts)
                {
                    this.StrayEntity[mObject.GameEntity] = (IStray)stray;
                }
            }
            ScriptComponentBehavior[] strayScripts2 = mObject.GameEntity.GetScriptComponents().Where(s => s is IStray).ToArray();
            foreach (IStray stray in strayScripts2)
            {
                this.StrayEntity[mObject.GameEntity] = (IStray)stray;
            }
        }

        private void DespawnSpawnedPrefab(GameEntity spawnedPrefab)
        {
            spawnedPrefab.Remove(80);
            this.SpawnedPrefabs.Remove(spawnedPrefab);
            if (this.StrayEntity.ContainsKey(spawnedPrefab)) this.StrayEntity.Remove(spawnedPrefab);
        }

        private void DespawnNearest(Agent userAgent)
        {
            foreach (GameEntity spawnedEntity in this.SpawnedPrefabs.ToArray())
            {
                Vec3 spawnedEntityOrigin = spawnedEntity.GetGlobalFrame().origin;
                Vec3 spawnerOrigin = this.SpawningPoint.GetGlobalFrame().origin;
                SpawnableItem sItem = this.SpawnableItems.FirstOrDefault(s => s.PrefabName == spawnedEntity.Name);
                if (sItem.SpawnerItem == null) continue;
                float distance = spawnedEntityOrigin.Distance(spawnerOrigin);
                if (distance <= this.DespawnArea)
                {
                    // Remove the object add it as to the agent's inventory, if no place drop on ground.
                    // userAgent.Equipment
                    NetworkCommunicator peer = userAgent.MissionPeer.GetNetworkPeer();
                    PersistentEmpireRepresentative empireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                    if (empireRepresentative.GetInventory().HasEnoughRoomFor(sItem.SpawnerItem, 1))
                    {
                        empireRepresentative.GetInventory().AddCountedItemSynced(sItem.SpawnerItem, 1, ItemHelper.GetMaximumAmmo(sItem.SpawnerItem));
                        this.DespawnSpawnedPrefab(spawnedEntity);
                        LoggerHelper.LogAnAction(userAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerDespawnedPrefab, null, new object[] {
                            sItem
                        });
                        break;
                    }
                    else
                    {
                        InformationComponent.Instance.SendMessage("You don't have enough room", new Color(1f, 0f, 0f).ToUnsignedInteger(), peer);
                    }
                }
            }
        }
    }
}

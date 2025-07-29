using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public struct UpgradeReceipt
    {
        public UpgradeReceipt(string upgradeItemId, int neededCount)
        {
            this.UpgradeItem = MBObjectManager.Instance.GetObject<ItemObject>(upgradeItemId);
            this.NeededCount = neededCount;
        }
        public ItemObject UpgradeItem { get; private set; }
        public int NeededCount { get; private set; }
    }
    public class PE_UpgradeableBuildings : PE_DestructableComponent, IMissionObjectHash
    {
        public int CastleIndex = 0;
        public string BuildingName = "UNKNOWN";
        public string Tier0Tag;
        public string Tier1Tag;
        public string Tier2Tag;
        public string Tier3Tag;
        public string BuildingInteractiveTag;

        public int Tier1RequiredEngineering = 10;
        public int Tier2RequiredEngineering = 10;
        public int Tier3RequiredEngineering = 10;

        public int Tier1MaxHit = 200;
        public int Tier2MaxHit = 300;
        public int Tier3MaxHit = 400;
        public int CurrentTier
        {
            get; private set;
        }

        public string Tier1UpgradeReceipts;
        public string Tier2UpgradeReceipts;
        public string Tier3UpgradeReceipts;

        public string ParticleEffectOnUpgrade = "";
        public string SoundEffectOnUpgrade = "";
        public string BuildItem = "pe_buildhammer";

        public int Tier1CraftingEngineering = 10;
        public int Tier2CraftingEngineering = 15;
        public int Tier3CraftingEngineering = 20;

        private int MaxTier = 1;

        public bool IsUpgrading { get; private set; }

        private List<UpgradeReceipt> Tier1Upgrade = new List<UpgradeReceipt>();
        private List<UpgradeReceipt> Tier2Upgrade = new List<UpgradeReceipt>();
        private List<UpgradeReceipt> Tier3Upgrade = new List<UpgradeReceipt>();

        private GameEntity _currentTierState;
        private GameEntity _tier0State;
        private GameEntity _tier1State;
        private GameEntity _tier2State;
        private GameEntity _tier3State;
        private PE_InventoryEntity _upgradeInventory;
        private PlayerInventoryComponent _playerInventoryComponent;



        protected bool ValidateValues()
        {
            if (base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.Tier0Tag)) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " Tier0Tag not found in childrens. " + this.Tier0Tag + " not found in childrens");
                return false;
            }
            if (base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.Tier1Tag)) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " Tier1Tag not found in childrens. " + this.Tier1Tag + " not found in childrens");
                return false;
            }
            if (this.Tier2Tag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.Tier2Tag)) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " Tier2Tag not found in childrens. " + this.Tier2Tag + " not found in childrens");
                return false;
            }
            if (this.Tier3Tag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.Tier3Tag)) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " Tier3Tag not found in childrens. " + this.Tier3Tag + " not found in childrens");
                return false;
            }
            if (base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.BuildingInteractiveTag)) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " BuildingInteractiveTag not found in childrens. " + this.BuildingInteractiveTag + " not found in childrens");
                return false;
            }
            try
            {
                this.ParseUpgradeReceipts(this.Tier1UpgradeReceipts);
            }
            catch (Exception e)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " Tier1UpgradeReceipts could not parsed. Make sure it's in defined format");
                return false;
            }
            try
            {
                this.ParseUpgradeReceipts(this.Tier2UpgradeReceipts);
            }
            catch (Exception e)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " Tier2UpgradeReceipts could not parsed. Make sure it's in defined format");
                return false;
            }
            try
            {
                this.ParseUpgradeReceipts(this.Tier3UpgradeReceipts);
            }
            catch (Exception e)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " Tier3UpgradeReceipts could not parsed. Make sure it's in defined format");
                return false;
            }
            return true;
        }
        protected override void OnSceneSave(string saveFolder)
        {
            this.ValidateValues();
        }
        protected override bool OnCheckForProblems()
        {
            return this.ValidateValues();
        }

        private List<UpgradeReceipt> ParseUpgradeReceipts(string receiptList)
        {
            List<UpgradeReceipt> upgradeReceipts = new List<UpgradeReceipt>();
            string[] repairReceipt = receiptList.Split(',');
            foreach (string receipt in repairReceipt)
            {
                string[] inflictedReceipt = receipt.Split('*');
                string receiptId = inflictedReceipt[0];
                int count = int.Parse(inflictedReceipt[1]);
                upgradeReceipts.Add(new UpgradeReceipt(receiptId, count));
            }
            return upgradeReceipts;
        }
        protected override void OnInit()
        {
            this.Tier1Upgrade = this.ParseUpgradeReceipts(this.Tier1UpgradeReceipts);
            this.Tier2Upgrade = this.ParseUpgradeReceipts(this.Tier2UpgradeReceipts);
            this.Tier3Upgrade = this.ParseUpgradeReceipts(this.Tier3UpgradeReceipts);
            this.CurrentTier = 0;
            this._currentTierState = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.Tier0Tag));
            this._tier0State = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.Tier0Tag));
            this._tier1State = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.Tier1Tag));
            this._tier2State = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.Tier2Tag));
            this._tier3State = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.Tier3Tag));

            _tier0State.SetVisibilityExcludeParents(true);
            _tier1State.SetVisibilityExcludeParents(false);
            if (_tier2State != null)
            {
                _tier2State.SetVisibilityExcludeParents(false);
                this.MaxTier = 2;
            }
            if (_tier3State != null)
            {
                _tier3State.SetVisibilityExcludeParents(false);
                this.MaxTier = 3;
            }

            this._upgradeInventory = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.BuildingInteractiveTag)).GetFirstScriptOfType<PE_InventoryEntity>();
            this._playerInventoryComponent = Mission.Current.GetMissionBehavior<PlayerInventoryComponent>();
            this.MaxHitPoint = this.Tier1MaxHit;
        }
        public void SetIsUpgrading(bool isUpgrading)
        {
            this.IsUpgrading = isUpgrading;
        }

        public int GetNextMaxHit()
        {
            if (this.CurrentTier == 0) return this.Tier1MaxHit;
            if (this.CurrentTier == 1) return this.Tier2MaxHit;
            if (this.CurrentTier == 2) return this.Tier3MaxHit;
            return 0;
        }
        public GameEntity GetNextUpgrade()
        {
            if (this.CurrentTier == 0) return this._tier1State;
            if (this.CurrentTier == 1) return this._tier2State;
            if (this.CurrentTier == 2) return this._tier3State;
            return null;
        }
        public GameEntity GetEntityFromTier(int tier)
        {
            if (tier == 0) return this._tier0State;
            if (tier == 1) return this._tier1State;
            if (tier == 2) return this._tier2State;
            if (tier == 3) return this._tier3State;
            return null;
        }
        public List<UpgradeReceipt> GetUpgradeReceipts()
        {
            if (this.CurrentTier == 0) return this.Tier1Upgrade;
            if (this.CurrentTier == 1) return this.Tier2Upgrade;
            if (this.CurrentTier == 2) return this.Tier3Upgrade;
            return null;
        }
        public int GetRequiredEngineeringForUpgrade()
        {
            if (this.CurrentTier == 0) return this.Tier1RequiredEngineering;
            if (this.CurrentTier == 1) return this.Tier2RequiredEngineering;
            if (this.CurrentTier == 2) return this.Tier3RequiredEngineering;
            return -1;
        }
        public void SetTier(int tier)
        {
            if (tier > 3) return;
            GameEntity tierEntity = this.GetEntityFromTier(tier);
            this._currentTierState.SetVisibilityExcludeParents(false);
            this._currentTierState = tierEntity;
            this._currentTierState.SetVisibilityExcludeParents(true);
            this.CurrentTier = tier;
            this.MaxHitPoint = this.GetNextMaxHit();
        }
        public void UpgradeBuilding()
        {
            GameEntity nextUpgrade = this.GetNextUpgrade();
            if (nextUpgrade == null) return;
            this._currentTierState.SetVisibilityExcludeParents(false);
            this._currentTierState = nextUpgrade;
            this._currentTierState.SetVisibilityExcludeParents(true);
            this.CurrentTier = this.CurrentTier + 1;
            this.MaxHitPoint = this.GetNextMaxHit();

        }
        public override void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackerScriptComponentBehavior)
        {
            this.HitPoint = hitPoint;
            MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
            if (this.HitPoint > this.MaxHitPoint) this.HitPoint = this.MaxHitPoint;
            if (this.HitPoint < 0) this.HitPoint = 0;
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new SyncObjectHitpointsPE(this, impactDirection, this.HitPoint));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
            }
            if (this.HitPoint == this.MaxHitPoint && this.IsUpgrading)
            {
                if (this.ParticleEffectOnUpgrade != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(this.ParticleEffectOnUpgrade), globalFrame);
                }
                if (this.SoundEffectOnUpgrade != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(this.SoundEffectOnUpgrade), globalFrame.origin, false, true, -1, -1);
                }
                this.UpgradeBuilding();
                this.IsUpgrading = false;
                if (GameNetwork.IsServer)
                {
                    SaveSystemBehavior.HandleCreateOrSaveUpgradebleBuilding(this);
                }
                this.HitPoint = 0;
            }


        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            reportDamage = false;
            MissionWeapon missionWeapon = weapon;
            WeaponComponentData currentUsageItem = missionWeapon.CurrentUsageItem;
            if (attackerAgent == null) return false;
            NetworkCommunicator player = attackerAgent.MissionPeer.GetNetworkPeer();
            bool isAdmin = Main.IsPlayerAdmin(player);
            if (isAdmin && missionWeapon.Item != null && missionWeapon.Item.StringId == "pe_adminhammer")
            {
                if (this.CurrentTier < this.MaxTier)
                {
                    this.IsUpgrading = true;
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new UpgradeableBuildingUpgrading(this.IsUpgrading, this));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                    this.SetHitPoint(this.MaxHitPoint, impactDirection, attackerScriptComponentBehavior);
                    this.IsUpgrading = false;
                }
                return true;
            }
            if (missionWeapon.Item == null ||
                missionWeapon.Item.StringId != this.BuildItem) return false;


            if (this.IsUpgrading)
            {
                if (
                    attackerAgent.Character.GetSkillValue(DefaultSkills.Engineering) >= this.GetRequiredEngineeringForUpgrade() &&
                    missionWeapon.Item != null &&
                    missionWeapon.Item.StringId == this.BuildItem &&
                    attackerAgent.IsHuman &&
                    attackerAgent.IsPlayerControlled &&
                    this.HitPoint != this.MaxHitPoint
                )
                {
                    InformationComponent.Instance.SendMessage(this.HitPoint + " / " + this.MaxHitPoint + " building...", 0x02ab89d9, player);
                    this.SetHitPoint(this.HitPoint + damage, impactDirection, attackerScriptComponentBehavior);
                }
            }
            else
            {
                InformationComponent.Instance.SendMessage("You are trying to build " + this.BuildingName + " building.", 0x02ab89d9, player);
                if (this.CurrentTier >= this.MaxTier)
                {
                    InformationComponent.Instance.SendMessage("This building is fully upgraded", new Color(0f, 1f, 0f).ToUnsignedInteger(), player);
                    return false;
                }
                if (attackerAgent.Character.GetSkillValue(DefaultSkills.Engineering) < this.GetRequiredEngineeringForUpgrade())
                {
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Not_Qualified", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), player);
                    return false;
                }


                GameEntity nextUpgrade = this.GetNextUpgrade();
                List<UpgradeReceipt> nextUpgradeReceipts = this.GetUpgradeReceipts();

                Inventory upgradeInv = this._playerInventoryComponent.CustomInventories[this._upgradeInventory.InventoryId];
                // bool playerHasAllItems = this.receipt.All((r) => persistentEmpireRepresentative.GetInventory().IsInventoryIncludes(r.RepairItem, r.NeededCount));
                bool isUpgradeInventoryHasAllItems = nextUpgradeReceipts.All((r) => upgradeInv.IsInventoryIncludes(r.UpgradeItem, r.NeededCount));
                if (!isUpgradeInventoryHasAllItems)
                {
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Required_Items", null).ToString(), 0x02ab89d9, player);
                    foreach (UpgradeReceipt r in nextUpgradeReceipts)
                    {
                        InformationComponent.Instance.SendMessage(r.NeededCount + " * " + r.UpgradeItem.Name.ToString(), 0x02ab89d9, player);
                    }
                    return false;
                }
                foreach (UpgradeReceipt r in nextUpgradeReceipts)
                {
                    upgradeInv.RemoveCountedItem(r.UpgradeItem, r.NeededCount);
                }
                SaveSystemBehavior.HandleCreateOrSaveInventory(upgradeInv.InventoryId);
                this.IsUpgrading = true;
                if (GameNetwork.IsServer)
                {
                    SaveSystemBehavior.HandleCreateOrSaveUpgradebleBuilding(this);
                }
                // Broadcast from server its upgrading
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new UpgradeableBuildingUpgrading(this.IsUpgrading, this));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                InformationComponent.Instance.SendMessage("Upgrading started. Hit your hammer to build it !", 0x02ab89d9, player);
            }

            return true;
        }

        public MissionObject GetMissionObject()
        {
            return this;
        }
    }
}

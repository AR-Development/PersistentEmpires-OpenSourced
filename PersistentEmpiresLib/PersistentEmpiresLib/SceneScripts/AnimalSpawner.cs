using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public struct Receipt
    {
        public Receipt(string repairReceiptId, int count)
        {
            this.Item = MBObjectManager.Instance.GetObject<ItemObject>(repairReceiptId);
            this.NeededCount = count;
        }

        public ItemObject Item { get; private set; }
        public int NeededCount { get; private set; }
    }
    public class PE_AnimalSpawner : UsableMissionObject
    {
        public string SpawnerName = "Cow";
        public string AnimalId = "cow";
        public string NeededItemRecipies = "pe_hardwood*2";
        public string DropItemRecipies = "pe_buildhammer*1,pe_hardwood*2";
        public string ButcheringItem = "pe_buildhammer";
        public string Animation = "act_main_story_become_king_crowd_06";
        public string RelevantButcheringSkill = "Hunting";
        public string HorseHarness = "";
        public int RelevantRequiredSkill = 10;

        public int WaitTimeInSeconds = 60;
        public int MaxAgentCount = 10;
        public int DenarCost = 100;

        private long UseWillEndAt = 0;
        private long UseStartedAt = 0;

        private List<Receipt> NeededReceipt = new List<Receipt>();
        private List<Receipt> DropReceipt = new List<Receipt>();
        private List<Agent> SpawnedAnimals = new List<Agent>();

        private List<Receipt> ParseReceipts(string value)
        {
            List<Receipt> parsedReceipt = new List<Receipt>();
            string[] repairReceipt = value.Split(',');
            foreach (string receipt in repairReceipt)
            {
                string[] inflictedReceipt = receipt.Split('*');
                string receiptId = inflictedReceipt[0];
                int count = int.Parse(inflictedReceipt[1]);
                parsedReceipt.Add(new Receipt(receiptId, count));
            }
            return parsedReceipt;
        }
        protected override void OnInit()
        {
            base.OnInit();
            base.ActionMessage = new TextObject(this.SpawnerName);
            TextObject descriptionMessage = new TextObject("Press {KEY} To Make One");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
            this.NeededReceipt = this.ParseReceipts(this.NeededItemRecipies);
            this.DropReceipt = this.ParseReceipts(this.DropItemRecipies);
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Animal Spawner";
        }
        public void RemoveSpawnedAnimal(Agent animal)
        {
            this.SpawnedAnimals.Remove(animal);
        }
        public List<Receipt> GetDropReceipts()
        {
            return this.DropReceipt;
        }
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            /*if (GameNetwork.IsClientOrReplay && base.HasUser)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel2;
            }*/
            if (GameNetwork.IsServer && base.HasUser)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel2;
            }
            return base.GetTickRequirement();
        }
        protected override void OnTickOccasionally(float currentFrameDeltaTime)
        {
            this.OnTickParallel2(currentFrameDeltaTime);
        }
        protected override void OnTickParallel2(float dt)
        {
            base.OnTickParallel2(dt);
            if (GameNetwork.IsServer)
            {
                if (base.HasUser)
                {
                    if (this.UseWillEndAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    {
                        base.UserAgent.StopUsingGameObjectMT(base.UserAgent.CanUseObject(this));
                    }
                }
            }
        }
        public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
        {
            base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
            Debug.Print("[USING LOG] AGENT STOPPEDUSING PE_ANIMALSPAWNER");
            userAgent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloatRanged(1f), false, -0.2f, 0, true);
            if (isSuccessful)
            {
                if (GameNetwork.IsServer)
                {
                    NetworkCommunicator peer = userAgent.MissionPeer.GetNetworkPeer();
                    PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                    Inventory playerInventory = persistentEmpireRepresentative.GetInventory();
                    bool inventoryContainsEveryItem = this.NeededReceipt.All((r) => playerInventory.IsInventoryIncludes(r.Item, r.NeededCount));
                    if (!inventoryContainsEveryItem)
                    {
                        InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Required_Items", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                        foreach (Receipt r in this.NeededReceipt)
                        {
                            InformationComponent.Instance.SendMessage(r.NeededCount + " * " + r.Item.Name.ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                        }
                        return;
                    }
                    if (!persistentEmpireRepresentative.ReduceIfHaveEnoughGold(this.DenarCost))
                    {
                        InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Not_Enough_Gold", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                        return;
                    }

                    foreach (Receipt r in this.NeededReceipt)
                    {
                        playerInventory.RemoveCountedItem(r.Item, r.NeededCount);
                    }
                    MatrixFrame globalFrame = this.GameEntity.GetGlobalFrame();
                    ItemRosterElement itemRosterElement = new ItemRosterElement(Game.Current.ObjectManager.GetObject<ItemObject>(this.AnimalId), 0, null);
                    globalFrame.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
                    Mission mission = Mission.Current;
                    ItemRosterElement rosterElement = itemRosterElement;
                    ItemRosterElement harnessRosterElement = this.HorseHarness == "" ? default(ItemRosterElement) : new ItemRosterElement(MBObjectManager.Instance.GetObject<ItemObject>(this.HorseHarness), 1);
                    Vec2 asVec = globalFrame.rotation.f.AsVec2;
                    Agent agent = mission.SpawnMonster(rosterElement, harnessRosterElement, globalFrame.origin, asVec, -1);
                    AnimalSpawnSettings.CheckAndSetAnimalAgentFlags(this.GameEntity, agent);
                    SpawnedAnimals.Add(agent);
                    AnimalButcheringBehavior.Instance.AgentToAnimalSpawner[agent] = this;
                }
            }
            if (userAgent.IsMine) PEInformationManager.StopCounter();
        }
        public override void OnUse(Agent userAgent)
        {
            if (GameNetwork.IsServer)
            {
                Debug.Print("[USING LOG] AGENT USING " + this.GetType().Name);
                NetworkCommunicator peer = userAgent.MissionPeer.GetNetworkPeer();
                if (this.SpawnedAnimals.Count >= this.MaxAgentCount)
                {
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_AnimalSpawner1", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                if (this.HasUser)
                {
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Already_In_Use", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }

                base.OnUse(userAgent);
                ActionIndexCache actionIndexCache = ActionIndexCache.Create(this.Animation);
                userAgent.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                this.UseStartedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                this.UseWillEndAt = this.UseStartedAt + this.WaitTimeInSeconds;
            }
            if (GameNetwork.IsClient)
            {
                if (userAgent.IsMine)
                {
                    PEInformationManager.StartCounter("Spawning animal...", this.WaitTimeInSeconds);
                }
            }
        }
    }
}

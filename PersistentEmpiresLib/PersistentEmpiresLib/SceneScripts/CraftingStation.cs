using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
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
    public struct CraftingRecipe
    {
        public ItemObject Item;
        public int NeededCount;
        public CraftingRecipe(String itemId, int neededCount)
        {
            this.Item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            this.NeededCount = neededCount;
        }
    }
    public struct Craftable
    {
        public List<CraftingRecipe> Recipe;
        public int OutputCount;
        public ItemObject Item;
        public int Tier;
        public int RequiredEngineering;
        public int CraftTime;
        public SkillObject RelevantSkill;
        public Craftable(List<CraftingRecipe> receipts, String itemId, int outputCount, int tier, int requiredEngineering, int craftTime, string relevantSkill)
        {
            this.Recipe = receipts;
            this.Item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            this.OutputCount = outputCount;
            this.Tier = tier;
            this.RequiredEngineering = requiredEngineering;
            this.CraftTime = craftTime;
            this.RelevantSkill = MBObjectManager.Instance.GetObject<SkillObject>(relevantSkill);
        }
    }
    public class PE_CraftingStation : PE_UsableFromDistance
    {
        public string StationName = "Carpenter Bench";
        public string ModuleFolder = Main.ModuleName;
        public CraftingComponent craftingComponent { get; private set; }
        public PE_UpgradeableBuildings upgradeableBuilding { get; private set; }
        private PlayerInventoryComponent playerInventoryComponent;
        public List<Craftable> Craftables { get; private set; }
        public string Animation = "";
        public string CraftingRecieptTag = "";
        public string RelevantSkillId = "Engineering";
        private string Tier1Craftings = "";
        private string Tier2Craftings = "";
        private string Tier3Craftings = "";
        public List<Craftable> ParseStringToCraftables(string allCraftableReceipt, int tier, int requiredEngineering)
        {
            List<Craftable> craftables = new List<Craftable>();
            if (allCraftableReceipt == "") return craftables;
            foreach (string receipt in allCraftableReceipt.Split('|'))
            {
                if (receipt.Trim() == "") continue;
                string leftSide = receipt.Split('=')[0];
                string rightSide = receipt.Split('=')[1];
                List<CraftingRecipe> cReceipts = new List<CraftingRecipe>();
                foreach (string r in rightSide.Split(','))
                {
                    string itemId = r.Split('*')[0];
                    int count = int.Parse(r.Split('*')[1]);
                    cReceipts.Add(new CraftingRecipe(itemId, count));
                }
                int craftTime = int.Parse(leftSide.Split('*')[0]);
                string craftableItemId = leftSide.Split('*')[1];
                int outputAmount = int.Parse(leftSide.Split('*')[2]);
                Craftable craftable = new Craftable(cReceipts, craftableItemId, outputAmount, tier, requiredEngineering, craftTime, this.RelevantSkillId);
                craftables.Add(craftable);
            }
            return craftables;
        }
        public void LoadCraftables()
        {
            List<Craftable> tier1Crafts = this.ParseStringToCraftables(this.Tier1Craftings, 1, this.upgradeableBuilding.Tier1CraftingEngineering);
            List<Craftable> tier2Crafts = this.ParseStringToCraftables(this.Tier2Craftings, 2, this.upgradeableBuilding.Tier2CraftingEngineering);
            List<Craftable> tier3Crafts = this.ParseStringToCraftables(this.Tier3Craftings, 3, this.upgradeableBuilding.Tier3CraftingEngineering);
            this.Craftables = new List<Craftable>();
            this.Craftables.AddRange(tier1Crafts);
            this.Craftables.AddRange(tier2Crafts);
            this.Craftables.AddRange(tier3Crafts);
        }
        protected override void OnInit()
        {
            base.OnInit();
            TextObject actionMessage = new TextObject("Use {Station} To Craft");
            actionMessage.SetTextVariable("Station", this.StationName);
            base.ActionMessage = actionMessage;
            TextObject descriptionMessage = new TextObject("Press {KEY} To Interact");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
            this.upgradeableBuilding = base.GameEntity.Parent.Parent.GetFirstScriptOfType<PE_UpgradeableBuildings>();
            if (base.GameEntity.Parent.Parent.GetFirstChildEntityWithTag(this.CraftingRecieptTag) != null)
            {
                PE_CraftingReceipt craftingRecieptEntity = base.GameEntity.Parent.Parent.GetFirstChildEntityWithTag(this.CraftingRecieptTag).GetFirstScriptOfType<PE_CraftingReceipt>();
                this.Tier1Craftings = craftingRecieptEntity.Tier1Craftings;
                this.Tier2Craftings = craftingRecieptEntity.Tier2Craftings;
                this.Tier3Craftings = craftingRecieptEntity.Tier3Craftings;
            }
            else
            {
                string xmlPath = ModuleHelper.GetXmlPath(this.ModuleFolder, "CraftingRecipies/" + this.CraftingRecieptTag);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlPath);
                foreach (XmlNode node in xmlDocument.DocumentElement.ChildNodes)
                {
                    if (node.Name == "Tier1Craftings") this.Tier1Craftings = node.InnerText.Trim();
                    else if (node.Name == "Tier2Craftings") this.Tier2Craftings = node.InnerText.Trim();
                    else if (node.Name == "Tier3Craftings") this.Tier3Craftings = node.InnerText.Trim();
                }
            }
            this.playerInventoryComponent = Mission.Current.GetMissionBehavior<PlayerInventoryComponent>();
            this.craftingComponent = Mission.Current.GetMissionBehavior<CraftingComponent>();
            this.LoadCraftables();
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Crafting Station Named As " + this.StationName;
        }



        public override void OnUse(Agent userAgent)
        {
            Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);
            if (!base.IsUsable(userAgent))
            {
                userAgent.StopUsingGameObjectMT(false);
                return;
            }
            base.OnUse(userAgent);
            userAgent.StopUsingGameObjectMT(true);
            if (GameNetwork.IsServer)
            {
                this.craftingComponent.AgentRequestCrafting(userAgent, this);
            }

        }
    }
}

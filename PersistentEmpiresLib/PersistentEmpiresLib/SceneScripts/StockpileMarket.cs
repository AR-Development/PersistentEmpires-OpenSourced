using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class MarketItem
    {
        public ItemObject Item;
        public int Stock;
        public int MaximumPrice;
        public float CurrentPrice;
        public int MinimumPrice;
        public int Constant;
        public int Stability;
        public int Tier;
        public bool Dirty = false;
        // X is stock Y is price x^(1/stability) * y = k
        public MarketItem(string itemId, int maximumPrice, int minimumPrice, int stability, int tier)
        {
            this.Item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            this.MaximumPrice = maximumPrice;
            this.MinimumPrice = minimumPrice;
            this.Constant = MaximumPrice;
            this.Stability = stability;
            this.CurrentPrice = MathF.Pow(this.Constant, 1f / this.Stability);
            this.Tier = tier;
        }
        public void UpdateReserve(int newStock)
        {
            Dirty = true;

            if (newStock > 999) newStock = 999;
            
            if (newStock < 0) newStock = 0;

            if (newStock < 1)
            {
                this.CurrentPrice = MathF.Pow(this.Constant, 1f / this.Stability);
            }
            else
            {
                this.CurrentPrice = MathF.Pow(this.Constant / (float)newStock, 1f / this.Stability);
            }
            this.Stock = newStock;
        }
        public int BuyPrice()
        {
            float fakeStock = this.Stock;
            if (this.Stock < 2) return this.MaximumPrice;
            float denominator = MathF.Pow(fakeStock - 1, 1f / this.Stability);
            float numerator = this.Constant;
            int buyPrice = Math.Abs((int)((numerator / denominator) - MathF.Pow(this.CurrentPrice, 1f / this.Stability)));
            if (buyPrice < this.MinimumPrice) buyPrice = this.MinimumPrice;
            return buyPrice;
        }
        public int SellPrice()
        {
            float fakeStock = this.Stock;
            if (this.Stock < 1) fakeStock = 1;
            float denominator = MathF.Pow(fakeStock + 1, 1f / this.Stability);
            float numerator = this.Constant;
            int price = Math.Abs((int)(MathF.Pow(this.CurrentPrice, 1f / this.Stability) - (numerator / denominator)));
            if (price < this.MinimumPrice) price = (this.MinimumPrice * 90) / 100;
            return (price * 85) / 100;
        }
    }
    public class CraftingBox
    {
        public ItemObject BoxItem;
        public int MinTierLevel;
        public int MaxTierLevel;
        public CraftingBox(string itemId, string minTierLevel, string maxTierLevel)
        {
            this.BoxItem = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            this.MinTierLevel = int.Parse(minTierLevel);
            this.MaxTierLevel = int.Parse(maxTierLevel);
        }
    }
    public class PE_StockpileMarket : PE_UsableFromDistance, IMissionObjectHash
    {

        public static int MAX_STOCK_COUNT = 1000;
        public string XmlFile = "examplemarket"; // itemId*minimum*maximum,itemId*minimum*maximum
        public string ModuleFolder = Main.ModuleName;
        protected override bool LockUserFrames
        {
            get
            {
                return false;
            }
        }
        protected override bool LockUserPositions
        {
            get
            {
                return false;
            }
        }

        public List<MarketItem> MarketItems { get; private set; }
        public List<CraftingBox> CraftingBoxes { get; private set; }
        public StockpileMarketComponent stockpileMarketComponent { get; private set; }
        protected void LoadMarketItems(string innerText, int tier)
        {

            if (innerText == "") return;
            foreach (string marketItemStr in innerText.Trim().Split('|'))
            {
                string[] values = marketItemStr.Split('*');
                string itemId = values[0];
                int minPrice = int.Parse(values[1]);
                int maxPrice = int.Parse(values[2]);
                int stability = values.Length > 4 ? int.Parse(values[3]) : 10;

                ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
                if (item == null)
                {
                    Debug.Print(" ERROR IN MARKET SERIALIZATION " + this.XmlFile + " ITEM ID " + itemId + " NOT FOUND !!! ", 0, Debug.DebugColor.Red);
                }
                this.MarketItems.Add(new MarketItem(itemId, maxPrice, minPrice, stability, tier));
            }
        }
        protected void LoadCraftingBoxes(string innerText)
        {
            this.CraftingBoxes = new List<CraftingBox>();
            if (innerText == "") return;
            foreach (string craftingBoxStr in innerText.Trim().Split('|'))
            {
                string[] values = craftingBoxStr.Split('*');
                string itemId = values[0];
                string minTierLevel = values[1];
                string maxTierLevel = values[2];
                this.CraftingBoxes.Add(new CraftingBox(itemId, minTierLevel, maxTierLevel));
            }
        }

        protected override void OnInit()
        {
            base.OnInit();
            TextObject actionMessage = new TextObject("Browse the Market");
            base.ActionMessage = actionMessage;
            TextObject descriptionMessage = new TextObject("Press {KEY} To Browse");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
            this.stockpileMarketComponent = Mission.Current.GetMissionBehavior<StockpileMarketComponent>();
            Debug.Print("Initiating Stockpile Market With " + this.ModuleFolder + " Module");
            string xmlPath = ModuleHelper.GetXmlPath(this.ModuleFolder, "Markets/" + this.XmlFile);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);
            this.MarketItems = new List<MarketItem>();
            this.LoadMarketItems(xmlDocument.SelectSingleNode("/Market/Tier1Items").InnerText, 1);
            this.LoadMarketItems(xmlDocument.SelectSingleNode("/Market/Tier2Items").InnerText, 2);
            this.LoadMarketItems(xmlDocument.SelectSingleNode("/Market/Tier3Items").InnerText, 3);
            this.LoadMarketItems(xmlDocument.SelectSingleNode("/Market/Tier4Items").InnerText, 4);
            this.LoadCraftingBoxes(xmlDocument.SelectSingleNode("/Market/CraftingBoxes").InnerText);
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Stockpile Market";
        }

        public override void OnUse(Agent userAgent)
        {
            if (!base.IsUsable(userAgent))
            {
                userAgent.StopUsingGameObjectMT(false);
                return;
            }
            base.OnUse(userAgent);
            Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);

            if (GameNetwork.IsServer)
            {
                this.stockpileMarketComponent.OpenStockpileMarketForPeer(this, userAgent.MissionPeer.GetNetworkPeer());
                userAgent.StopUsingGameObjectMT(true);
            }
        }
        public void DeserializeStocks(string serialized)
        {
            string[] elements = serialized.Split('|');
            foreach (string s in elements)
            {
                ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(s.Split('*')[0]);
                if (item == null)
                {
                    Debug.Print(" ERROR IN MARKET SERIALIZATION " + this.XmlFile + " ITEM ID " + s.Split('*')[0] + " NOT FOUND !!! ", 0, Debug.DebugColor.Red);
                }
                int stock = int.Parse(s.Split('*')[1]);
                MarketItems.Find(m => m.Item.StringId == item.StringId).UpdateReserve(stock);
            }
        }
        public string SerializeStocks()
        {
            return string.Join("|", MarketItems.Select(s => s.Item.StringId + "*" + s.Stock));
        }

        public MissionObject GetMissionObject()
        {
            return this;
        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            reportDamage = false;
            if (attackerAgent == null) return false;
            NetworkCommunicator player = attackerAgent.MissionPeer.GetNetworkPeer();
            bool isAdmin = Main.IsPlayerAdmin(player);
            if (isAdmin && weapon.Item != null && weapon.Item.StringId == "pe_adminstockfiller")
            {
                foreach (MarketItem marketItem in this.MarketItems)
                {
                    var currentStock = marketItem.Stock;
                    if (currentStock + 10 < 900)
                    {
                        marketItem.UpdateReserve(currentStock + 10);
                    }
                }
                InformationComponent.Instance.SendMessage("Stocks updated", Colors.Blue.ToUnsignedInteger(), player);
            }
            return true;
        }
    }
}

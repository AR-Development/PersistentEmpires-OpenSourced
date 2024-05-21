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
    public class PE_StockpileMarket : PE_UsableFromDistance, IMissionObjectHash
    {
        public static int MAX_STOCK_COUNT = 1000;        
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

        protected override void OnInit()
        {
            base.OnInit();
            TextObject actionMessage = new TextObject("Browse the Market");
            base.ActionMessage = actionMessage;
            TextObject descriptionMessage = new TextObject("Press {KEY} To Browse");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
            this.stockpileMarketComponent = Mission.Current.GetMissionBehavior<StockpileMarketComponent>();

            MarketItems = StockpileMarketComponent.MarketItems.ToList();
            CraftingBoxes = StockpileMarketComponent.CraftingBoxes.ToList();
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
                    Debug.Print(" ERROR IN MARKET SERIALIZATION " + StockpileMarketComponent.XmlFile + " ITEM ID " + s.Split('*')[0] + " NOT FOUND !!! ", 0, Debug.DebugColor.Red);
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

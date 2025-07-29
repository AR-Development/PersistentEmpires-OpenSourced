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

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_TradeCenter : PE_UsableFromDistance
    {
        public static int MAX_STOCK_COUNT = 1000;
        public string XmlFile = "exampletradecenter";
        public List<MarketItem> MarketItems { get; private set; }
        private TradingCenterBehavior tradingCenterBehavior;
        private CastlesBehavior castlesBehavior;
        public int RandomizeDelayMinutes = 60;
        public int CastleIndex = 0;
        public int RandomMinStock = 10;
        public int RandomMaxStock = 300;
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

        public void Randomize(Random randomizer)
        {
            foreach (MarketItem marketItem in this.MarketItems)
            {
                int randomStock = randomizer.Next(this.RandomMinStock, this.RandomMaxStock);
                marketItem.UpdateReserve(randomStock);
            }
        }

        protected void LoadMarketItems(string innerText, int tier)
        {

            if (innerText == "") return;
            foreach (string marketItemStr in innerText.Trim().Split('|'))
            {
                string[] values = marketItemStr.Split('*');
                string itemId = values[0];
                int minPrice = int.Parse(values[1]);
                int maxPrice = int.Parse(values[2]);
                int stability = values.Length >= 4 ? int.Parse(values[3]) : 10;
                this.MarketItems.Add(new MarketItem(itemId, maxPrice, minPrice, stability, tier));
            }
        }


        protected override void OnInit()
        {
            base.OnInit();
            TextObject actionMessage = new TextObject("Browse Trading Center");
            base.ActionMessage = actionMessage;
            TextObject descriptionMessage = new TextObject("Press {KEY} To Browse");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
            this.tradingCenterBehavior = Mission.Current.GetMissionBehavior<TradingCenterBehavior>();
            this.castlesBehavior = Mission.Current.GetMissionBehavior<CastlesBehavior>();
            string xmlPath = ModuleHelper.GetXmlPath(Main.ModuleName, "Markets/" + this.XmlFile);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);
            this.MarketItems = new List<MarketItem>();
            this.LoadMarketItems(xmlDocument.SelectSingleNode("/Market/Tier1Items").InnerText, 1);
            this.LoadMarketItems(xmlDocument.SelectSingleNode("/Market/Tier2Items").InnerText, 2);
            this.LoadMarketItems(xmlDocument.SelectSingleNode("/Market/Tier3Items").InnerText, 3);
            this.LoadMarketItems(xmlDocument.SelectSingleNode("/Market/Tier4Items").InnerText, 4);

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
                this.tradingCenterBehavior.OpenTradingCenterForPeer(this, userAgent.MissionPeer.GetNetworkPeer());
                userAgent.StopUsingGameObjectMT(true);
            }
        }

        public string GetCastleName()
        {
            return this.castlesBehavior.castles[this.CastleIndex].CastleName;
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Trading Center";
        }
    }
}

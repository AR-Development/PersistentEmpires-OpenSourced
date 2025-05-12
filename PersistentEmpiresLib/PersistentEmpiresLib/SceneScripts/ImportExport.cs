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
    public struct GoodItem
    {
        public ItemObject ItemObj;
        public int ExportPrice;
        public int ImportPrice;
        public GoodItem(ItemObject itemObj, int exportPrice, int importPrice)
        {
            this.ItemObj = itemObj;
            this.ExportPrice = exportPrice;
            this.ImportPrice = importPrice;
        }
    }
    public class PE_ImportExport : PE_UsableFromDistance
    {
        public string XmlFile = "importexport";
        public string ModuleFolder = Main.ModuleName;
        public string TradeableItems { get; private set; }
        private List<GoodItem> goodItems;
        private ImportExportComponent importExportComponent;
        protected override void OnInit()
        {
            base.OnInit();
            this.importExportComponent = Mission.Current.GetMissionBehavior<ImportExportComponent>();
            TextObject actionMessage = new TextObject("Export/Import some goods for fixed price");
            base.ActionMessage = actionMessage;
            TextObject descriptionMessage = new TextObject("Press {KEY} To Export/Import");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
            this.goodItems = new List<GoodItem>();

            try
            {
                Debug.Print("Initiating ImportExport Market With " + this.ModuleFolder + " Module");
                string xmlPath = ModuleHelper.GetXmlPath(this.ModuleFolder, "Markets/" + this.XmlFile);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlPath);

                this.TradeableItems = xmlDocument.DocumentElement.InnerText.Trim();

                foreach (string goodStr in this.TradeableItems.Split('|'))
                {
                    string[] goodSplitted = goodStr.Split(',');
                    ItemObject itemObject = MBObjectManager.Instance.GetObject<ItemObject>(goodSplitted[0]);
                    if (itemObject == null) continue;
                    int exportPrice = int.Parse(goodSplitted[1]);
                    int importPrice = int.Parse(goodSplitted[2]);
                    this.goodItems.Add(new GoodItem(itemObject, exportPrice, importPrice));
                }
            }
            catch(Exception)
            { }
        }

        public List<GoodItem> GetGoodItems()
        {
            return this.goodItems;
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Import/Export";
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
            if (GameNetwork.IsServer)
            {
                this.importExportComponent.OpenImportExportForPeer(userAgent.MissionPeer.GetNetworkPeer(), this);
                userAgent.StopUsingGameObjectMT(true);
            }
        }
    }
}

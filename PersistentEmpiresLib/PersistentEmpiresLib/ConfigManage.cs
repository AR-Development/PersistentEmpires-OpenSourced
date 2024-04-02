using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.ModuleManager;

namespace PersistentEmpiresLib
{
    public class ConfigManager
    {
        public static string XmlFile = "GeneralConfig";

        public static int StartingGold { get; private set; }
        public static bool VoiceChatEnabled { get; private set; }
        public static bool DontOverrideMangonelHit { get; private set; }


        public static void Initialize()
        {
            StartingGold = GetStartingGold();
            VoiceChatEnabled = GetVoiceChatEnabled();
            DontOverrideMangonelHit = GetBoolConfig("DontOverrideMangonelHit", false);
        }
        public static bool GetVoiceChatEnabled()
        {
            string xmlPath = ModuleHelper.GetXmlPath("PersistentEmpires", "Configs/" + XmlFile);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);
            XmlNode portElement = xmlDocument.SelectSingleNode("/GeneralConfig/VoiceChatEnabled");
            return portElement.InnerText == "true";
        }

        public static int GetStartingGold()
        {
            string xmlPath = ModuleHelper.GetXmlPath("PersistentEmpires", "Configs/" + XmlFile);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);
            XmlNode portElement = xmlDocument.SelectSingleNode("/GeneralConfig/StartingGold");
            return int.Parse(portElement.InnerText);
        }

        public static bool GetBoolConfig(string config, bool defValue)
        {
            string result = GetStrConfig(config, defValue.ToString());

            if (result == "true") return true;
            else return false;
        }

        public static int GetIntConfig(string config, int defValue)
        {
            string xmlPath = ModuleHelper.GetXmlPath("PersistentEmpires", "Configs/" + XmlFile);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);
            XmlNode portElement = xmlDocument.SelectSingleNode("/GeneralConfig/" + config);
            return portElement == null ? defValue : int.Parse(portElement.InnerText);
        }
        public static string GetStrConfig(string config, string defValue)
        {
            string xmlPath = ModuleHelper.GetXmlPath("PersistentEmpires", "Configs/" + XmlFile);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);
            XmlNode portElement = xmlDocument.SelectSingleNode("/GeneralConfig/" + config);
            return portElement == null ? defValue : portElement.InnerText.Trim();
        }
    }
}

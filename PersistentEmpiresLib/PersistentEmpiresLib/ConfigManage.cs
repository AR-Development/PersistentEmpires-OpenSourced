#if SERVER
using System.IO;
using System.Xml;
using TaleWorlds.ModuleManager;

namespace PersistentEmpiresLib
{
    public class ConfigManager
    {
        public static string XmlFile = "GeneralConfig";
        internal static string RulesXmlFile = "Rules";
        public static int StartingGold { get; private set; }
        public static bool VoiceChatEnabled { get; private set; }
        public static bool DontOverrideMangonelHit { get; private set; }
        public static XmlDocument _xmlDocument = null;
        public static XmlDocument _rulesDocument = null;
        public static XmlDocument XmlDocument
        {
            get
            {

                if(_xmlDocument == null)
                {
                    string xmlPath = ModuleHelper.GetXmlPath(Main.ModuleName, "Configs/" + XmlFile);
                    _xmlDocument = new XmlDocument();
                    _xmlDocument.Load(xmlPath);
                }

                return _xmlDocument;
            }
        }

        public static XmlDocument Rules
        {
            get
            {
                if (_rulesDocument == null)
                {
                    string xmlPath = ModuleHelper.GetXmlPath(Main.ModuleName, "Configs/" + RulesXmlFile);
                    if (File.Exists(xmlPath))
                    {
                        _rulesDocument = new XmlDocument();
                        _rulesDocument.Load(xmlPath);
                    }
                }

                return _rulesDocument;
            }
        }

        public static void Initialize()
        {
            StartingGold = GetStartingGold();
            VoiceChatEnabled = GetVoiceChatEnabled();
            DontOverrideMangonelHit = GetBoolConfig("DontOverrideMangonelHit", false);
        }

        public static bool GetVoiceChatEnabled()
        {
            XmlNode portElement = XmlDocument.SelectSingleNode("/GeneralConfig/VoiceChatEnabled");
            return portElement.InnerText == "true";
        }

        public static int GetStartingGold()
        {
            XmlNode portElement = XmlDocument.SelectSingleNode("/GeneralConfig/StartingGold");
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
            XmlNode portElement = XmlDocument.SelectSingleNode("/GeneralConfig/" + config);
            return portElement == null ? defValue : int.Parse(portElement.InnerText);
        }

        public static string GetStrConfig(string config, string defValue)
        {
            XmlNode portElement = XmlDocument.SelectSingleNode("/GeneralConfig/" + config);
            return portElement == null ? defValue : portElement.InnerText.Trim();
        }
    }
}
#endif
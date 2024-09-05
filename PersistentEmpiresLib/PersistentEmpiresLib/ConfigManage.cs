/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
        public static XmlDocument _xmlDocument = null;
        public static XmlDocument XmlDocument
        {
            get
            {
                if(_xmlDocument == null)
                {
                    string xmlPath = ModuleHelper.GetXmlPath("PersistentEmpires", "Configs/" + XmlFile);
                    _xmlDocument = new XmlDocument();
                    _xmlDocument.Load(xmlPath);
                }

                return _xmlDocument;
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

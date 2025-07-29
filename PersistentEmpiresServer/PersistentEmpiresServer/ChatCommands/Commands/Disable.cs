using PersistentEmpiresLib;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Disable : Command
    {
        private uint? _color = null;

        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("DisableColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
                }

                return _color.Value;
            }
        }

        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return AdminServerBehavior.Instance.IsPlayerAdmin(networkPeer);
        }

        public string Command()
        {
            return $"{ChatCommandSystem.Instance.CommandPrefix}disable";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative.IsAdmin == false) return false;
            if (ChatCommandSystem.Instance.DisableGlobalChat == false)
            {
                InformationComponent.Instance.BroadcastQuickInformation("Global chat has been disabled.", Color);
                ChatCommandSystem.Instance.DisableGlobalChat = true;
            }
            else
            {
                InformationComponent.Instance.BroadcastQuickInformation("Global chat has been enabled.", Color);
                ChatCommandSystem.Instance.DisableGlobalChat = false;
            }
            return true;
        }

        public string Description()
        {
            return "Disable global chat.";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()}{Environment.NewLine}" +
                    $"Color: Same as this message{Environment.NewLine}" +
                    $"Description: Disables global chat{Environment.NewLine}" +
                    $"Example: {Command()}{Environment.NewLine}";
        }

        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("DisableEnabled", true);
        }
    }
}
using PersistentEmpiresLib;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Announce : Command
    {
        private uint? _color = null;

        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("AnnounceColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
                }

                return _color.Value;
            }
        }

        public string Command()
        {
            return $"{ChatCommandSystem.Instance.CommandPrefix}a";
        }

        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return AdminServerBehavior.Instance.IsPlayerAdmin(networkPeer);
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (args == null) return false;

            DiscordBehavior.NotifyAnnounce(networkPeer.UserName, String.Join(" ", args));
            InformationComponent.Instance.BroadcastAnnouncement("[" + networkPeer.UserName + "] " + String.Join(" ", args), Color);
            return true;
        }

        public string Description()
        {
            return "Make an admin announce";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()} [text]{Environment.NewLine}" +
                    $"Parameter: [text] admin message to be send to all users{Environment.NewLine}" +
                    $"Color: Same as this message{Environment.NewLine}" +
                    $"Description: Sends admin message to all players{Environment.NewLine}" +
                    $"Example: {Command()} test{Environment.NewLine}";
        }

        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("AnnounceEnabled", true);
        }
    }
}
using PersistentEmpiresLib;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresServer.ServerMissions;
using System;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Do : Command
    {
        private static int _distance = ConfigManager.GetIntConfig("DoDistance", 30);
        private uint? _color = null;
        private static bool _bubble = ConfigManager.GetBoolConfig("DoBubble", true);

        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("DoColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
                }

                return _color.Value;
            }
        }

        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return $"{ChatCommandSystem.Instance.CommandPrefix}do";
        }

        public bool Execute(NetworkCommunicator player, string[] args)
        {
            if (player.ControlledAgent == null || args == null) return false;

            var message = String.Join(" ", args) + " (" + player.UserName + ")";

            this.SendMessageToPlayers(player, _distance, message, Color, _bubble, LogAction.DoCommand);

            return true;
        }

        public string Description()
        {
            return "Do command";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()} [Text]{Environment.NewLine}" +
                    $"Parameter: [Text] text to be shown{Environment.NewLine}" +
                    $"Color: Same as this message{Environment.NewLine}" +
                    $"Description: Do command{Environment.NewLine}" +
                    $"Example: {Command()}{Environment.NewLine}";
        }

        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("DoEnabled", true);
        }
    }
}
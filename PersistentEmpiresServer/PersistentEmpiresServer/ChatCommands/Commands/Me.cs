using PersistentEmpiresLib;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresServer.ServerMissions;
using System;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Me : Command
    {
        private static int _distance = ConfigManager.GetIntConfig("MeDistance", 30);
        private uint? _color = null;
        private static bool _bubble = ConfigManager.GetBoolConfig("MeBubble", true);

        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("MeColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
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
            return $"{ChatCommandSystem.Instance.CommandPrefix}me";
        }

        public bool Execute(NetworkCommunicator player, string[] args)
        {
            if (player.ControlledAgent == null || args == null) return false;

            var message = "* " + player.UserName + " " + String.Join(" ", args);

            this.SendMessageToPlayers(player, _distance, message, Color, _bubble, LogAction.MeCommand);

            return true;
        }

        public string Description()
        {
            return "Emote description.";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()} [Text]{Environment.NewLine}" +
                    $"Parameter: [Text] emote text to be displayed{Environment.NewLine}" +
                    $"Color: Same as this message{Environment.NewLine}" +
                    $"Description: Displays detailed information about command use{Environment.NewLine}" +
                    $"Example: {Command()} eats{Environment.NewLine}";
        }

        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("MeEnabled", true);
        }
    }
}
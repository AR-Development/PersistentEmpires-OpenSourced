using PersistentEmpiresLib;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresServer.ServerMissions;
using System;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    internal class Dice : Command
    {
        private static int _distance = ConfigManager.GetIntConfig("DiceDistance", 30);
        private uint? _color = null;
        private static bool _bubble = ConfigManager.GetBoolConfig("DiceBubble", true);

        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("DiceColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
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
            return $"{ChatCommandSystem.Instance.CommandPrefix}dice";
        }

        public bool Execute(NetworkCommunicator player, string[] args)
        {
            if (player.ControlledAgent == null) return false;

            var rnd = new Random();
            var random = rnd.Next(1, 7);
            var message = $"{player.UserName} dices {random}.";

            this.SendMessageToPlayers(player, _distance, message, Color, _bubble, LogAction.DiceCommand);

            return true;
        }

        public string Description()
        {
            return "/dice generates random number between 1 and 6";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()}{Environment.NewLine}" +
                    $"Color: Same as this message{Environment.NewLine}" +
                    $"Description: Generates random number between 1 and 6{Environment.NewLine}" +
                    $"Example: {Command()}{Environment.NewLine}";
        }
        
        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("DiceEnabled", true);
        }
    }
}
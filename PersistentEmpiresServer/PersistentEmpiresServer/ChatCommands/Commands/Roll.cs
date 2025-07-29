using PersistentEmpiresLib;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    internal class Roll : Command
    {
        private static int _distance = ConfigManager.GetIntConfig("RollDistance", 30);
        private uint? _color = null;
        private static bool _bubble = ConfigManager.GetBoolConfig("RollBubble", true);

        public uint Color
        {
            get
            {
                if (_color == null)
                {
                    _color = TaleWorlds.Library.Color.ConvertStringToColor(ConfigManager.GetStrConfig("RollColor", ChatCommandSystem.Instance.DefaultMessageColor)).ToUnsignedInteger();
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
            return $"{ChatCommandSystem.Instance.CommandPrefix}roll";
        }

        public bool Execute(NetworkCommunicator player, string[] args)
        {
            if (player.ControlledAgent == null) return false;

            var minInt = 0;
            var maxInt = 100;
            int tmp;

            if (args != null && args.Count() > 1)
            {
                if (int.TryParse(args[1], out tmp))
                {
                    if (args.Count() > 2)
                    {
                        minInt = tmp;
                    }
                    else
                    {
                        maxInt = tmp;
                    }
                }
            }

            if (args != null && args.Count() > 2)
            {
                if (int.TryParse(args[2], out tmp))
                {
                    maxInt = tmp;
                }
            }

            var rnd = new Random();
            var random = rnd.Next(minInt, maxInt + 1);
            var message = $"{player.UserName} rolls {random} from between {minInt} to {maxInt}.";

            this.SendMessageToPlayers(player, _distance, message, Color, _bubble, LogAction.RollCommand);

            return true;
        }

        public string Description()
        {
            return $"{Command()} (intMin) (intMax) generates a random number between intMin (0) and intMax (100)";
        }

        public string DetailedDescription()
        {
            return $"Usage: {Command()} {Environment.NewLine}" +
                            $"Usage2: {Command()} [intMax]{Environment.NewLine}" +
                            $"Usage3: {Command()} [intMin] [intMax]{Environment.NewLine}" +
                            $"Parameter: [intMax] lowest integer for random number{Environment.NewLine}" +
                            $"Parameter: [intMax] highest integer for random number{Environment.NewLine}" +
                            $"Color: Same as this message{Environment.NewLine}" +
                            $"Description: Generates a random number between [intMin](0 if [intMin] is not supplied) and [intMax](100 if [intMax] is not supplied){Environment.NewLine}" +
                            $"Example: {Command()}{Environment.NewLine}" +
                            $"Example2: {Command()} 10{Environment.NewLine}" +
                            $"Example3: {Command()} 10 15{Environment.NewLine}";
        }

        public bool IsEnabled()
        {
            return ConfigManager.GetBoolConfig("RollEnabled", true);
        }
    }
}
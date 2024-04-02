using System;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    internal class Roll : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return "!roll";
        }

        public string Description()
        {
            return $"/roll (intMin) (intMax) generates a random number between intMin (0) and intMax (100)";
        }

        public bool Execute(NetworkCommunicator player, string[] args)
        {
            if (player.ControlledAgent == null) return false;
            
            var minInt = 0;
            var maxInt = 100;
            int tmp;
            
            if (args.Count() > 1)
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

            if (args.Count() > 2)
            {
                if (int.TryParse(args[2], out tmp))
                {
                    maxInt = tmp;
                }
            }
            
            var rnd = new Random();
            var random = rnd.Next(minInt, maxInt + 1);
            var message = $"{player.UserName} rolls {random} from between {minInt} to {maxInt}.";

            this.SendMessageToPlayers(player, 30, message);

            return true;
        }
    }
}
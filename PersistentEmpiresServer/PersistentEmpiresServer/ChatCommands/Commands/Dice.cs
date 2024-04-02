using System;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    internal class Dice : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return "!dice";
        }

        public string Description()
        {
            return "/dice generates random number between 1 and 6";
        }

        public bool Execute(NetworkCommunicator player, string[] args)
        {
            if (player.ControlledAgent == null) return false;

            var rnd = new Random();
            var random = rnd.Next(1, 7);
            var message = $"{player.UserName} dices {random}.";

            this.SendMessageToPlayers(player, 30, message);

            return true;
        }
    }
}
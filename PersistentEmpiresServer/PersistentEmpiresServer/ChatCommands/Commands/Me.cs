using System;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Me : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return "!me";
        }

        public string Description()
        {
            return "Me command.";
        }

        public bool Execute(NetworkCommunicator player, string[] args)
        {
            if (player.ControlledAgent == null) return false;

            var message = "* " + player.UserName + " " + String.Join(" ", args);

            this.SendMessageToPlayers(player, 30, message);
            
            return true;
        }
    }
}

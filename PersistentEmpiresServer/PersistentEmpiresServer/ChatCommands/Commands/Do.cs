using System;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Do : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return "!do";
        }

        public string Description()
        {
            return "Do command";
        }

        public bool Execute(NetworkCommunicator player, string[] args)
        {
            if (player.ControlledAgent == null) return false;

            var message = String.Join(" ", args) + " (" + player.UserName + ")";
            
            this.SendMessageToPlayers(player, 30, message);
            
            return true;
        }
    }
}

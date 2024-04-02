using NetworkMessages.FromServer;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Help : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return "!help";
        }

        public string Description()
        {
            return "Help message";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            string[] commands = ChatCommandSystem.Instance.commands.Keys.ToArray();
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("-==== Command List ===-"));
            GameNetwork.EndModuleEventAsServer();

            foreach (string command in commands)
            {
                Command commandExecutable = ChatCommandSystem.Instance.commands[command];
                if (commandExecutable.CanUse(networkPeer))
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new ServerMessage(command + ": " + commandExecutable.Description()));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
            return true;
        }
    }
}

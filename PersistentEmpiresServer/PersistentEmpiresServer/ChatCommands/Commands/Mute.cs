using NetworkMessages.FromServer;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Mute : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return AdminServerBehavior.Instance.IsPlayerAdmin(networkPeer);
        }

        public string Command()
        {
            return "!mute";
        }

        public string Description()
        {
            return "Mutes a player from global chat. Caution ! First user that contains the provided input will be muted. Usage !mute <Player Name>";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            if (args.Length == 0)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Please provide a username. Player that contains provided input will be muted"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            NetworkCommunicator targetPeer = null;
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                if (peer.UserName.Contains(string.Join(" ", args)))
                {
                    targetPeer = peer;
                    break;
                }
            }
            if (targetPeer == null)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new ServerMessage("Target player not found"));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }

            if(ChatCommandSystem.Instance.Muted.ContainsKey(targetPeer))
            {
                InformationComponent.Instance.SendMessage("Unmuted.", Colors.Green.ToUnsignedInteger(), networkPeer);
                ChatCommandSystem.Instance.Muted.Remove(targetPeer);
            }
            else
            {
                InformationComponent.Instance.SendMessage("Muted.", Colors.Green.ToUnsignedInteger(), networkPeer);
                ChatCommandSystem.Instance.Muted[targetPeer] = true;
            }

            return true;
            // throw new NotImplementedException();
        }
    }
}

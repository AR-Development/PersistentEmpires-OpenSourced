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
    public class VMute : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return AdminServerBehavior.Instance.IsPlayerAdmin(networkPeer);
        }

        public string Command()
        {
            return "!vmute";
        }

        public string Description()
        {
            return "Mutes a player from voice chat. Caution ! First user that contains the provided input will be muted. Usage !vmute <Player Name>";
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
            ProximityChatComponent pcc = Mission.Current.GetMissionBehavior<ProximityChatComponent>();
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

            if (pcc.GlobalMuted.ContainsKey(targetPeer))
            {
                InformationComponent.Instance.SendMessage("Unmuted.", Colors.Green.ToUnsignedInteger(), networkPeer);
                pcc.GlobalMuted.Remove(targetPeer);
            }
            else
            {
                InformationComponent.Instance.SendMessage("Muted.", Colors.Green.ToUnsignedInteger(), networkPeer);
                pcc.GlobalMuted[targetPeer] = true;
            }
            return true;
        }
    }
}

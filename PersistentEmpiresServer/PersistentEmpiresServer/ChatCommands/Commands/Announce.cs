using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Announce : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return AdminServerBehavior.Instance.IsPlayerAdmin(networkPeer);
        }

        public string Command()
        {
            return "!a";
        }

        public string Description()
        {
            return "Make an admin announce";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            InformationComponent.Instance.BroadcastAnnouncement("[" + networkPeer.UserName + "] " + String.Join(" ", args));
            return true;
        }
    }
}

using PersistentEmpiresLib;
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
    public class Disable : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return AdminServerBehavior.Instance.IsPlayerAdmin(networkPeer);
        }

        public string Command()
        {
            return "!disable";
        }

        public string Description()
        {
            return "Disable global chat.";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative.IsAdmin == false) return false;
            if (ChatCommandSystem.Instance.DisableGlobalChat == false)
            {
                InformationComponent.Instance.BroadcastQuickInformation("Global chat has been disabled.");
                ChatCommandSystem.Instance.DisableGlobalChat = true;
            }
            else
            {
                InformationComponent.Instance.BroadcastQuickInformation("Global chat has been enabled.");
                ChatCommandSystem.Instance.DisableGlobalChat = false;
            }
            return true;
        }
    }
}

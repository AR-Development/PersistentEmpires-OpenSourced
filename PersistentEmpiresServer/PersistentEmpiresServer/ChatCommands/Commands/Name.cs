using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Text.RegularExpressions;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Name : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return "!name";
        }

        public string Description()
        {
            return "Change your name";
        }

        private bool checkAlphaNumeric(String name)
        {
            // ^[a-zA-Z0-9\s,\[,\],\(,\)]*$
            Regex rg = new Regex(@"^[a-zA-Z0-9ğüşöçıİĞÜŞÖÇ.\s,\[,\],\(,\),_,-,\p{IsCJKUnifiedIdeographs}]*$");
            return rg.IsMatch(name);
        }


        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            if (AdminServerBehavior.Instance.LastChangedName.ContainsKey(networkPeer))
            {
                long lastTime = AdminServerBehavior.Instance.LastChangedName[networkPeer];
                if (lastTime + AdminServerBehavior.Instance.cooldown > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    InformationComponent.Instance.SendMessage(
                        String.Format("You need to wait {0} seconds", (lastTime + AdminServerBehavior.Instance.cooldown) - DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                        Colors.Red.ToUnsignedInteger(), networkPeer
                    );
                    return false;
                }
            }

            string newName = String.Join(" ", args);
            if (newName.Length == 0)
            {
                InformationComponent.Instance.SendMessage("Custom name cannot be empty", Colors.Red.ToUnsignedInteger(), networkPeer);
                return false;
            }
            if(!checkAlphaNumeric(newName))
            {
                InformationComponent.Instance.SendMessage("Custom name should be alpha numeric", Colors.Red.ToUnsignedInteger(), networkPeer);
                return false;
            }
            if (persistentEmpireRepresentative.HaveEnoughGold(AdminServerBehavior.Instance.nameChangeGold) == false)
            {
                InformationComponent.Instance.SendMessage("You need " + AdminServerBehavior.Instance.nameChangeGold + " gold.", Colors.Red.ToUnsignedInteger(), networkPeer);
                return false;
            }
            bool result = SaveSystemBehavior.HandlePlayerUpdateCustomName(networkPeer, newName);
            if (result == false)
            {
                InformationComponent.Instance.SendMessage("You can't set this name", Colors.Red.ToUnsignedInteger(), networkPeer);
                return false;
            }
            persistentEmpireRepresentative.ReduceIfHaveEnoughGold(AdminServerBehavior.Instance.nameChangeGold);
            InformationComponent.Instance.SendMessage("Your name is changed. You need to relog to take effect.", Colors.Green.ToUnsignedInteger(), networkPeer);
            LoggerHelper.LogAnAction(networkPeer, LogAction.PlayerChangedName, null, new object[] { newName });
            AdminServerBehavior.Instance.LastChangedName[networkPeer] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            return true;
        }
    }
}

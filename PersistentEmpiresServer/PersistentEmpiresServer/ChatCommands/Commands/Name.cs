/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using PersistentEmpiresLib;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Text.RegularExpressions;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

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
            if (!checkAlphaNumeric(newName))
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

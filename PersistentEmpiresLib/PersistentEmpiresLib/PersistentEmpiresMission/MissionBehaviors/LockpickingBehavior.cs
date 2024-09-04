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

using PersistentEmpiresLib.PersistentEmpiresGameModels;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class LockpickingBehavior : MissionLogic
    {
        public static LockpickingBehavior Instance;
        public string ItemId;
        public Random random;
        public Dictionary<Agent, int> pickedAgents = new Dictionary<Agent, int>();
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            random = new Random();
            Instance = this;
            if (GameNetwork.IsServer)
            {
                this.ItemId = ConfigManager.GetStrConfig("LockpickItem", "pe_lockpick");

            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            foreach (Agent a in pickedAgents.Keys.ToList())
            {
                if (pickedAgents.ContainsKey(a) == false) continue;
                pickedAgents[a]--;
                if (pickedAgents[a] == 0)
                {
                    pickedAgents.Remove(a);
                    EquipmentIndex index = a.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                    if (index != EquipmentIndex.None)
                    {
                        a.RemoveEquippedWeapon(index);
                    }
                }
            }
        }

        public bool Lockpick(Agent picker, MissionWeapon usedItem)
        {
            SkillObject lockpicking = PersistentEmpireSkills.Lockpicking;
            if (usedItem.IsEmpty) return false;
            if (usedItem.Item.StringId != this.ItemId) return false;
            int lockPick = picker.Character.GetSkillValue(lockpicking);
            if (lockPick == 0) return false;

            int chance = random.Next(100);

            if (chance >= 1 && chance < 25)
            {
                NetworkCommunicator player = picker.MissionPeer.GetNetworkPeer();
                InformationComponent.Instance.SendMessage("Lockpicked successfully but your lockpick destroyed", Colors.Green.ToUnsignedInteger(), player);
                Debug.Print("Lockpicked, crash test");
                pickedAgents[picker] = 5;

                /*EquipmentIndex index = picker.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                if (index == EquipmentIndex.None) return false;
                picker.RemoveEquippedWeapon(index);*/
                return true;
            }

            if (chance >= 25 && chance < 50)
            {
                NetworkCommunicator player = picker.MissionPeer.GetNetworkPeer();
                InformationComponent.Instance.SendMessage("Lockpicked successfully", Colors.Green.ToUnsignedInteger(), player);
                return true;
            }

            if (chance >= 50 && chance < 75)
            {
                NetworkCommunicator player = picker.MissionPeer.GetNetworkPeer();
                InformationComponent.Instance.SendMessage("Try again", Colors.Red.ToUnsignedInteger(), player);
                return false;
            }

            if (chance >= 75 && chance < 100)
            {
                NetworkCommunicator player = picker.MissionPeer.GetNetworkPeer();
                InformationComponent.Instance.SendMessage("Your lockpick destroyed", Colors.Red.ToUnsignedInteger(), player);
                Debug.Print("Lockpicked, crash test");
                pickedAgents[picker] = 5;

                return false;
            }

            return false;
        }
    }
}

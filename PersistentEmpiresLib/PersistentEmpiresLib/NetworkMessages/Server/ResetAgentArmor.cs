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

using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class ResetAgentArmor : GameNetworkMessage
    {
        public ResetAgentArmor() { }
        public ResetAgentArmor(Agent agent, Equipment equipment)
        {
            this.agent = agent;
            this.equipment = equipment;
        }
        public Agent agent { get; set; }
        public Equipment equipment { get; set; }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Equipment;
        }

        protected override string OnGetLogFormat()
        {
            return "Update agent armors";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.agent = Mission.MissionNetworkHelper.GetAgentFromIndex(GameNetworkMessage.ReadAgentIndexFromPacket(ref result));
            this.equipment = new Equipment();
            for (EquipmentIndex equipmentIndex2 = EquipmentIndex.Weapon0; equipmentIndex2 < EquipmentIndex.NumEquipmentSetSlots; equipmentIndex2++)
            {
                this.equipment.AddEquipmentToSlotWithoutAgent(equipmentIndex2, ModuleNetworkData.ReadItemReferenceFromPacket(MBObjectManager.Instance, ref result));
            }

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteAgentIndexToPacket(this.agent.Index);
            for (EquipmentIndex equipmentIndex2 = EquipmentIndex.Weapon0; equipmentIndex2 < EquipmentIndex.NumEquipmentSetSlots; equipmentIndex2++)
            {
                ModuleNetworkData.WriteItemReferenceToPacket(this.equipment.GetEquipmentFromSlot(equipmentIndex2));
            }
        }
    }
}

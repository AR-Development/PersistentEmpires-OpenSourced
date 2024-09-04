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

using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RegisterClientEquipmentOnWound : GameNetworkMessage
    {
        public List<string> Equipments;
        public RegisterClientEquipmentOnWound() { }

        public RegisterClientEquipmentOnWound(List<string> playerEquipment)
        {
            Equipments = playerEquipment;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Client RegisterClientEquipmentOnWound";
        }

        protected override bool OnRead()
        {
            bool result = true;
            Equipments = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                Equipments.Add(GameNetworkMessage.ReadStringFromPacket(ref result));
            }
            return result;
        }

        protected override void OnWrite()
        {

            for (int i = 0; i < 4; i++)
            {
                GameNetworkMessage.WriteStringToPacket(Equipments[i]);
            }
        }
    }
}

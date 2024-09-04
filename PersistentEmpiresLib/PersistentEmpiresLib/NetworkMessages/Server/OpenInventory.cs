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

using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class OpenInventory : GameNetworkMessage
    {
        public string InventoryId { get; set; }
        public Inventory PlayerInventory;
        public Inventory RequestedInventory;
        public OpenInventory() { }

        public OpenInventory(string InventoryId, Inventory playerInventory, Inventory requestedInventory)
        {
            this.InventoryId = InventoryId;
            this.PlayerInventory = playerInventory;
            this.RequestedInventory = requestedInventory;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Equipment;
        }

        protected override string OnGetLogFormat()
        {
            return "Open inventory";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.InventoryId = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.PlayerInventory = PENetworkModule.ReadInventoryPlayer(ref result);
            if (this.InventoryId != "")
            {
                this.RequestedInventory = PENetworkModule.ReadCustomInventory(this.InventoryId, ref result);
            }
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.InventoryId);
            PENetworkModule.WriteInventoryPlayer(this.PlayerInventory);
            if (this.InventoryId != "")
            {
                PENetworkModule.WriteCustomInventory(this.RequestedInventory);
            }
        }
    }
}

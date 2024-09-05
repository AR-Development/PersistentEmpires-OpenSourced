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
    public sealed class ExecuteInventoryTransfer : GameNetworkMessage
    {
        public string DraggedSlot { get; set; }
        public string DroppedSlot { get; set; }
        public ItemObject DraggedSlotItem { get; set; }
        public ItemObject DroppedSlotItem { get; set; }
        public int DraggedSlotCount { get; set; }
        public int DroppedSlotCount { get; set; }
        public ExecuteInventoryTransfer()
        {
        }

        public ExecuteInventoryTransfer(string draggedSlot, string droppedSlot, ItemObject draggedSlotItem, ItemObject droppedSlotItem, int draggedSlotCount, int droppedSlotCount)
        {
            this.DraggedSlot = draggedSlot;
            this.DroppedSlot = droppedSlot;
            this.DraggedSlotItem = draggedSlotItem;
            this.DroppedSlotItem = droppedSlotItem;
            this.DraggedSlotCount = draggedSlotCount;
            this.DroppedSlotCount = droppedSlotCount;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Equipment;
        }

        protected override string OnGetLogFormat()
        {
            return "Equipment execute inventory";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.DraggedSlot = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.DroppedSlot = GameNetworkMessage.ReadStringFromPacket(ref result);
            string draggedSlotItemString = GameNetworkMessage.ReadStringFromPacket(ref result);
            string droppedSlotItemString = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.DraggedSlotItem = draggedSlotItemString != "" ? MBObjectManager.Instance.GetObject<ItemObject>(draggedSlotItemString) : null;
            this.DroppedSlotItem = droppedSlotItemString != "" ? MBObjectManager.Instance.GetObject<ItemObject>(droppedSlotItemString) : null;
            this.DraggedSlotCount = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 256, true), ref result);
            this.DroppedSlotCount = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 256, true), ref result);


            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.DraggedSlot);
            GameNetworkMessage.WriteStringToPacket(this.DroppedSlot);
            GameNetworkMessage.WriteStringToPacket(this.DraggedSlotItem != null ? this.DraggedSlotItem.StringId : "");
            GameNetworkMessage.WriteStringToPacket(this.DroppedSlotItem != null ? this.DroppedSlotItem.StringId : "");
            GameNetworkMessage.WriteIntToPacket(this.DraggedSlotCount, new CompressionInfo.Integer(0, 256, true));
            GameNetworkMessage.WriteIntToPacket(this.DroppedSlotCount, new CompressionInfo.Integer(0, 256, true));
        }
    }
}

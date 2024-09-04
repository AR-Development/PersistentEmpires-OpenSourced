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

using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestInventoryTransfer : GameNetworkMessage
    {
        public string DraggedTag { get; set; }
        public string DroppedTag { get; set; }
        public RequestInventoryTransfer() { }
        public RequestInventoryTransfer(string draggedTag, string droppedTag)
        {
            this.DraggedTag = draggedTag;
            this.DroppedTag = droppedTag;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Player request an inventory transfer";
        }

        protected override bool OnRead()
        {
            bool result = false;
            this.DraggedTag = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.DroppedTag = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.DraggedTag);
            GameNetworkMessage.WriteStringToPacket(this.DroppedTag);
        }
    }
}

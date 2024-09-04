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

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class PlayerJoinedFaction : GameNetworkMessage
    {

        public int factionIndex { get; set; }
        public int joinedFrom { get; set; }
        public NetworkCommunicator player { get; set; }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        public PlayerJoinedFaction(int factionIndex, int joinedFrom, NetworkCommunicator player)
        {
            this.factionIndex = factionIndex;
            this.joinedFrom = joinedFrom;
            this.player = player;
        }

        public PlayerJoinedFaction()
        {
        }

        protected override string OnGetLogFormat()
        {
            return "User added to faction";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.factionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            this.joinedFrom = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            this.player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            // throw new NotImplementedException();
            GameNetworkMessage.WriteIntToPacket(factionIndex, new CompressionInfo.Integer(-1, 200, true));
            GameNetworkMessage.WriteIntToPacket(joinedFrom, new CompressionInfo.Integer(-1, 200, true));
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.player);
        }
    }
}

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
    public sealed class FactionPollResponse : GameNetworkMessage
    {
        public bool Accepted { get; private set; }

        public FactionPollResponse()
        {
        }
        public FactionPollResponse(bool accepted)
        {
            this.Accepted = accepted;
        }
        protected override bool OnRead()
        {
            bool result = true;
            this.Accepted = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        // Token: 0x06000099 RID: 153 RVA: 0x00002BFD File Offset: 0x00000DFD
        protected override void OnWrite()
        {
            GameNetworkMessage.WriteBoolToPacket(this.Accepted);
        }

        // Token: 0x0600009A RID: 154 RVA: 0x00002C0A File Offset: 0x00000E0A
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        // Token: 0x0600009B RID: 155 RVA: 0x00002C12 File Offset: 0x00000E12
        protected override string OnGetLogFormat()
        {
            return "Receiving faction poll response: " + (this.Accepted ? "Accepted." : "Not accepted.");
        }
    }
}

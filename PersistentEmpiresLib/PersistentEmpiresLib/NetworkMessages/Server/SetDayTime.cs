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
    public sealed class SetDayTime : GameNetworkMessage
    {
        public float TimeOfDay;
        public SetDayTime() { }
        public SetDayTime(float time)
        {
            this.TimeOfDay = time;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Mission;
        }

        protected override string OnGetLogFormat()
        {
            return "SetDayTime";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.TimeOfDay = GameNetworkMessage.ReadFloatFromPacket(new CompressionInfo.Float(0f, 24f, 22), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteFloatToPacket(this.TimeOfDay, new CompressionInfo.Float(0f, 24f, 22));
        }
    }
}


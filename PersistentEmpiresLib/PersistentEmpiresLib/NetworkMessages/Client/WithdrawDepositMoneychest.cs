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
    public sealed class WithdrawDepositMoneychest : GameNetworkMessage
    {
        public int Amount;
        public MissionObject MoneyChest;
        public bool Withdraw;
        public WithdrawDepositMoneychest() { }
        public WithdrawDepositMoneychest(MissionObject mc, int amount, bool withdraw)
        {
            this.Amount = amount;
            this.Withdraw = withdraw;
            this.MoneyChest = mc;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Client WithdrawMoneychest";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Amount = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 100000000, true), ref result);
            this.Withdraw = GameNetworkMessage.ReadBoolFromPacket(ref result);
            this.MoneyChest = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.Amount, new CompressionInfo.Integer(0, 100000000, true));
            GameNetworkMessage.WriteBoolToPacket(this.Withdraw);
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.MoneyChest.Id);
        }
    }
}

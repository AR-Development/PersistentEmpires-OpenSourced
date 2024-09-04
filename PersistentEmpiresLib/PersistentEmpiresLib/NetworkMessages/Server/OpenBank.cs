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
    public sealed class OpenBank : GameNetworkMessage
    {
        public MissionObject Bank;
        public int Amount;
        public int TaxRate;
        public OpenBank()
        {

        }

        public OpenBank(MissionObject bank, int amount, int taxrate)
        {
            this.Bank = bank;
            this.Amount = amount;
            this.TaxRate = taxrate;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "OpenBank";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Bank = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.Amount = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 1000000000, true), ref result);
            this.TaxRate = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 100, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.Bank.Id);
            GameNetworkMessage.WriteIntToPacket(this.Amount, new CompressionInfo.Integer(0, 1000000000, true));
            GameNetworkMessage.WriteIntToPacket(this.TaxRate, new CompressionInfo.Integer(0, 100, true));
        }
    }
}

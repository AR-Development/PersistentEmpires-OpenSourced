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
    public sealed class UpgradeableBuildingSetTier : GameNetworkMessage
    {
        public UpgradeableBuildingSetTier() { }
        public UpgradeableBuildingSetTier(int tier, MissionObject upgradingObject)
        {
            this.Tier = tier;
            this.UpgradingObject = upgradingObject;
        }
        public int Tier { get; set; }
        public MissionObject UpgradingObject;
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Upgrade tier changed";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Tier = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 4, true), ref result);
            this.UpgradingObject = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.Tier, new CompressionInfo.Integer(-1, 4, true));
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.UpgradingObject.Id);
        }
    }
}

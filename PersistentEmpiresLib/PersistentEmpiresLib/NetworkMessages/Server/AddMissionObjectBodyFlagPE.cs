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

using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class AddMissionObjectBodyFlagPE : GameNetworkMessage
    {
        public MissionObject MissionObject { get; private set; }

        public BodyFlags BodyFlags { get; private set; }

        public bool ApplyToChildren { get; private set; }

        public AddMissionObjectBodyFlagPE(MissionObject missionObject, BodyFlags bodyFlags, bool applyToChildren)
        {
            this.MissionObject = missionObject;
            this.BodyFlags = bodyFlags;
            this.ApplyToChildren = applyToChildren;
        }

        // Token: 0x06000360 RID: 864 RVA: 0x00006AE4 File Offset: 0x00004CE4
        public AddMissionObjectBodyFlagPE()
        {
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjectsDetailed;
        }
        protected override bool OnRead()
        {
            bool result = true;
            this.MissionObject = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.BodyFlags = (BodyFlags)GameNetworkMessage.ReadIntFromPacket(CompressionBasic.FlagsCompressionInfo, ref result);
            this.ApplyToChildren = GameNetworkMessage.ReadBoolFromPacket(ref result);

            return result;
        }
        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.MissionObject.Id);
            GameNetworkMessage.WriteIntToPacket((int)this.BodyFlags, CompressionBasic.FlagsCompressionInfo);
            GameNetworkMessage.WriteBoolToPacket(this.ApplyToChildren);
        }
        protected override string OnGetLogFormat()
        {
            return string.Concat(new object[]
            {
                "Add bodyflags: ",
                this.BodyFlags,
                " to MissionObject with ID: ",
                this.MissionObject.Id,
                " and with name: ",
                this.MissionObject.GameEntity.Name,
                this.ApplyToChildren ? "" : " and to all of its children."
            });
        }

    }
}

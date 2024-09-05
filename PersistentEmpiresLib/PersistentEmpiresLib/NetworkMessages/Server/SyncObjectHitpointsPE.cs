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
 
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SyncObjectHitpointsPE : GameNetworkMessage
    {
        public MissionObject MissionObject { get; private set; }

        public Vec3 ImpactDirection { get; private set; }
        // Token: 0x170001B5 RID: 437
        // (get) Token: 0x06000798 RID: 1944 RVA: 0x0000DD42 File Offset: 0x0000BF42
        // (set) Token: 0x06000799 RID: 1945 RVA: 0x0000DD4A File Offset: 0x0000BF4A
        public float Hitpoints { get; private set; }

        // Token: 0x0600079A RID: 1946 RVA: 0x0000DD53 File Offset: 0x0000BF53
        public SyncObjectHitpointsPE(MissionObject missionObject, Vec3 impactDirection, float hitpoints)
        {
            this.MissionObject = missionObject;
            this.Hitpoints = hitpoints;
            this.ImpactDirection = impactDirection;
        }

        // Token: 0x0600079B RID: 1947 RVA: 0x0000DD69 File Offset: 0x0000BF69
        public SyncObjectHitpointsPE()
        {
        }
        protected override bool OnRead()
        {
            bool result = true;
            this.MissionObject = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.Hitpoints = GameNetworkMessage.ReadFloatFromPacket(CompressionMission.UsableGameObjectHealthCompressionInfo, ref result);
            this.ImpactDirection = GameNetworkMessage.ReadVec3FromPacket(CompressionMission.UsableGameObjectBlowDirection, ref result);


            return result;
        }

        // Token: 0x0600079D RID: 1949 RVA: 0x0000DDA3 File Offset: 0x0000BFA3
        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.MissionObject.Id);
            GameNetworkMessage.WriteFloatToPacket(MathF.Max(this.Hitpoints, 0f), CompressionMission.UsableGameObjectHealthCompressionInfo);
            GameNetworkMessage.WriteVec3ToPacket(this.ImpactDirection, CompressionMission.UsableGameObjectBlowDirection);
        }

        // Token: 0x0600079E RID: 1950 RVA: 0x0000DDCA File Offset: 0x0000BFCA
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjectsDetailed | MultiplayerMessageFilter.SiegeWeaponsDetailed;
        }

        // Token: 0x0600079F RID: 1951 RVA: 0x0000DDD4 File Offset: 0x0000BFD4
        protected override string OnGetLogFormat()
        {
            return string.Concat(new object[]
            {
                "Synchronize HitPoints: ",
                this.Hitpoints,
                " of MissionObject with Id: ",
                this.MissionObject.Id,
                " and name: ",
                this.MissionObject.GameEntity.Name
            });
        }
    }
}

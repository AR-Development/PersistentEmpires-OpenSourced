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
    public sealed class AddPhysicsToMissionObject : GameNetworkMessage
    {
        public MissionObject MissionObject { get; private set; }
        public Vec3 InitialVelocity { get; set; }
        public Vec3 AngularVelocity { get; set; }
        public string PhysicsMaterial { get; set; }

        public AddPhysicsToMissionObject() { }
        public AddPhysicsToMissionObject(MissionObject missionObject, Vec3 initialVelocity, Vec3 angularVelocity, string physicsMaterial)
        {
            this.MissionObject = missionObject;
            this.InitialVelocity = initialVelocity;
            this.AngularVelocity = angularVelocity;
            this.PhysicsMaterial = physicsMaterial;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjectsDetailed;
        }

        protected override string OnGetLogFormat()
        {
            return "Physics added to game object";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.MissionObject = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.InitialVelocity = GameNetworkMessage.ReadVec3FromPacket(CompressionMission.SpawnedItemVelocityCompressionInfo, ref result);
            this.AngularVelocity = GameNetworkMessage.ReadVec3FromPacket(CompressionMission.SpawnedItemAngularVelocityCompressionInfo, ref result);
            this.PhysicsMaterial = GameNetworkMessage.ReadStringFromPacket(ref result);



            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.MissionObject.Id);
            GameNetworkMessage.WriteVec3ToPacket(this.InitialVelocity, CompressionMission.SpawnedItemVelocityCompressionInfo);
            GameNetworkMessage.WriteVec3ToPacket(this.AngularVelocity, CompressionMission.SpawnedItemAngularVelocityCompressionInfo);
            GameNetworkMessage.WriteStringToPacket(this.PhysicsMaterial);
        }
    }
}

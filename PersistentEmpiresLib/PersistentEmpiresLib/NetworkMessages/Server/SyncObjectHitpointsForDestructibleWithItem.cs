using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SyncObjectHitpointsForDestructibleWithItem : GameNetworkMessage
    {
        public MissionObject MissionObject { get; private set; }

        public Vec3 ImpactDirection { get; private set; }
        // Token: 0x170001B5 RID: 437
        // (get) Token: 0x06000798 RID: 1944 RVA: 0x0000DD42 File Offset: 0x0000BF42
        // (set) Token: 0x06000799 RID: 1945 RVA: 0x0000DD4A File Offset: 0x0000BF4A
        public float Hitpoints { get; private set; }

        // Token: 0x0600079A RID: 1946 RVA: 0x0000DD53 File Offset: 0x0000BF53
        public SyncObjectHitpointsForDestructibleWithItem(MissionObject missionObject, Vec3 impactDirection, float hitpoints)
        {
            this.MissionObject = missionObject;
            this.Hitpoints = hitpoints;
            this.ImpactDirection = impactDirection;
        }

        // Token: 0x0600079B RID: 1947 RVA: 0x0000DD69 File Offset: 0x0000BF69
        public SyncObjectHitpointsForDestructibleWithItem()
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
                "Destructible Item HitPoints: ",
                this.Hitpoints,
                " of MissionObject with Id: ",
                this.MissionObject.Id,
                " and name: ",
                this.MissionObject.GameEntity.Name
            });
        }
    }
}

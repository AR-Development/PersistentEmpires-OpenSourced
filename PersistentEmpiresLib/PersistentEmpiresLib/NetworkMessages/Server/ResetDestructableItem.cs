using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class ResetDestructableItem : GameNetworkMessage
    {

        public ResetDestructableItem()
        {
        }

        public ResetDestructableItem(MissionObject missionObject)
        {
            this.MissionObject = missionObject;
        }

        public MissionObject MissionObject { get; private set; }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjectsDetailed | MultiplayerMessageFilter.SiegeWeaponsDetailed;
        }

        // Token: 0x0600079F RID: 1951 RVA: 0x0000DDD4 File Offset: 0x0000BFD4
        protected override string OnGetLogFormat()
        {
            return string.Concat(new object[]
            {
                "Reset Object: ",
                this.MissionObject.Id,
                " and name: ",
                this.MissionObject.GameEntity.Name
            });
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.MissionObject = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.MissionObject.Id);
        }
    }
}

using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class ResetSiegeTower : GameNetworkMessage
    {
        public ResetSiegeTower() { }
        public ResetSiegeTower(MissionObject siegeTower)
        {
            this.SiegeTower = siegeTower;
        }

        public MissionObject SiegeTower { get; private set; }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "ResetSiegeTower";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.SiegeTower = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.SiegeTower.Id);
        }
    }
}

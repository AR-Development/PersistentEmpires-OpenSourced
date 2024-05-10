using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class StartTurningRightMoveableMachineServer : GameNetworkMessage
    {
        public MissionObject Machine { get; set; }
        public MatrixFrame Frame { get; set; }

        public StartTurningRightMoveableMachineServer(MissionObject machine, MatrixFrame frame)
        {
            this.Machine = machine;
            this.Frame = frame;
        }
        public StartTurningRightMoveableMachineServer()
        {
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "StartMovingUpMoveableMachineServer";
        }

        protected override bool OnRead()
        {
            bool res = true;
            this.Machine = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref res));
            this.Frame = GameNetworkMessage.ReadMatrixFrameFromPacket(ref res);
            return res;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.Machine.Id);
            GameNetworkMessage.WriteMatrixFrameToPacket(this.Frame);
        }
    }
}

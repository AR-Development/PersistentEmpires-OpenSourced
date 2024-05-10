using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class StopMovingBackwardMoveableMachine : GameNetworkMessage
    {
        public MissionObject Object { get; set; }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }
        public StopMovingBackwardMoveableMachine() { }
        public StopMovingBackwardMoveableMachine(MissionObject Object)
        {
            this.Object = Object;
        }
        protected override string OnGetLogFormat()
        {
            return "Stop Moving Backward Game Object";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Object = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.Object.Id);
        }
    }
}

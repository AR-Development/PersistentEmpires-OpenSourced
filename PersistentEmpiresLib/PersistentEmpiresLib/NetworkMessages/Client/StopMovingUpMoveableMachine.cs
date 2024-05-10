using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class StopMovingUpMoveableMachine : GameNetworkMessage
    {
        public MissionObject Object { get; set; }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }
        public StopMovingUpMoveableMachine() { }
        public StopMovingUpMoveableMachine(MissionObject Object)
        {
            this.Object = Object;
        }
        protected override string OnGetLogFormat()
        {
            return "Stop Moving Up Game Object";
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

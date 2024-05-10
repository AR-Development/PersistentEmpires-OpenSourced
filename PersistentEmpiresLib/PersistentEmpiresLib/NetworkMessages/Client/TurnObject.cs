using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class TurnObject : GameNetworkMessage
    {
        public bool Right { get; set; }
        public MissionObject Object { get; set; }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }
        public TurnObject() { }
        public TurnObject(MissionObject Object, bool Right)
        {
            this.Object = Object;
            this.Right = Right;
        }
        protected override string OnGetLogFormat()
        {
            return "Move Game Object";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Right = GameNetworkMessage.ReadBoolFromPacket(ref result);
            this.Object = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteBoolToPacket(this.Right);
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.Object.Id);
        }
    }
}

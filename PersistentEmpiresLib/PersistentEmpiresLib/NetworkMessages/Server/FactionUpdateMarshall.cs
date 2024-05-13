using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class FactionUpdateMarshall : GameNetworkMessage
    {
        public int FactionIndex;
        public NetworkCommunicator TargetPlayer;
        public bool IsMarshall;

        public FactionUpdateMarshall() { }
        public FactionUpdateMarshall(int FactionIndex, NetworkCommunicator TargetPlayer, bool IsMarshall)
        {
            this.TargetPlayer = TargetPlayer;
            this.FactionIndex = FactionIndex;
            this.IsMarshall = IsMarshall;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "FactionUpdateMarshall";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            this.TargetPlayer = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            this.IsMarshall = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.FactionIndex, new CompressionInfo.Integer(-1, 200, true));
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.TargetPlayer);
            GameNetworkMessage.WriteBoolToPacket(this.IsMarshall);
        }
    }
}

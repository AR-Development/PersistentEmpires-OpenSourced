using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SyncMember : GameNetworkMessage
    {
        public NetworkCommunicator Peer;
        public int FactionIndex;
        public bool IsMarshall;
        public bool CanUseLordPoll;
        public SyncMember() { }
        public SyncMember(NetworkCommunicator peer, int factionIndex, bool isMarshall, bool canUseLordPoll)
        {
            Peer = peer;
            FactionIndex = factionIndex;
            IsMarshall = isMarshall;
            CanUseLordPoll = canUseLordPoll;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync members";
        }

        protected override bool OnRead()
        {
            bool result = true;
            Peer = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            IsMarshall = GameNetworkMessage.ReadBoolFromPacket(ref result);
            CanUseLordPoll = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(Peer);
            GameNetworkMessage.WriteIntToPacket(FactionIndex, new CompressionInfo.Integer(-1, 200, true));
            GameNetworkMessage.WriteBoolToPacket(IsMarshall);
            GameNetworkMessage.WriteBoolToPacket(CanUseLordPoll);
        }
    }
}
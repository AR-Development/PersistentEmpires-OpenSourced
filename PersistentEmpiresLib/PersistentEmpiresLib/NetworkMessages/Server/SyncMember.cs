using PersistentEmpiresLib.ErrorLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public SyncMember() { }
        public SyncMember(NetworkCommunicator Peer, int FactionIndex, bool IsMarshall)
        {
            this.Peer = Peer;
            this.FactionIndex = FactionIndex;
            this.IsMarshall = IsMarshall;
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
            this.Peer = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            this.FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            this.IsMarshall = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Peer);
            GameNetworkMessage.WriteIntToPacket(this.FactionIndex,new CompressionInfo.Integer(-1, 200, true));
            GameNetworkMessage.WriteBoolToPacket(this.IsMarshall);
        }
    }
}

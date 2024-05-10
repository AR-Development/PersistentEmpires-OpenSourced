using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SendBatchVoiceToPlay : GameNetworkMessage
    {
        public NetworkCommunicator Peer;
        public byte[] PackedBuffer;
        public int[] BufferLens;

        public SendBatchVoiceToPlay() { }
        public SendBatchVoiceToPlay(NetworkCommunicator player, byte[] packedBuffer, int[] bufferLens)
        {
            this.Peer = player;
            this.PackedBuffer = packedBuffer;
            this.BufferLens = bufferLens;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "SendBatchVoiceToPlay";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Peer = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            int bufferLenLen = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 10, true), ref result);

            this.BufferLens = new int[bufferLenLen];
            int sum = 0;
            for (int i = 0; i < bufferLenLen; i++)
            {
                this.BufferLens[i] = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 1440, true), ref result);
                sum += this.BufferLens[i];
            }
            this.PackedBuffer = new byte[sum];
            GameNetworkMessage.ReadByteArrayFromPacket(this.PackedBuffer, 0, sum, ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Peer);
            GameNetworkMessage.WriteIntToPacket(this.BufferLens.Length, new CompressionInfo.Integer(0, 10, true));
            for (int i = 0; i < this.BufferLens.Length; i++)
            {
                GameNetworkMessage.WriteIntToPacket(this.BufferLens[i], new CompressionInfo.Integer(0, 1440, true));
            }
            GameNetworkMessage.WriteByteArrayToPacket(this.PackedBuffer, 0, this.PackedBuffer.Length);
        }
    }
}

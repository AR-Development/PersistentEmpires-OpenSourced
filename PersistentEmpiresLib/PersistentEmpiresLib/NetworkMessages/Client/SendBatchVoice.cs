using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]

    public sealed class SendBatchVoice : GameNetworkMessage
    {
        public byte[] PackedBuffer;
        public int[] BufferLens;
        public SendBatchVoice()
        {
        }

        public SendBatchVoice(byte[][] bufferBatch, int[] bufferLens)
        {
            int sum = 0;
            for (int i = 0; i < bufferLens.Length; i++)
            {
                sum += bufferLens[i];
            }

            this.PackedBuffer = new byte[sum];
            int dstOffset = 0;
            for (int i = 0; i < bufferLens.Length; i++)
            {
                Buffer.BlockCopy(bufferBatch[i], 0, this.PackedBuffer, dstOffset, bufferLens[i]);
                dstOffset += bufferLens[i];
            }

            this.BufferLens = bufferLens;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Mission;
        }

        protected override string OnGetLogFormat()
        {
            return "SendBatchVoice";
        }

        protected override bool OnRead()
        {
            bool result = true;
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
            GameNetworkMessage.WriteIntToPacket(this.BufferLens.Length, new CompressionInfo.Integer(0, 10, true));
            for (int i = 0; i < this.BufferLens.Length; i++)
            {
                GameNetworkMessage.WriteIntToPacket(this.BufferLens[i], new CompressionInfo.Integer(0, 1440, true));
            }
            GameNetworkMessage.WriteByteArrayToPacket(this.PackedBuffer, 0, this.PackedBuffer.Length);
        }
    }
}

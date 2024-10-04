using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SendRulesToNewClientMessage : GameNetworkMessage
    {
        public int PackageId { get; private set; }
        public int PackageCount { get; private set; }
        public string ConfigChunk { get; private set; }

        public SendRulesToNewClientMessage()
        {

        }

        public SendRulesToNewClientMessage(int packageId, int packageCount, string configChunk)
        {
            this.PackageId = packageId;
            this.PackageCount = packageCount;
            this.ConfigChunk = configChunk;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "CustomBubbleMessage";
        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;
            PackageId = ReadIntFromPacket(new CompressionInfo.Integer(0, 1000, true), ref bufferReadValid);
            PackageCount = ReadIntFromPacket(new CompressionInfo.Integer(0, 1000, true), ref bufferReadValid);
            ConfigChunk = ReadStringFromPacket(ref bufferReadValid);
            return bufferReadValid;
        }

        protected override void OnWrite()
        {
            WriteIntToPacket(PackageId, new CompressionInfo.Integer(0, 1000, true));
            WriteIntToPacket(PackageCount, new CompressionInfo.Integer(0, 1000, true));
            WriteStringToPacket(ConfigChunk);
        }
    }
}
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class CustomBubbleMessage : GameNetworkMessage
    {
        public CustomBubbleMessage() { }
        public CustomBubbleMessage(NetworkCommunicator sender, string message, uint color)
        {
            Message = message;
            Sender = sender;
            Color = color;
        }

        public string Message { get; private set; }
        public NetworkCommunicator Sender { get; private set; }
        public uint Color { get; private set; }

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
            bool result = true;
            Message = GameNetworkMessage.ReadStringFromPacket(ref result);
            Sender = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            Color = GameNetworkMessage.ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref result);

            return true;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.Message);
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Sender);
            GameNetworkMessage.WriteUintToPacket(Color, CompressionBasic.ColorCompressionInfo);
        }
    }
}

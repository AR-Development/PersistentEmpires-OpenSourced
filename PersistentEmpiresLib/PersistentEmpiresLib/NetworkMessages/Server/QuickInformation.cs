using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class QuickInformation : GameNetworkMessage
    {
        public string Message;
        public uint Color;

        public QuickInformation()
        {
        }

        public QuickInformation(string message, uint color)
        {
            Message = message;
            Color = color;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Quick information sent";
        }

        protected override bool OnRead()
        {
            bool result = true;
            Message = GameNetworkMessage.ReadStringFromPacket(ref result);
            Color = GameNetworkMessage.ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(Message);
            GameNetworkMessage.WriteUintToPacket(Color, CompressionBasic.ColorCompressionInfo);
        }
    }
}

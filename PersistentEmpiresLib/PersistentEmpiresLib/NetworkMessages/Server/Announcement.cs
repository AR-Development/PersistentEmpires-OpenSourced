using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class Announcement : GameNetworkMessage
    {
        public string Message;
        public uint Color;

        public Announcement()
        {
        }
        public Announcement(string Message, uint Color)
        {
            this.Message = Message;
            this.Color = Color;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Announcement made";
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

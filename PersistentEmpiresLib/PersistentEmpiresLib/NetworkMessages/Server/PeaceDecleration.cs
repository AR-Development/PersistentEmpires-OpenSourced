using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class PeaceDecleration : GameNetworkMessage
    {
        public int PeaceDeclarerIndex { get; set; }
        public int PeaceDeclaredTo { get; set; }
        public PeaceDecleration() { }
        public PeaceDecleration(int PeaceDeclarer, int PeaceDeclaredTo)
        {
            this.PeaceDeclarerIndex = PeaceDeclarer;
            this.PeaceDeclaredTo = PeaceDeclaredTo;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "PeaceDecleration";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.PeaceDeclarerIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 200, true), ref result);
            this.PeaceDeclaredTo = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 200, true), ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.PeaceDeclarerIndex, new CompressionInfo.Integer(0, 200, true));
            GameNetworkMessage.WriteIntToPacket(this.PeaceDeclaredTo, new CompressionInfo.Integer(0, 200, true));
        }
    }
}

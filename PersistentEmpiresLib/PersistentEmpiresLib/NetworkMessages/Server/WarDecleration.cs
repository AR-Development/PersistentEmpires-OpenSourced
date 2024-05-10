using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class WarDecleration : GameNetworkMessage
    {
        public int WarDeclarerIndex { get; set; }
        public int WarDeclaredTo { get; set; }
        public WarDecleration() { }
        public WarDecleration(int WarDeclarerIndex, int WarDeclaredTo)
        {
            this.WarDeclarerIndex = WarDeclarerIndex;
            this.WarDeclaredTo = WarDeclaredTo;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "WarDecleration";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.WarDeclarerIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 200, true), ref result);
            this.WarDeclaredTo = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 200, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.WarDeclarerIndex, new CompressionInfo.Integer(0, 200, true));
            GameNetworkMessage.WriteIntToPacket(this.WarDeclaredTo, new CompressionInfo.Integer(0, 200, true));
        }
    }
}

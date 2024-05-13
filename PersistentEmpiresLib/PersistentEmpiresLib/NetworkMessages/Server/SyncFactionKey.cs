using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SyncFactionKey : GameNetworkMessage
    {
        public int FactionIndex;
        public string PlayerId;
        public int KeyType;
        public SyncFactionKey() { }
        public SyncFactionKey(int factionIndex, string playerId, int keyType)
        {
            this.FactionIndex = factionIndex;
            this.PlayerId = playerId;
            this.KeyType = keyType;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "SyncFactionKey";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 200, true), ref result);
            this.PlayerId = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.KeyType = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 10, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.FactionIndex, new CompressionInfo.Integer(0, 200, true));
            GameNetworkMessage.WriteStringToPacket(this.PlayerId);
            GameNetworkMessage.WriteIntToPacket(this.KeyType, new CompressionInfo.Integer(0, 10, true));
        }
    }
}

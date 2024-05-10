using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestFactionKeys : GameNetworkMessage
    {
        public int KeyType;
        public RequestFactionKeys() { }
        public RequestFactionKeys(int keyType)
        {
            // keyType == 0 Door
            // keyType == 1 Chest
            this.KeyType = keyType;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "RequestFactionKeys";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.KeyType = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 10, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.KeyType, new CompressionInfo.Integer(-1, 10, true));
        }
    }
}

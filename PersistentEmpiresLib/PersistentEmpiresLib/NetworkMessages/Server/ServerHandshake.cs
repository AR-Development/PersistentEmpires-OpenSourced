using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class ServerHandshake : GameNetworkMessage
    {
        public string ServerSignature { get; set; }

        public ServerHandshake(string serverSignature)
        {
            this.ServerSignature = serverSignature;
        }
        public ServerHandshake() { }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "ServerHandshake";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.ServerSignature = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.ServerSignature);
        }
    }
}

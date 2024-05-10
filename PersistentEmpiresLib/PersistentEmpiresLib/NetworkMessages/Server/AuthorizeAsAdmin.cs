using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class AuthorizeAsAdmin : GameNetworkMessage
    {
        public AuthorizeAsAdmin() { }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received AuthorizeAsAdmin";
        }

        protected override bool OnRead()
        {
            return true;
        }

        protected override void OnWrite()
        {

        }
    }
}

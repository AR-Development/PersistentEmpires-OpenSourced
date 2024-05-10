using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestStartEat : GameNetworkMessage
    {
        public RequestStartEat() { }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.AgentsDetailed;
        }

        protected override string OnGetLogFormat()
        {
            return "Request eat";
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

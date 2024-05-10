using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestRevealItemBag : GameNetworkMessage
    {
        public RequestRevealItemBag() { }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Reveal Item Bag Requested";
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

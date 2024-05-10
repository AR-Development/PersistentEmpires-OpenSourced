using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestAnimation : GameNetworkMessage
    {
        public string ActionId;
        public RequestAnimation() { }
        public RequestAnimation(string actionId)
        {
            this.ActionId = actionId;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "RequestAnimation";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.ActionId = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.ActionId);
        }
    }
}

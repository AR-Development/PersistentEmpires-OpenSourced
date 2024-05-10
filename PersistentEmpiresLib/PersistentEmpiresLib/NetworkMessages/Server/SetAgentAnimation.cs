using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]

    public sealed class SetAgentAnimation : GameNetworkMessage
    {
        public Agent AnimAgent;
        public string ActionId;

        public SetAgentAnimation() { }
        public SetAgentAnimation(Agent _animAgent, string _actionId)
        {
            this.AnimAgent = _animAgent;
            this.ActionId = _actionId;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "SetAgentAnimation";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.AnimAgent = Mission.MissionNetworkHelper.GetAgentFromIndex(GameNetworkMessage.ReadAgentIndexFromPacket(ref result));
            this.ActionId = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteAgentIndexToPacket(this.AnimAgent.Index);
            GameNetworkMessage.WriteStringToPacket(this.ActionId);
        }
    }
}

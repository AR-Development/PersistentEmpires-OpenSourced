using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class AgentLabelConfig : GameNetworkMessage
    {
        public AgentLabelConfig()
        {
        }
        public AgentLabelConfig(bool enabled)
        {
            this.Enabled = enabled;
        }
        public bool Enabled = true;
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.All;
        }

        protected override string OnGetLogFormat()
        {
            return "AgentLabelConfig";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Enabled = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteBoolToPacket(this.Enabled);
        }
    }
}

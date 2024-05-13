using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class AgentPlayingInstrument : GameNetworkMessage
    {
        public Agent PlayerAgent;
        public int PlayingInstrumentIndex;
        public bool IsPlaying;
        public AgentPlayingInstrument() { }
        public AgentPlayingInstrument(Agent agent, int index, bool isPlaying)
        {
            this.PlayerAgent = agent;
            this.PlayingInstrumentIndex = index;
            this.IsPlaying = isPlaying;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Agents;
        }

        protected override string OnGetLogFormat()
        {
            return "AgentPlayingInstrument";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.PlayerAgent = Mission.MissionNetworkHelper.GetAgentFromIndex(GameNetworkMessage.ReadAgentIndexFromPacket(ref result));
            this.PlayingInstrumentIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 100, true), ref result);
            this.IsPlaying = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteAgentIndexToPacket(this.PlayerAgent.Index);
            GameNetworkMessage.WriteIntToPacket(this.PlayingInstrumentIndex, new CompressionInfo.Integer(0, 100, true));
            GameNetworkMessage.WriteBoolToPacket(this.IsPlaying);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]

    public sealed class BehadeAgentPacket : GameNetworkMessage
    {
        public Agent BodyAgent;
        public Agent HeadAgent;
        public BehadeAgentPacket() { }
        public BehadeAgentPacket(Agent bodyAgent, Agent headAgent)
        {
            this.HeadAgent = headAgent;
            this.BodyAgent = bodyAgent;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "BehadeAgentPacket";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.BodyAgent = Mission.MissionNetworkHelper.GetAgentFromIndex(GameNetworkMessage.ReadAgentIndexFromPacket(ref result));
            this.HeadAgent = Mission.MissionNetworkHelper.GetAgentFromIndex(GameNetworkMessage.ReadAgentIndexFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteAgentIndexToPacket(this.BodyAgent.Index);
            GameNetworkMessage.WriteAgentIndexToPacket(this.HeadAgent.Index);
        }
    }
}

using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SyncAttachToAgent : GameNetworkMessage
    {
        public PE_AttachToAgent AttachToAgent;
        public Agent UserAgent;

        public SyncAttachToAgent() { }
        public SyncAttachToAgent(PE_AttachToAgent attachToAgent, Agent userAgent)
        {
            this.AttachToAgent = attachToAgent;
            this.UserAgent = userAgent;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync Attach To Agent";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.AttachToAgent = (PE_AttachToAgent)Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.UserAgent = Mission.MissionNetworkHelper.GetAgentFromIndex(GameNetworkMessage.ReadAgentIndexFromPacket(ref result), true);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.AttachToAgent.Id);
            GameNetworkMessage.WriteAgentIndexToPacket(this.UserAgent.Index);
        }
    }
}

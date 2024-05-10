using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class FactionLordPollOpened : GameNetworkMessage
    {
        public FactionLordPollOpened() { }

        public FactionLordPollOpened(NetworkCommunicator pollCreator, NetworkCommunicator lordCandidate)
        {
            this.PollCreator = pollCreator;
            this.LordCandidate = lordCandidate;
        }

        public NetworkCommunicator PollCreator { get; private set; }
        public NetworkCommunicator LordCandidate { get; private set; }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Server started a faction lord polling";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.PollCreator = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            this.LordCandidate = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.PollCreator);
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.LordCandidate);

            // throw new NotImplementedException();
        }
    }
}

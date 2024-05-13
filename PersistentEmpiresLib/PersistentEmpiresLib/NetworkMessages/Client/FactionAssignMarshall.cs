using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class FactionAssignMarshall : GameNetworkMessage
    {
        public NetworkCommunicator TargetPlayer;

        public FactionAssignMarshall()
        {

        }
        public FactionAssignMarshall(NetworkCommunicator networkCommunicator)
        {
            this.TargetPlayer = networkCommunicator;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Peers;
        }

        protected override string OnGetLogFormat()
        {
            return "Faction Assign Marshall";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.TargetPlayer = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.TargetPlayer);
        }
    }
}

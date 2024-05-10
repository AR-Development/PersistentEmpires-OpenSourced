using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestLordshipTransfer : GameNetworkMessage
    {
        public NetworkCommunicator TargetPlayer;

        public RequestLordshipTransfer()
        {

        }
        public RequestLordshipTransfer(NetworkCommunicator TargetPlayer)
        {
            this.TargetPlayer = TargetPlayer;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Lordship transfer";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.TargetPlayer = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            return true;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.TargetPlayer);
        }
    }
}

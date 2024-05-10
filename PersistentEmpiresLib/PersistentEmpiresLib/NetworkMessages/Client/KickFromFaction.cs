using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class KickFromFaction : GameNetworkMessage
    {
        public NetworkCommunicator Target;
        public KickFromFaction() { }
        public KickFromFaction(NetworkCommunicator target)
        {
            this.Target = target;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Kick from a faction requested";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Target = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Target);
        }
    }
}

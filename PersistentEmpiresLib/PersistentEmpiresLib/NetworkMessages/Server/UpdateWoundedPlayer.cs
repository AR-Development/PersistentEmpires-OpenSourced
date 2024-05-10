using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class UpdateWoundedPlayer : GameNetworkMessage
    {
        public UpdateWoundedPlayer()
        {

        }
        public UpdateWoundedPlayer(NetworkCommunicator player, bool isWounded)
        {
            this.Player = player;
            this.IsWounded = isWounded;
        }
        public NetworkCommunicator Player;
        public bool IsWounded;
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Agents;
        }

        protected override string OnGetLogFormat()
        {
            return "UpdateWoundedPlayer";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            this.IsWounded = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
            GameNetworkMessage.WriteBoolToPacket(this.IsWounded);
        }
    }
}

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
        public UpdateWoundedPlayer(string playerId, bool isWounded)
        {
            this.PlayerId = playerId;
            this.IsWounded = isWounded;
        }
        public string PlayerId;
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
            this.PlayerId = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.IsWounded = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.PlayerId);
            GameNetworkMessage.WriteBoolToPacket(this.IsWounded);
        }
    }
}

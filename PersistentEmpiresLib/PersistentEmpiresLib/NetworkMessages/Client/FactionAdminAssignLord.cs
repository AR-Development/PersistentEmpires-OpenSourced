using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class FactionAdminAssignLord : GameNetworkMessage
    {
        public NetworkCommunicator TargetPlayer;
        public int TargetFactionId;

        public FactionAdminAssignLord()
        {

        }
        public FactionAdminAssignLord(NetworkCommunicator networkCommunicator, int targetFactionId)
        {
            TargetPlayer = networkCommunicator;
            TargetFactionId = targetFactionId;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Peers;
        }

        protected override string OnGetLogFormat()
        {
            return "Faction Assign Lord";
        }

        protected override bool OnRead()
        {
            bool result = true;
            TargetPlayer = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            TargetFactionId = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(TargetPlayer);
            GameNetworkMessage.WriteIntToPacket(TargetFactionId, new CompressionInfo.Integer(-1, 200, true));
        }
    }
}

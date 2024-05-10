using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class PlayerJoinedFaction : GameNetworkMessage
    {

        public int factionIndex { get; set; }
        public int joinedFrom { get; set; }
        public NetworkCommunicator player { get; set; }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        public PlayerJoinedFaction(int factionIndex, int joinedFrom, NetworkCommunicator player)
        {
            this.factionIndex = factionIndex;
            this.joinedFrom = joinedFrom;
            this.player = player;
        }

        public PlayerJoinedFaction()
        {
        }

        protected override string OnGetLogFormat()
        {
            return "User added to faction";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.factionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            this.joinedFrom = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            this.player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            // throw new NotImplementedException();
            GameNetworkMessage.WriteIntToPacket(factionIndex, new CompressionInfo.Integer(-1, 200, true));
            GameNetworkMessage.WriteIntToPacket(joinedFrom, new CompressionInfo.Integer(-1, 200, true));
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.player);
        }
    }
}

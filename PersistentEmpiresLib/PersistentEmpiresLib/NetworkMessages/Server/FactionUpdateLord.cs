using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class FactionUpdateLord : GameNetworkMessage
    {
        public FactionUpdateLord() { }
        public FactionUpdateLord(int factionIndex, NetworkCommunicator player)
        {
            this.FactionIndex = factionIndex;
            this.Player = player;
        }

        public int FactionIndex { get; private set; }
        public NetworkCommunicator Player { get; private set; }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "New faction lord";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.FactionIndex, new CompressionInfo.Integer(-1, 200, true));
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
        }
    }
}

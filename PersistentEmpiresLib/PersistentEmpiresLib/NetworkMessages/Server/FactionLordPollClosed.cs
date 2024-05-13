using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class FactionLordPollClosed : GameNetworkMessage
    {
        // public NetworkCommunicator TargetPlayer {
        public FactionLordPollClosed()
        {
        }

        public FactionLordPollClosed(NetworkCommunicator TargetPlayer, bool Accepted, int FactionIndex)
        {
            this.TargetPlayer = TargetPlayer;
            this.Accepted = Accepted;
            this.FactionIndex = FactionIndex;
        }

        public NetworkCommunicator TargetPlayer { get; private set; }
        public bool Accepted { get; private set; }

        public int FactionIndex { get; private set; }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Faction selected a new lord";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.TargetPlayer = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            this.Accepted = GameNetworkMessage.ReadBoolFromPacket(ref result);
            this.FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200), ref result);


            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.TargetPlayer);
            GameNetworkMessage.WriteBoolToPacket(this.Accepted);
            GameNetworkMessage.WriteIntToPacket(this.FactionIndex, new CompressionInfo.Integer(-1, 200));
        }
    }
}

using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class RevealMoneyPouchServer : GameNetworkMessage
    {
        public NetworkCommunicator Player;
        public int Gold;

        public RevealMoneyPouchServer()
        {

        }
        public RevealMoneyPouchServer(NetworkCommunicator Player, int Gold)
        {
            this.Player = Player;
            this.Gold = Gold;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Money pouch revealed";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            this.Gold = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, Int32.MaxValue, true), ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
            GameNetworkMessage.WriteIntToPacket(this.Gold, new CompressionInfo.Integer(0, Int32.MaxValue, true));
        }
    }
}

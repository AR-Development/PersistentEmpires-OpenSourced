using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class PlayerIsTypingMessageServer : GameNetworkMessage
    {
        public NetworkCommunicator Sender;

        public PlayerIsTypingMessageServer() { }

        public PlayerIsTypingMessageServer(NetworkCommunicator sender)
        {
            Sender = sender;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "PlayerIsTypingMessageServer";
        }

        protected override bool OnRead()
        {
            bool result = true;
            Sender = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Sender);
        }
    }
}
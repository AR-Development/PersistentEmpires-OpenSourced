using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class LocalMessageServer : GameNetworkMessage
    {
        public String Message;
        public NetworkCommunicator Sender;

        public LocalMessageServer() { }
        public LocalMessageServer(String message, NetworkCommunicator sender)
        {
            this.Sender = sender;
            this.Message = message;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Local message received";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Sender = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            this.Message = GameNetworkMessage.ReadStringFromPacket(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Sender);
            GameNetworkMessage.WriteStringToPacket(this.Message);
        }
    }
}

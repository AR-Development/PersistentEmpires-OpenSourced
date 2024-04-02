using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class CustomBubbleMessage : GameNetworkMessage
    {
        public CustomBubbleMessage() { }
        public CustomBubbleMessage(NetworkCommunicator sender,string Message)
        {
            this.Message = Message;
            this.Sender = sender;
        }

        public string Message { get; private set; }
        public NetworkCommunicator Sender { get; private set; }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "CustomBubbleMessage";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Message = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.Sender = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            return true;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.Message);
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Sender);
        }
    }
}

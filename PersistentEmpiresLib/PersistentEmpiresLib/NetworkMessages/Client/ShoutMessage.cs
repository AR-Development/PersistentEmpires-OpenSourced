using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class ShoutMessage : GameNetworkMessage
    {
        public String Text { get; set; }
        public ShoutMessage() { }
        public ShoutMessage(string text)
        {
            this.Text = text;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "ShoutMessage chat";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Text = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.Text);
        }
    }
}

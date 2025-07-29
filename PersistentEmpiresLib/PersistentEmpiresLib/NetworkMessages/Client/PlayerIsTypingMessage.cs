using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class PlayerIsTypingMessage : GameNetworkMessage
    {
        public string Text { get; set; }
        public PlayerIsTypingMessage() 
        {
            Text = "a";
        }
        
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "PlayerIsTypingMessage";
        }

        protected override bool OnRead()
        {
            bool result = true;
            Text = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(Text);
        }
    }
}
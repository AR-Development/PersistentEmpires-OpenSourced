using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class EnableVoiceChat : GameNetworkMessage
    {

        public EnableVoiceChat() { }
        public EnableVoiceChat(bool enabled)
        {
            Enabled = enabled;
        }

        public bool Enabled { get; private set; }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.NormalLogging;
        }

        protected override string OnGetLogFormat()
        {
            return "Enable / disable VoiceChat";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Enabled = GameNetworkMessage.ReadBoolFromPacket(ref result);


            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteBoolToPacket(this.Enabled);
        }
    }
}

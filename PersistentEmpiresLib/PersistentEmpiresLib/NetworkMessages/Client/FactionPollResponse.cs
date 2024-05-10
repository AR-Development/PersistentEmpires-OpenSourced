using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class FactionPollResponse : GameNetworkMessage
    {
        public bool Accepted { get; private set; }

        public FactionPollResponse()
        {
        }
        public FactionPollResponse(bool accepted)
        {
            this.Accepted = accepted;
        }
        protected override bool OnRead()
        {
            bool result = true;
            this.Accepted = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        // Token: 0x06000099 RID: 153 RVA: 0x00002BFD File Offset: 0x00000DFD
        protected override void OnWrite()
        {
            GameNetworkMessage.WriteBoolToPacket(this.Accepted);
        }

        // Token: 0x0600009A RID: 154 RVA: 0x00002C0A File Offset: 0x00000E0A
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        // Token: 0x0600009B RID: 155 RVA: 0x00002C12 File Offset: 0x00000E12
        protected override string OnGetLogFormat()
        {
            return "Receiving faction poll response: " + (this.Accepted ? "Accepted." : "Not accepted.");
        }
    }
}

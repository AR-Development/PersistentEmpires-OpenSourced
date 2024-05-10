using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class FactionPollProgress : GameNetworkMessage
    {
        public int VotesAccepted { get; private set; }
        public int VotesRejected { get; private set; }
        public FactionPollProgress() { }

        public FactionPollProgress(int voteAccepted, int voteRejected)
        {
            this.VotesAccepted = voteAccepted;
            this.VotesRejected = voteRejected;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Update on the voting progress.";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.VotesAccepted = GameNetworkMessage.ReadIntFromPacket(CompressionBasic.PlayerCompressionInfo, ref result);
            this.VotesRejected = GameNetworkMessage.ReadIntFromPacket(CompressionBasic.PlayerCompressionInfo, ref result);

            return result;
        }

        // Token: 0x06000302 RID: 770 RVA: 0x0000634C File Offset: 0x0000454C
        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.VotesAccepted, CompressionBasic.PlayerCompressionInfo);
            GameNetworkMessage.WriteIntToPacket(this.VotesRejected, CompressionBasic.PlayerCompressionInfo);
        }
    }
}

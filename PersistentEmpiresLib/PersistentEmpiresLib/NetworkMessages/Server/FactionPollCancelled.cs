using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class FactionPollCancelled : GameNetworkMessage
    {
        public int FactionIndex;
        public FactionPollCancelled() { }

        public FactionPollCancelled(int factionIndex)
        {
            this.FactionIndex = factionIndex;
        }
        protected override bool OnRead()
        {
            bool result = true;
            this.FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200), ref result);

            return result;
        }

        // Token: 0x060002F8 RID: 760 RVA: 0x000062C6 File Offset: 0x000044C6
        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.FactionIndex, new CompressionInfo.Integer(-1, 200));
        }

        // Token: 0x060002F9 RID: 761 RVA: 0x000062C8 File Offset: 0x000044C8
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        // Token: 0x060002FA RID: 762 RVA: 0x000062D0 File Offset: 0x000044D0
        protected override string OnGetLogFormat()
        {
            return "Poll cancelled.";
        }
    }
}

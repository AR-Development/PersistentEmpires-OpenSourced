using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SyncGold : GameNetworkMessage
    {
        public int Gold;
        public SyncGold() { }
        public SyncGold(int gold)
        {
            this.Gold = gold;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync gold";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Gold = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, Int32.MaxValue, true), ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.Gold, new CompressionInfo.Integer(0, Int32.MaxValue, true));
        }
    }
}

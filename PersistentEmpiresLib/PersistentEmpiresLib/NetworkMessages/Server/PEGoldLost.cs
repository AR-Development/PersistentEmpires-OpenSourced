using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class PEGoldLost : GameNetworkMessage
    {
        public int Lost;
        public PEGoldLost() { }
        public PEGoldLost(int gold)
        {
            this.Lost = gold;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Gold lost";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Lost = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, Int32.MaxValue, true), ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.Lost, new CompressionInfo.Integer(0, Int32.MaxValue, true));
        }
    }
}

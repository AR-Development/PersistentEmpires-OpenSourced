using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class PEGoldGain : GameNetworkMessage
    {
        public int Gain;
        public PEGoldGain() { }
        public PEGoldGain(int gold)
        {
            this.Gain = gold;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Gold gain";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Gain = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, Int32.MaxValue, true), ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.Gain, new CompressionInfo.Integer(0, Int32.MaxValue, true));
        }
    }
}

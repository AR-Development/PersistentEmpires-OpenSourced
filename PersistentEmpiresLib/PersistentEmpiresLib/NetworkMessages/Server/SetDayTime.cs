using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SetDayTime : GameNetworkMessage
    {
        public float TimeOfDay;
        public SetDayTime() { }
        public SetDayTime(float time)
        {
            this.TimeOfDay = time;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Mission;
        }

        protected override string OnGetLogFormat()
        {
            return "SetDayTime";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.TimeOfDay = GameNetworkMessage.ReadFloatFromPacket(new CompressionInfo.Float(0f, 24f, 22), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteFloatToPacket(this.TimeOfDay, new CompressionInfo.Float(0f, 24f, 22));
        }
    }
}


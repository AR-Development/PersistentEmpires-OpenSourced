using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestDropMoney : GameNetworkMessage
    {
        public int Amount { get; set; }
        public RequestDropMoney() { }
        public RequestDropMoney(int amount)
        {
            this.Amount = amount;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Drop money";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Amount = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, int.MaxValue, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.Amount, new CompressionInfo.Integer(0, int.MaxValue, true));
        }
    }
}

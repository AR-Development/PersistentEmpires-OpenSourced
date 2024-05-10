using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class WithdrawDepositMoneychest : GameNetworkMessage
    {
        public int Amount;
        public MissionObject MoneyChest;
        public bool Withdraw;
        public WithdrawDepositMoneychest() { }
        public WithdrawDepositMoneychest(MissionObject mc, int amount, bool withdraw)
        {
            this.Amount = amount;
            this.Withdraw = withdraw;
            this.MoneyChest = mc;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Client WithdrawMoneychest";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Amount = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 100000000, true), ref result);
            this.Withdraw = GameNetworkMessage.ReadBoolFromPacket(ref result);
            this.MoneyChest = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.Amount, new CompressionInfo.Integer(0, 100000000, true));
            GameNetworkMessage.WriteBoolToPacket(this.Withdraw);
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.MoneyChest.Id);
        }
    }
}

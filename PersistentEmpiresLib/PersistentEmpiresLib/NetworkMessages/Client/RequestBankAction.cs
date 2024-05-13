using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestBankAction : GameNetworkMessage
    {
        public MissionObject Bank;
        public int Amount;
        public bool Deposit;

        public RequestBankAction() { }
        public RequestBankAction(PE_Bank bank, int amount, bool deposit)
        {
            this.Bank = bank;
            this.Amount = amount;
            this.Deposit = deposit;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "RequestDepositBank";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Bank = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.Amount = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 1000000000, true), ref result);
            this.Deposit = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.Bank.Id);
            GameNetworkMessage.WriteIntToPacket(this.Amount, new CompressionInfo.Integer(0, 1000000000, true));
            GameNetworkMessage.WriteBoolToPacket(this.Deposit);
        }
    }
}

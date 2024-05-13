using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class OpenBank : GameNetworkMessage
    {
        public MissionObject Bank;
        public int Amount;
        public int TaxRate;
        public OpenBank()
        {

        }

        public OpenBank(MissionObject bank, int amount, int taxrate)
        {
            this.Bank = bank;
            this.Amount = amount;
            this.TaxRate = taxrate;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "OpenBank";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Bank = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.Amount = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 1000000000, true), ref result);
            this.TaxRate = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 100, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.Bank.Id);
            GameNetworkMessage.WriteIntToPacket(this.Amount, new CompressionInfo.Integer(0, 1000000000, true));
            GameNetworkMessage.WriteIntToPacket(this.TaxRate, new CompressionInfo.Integer(0, 100, true));
        }
    }
}

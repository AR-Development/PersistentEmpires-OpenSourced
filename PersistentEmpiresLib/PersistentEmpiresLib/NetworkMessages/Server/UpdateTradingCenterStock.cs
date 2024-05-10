using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class UpdateTradingCenterStock : GameNetworkMessage
    {
        public MissionObject TradingCenter;
        public int NewStock;
        public int ItemIndex;

        public UpdateTradingCenterStock() { }
        public UpdateTradingCenterStock(MissionObject tradingCenter, int newStock, int itemIndex)
        {
            this.TradingCenter = tradingCenter;
            this.ItemIndex = itemIndex;
            this.NewStock = newStock;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "UpdateTradingCenterStock";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.TradingCenter = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.NewStock = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, PE_TradeCenter.MAX_STOCK_COUNT, true), ref result);
            this.ItemIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 4096, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.TradingCenter.Id);
            GameNetworkMessage.WriteIntToPacket(this.NewStock, new CompressionInfo.Integer(0, PE_TradeCenter.MAX_STOCK_COUNT, true));
            GameNetworkMessage.WriteIntToPacket(this.ItemIndex, new CompressionInfo.Integer(0, 4096, true));
        }
    }
}

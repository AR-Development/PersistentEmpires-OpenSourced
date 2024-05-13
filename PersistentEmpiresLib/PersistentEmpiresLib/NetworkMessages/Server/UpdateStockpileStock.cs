using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class UpdateStockpileStock : GameNetworkMessage
    {
        public MissionObject StockpileMarket;
        public int NewStock;
        public int ItemIndex;

        public UpdateStockpileStock() { }

        public UpdateStockpileStock(MissionObject stockPileMarket, int newStock, int itemIndex)
        {
            this.ItemIndex = itemIndex;
            this.NewStock = newStock;
            this.StockpileMarket = stockPileMarket;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Update stockpile stock";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.StockpileMarket = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.NewStock = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, PE_StockpileMarket.MAX_STOCK_COUNT, true), ref result);
            this.ItemIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 4096, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.StockpileMarket.Id);
            GameNetworkMessage.WriteIntToPacket(this.NewStock, new CompressionInfo.Integer(0, PE_StockpileMarket.MAX_STOCK_COUNT, true));
            GameNetworkMessage.WriteIntToPacket(this.ItemIndex, new CompressionInfo.Integer(0, 4096, true));
        }
    }
}

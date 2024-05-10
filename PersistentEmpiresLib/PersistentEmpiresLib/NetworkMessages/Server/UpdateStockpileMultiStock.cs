using PersistentEmpiresLib.SceneScripts;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class UpdateStockpileMultiStock : GameNetworkMessage
    {
        public MissionObject Stockpile;
        public List<int> Indexes;
        public List<int> Stocks;

        public UpdateStockpileMultiStock() { }

        public UpdateStockpileMultiStock(PE_StockpileMarket stockpile, List<int> Indexes, List<int> Stocks)
        {
            this.Stockpile = stockpile;
            this.Indexes = Indexes;
            this.Stocks = Stocks;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Received UpdateStockpileMultiStock";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Stockpile = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.Indexes = new List<int>();
            this.Stocks = new List<int>();

            int indexLen = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 4096, true), ref result);
            for (int i = 0; i < indexLen; i++)
            {
                this.Indexes.Add(GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 4096, true), ref result));
            }

            int stocksLen = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 4096, true), ref result);
            for (int i = 0; i < stocksLen; i++)
            {
                this.Stocks.Add(GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 4096, true), ref result));
            }
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.Stockpile.Id);
            GameNetworkMessage.WriteIntToPacket(this.Indexes.Count, new CompressionInfo.Integer(0, 4096, true));
            for (int i = 0; i < this.Indexes.Count; i++)
            {
                GameNetworkMessage.WriteIntToPacket(this.Indexes[i], new CompressionInfo.Integer(0, 4096, true));
            }

            GameNetworkMessage.WriteIntToPacket(this.Stocks.Count, new CompressionInfo.Integer(0, 4096, true));
            for (int i = 0; i < this.Stocks.Count; i++)
            {
                GameNetworkMessage.WriteIntToPacket(this.Stocks[i], new CompressionInfo.Integer(0, 4096, true));
            }
        }
    }
}

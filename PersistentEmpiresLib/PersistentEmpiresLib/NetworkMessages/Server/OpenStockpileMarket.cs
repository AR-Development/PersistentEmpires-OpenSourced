using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class OpenStockpileMarket : GameNetworkMessage
    {
        public MissionObject StockpileMarketEntity;
        public Inventory PlayerInventory;
        // public List<int> Stocks;
        public OpenStockpileMarket() { }
        public OpenStockpileMarket(PE_StockpileMarket stockpileMarketEntity, Inventory playerInventory)
        {
            this.StockpileMarketEntity = stockpileMarketEntity;
            this.PlayerInventory = playerInventory;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Open Stockpile Market";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.StockpileMarketEntity = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.PlayerInventory = PENetworkModule.ReadInventoryPlayer(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.StockpileMarketEntity.Id);
            PENetworkModule.WriteInventoryPlayer(this.PlayerInventory);
        }
    }
}

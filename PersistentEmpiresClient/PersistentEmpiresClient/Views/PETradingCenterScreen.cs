using PersistentEmpires.Views.ViewsVM;
using PersistentEmpires.Views.ViewsVM.StockpileMarket;
using PersistentEmpiresClient.Views;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.Views
{
    public class PETradingCenterScreen : PEBaseItemList<PETradeCenterVM, PEStockpileMarketItemVM, MarketItem>
    {
        private TradingCenterBehavior tradingCenterBehavior;
        private PE_TradeCenter ActiveEntity;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this.tradingCenterBehavior = base.Mission.GetMissionBehavior<TradingCenterBehavior>();
            this.tradingCenterBehavior.OnTradingCenterOpen += this.OnOpen;
            this.tradingCenterBehavior.OnTradingCenterUpdate += this.OnUpdate;
            this.tradingCenterBehavior.OnTradingCenterUpdateMultiHandler += this.OnUpdateMulti;
            this._dataSource = new PETradeCenterVM(base.HandleClickItem);
        }

        private void OnUpdateMulti(PE_TradeCenter stockpileMarket, List<int> indexes, List<int> stocks)
        {
            if (this.IsActive)
            {
                for (int i = 0; i < indexes.Count; i++)
                {
                    int index = indexes[i];
                    int stock = stocks[i];
                    this._dataSource.ItemsList[index].Stock = stock;
                }
                this._dataSource.OnPropertyChanged("FilteredItemList");
            }
        }

        private void OnUpdate(PE_TradeCenter stockpileMarket, int itemIndex, int newStock)
        {
            if (this.IsActive)
            {
                this._dataSource.ItemsList[itemIndex].Stock = newStock;
                this._dataSource.OnPropertyChanged("FilteredItemList");
            }
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            if (affectedAgent.IsMine)
            {
                this.Close();
            }
        }

        public override void Close()
        {
            if (this.IsActive)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new RequestCloseTradingCenter(this.ActiveEntity));
                GameNetwork.EndModuleEventAsClient();
                this.CloseAux();
            }
        }

        private void OnOpen(PE_TradeCenter tradeCenter, Inventory playerInventory)
        {
            if (this.IsActive) return;
            this.ActiveEntity = tradeCenter;
            this._dataSource.TradeCenter = tradeCenter;
            this._dataSource.Buy = Buy;
            this._dataSource.Sell = Sell;
            this._dataSource.GetPrices = GetPrices;
            base.OnOpen(tradeCenter.MarketItems, playerInventory, "PETradingCenter");
        }

        public void Buy(PEStockpileMarketItemVM stockpileMarketItemVM)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestTradingBuyItem(this.ActiveEntity, stockpileMarketItemVM.ItemIndex));
            GameNetwork.EndModuleEventAsClient();
        }

        public void Sell(PEStockpileMarketItemVM stockpileMarketItemVM)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestTradingSellItem(this.ActiveEntity, stockpileMarketItemVM.ItemIndex));
            GameNetwork.EndModuleEventAsClient();
        }

        public void GetPrices(PEStockpileMarketItemVM stockpileMarketItemVM)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestTradingPrices(this.ActiveEntity, stockpileMarketItemVM.ItemIndex));
            GameNetwork.EndModuleEventAsClient();
        }
    }
}

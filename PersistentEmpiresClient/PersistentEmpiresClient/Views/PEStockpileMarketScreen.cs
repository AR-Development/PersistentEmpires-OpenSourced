using PersistentEmpires.Views.ViewsVM;
using PersistentEmpires.Views.ViewsVM.StockpileMarket;
using PersistentEmpiresClient.Views;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.Views
{
    public class PEStockpileMarketScreen : PEBaseItemList<PEStockpileMarketVM, PEStockpileMarketItemVM, MarketItem>
    {
        private StockpileMarketComponent _stockpileMarketComponent;
        private PE_StockpileMarket ActiveEntity;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._stockpileMarketComponent = base.Mission.GetMissionBehavior<StockpileMarketComponent>();
            this._stockpileMarketComponent.OnStockpileMarketOpen += this.OnOpen;
            this._stockpileMarketComponent.OnStockpileMarketUpdate += this.OnUpdate;
            this._stockpileMarketComponent.OnStockpileMarketUpdateMultiHandler += this.OnUpdateMulti;
            this._dataSource = new PEStockpileMarketVM(this.HandleClickItem);
        }

        private void OnUpdateMulti(PE_StockpileMarket stockpileMarket, List<int> indexes, List<int> stocks)
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

        private void OnUpdate(PE_StockpileMarket stockpileMarket, int itemIndex, int newStock)
        {
            if (this.IsActive)
            {
                Console.WriteLine(itemIndex);
                Console.WriteLine(this._dataSource.FilteredItemList.ToString());
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

        public override void HandleClickItem(PEItemVM clickedSlot)
        {
            if (this._gauntletLayer.Input.IsShiftDown())
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new InventoryHotkey(clickedSlot.DropTag));
                GameNetwork.EndModuleEventAsClient();
            }
            if (this._gauntletLayer.Input.IsControlDown() && clickedSlot.Item != null && clickedSlot.Count > 0)
            {
                PEStockpileMarketItemVM item = _dataSource.FilteredItemList.ToList().FirstOrDefault(itemVm => itemVm.MarketItem.Item.StringId == clickedSlot.Item.StringId);
                if (item != null) this.Sell(item);
            }
        }

        public override void Close()
        {
            if (this.IsActive)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new RequestCloseStockpileMarket(this.ActiveEntity));
                GameNetwork.EndModuleEventAsClient();
                this.CloseAux();
            }
        }

        private void OnOpen(PE_StockpileMarket stockpileMarket, Inventory playerInventory)
        {
            if (this.IsActive) return;
            this.ActiveEntity = stockpileMarket;
            this._dataSource.StockpileMarket = stockpileMarket;
            this._dataSource.Buy = Buy;
            this._dataSource.Sell = Sell;
            this._dataSource.UnpackBoxes = UnpackBoxes;
            base.OnOpen(stockpileMarket.MarketItems, playerInventory, "PEStockpileMarket");
        }

        public void Buy(PEStockpileMarketItemVM stockpileMarketItemVM)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestBuyItem(this.ActiveEntity, stockpileMarketItemVM.ItemIndex));
            GameNetwork.EndModuleEventAsClient();
        }

        public void Sell(PEStockpileMarketItemVM stockpileMarketItemVM)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestSellItem(this.ActiveEntity, stockpileMarketItemVM.ItemIndex));
            GameNetwork.EndModuleEventAsClient();
        }

        public void UnpackBoxes()
        {
            List<PEItemVM> items = this._dataSource.PlayerInventory.InventoryItems.ToList();
            for (int i = 0; i < items.Count; i++)
            {
                PEItemVM item = items[i];
                if (item.Count == 0) continue;
                CraftingBox box = this.ActiveEntity.CraftingBoxes.Where(c => c.BoxItem.StringId == item.Item.StringId).FirstOrDefault();
                if (box == null) continue;
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StockpileUnpackBox(i, this.ActiveEntity));
                GameNetwork.EndModuleEventAsClient();
                break;
            }
        }
    }
}

using PersistentEmpires.Views.ViewsVM.StockpileMarket;
using PersistentEmpiresClient.ViewsVM;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.SceneScripts;
using System;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PETradeCenterVM : PEBaseItemListVM<PEStockpileMarketItemVM, MarketItem>
    {
        private PE_TradeCenter tradeCenter;
        private PEStockpileMarketItemVM _selectedItem;
        private Action<PEStockpileMarketItemVM> _buy;
        private Action<PEStockpileMarketItemVM> _sell;
        private Action<PEStockpileMarketItemVM> _getPrices;

        public PETradeCenterVM(Action<PEItemVM> _handleClickItem) : base(_handleClickItem)
        {
        }

        public override void AddItem(object obj, int i)
        {
            MarketItem item = (MarketItem)obj;
            this.FilteredItemList.Add(new PEStockpileMarketItemVM(item, i, (selected) =>
            {
                this.SelectedItem = selected;
            }));
        }

        public void RefreshValues(PE_TradeCenter tradeCenter, Inventory inventory, Action<PEStockpileMarketItemVM> buy, Action<PEStockpileMarketItemVM> sell, Action<PEStockpileMarketItemVM> getPrices)
        {
            this.TradeCenter = tradeCenter;
            this.Buy = buy;
            this.Sell = sell;
            this.GetPrices = getPrices;
        }

        public void ExecuteBuy()
        {
            this.Buy(this.SelectedItem);
        }

        public void ExecuteSell()
        {
            this.Sell(this.SelectedItem);
        }

        public void ExecuteGetPrices()
        {
            this.GetPrices(this.SelectedItem);
        }

        [DataSourceProperty]
        public bool CanImport
        {
            get => this.SelectedItem != null;
        }

        [DataSourceProperty]
        public bool CanExport
        {
            get => this.SelectedItem != null;
        }

        [DataSourceProperty]
        public PEStockpileMarketItemVM SelectedItem
        {
            get => this._selectedItem;
            set
            {
                if (value != this._selectedItem)
                {
                    if (this._selectedItem != null)
                    {
                        this._selectedItem.IsSelected = false;
                    }
                    this._selectedItem = value;
                    if (this._selectedItem != null)
                    {
                        this._selectedItem.IsSelected = true;
                    }
                    base.OnPropertyChangedWithValue(value, "SelectedItem");
                    base.OnPropertyChanged("CanExport");
                    base.OnPropertyChanged("CanImport");
                }
            }
        }

        public PE_TradeCenter TradeCenter { get => tradeCenter; set => tradeCenter = value; }
        public Action<PEStockpileMarketItemVM> Buy { get => _buy; set => _buy = value; }
        public Action<PEStockpileMarketItemVM> Sell { get => _sell; set => _sell = value; }
        public Action<PEStockpileMarketItemVM> GetPrices { get => _getPrices; set => _getPrices = value; }
    }
}

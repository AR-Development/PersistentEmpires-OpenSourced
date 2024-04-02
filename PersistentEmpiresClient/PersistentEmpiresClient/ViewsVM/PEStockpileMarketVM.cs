using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpires.Views.ViewsVM.StockpileMarket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEStockpileMarketVM : ViewModel
    {
        private PEStockpileMarketItemVM _selectedItem;
        private MBBindingList<PEStockpileMarketItemVM> _itemList;
        private PEInventoryVM _playerInventory;
        private string _filter;
        private MBBindingList<PEStockpileMarketItemVM> _filteredItemList;
        private Action<PEStockpileMarketItemVM> _buy;
        private Action<PEStockpileMarketItemVM> _sell;
        private Action _unpackBoxes;
        private Action<PEItemVM> _handleClickItem;
        private SelectorVM<SelectorItemVM> _stockFilter;
        private SelectorVM<SelectorItemVM> _tierFilter;
        private SelectorVM<SelectorItemVM> _cultureFilter;
        private SelectorVM<SelectorItemVM> _itemCategoryFilter;

        public PEStockpileMarketVM(Action<PEItemVM> _handleClickItem) {
            this._handleClickItem = _handleClickItem;
        }
        public void RefreshValues(PE_StockpileMarket stockpileMarket, Inventory inventory, Action<PEStockpileMarketItemVM> buy, Action<PEStockpileMarketItemVM> sell, Action unpackBoxes) {
            this.ItemList = new MBBindingList<PEStockpileMarketItemVM>();
            this.SelectedItem = null;
            this.PlayerInventory = new PEInventoryVM(this._handleClickItem);
            this.PlayerInventory.SetItems(inventory);
            for (int i = 0; i < stockpileMarket.MarketItems.Count; i++)
            {
                MarketItem marketItem = stockpileMarket.MarketItems[i];
                this.ItemList.Add(new PEStockpileMarketItemVM(marketItem, i, (selected) => {
                    this.SelectedItem = selected;
                }));
            }
            this._buy = buy;
            this._sell = sell;
            this._unpackBoxes = unpackBoxes;

            List<string> stockFilterList = new List<string>();
            stockFilterList.Add("All");
            stockFilterList.Add("Available Stocks");
            stockFilterList.Add("Not Available Stocks");
            this.StockFilter = new SelectorVM<SelectorItemVM>(stockFilterList, 0, this.OnStockFilterSelected);

            /*List<string> tierFilterList = new List<string>();
            tierFilterList.Add("All");
            tierFilterList.Add("Tier 1");
            tierFilterList.Add("Tier 2");
            tierFilterList.Add("Tier 3");
            this.TierFilter = new SelectorVM<SelectorItemVM>(tierFilterList, 0, this.OnTierFilterSelected);*/

            List<string> cultureFilterList = new List<string>();
            cultureFilterList.Add("All");
            cultureFilterList.Add("Vlandia");
            cultureFilterList.Add("Khuzait");
            cultureFilterList.Add("Aserai");
            cultureFilterList.Add("Sturgia");
            cultureFilterList.Add("Battania");
            cultureFilterList.Add("Empire");
            cultureFilterList.Add("Neutral Culture");
            this.CultureFilter = new SelectorVM<SelectorItemVM>(cultureFilterList, 0, this.OnCultureFilterSelected);

            List<string> itemCategoryList = new List<string>();
            itemCategoryList.Add("All");
            itemCategoryList.Add("Invalid");
            itemCategoryList.Add("Horse");
            itemCategoryList.Add("OneHandedWeapon");
            itemCategoryList.Add("TwoHandedWeapon");
            itemCategoryList.Add("Polearm");
            itemCategoryList.Add("Arrows");
            itemCategoryList.Add("Bolts");
            itemCategoryList.Add("Shield");
            itemCategoryList.Add("Bow");
            itemCategoryList.Add("Crossbow");
            itemCategoryList.Add("Thrown");
            itemCategoryList.Add("Goods");
            itemCategoryList.Add("HeadArmor");
            itemCategoryList.Add("BodyArmor");
            itemCategoryList.Add("LegArmor");
            itemCategoryList.Add("HandArmor");
            itemCategoryList.Add("Pistol");
            itemCategoryList.Add("Musket");
            itemCategoryList.Add("Bullets");
            itemCategoryList.Add("Animal");
            itemCategoryList.Add("Book");
            itemCategoryList.Add("ChestArmor");
            itemCategoryList.Add("Cape");
            itemCategoryList.Add("HorseHarness");
            itemCategoryList.Add("Banner");
            this.ItemCategoryFilter = new SelectorVM<SelectorItemVM>(itemCategoryList, 0, this.OnCategoryFilterSelected);
        }

        private void OnCategoryFilterSelected(SelectorVM<SelectorItemVM> obj)
        {
            base.OnPropertyChanged("FilteredItemList");
        }
        private void OnCultureFilterSelected(SelectorVM<SelectorItemVM> obj)
        {
            // 
            base.OnPropertyChanged("FilteredItemList");
        }

        /*private void OnTierFilterSelected(SelectorVM<SelectorItemVM> obj)
        {
            // 
            base.OnPropertyChanged("FilteredItemList");
        }*/

        private void OnStockFilterSelected(SelectorVM<SelectorItemVM> obj)
        {
            // 
            base.OnPropertyChanged("FilteredItemList");
        }

        public void ExecuteBuy() {
            this._buy(this.SelectedItem);
        }
        public void ExecuteSell() {
            this._sell(this.SelectedItem);
        }
        public void ExecuteUnpackBoxes() {
            this._unpackBoxes();
        }

        [DataSourceProperty]
        public string Filter
        {
            get => this._filter;
            set
            {
                if (value != this._filter)
                {
                    this._filter = value;
                    base.OnPropertyChangedWithValue(value, "Filter");
                    if (value != null || value != "")
                    {
                        /*this._filteredItemList = new MBBindingList<PEStockpileMarketItemVM>();
                        foreach (PEStockpileMarketItemVM i in this._itemList)
                        {
                            if (i.ItemName.ToLower().Contains(this.Filter.ToLower())) this._filteredItemList.Add(i);
                        }*/
                        base.OnPropertyChanged("FilteredItemList");
                    }

                }
            }
        }
        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> ItemCategoryFilter
        {
            get => this._itemCategoryFilter;
            set
            {
                if (this._itemCategoryFilter != value)
                {
                    this._itemCategoryFilter = value;
                    base.OnPropertyChangedWithValue(value, "ItemCategoryFilter");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<PEStockpileMarketItemVM> FilteredItemList
        {
            get
            {
                List<PEStockpileMarketItemVM> filteredItems = new List<PEStockpileMarketItemVM>();
                foreach(PEStockpileMarketItemVM i in this.ItemList)
                {
                    filteredItems.Add(i);
                }
                if(this.StockFilter.SelectedIndex == 1)
                {
                    filteredItems = filteredItems.FindAll(p => p.Stock > 0);
                }else if(this.StockFilter.SelectedIndex == 2)
                {
                    filteredItems = filteredItems.FindAll(p => p.Stock == 0);
                }
                if(this.CultureFilter.SelectedIndex == 1)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Culture != null && p.MarketItem.Item.Culture.StringId == "vlandia");
                }
                else if (this.CultureFilter.SelectedIndex == 2)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Culture != null && p.MarketItem.Item.Culture.StringId == "khuzait");
                }
                else if (this.CultureFilter.SelectedIndex == 3)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Culture != null && p.MarketItem.Item.Culture.StringId == "aserai");
                }
                else if (this.CultureFilter.SelectedIndex == 4)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Culture != null && p.MarketItem.Item.Culture.StringId == "sturgia");
                }
                else if (this.CultureFilter.SelectedIndex == 5)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Culture != null && p.MarketItem.Item.Culture.StringId == "battania");
                }
                else if (this.CultureFilter.SelectedIndex == 6)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Culture != null && p.MarketItem.Item.Culture.StringId == "empire");
                }
                else if (this.CultureFilter.SelectedIndex == 7)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Culture != null && p.MarketItem.Item.Culture.StringId == "neutral_culture");
                }

                if (this.ItemCategoryFilter.SelectedIndex == 1)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Invalid);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 2)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Horse);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 3)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.OneHandedWeapon);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 4)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.TwoHandedWeapon);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 5)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Polearm);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 6)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Arrows);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 7)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Bolts);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 8)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Shield);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 9)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Bow);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 10)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Crossbow);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 11)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Thrown);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 12)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Goods);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 13)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.HeadArmor);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 14)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.BodyArmor);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 15)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.LegArmor);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 16)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.HandArmor);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 17)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Pistol);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 18)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Musket);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 19)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Bullets);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 20)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Animal);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 21)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Book);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 22)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.ChestArmor);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 23)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Cape);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 24)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.HorseHarness);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 25)
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Banner);
                }

                if (this.Filter != null && this.Filter != "")
                {
                    filteredItems = filteredItems.FindAll(p => p.MarketItem.Item.Culture != null && p.ItemName.ToLower().Contains(this.Filter.ToLower()));
                }
                MBBindingList<PEStockpileMarketItemVM> _filter = new MBBindingList<PEStockpileMarketItemVM>();
                foreach (PEStockpileMarketItemVM p in filteredItems) _filter.Add(p);
                return _filter;
            }
        }
        [DataSourceProperty]
        public PEInventoryVM PlayerInventory
        {
            get => this._playerInventory;
            set
            {
                if (value != this._playerInventory)
                {
                    this._playerInventory = value;
                    base.OnPropertyChangedWithValue(value, "PlayerInventory");
                }
            }
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
        [DataSourceProperty]
        public MBBindingList<PEStockpileMarketItemVM> ItemList
        {
            get => this._itemList;
            set
            {
                if (value != this._itemList)
                {
                    this._itemList = value;
                    base.OnPropertyChangedWithValue(value, "ItemList");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> StockFilter {
            get => this._stockFilter;
            set
            {
                if(this._stockFilter != value)
                {
                    this._stockFilter = value;
                    base.OnPropertyChangedWithValue(value, "StockFilter");
                }
            }
        }
        /*[DataSourceProperty]
        public SelectorVM<SelectorItemVM> TierFilter
        {
            get => this._tierFilter;
            set
            {
                if (this._tierFilter != value)
                {
                    this._tierFilter = value;
                    base.OnPropertyChangedWithValue(value, "TierFilter");
                }
            }
        }*/
        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> CultureFilter
        {
            get => this._cultureFilter;
            set
            {
                if (this._cultureFilter != value)
                {
                    this._cultureFilter = value;
                    base.OnPropertyChangedWithValue(value, "CultureFilter");
                }
            }
        }

    }
}

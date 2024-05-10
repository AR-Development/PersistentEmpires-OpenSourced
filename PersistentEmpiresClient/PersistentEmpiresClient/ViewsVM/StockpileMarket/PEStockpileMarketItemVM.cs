using PersistentEmpiresLib.SceneScripts;
using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.StockpileMarket
{
    public class PEStockpileMarketItemVM : ViewModel
    {
        public MarketItem MarketItem;
        public int ItemIndex;

        private ImageIdentifierVM _imageIdentifier;
        private bool _isSelected;
        private string _itemName;
        private int _stock;
        private int _constant;
        private Action<PEStockpileMarketItemVM> _executeSelect;
        public PEStockpileMarketItemVM(MarketItem marketItem, int itemIndex, Action<PEStockpileMarketItemVM> executeSelect)
        {
            this.MarketItem = marketItem;
            this.ImageIdentifier = new ImageIdentifierVM(marketItem.Item);
            this.ItemIndex = itemIndex;
            this.Stock = marketItem.Stock;
            this.Constant = marketItem.Constant;
            this.ItemName = marketItem.Item.Name.ToString();
            this._executeSelect = executeSelect;
        }

        [DataSourceProperty]
        public int BuyPrice
        {
            get => this.MarketItem.BuyPrice();
        }
        [DataSourceProperty]
        public int SellPrice
        {
            get => this.MarketItem.SellPrice();
        }
        public ImageIdentifierVM ImageIdentifier
        {
            get => this._imageIdentifier;
            set
            {
                if (value != this._imageIdentifier)
                {
                    this._imageIdentifier = value;
                    base.OnPropertyChangedWithValue(value, "ImageIdentifier");
                }
            }
        }
        [DataSourceProperty]
        public int Stock
        {
            get => this._stock;
            set
            {
                if (value != this._stock)
                {
                    this._stock = value;
                    base.OnPropertyChangedWithValue(value, "Stock");
                    base.OnPropertyChanged("BuyPrice");
                    base.OnPropertyChanged("SellPrice");
                }
            }
        }
        [DataSourceProperty]
        public int Constant
        {
            get => this._constant;
            set
            {
                if (value != this._constant)
                {
                    this._constant = value;
                    base.OnPropertyChangedWithValue(value, "Constant");
                    base.OnPropertyChanged("BuyPrice");
                    base.OnPropertyChanged("SellPrice");
                }
            }
        }
        [DataSourceProperty]
        public string ItemName
        {
            get => this._itemName;
            set
            {
                if (value != this._itemName)
                {
                    this._itemName = value;
                    base.OnPropertyChangedWithValue(value, "ItemName");
                }
            }
        }
        [DataSourceProperty]
        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                if (value != this._isSelected)
                {
                    this._isSelected = value;
                    base.OnPropertyChangedWithValue(value, "IsSelected");
                }
            }
        }

        public void ExecuteSelect()
        {
            this._executeSelect(this);
        }
        public void ExecuteHoverStart()
        {
            if (this.MarketItem.Item != null)
            {
                InformationManager.ShowTooltip(typeof(ItemObject), new object[] {
                    new EquipmentElement(this.MarketItem.Item)
                });
            }
        }

        public void ExecuteHoverEnd()
        {
            InformationManager.HideTooltip();
        }
    }
}

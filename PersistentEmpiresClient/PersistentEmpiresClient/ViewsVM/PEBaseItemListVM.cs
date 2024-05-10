using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresLib.Data;
using System;
using System.Collections.Generic;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;

namespace PersistentEmpiresClient.ViewsVM
{
    public class PEBaseItemListVM<T, U> : ViewModel
    {
        private List<U> items;
        private Action<PEItemVM> _handleClickItem;
        private PEInventoryVM _playerInventory;
        private string _nameFilter;
        private MBBindingList<T> _filteredItemList;
        private SelectorVM<SelectorItemVM> _stockFilter;
        private SelectorVM<SelectorItemVM> _cultureFilter;
        private SelectorVM<SelectorItemVM> _tierFilter;
        private SelectorVM<SelectorItemVM> _itemTypeFilter;
        private int lastRenderedIndex;

        private Dictionary<string, bool?> stockFilterDict = new Dictionary<string, bool?>()
        {
            {"All", null},
            {"Available Stock", true},
            {"No Stock", false}
        };

        private Dictionary<string, string> cultureFilterDict = new Dictionary<string, string>()
        {
            {"All", null},
            {"Vlandia", "vlandia"},
            {"Khuzait", "khuzait"},
            {"Aserai", "aserai"},
            {"Sturgia", "sturgia"},
            {"Battania", "battania"},
            {"Empire", "empire"},
            {"Neutral Culture", "neutral_culture"}
        };

        private Dictionary<string, int?> tierFilterDict = new Dictionary<string, int?>()
        {
            {"All", null},
            {"Tier 1", 1},
            {"Tier 2", 2},
            {"Tier 3", 3}
        };

        private Dictionary<string, TaleWorlds.Core.ItemObject.ItemTypeEnum?> itemTypeDict = new Dictionary<string, TaleWorlds.Core.ItemObject.ItemTypeEnum?>()
        {
            {"All", null},
            {"Invalid", TaleWorlds.Core.ItemObject.ItemTypeEnum.Invalid},
            {"Horse", TaleWorlds.Core.ItemObject.ItemTypeEnum.Horse},
            {"One Handed Weapon", TaleWorlds.Core.ItemObject.ItemTypeEnum.OneHandedWeapon},
            {"Two Handed Weapon", TaleWorlds.Core.ItemObject.ItemTypeEnum.TwoHandedWeapon},
            {"Polearm", TaleWorlds.Core.ItemObject.ItemTypeEnum.Polearm},
            {"Arrows", TaleWorlds.Core.ItemObject.ItemTypeEnum.Arrows},
            {"Bolts", TaleWorlds.Core.ItemObject.ItemTypeEnum.Bolts},
            {"Shield", TaleWorlds.Core.ItemObject.ItemTypeEnum.Shield},
            {"Bow", TaleWorlds.Core.ItemObject.ItemTypeEnum.Bow},
            {"Crossbow", TaleWorlds.Core.ItemObject.ItemTypeEnum.Crossbow},
            {"Thrown", TaleWorlds.Core.ItemObject.ItemTypeEnum.Thrown},
            {"Goods", TaleWorlds.Core.ItemObject.ItemTypeEnum.Goods},
            {"Head Armor", TaleWorlds.Core.ItemObject.ItemTypeEnum.HeadArmor},
            {"Body Armor", TaleWorlds.Core.ItemObject.ItemTypeEnum.BodyArmor},
            {"Chest Armor", TaleWorlds.Core.ItemObject.ItemTypeEnum.ChestArmor},
            {"Leg Armor", TaleWorlds.Core.ItemObject.ItemTypeEnum.LegArmor},
            {"Hand Armor", TaleWorlds.Core.ItemObject.ItemTypeEnum.HandArmor},
            {"Pistol", TaleWorlds.Core.ItemObject.ItemTypeEnum.Pistol},
            {"Musket", TaleWorlds.Core.ItemObject.ItemTypeEnum.Musket},
            {"Bullets", TaleWorlds.Core.ItemObject.ItemTypeEnum.Bullets},
            {"Animal", TaleWorlds.Core.ItemObject.ItemTypeEnum.Animal},
            {"Book", TaleWorlds.Core.ItemObject.ItemTypeEnum.Book},
            {"Cape", TaleWorlds.Core.ItemObject.ItemTypeEnum.Cape},
            {"Horse Harness", TaleWorlds.Core.ItemObject.ItemTypeEnum.HorseHarness},
            {"Banner", TaleWorlds.Core.ItemObject.ItemTypeEnum.Banner}
        };

        private bool? stockFilter = null;
        private string cultureFilter = null;
        private int? tierFilter = null;
        private TaleWorlds.Core.ItemObject.ItemTypeEnum? itemTypeFilter = null;

        protected int maxItemsRenderedPerTick = 25;

        public PEBaseItemListVM(Action<PEItemVM> _handleClickItem)
        {
            this._handleClickItem = _handleClickItem;
            this.lastRenderedIndex = 0;
            this.FilteredItemList = new MBBindingList<T>();
            this.NameFilter = "";
            this.StockFilter = new SelectorVM<SelectorItemVM>(new List<string>(this.stockFilterDict.Keys), 0, this.OnStockFilterSelected);
            this.CultureFilter = new SelectorVM<SelectorItemVM>(new List<string>(this.cultureFilterDict.Keys), 0, this.OnCultureFilterSelected);
            this.TierFilter = new SelectorVM<SelectorItemVM>(new List<string>(this.tierFilterDict.Keys), 0, this.OnTierFilterSelected);
            this.ItemTypeFilter = new SelectorVM<SelectorItemVM>(new List<string>(this.itemTypeDict.Keys), 0, this.OnItemTypeFilterSelected);
        }

        public virtual void AddItem(object obj, int index) { }

        public void RefreshValues(List<U> items, Inventory playerInventory)
        {
            this.ItemsList = items;

            this.PlayerInventory = new PEInventoryVM(this._handleClickItem);
            this.PlayerInventory.SetItems(playerInventory);
            ReRender();
        }

        private void ReRender()
        {
            this.lastRenderedIndex = 0;
            this.FilteredItemList.Clear();
        }

        private void OnStockFilterSelected(SelectorVM<SelectorItemVM> obj)
        {
            this.stockFilter = this.stockFilterDict[obj.SelectedItem.StringItem];
            ReRender();
        }

        private void OnCultureFilterSelected(SelectorVM<SelectorItemVM> obj)
        {
            this.cultureFilter = this.cultureFilterDict[obj.SelectedItem.StringItem];
            ReRender();
        }

        private void OnTierFilterSelected(SelectorVM<SelectorItemVM> obj)
        {
            this.tierFilter = this.tierFilterDict[obj.SelectedItem.StringItem];
            ReRender();
        }

        private void OnItemTypeFilterSelected(SelectorVM<SelectorItemVM> obj)
        {
            this.itemTypeFilter = this.itemTypeDict[obj.SelectedItem.StringItem];
            ReRender();
        }

        public void OnTick()
        {
            if (lastRenderedIndex == ItemsList.Count) return;

            int maxIndex = (ItemsList.Count - lastRenderedIndex) > maxItemsRenderedPerTick ? lastRenderedIndex + maxItemsRenderedPerTick : ItemsList.Count;
            for (int i = lastRenderedIndex; i < maxIndex - 1; i++)
            {
                dynamic item = ItemsList[i];
                if (this.NameFilter != null && this.NameFilter != "" && !item.Item.Name.ToString().ToLower().Contains(this.NameFilter.ToLower())) continue;
                if (stockFilter != null && (((bool)stockFilter && !(item.Stock > 0)) || (!(bool)stockFilter && (item.Stock != 0)))) continue;
                if (cultureFilter != null && item.Item.Culture.StringId != cultureFilter) continue;
                if (tierFilter != null && item.Tier != tierFilter) continue;
                if (itemTypeFilter != null && item.Item.Type != itemTypeFilter) continue;

                AddItem(item, i);
            }

            lastRenderedIndex = maxIndex;
        }

        public List<U> ItemsList { get => items; set => items = value; }

        [DataSourceProperty]
        public MBBindingList<T> FilteredItemList
        {
            get => this._filteredItemList;
            set
            {
                if (value != this._filteredItemList)
                {
                    this._filteredItemList = value;
                    base.OnPropertyChangedWithValue(value, "FilteredItemList");
                }
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
        public string NameFilter
        {
            get => this._nameFilter;
            set
            {
                if (value != this._nameFilter)
                {
                    this._nameFilter = value;
                    base.OnPropertyChangedWithValue(value, "NameFilter");
                    this.lastRenderedIndex = 0;
                    this.FilteredItemList.Clear();
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> StockFilter
        {
            get => this._stockFilter;
            set
            {
                if (this._stockFilter != value)
                {
                    this._stockFilter = value;
                    base.OnPropertyChangedWithValue(value, "StockFilter");
                }
            }
        }

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

        [DataSourceProperty]
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
        }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> ItemTypeFilter
        {
            get => this._itemTypeFilter;
            set
            {
                if (this._itemTypeFilter != value)
                {
                    this._itemTypeFilter = value;
                    base.OnPropertyChangedWithValue(value, "ItemTypeFilter");
                }
            }
        }
    }
}

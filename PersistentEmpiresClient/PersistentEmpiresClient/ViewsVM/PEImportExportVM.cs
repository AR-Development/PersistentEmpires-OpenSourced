using PersistentEmpires.Views.ViewsVM.ImportExport;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.SceneScripts;
using System;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEImportExportVM : ViewModel
    {
        private PEImportExportItemVM _selectedItem;
        private MBBindingList<PEImportExportItemVM> _itemList;
        private PEInventoryVM _playerInventory;
        private string _filter;
        private MBBindingList<PEImportExportItemVM> _filteredItemList;
        private Action<PEImportExportItemVM> _export;
        private Action<PEImportExportItemVM> _import;
        private Action<PEItemVM> _handleClickItem;
        public PEImportExportVM(Action<PEItemVM> _handleClickItem)
        {
            this._handleClickItem = _handleClickItem;
        }

        public void RefreshValues(PE_ImportExport importExportEntity, Inventory inventory, Action<PEImportExportItemVM> export, Action<PEImportExportItemVM> import)
        {
            this.ItemList = new MBBindingList<PEImportExportItemVM>();
            this.SelectedItem = null;
            this.PlayerInventory = new PEInventoryVM(this._handleClickItem);
            this.PlayerInventory.SetItems(inventory);
            foreach (GoodItem goodItem in importExportEntity.GetGoodItems())
            {
                this.ItemList.Add(new PEImportExportItemVM(goodItem.ItemObj, goodItem.ExportPrice, goodItem.ImportPrice, (selected) =>
                {
                    this.SelectedItem = selected;
                }));
            }
            this._export = export;
            this._import = import;
        }

        public void ExecuteExport()
        {
            this._export(this.SelectedItem);
        }

        public void ExecuteImport()
        {
            this._import(this.SelectedItem);
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
                        this._filteredItemList = new MBBindingList<PEImportExportItemVM>();
                        foreach (PEImportExportItemVM i in this._itemList)
                        {
                            if (i.ItemName.Contains(this.Filter)) this._filteredItemList.Add(i);
                        }
                        base.OnPropertyChanged("FilteredItemList");
                    }

                }
            }
        }
        [DataSourceProperty]
        public MBBindingList<PEImportExportItemVM> FilteredItemList
        {
            get
            {
                if (this.Filter == null || this.Filter == "")
                {
                    return this.ItemList;
                }
                else
                {
                    return this._filteredItemList;
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
        public MBBindingList<PEImportExportItemVM> ItemList
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
        public PEImportExportItemVM SelectedItem
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
    }
}

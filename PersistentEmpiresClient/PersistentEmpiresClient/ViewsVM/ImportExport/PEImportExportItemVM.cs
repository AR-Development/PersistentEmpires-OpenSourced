using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.ImportExport
{
    public class PEImportExportItemVM : ViewModel
    {
        public ItemObject Item;
        private ImageIdentifierVM _imageIdentifier;
        private int _exportPrice;
        private int _importPrice;
        private Action<PEImportExportItemVM> _executeSelect;
        private bool _isSelected;

        public PEImportExportItemVM(ItemObject item, int exportPrice, int importPrice, Action<PEImportExportItemVM> executeSelect)
        {
            this.Item = item;
            this.ImageIdentifier = new ImageIdentifierVM(item);
            this.ExportPrice = exportPrice;
            this.ImportPrice = importPrice;
            base.OnPropertyChanged("ItemName");
            this._executeSelect = executeSelect;
        }

        public void ExecuteSelect()
        {
            this._executeSelect(this);
        }
        public void ExecuteHoverStart()
        {
            if (this.Item != null)
            {
                InformationManager.ShowTooltip(typeof(ItemObject), new object[] {
                    new EquipmentElement(this.Item)
                });
            }
        }

        public void ExecuteHoverEnd()
        {
            InformationManager.HideTooltip();
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
        [DataSourceProperty]
        public string ItemName
        {
            get => this.Item.Name.ToString();
        }
        [DataSourceProperty]
        public int ImportPrice
        {
            get => this._importPrice;
            set
            {
                if (value != this._importPrice)
                {
                    this._importPrice = value;
                    base.OnPropertyChangedWithValue(value, "ImportPrice");
                }
            }
        }
        [DataSourceProperty]
        public int ExportPrice
        {
            get => this._exportPrice;
            set
            {
                if (value != this._exportPrice)
                {
                    this._exportPrice = value;
                    base.OnPropertyChangedWithValue(value, "ExportPrice");
                }
            }
        }
        [DataSourceProperty]
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
    }
}

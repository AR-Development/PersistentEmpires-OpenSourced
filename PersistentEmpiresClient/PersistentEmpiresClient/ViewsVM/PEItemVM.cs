using System;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEItemVM : ViewModel
    {
        public ItemObject Item;
        private ImageIdentifierVM _imageIdentifierVM;
        private int _count;
        private Action<PEItemVM, PEItemVM> _executeTransfer;
        private Action<PEItemVM> _handleClickItem;
        private Action<PEItemVM> _alternateClick;
        private bool _showTooltip = false;

        public PEItemVM(ItemObject item, int count, string dropTag, Action<PEItemVM, PEItemVM> executeTranfser, Action<PEItemVM> handleClickItem, Action<PEItemVM> alternateClick = null)
        {
            this.Item = item;
            if (item != null)
            {
                this.ImageIdentifier = new ImageIdentifierVM(item);
            }
            else
            {
                this.ImageIdentifier = new ImageIdentifierVM();
            }
            this.Count = count;
            this.DropTag = dropTag;
            this._executeTransfer = executeTranfser;
            this._handleClickItem = handleClickItem;
            this._alternateClick = alternateClick;
        }
        public PEItemVM(EquipmentElement equipmentItem, string dropTag, Action<PEItemVM, PEItemVM> executeTranfser, Action<PEItemVM> hotkeyTransfer, Action<PEItemVM> alternateClick = null)
        {

            if (equipmentItem.IsEmpty)
            {
                this.Count = 0;
                this.Item = null;
                this.ImageIdentifier = new ImageIdentifierVM();
            }
            else
            {
                this.Count = 1;
                this.Item = equipmentItem.Item;
                this.ImageIdentifier = new ImageIdentifierVM(equipmentItem.Item);
            }
            this.DropTag = dropTag;
            this._executeTransfer = executeTranfser;
            this._handleClickItem = hotkeyTransfer;
            this._alternateClick = alternateClick;
        }
        public PEItemVM(string dropTag, Action<PEItemVM, PEItemVM> executeTranfser)
        {
            this.Count = 0;
            this.Item = null;
            this.ImageIdentifier = new ImageIdentifierVM();
            this.DropTag = dropTag;
            this._executeTransfer = executeTranfser;
        }

        public void UpdateFromItem()
        {
            if (this.Item != null)
            {
                this.ImageIdentifier = new ImageIdentifierVM(this.Item);
            }
            else
            {
                this.ImageIdentifier = new ImageIdentifierVM();
            }
        }
        public string DropTag { get; set; }

        public void ExecuteClickAction()
        {
            this._handleClickItem(this);
        }

        public void ExecuteAlternateClick()
        {
            if (this._alternateClick != null) this._alternateClick(this);
            //InformationManager.DisplayMessage(new InformationMessage("Alternate Click !!!"));
        }

        [DataSourceProperty]
        public int Count
        {
            get => this._count;
            set
            {
                if (value != this._count)
                {
                    this._count = value;
                    base.OnPropertyChangedWithValue(value, "Count");
                }
            }
        }

        [DataSourceProperty]
        public bool IsShowTooltip
        {
            get => this._showTooltip;
            set
            {
                if (value != this._showTooltip)
                {
                    this._showTooltip = value;
                    base.OnPropertyChangedWithValue(value, "IsShowTooltip");
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM ImageIdentifier
        {
            get => this._imageIdentifierVM;
            set
            {
                if (value != this._imageIdentifierVM)
                {
                    this._imageIdentifierVM = value;
                    base.OnPropertyChangedWithValue(value, "ImageIdentifier");
                }
            }
        }
        public void RequestExecuteTransfer(PEItemVM draggedItem, int index)
        {
            this._executeTransfer(this, draggedItem);
        }

        public void ShowTooltip()
        {
            if (this.Item != null)
            {
                InformationManager.ShowTooltip(typeof(ItemObject), new object[] {
                    new EquipmentElement(this.Item)
                });
            }
        }
        public void HideTooltip()
        {
            InformationManager.HideTooltip();
        }
    }
}

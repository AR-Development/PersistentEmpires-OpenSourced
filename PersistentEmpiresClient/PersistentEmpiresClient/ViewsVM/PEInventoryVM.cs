using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Client;
using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEInventoryVM : ViewModel
    {
        public int _maxSlot = 5;
        public string TargetInventoryId = "";
        private MBBindingList<PEItemVM> _inventoryItems;
        private MBBindingList<PEItemVM> _requestedInventoryItems;
        private PEItemVM _helmSlot;
        private PEItemVM _bodySlot;
        private PEItemVM _capSlot;
        private PEItemVM _handSlot;
        private PEItemVM _legSlot;
        private PEItemVM _item0;
        private PEItemVM _item1;
        private PEItemVM _item2;
        private PEItemVM _item3;

        private Action<PEItemVM> _handleClickItem;

        public PEInventoryVM(Action<PEItemVM> handleClickItem)
        {
            this.MaxSlot = 0;
            this.InventoryItems = new MBBindingList<PEItemVM>();
            this.RequestedInventoryItems = new MBBindingList<PEItemVM>();
            this._handleClickItem = handleClickItem;
        }
        public PEInventoryVM(Inventory playerInventory, Equipment agentEquipment, Action<PEItemVM> handleClickItem)
        {
            this.SetItems(playerInventory);
            this.SetEquipmentSlots(agentEquipment);
            this._handleClickItem = handleClickItem;
        }
        public void SetItems(Inventory playerInventory)
        {
            this._currentPlayerInventory = playerInventory;

            this.MaxSlot = playerInventory.Slots.Count;
            this.InventoryItems = new MBBindingList<PEItemVM>();
            for (int i = 0; i < this.MaxSlot; i++)
            {
                this.InventoryItems.Add(new PEItemVM(playerInventory.Slots[i].Item, playerInventory.Slots[i].Count, "PlayerInventory_" + i, this.RequestExecuteTransfer, this._handleClickItem, this.TryEquipItem));
            }
            base.OnPropertyChangedWithValue(this._inventoryItems, "InventoryItems");
        }
        public void ExecuteDropItem(PEItemVM draggedItem, int index)
        {
            if (draggedItem.Item == null) return;
            // InformationManager.DisplayMessage(new InformationMessage("Hello"));
            InformationManager.ShowInquiry(
                new InquiryData(
                    GameTexts.FindText("PEInventoryInqCaption", null).ToString(),
                    GameTexts.FindText("PEInventoryInqText1", null).ToString() + draggedItem.Item.Name.ToString() + GameTexts.FindText("PEInventoryInqText2", null).ToString() + draggedItem.Count + GameTexts.FindText("PEInventoryInqText3", null).ToString(),
                    true,
                    true,
                    GameTexts.FindText("PE_InquiryData_Yes", null).ToString(),
                    GameTexts.FindText("PE_InquiryData_No", null).ToString(),
                    () =>
                    {
                        GameNetwork.BeginModuleEventAsClient();
                        GameNetwork.WriteMessage(new RequestDropItemFromInventory(draggedItem.DropTag));
                        GameNetwork.EndModuleEventAsClient();
                    }, () => { }));
        }
        public void SetRequestedItems(Inventory requestedInventory)
        {
            this._currentRequestedInventory = requestedInventory;
            this.RequestedInventoryItems = new MBBindingList<PEItemVM>();
            if (requestedInventory == null)
            {
                base.OnPropertyChangedWithValue(this._requestedInventoryItems, "RequestedInventoryItems");
                return;
            }
            this.TargetInventoryId = requestedInventory.InventoryId;

            for (int i = 0; i < requestedInventory.Slots.Count; i++)
            {
                this.RequestedInventoryItems.Add(new PEItemVM(requestedInventory.Slots[i].Item, requestedInventory.Slots[i].Count, requestedInventory.InventoryId + "_" + i, this.RequestExecuteTransfer, this._handleClickItem, this.TryMovingItem));
            }
            base.OnPropertyChangedWithValue(this._requestedInventoryItems, "RequestedInventoryItems");
        }
        public void RequestExecuteTransfer(PEItemVM droppedSlot, PEItemVM draggedSlot)
        {
            //InformationManager.DisplayMessage(new InformationMessage(draggedSlot.DropTag + " Dropped To " + droppedSlot.DropTag));
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestInventoryTransfer(draggedSlot.DropTag, droppedSlot.DropTag));
            GameNetwork.EndModuleEventAsClient();
            // Send Request to server, if server says it's OK, drag and drop the stuff
        }

        public void TryMovingItem(PEItemVM item)
        {
            if (this._currentPlayerInventory == null) return;
            if (item.Item == null || item.Count == 0) return;
            PEItemVM targetSlot = null;

            foreach (PEItemVM inventorySlot in this._inventoryItems)
            {
                if (inventorySlot.Item != null && inventorySlot.Count > 0 && inventorySlot.Count < 10 && inventorySlot.Item.StringId.Equals(item.Item.StringId) && item.Item.Type == ItemObject.ItemTypeEnum.Thrown)
                {
                    targetSlot = inventorySlot; break;
                }
                else if (inventorySlot.Item == null)
                {
                    targetSlot = inventorySlot; break;
                }
            }

            if (targetSlot != null)
            {
                this.RequestExecuteTransfer(targetSlot, item);
            }

        }

        public void TryEquipItem(PEItemVM item)
        {
            if (this._currentPlayerInventory == null) return;
            if (item.Item == null || item.Count == 0) return;
            PEItemVM targetSlot = null;
            if (item.Item.ItemType != ItemObject.ItemTypeEnum.BodyArmor &&
               item.Item.ItemType != ItemObject.ItemTypeEnum.Cape &&
               item.Item.ItemType != ItemObject.ItemTypeEnum.HeadArmor &&
               item.Item.ItemType != ItemObject.ItemTypeEnum.HandArmor &&
               item.Item.ItemType != ItemObject.ItemTypeEnum.LegArmor &&
               item.Item.ItemType != ItemObject.ItemTypeEnum.ChestArmor
                )
            {
                // Its not an armor
                if (this._item0.Item == null || this._item0.Count == 0)
                {
                    targetSlot = this._item0;
                }
                else if (this._item1.Item == null || this._item1.Count == 0)
                {
                    targetSlot = this._item1;
                }
                else if (this._item2.Item == null || this._item2.Count == 0)
                {
                    targetSlot = this._item2;
                }
                else if (this._item3.Item == null || this._item3.Count == 0)
                {
                    targetSlot = this._item3;
                }
            }
            else
            {
                switch (item.Item.ItemType)
                {
                    case ItemObject.ItemTypeEnum.BodyArmor:
                        targetSlot = this._bodySlot; break;
                    case ItemObject.ItemTypeEnum.HeadArmor:
                        targetSlot = this._helmSlot; break;
                    case ItemObject.ItemTypeEnum.HandArmor:
                        targetSlot = this._handSlot; break;
                    case ItemObject.ItemTypeEnum.LegArmor:
                        targetSlot = this._legSlot; break;
                    case ItemObject.ItemTypeEnum.Cape:
                        targetSlot = this._capSlot; break;
                    case ItemObject.ItemTypeEnum.ChestArmor:
                        targetSlot = this._bodySlot; break;
                }
            }
            if (targetSlot != null)
            {
                this.RequestExecuteTransfer(targetSlot, item);
            }
        }

        public void TryUnequipItem(PEItemVM item)
        {
            if (this._currentPlayerInventory == null) return;
            if (item.Item == null || item.Count == 0) return;

            PEItemVM targetSlot = null;

            foreach (PEItemVM inventorySlot in this._inventoryItems)
            {
                if (inventorySlot.Item != null && inventorySlot.Count > 0 && inventorySlot.Count < 10 && inventorySlot.Item.StringId.Equals(item.Item.StringId) && item.Item.Type == ItemObject.ItemTypeEnum.Thrown)
                {
                    targetSlot = inventorySlot; break;
                }
                else if (inventorySlot.Item == null)
                {
                    targetSlot = inventorySlot; break;
                }
            }

            if (targetSlot != null)
            {
                this.RequestExecuteTransfer(targetSlot, item);
            }
        }

        public void SetEquipmentSlots(Equipment agentEquipment)
        {
            this.HelmSlot = new PEItemVM(agentEquipment[EquipmentIndex.Head], "Equipment_5", this.RequestExecuteTransfer, this._handleClickItem, this.TryUnequipItem);
            this.CapSlot = new PEItemVM(agentEquipment[EquipmentIndex.Cape], "Equipment_9", this.RequestExecuteTransfer, this._handleClickItem, this.TryUnequipItem);
            this.BodySlot = new PEItemVM(agentEquipment[EquipmentIndex.Body], "Equipment_6", this.RequestExecuteTransfer, this._handleClickItem, this.TryUnequipItem);
            this.HandSlot = new PEItemVM(agentEquipment[EquipmentIndex.Gloves], "Equipment_8", this.RequestExecuteTransfer, this._handleClickItem, this.TryUnequipItem);
            this.LegSlot = new PEItemVM(agentEquipment[EquipmentIndex.Leg], "Equipment_7", this.RequestExecuteTransfer, this._handleClickItem, this.TryUnequipItem);

            this.Item0 = new PEItemVM(agentEquipment[EquipmentIndex.Weapon0], "Equipment_0", this.RequestExecuteTransfer, this._handleClickItem, this.TryUnequipItem);
            this.Item1 = new PEItemVM(agentEquipment[EquipmentIndex.Weapon1], "Equipment_1", this.RequestExecuteTransfer, this._handleClickItem, this.TryUnequipItem);
            this.Item2 = new PEItemVM(agentEquipment[EquipmentIndex.Weapon2], "Equipment_2", this.RequestExecuteTransfer, this._handleClickItem, this.TryUnequipItem);
            this.Item3 = new PEItemVM(agentEquipment[EquipmentIndex.Weapon3], "Equipment_3", this.RequestExecuteTransfer, this._handleClickItem, this.TryUnequipItem);
        }

        public void ExecuteRevealItemBag()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestRevealItemBag());
            GameNetwork.EndModuleEventAsClient();
        }

        [DataSourceProperty]
        public PEItemVM HelmSlot
        {
            get => this._helmSlot;
            set
            {
                if (value != this._helmSlot)
                {
                    this._helmSlot = value;
                    base.OnPropertyChangedWithValue(value, "HelmSlot");
                }
            }
        }

        [DataSourceProperty]
        public bool IsRequestedInventory
        {
            get => this._requestedInventoryItems.Count > 0;
        }
        [DataSourceProperty]
        public PEItemVM CapSlot
        {
            get => this._capSlot;
            set
            {
                if (value != this._capSlot)
                {
                    this._capSlot = value;
                    base.OnPropertyChangedWithValue(value, "CapSlot");
                }
            }
        }
        [DataSourceProperty]
        public PEItemVM BodySlot
        {
            get => this._bodySlot;
            set
            {
                if (value != this._bodySlot)
                {
                    this._bodySlot = value;
                    base.OnPropertyChangedWithValue(value, "BodySlot");
                }
            }
        }
        [DataSourceProperty]
        public PEItemVM HandSlot
        {
            get => this._handSlot;
            set
            {
                if (value != this._handSlot)
                {
                    this._handSlot = value;
                    base.OnPropertyChangedWithValue(value, "HandSlot");
                }
            }
        }
        [DataSourceProperty]
        public PEItemVM LegSlot
        {
            get => this._legSlot;
            set
            {
                if (value != this._legSlot)
                {
                    this._legSlot = value;
                    base.OnPropertyChangedWithValue(value, "LegSlot");
                }
            }
        }
        [DataSourceProperty]
        public PEItemVM Item0
        {
            get => this._item0;
            set
            {
                if (value != this._item0)
                {
                    this._item0 = value;
                    base.OnPropertyChangedWithValue(value, "Item0");
                }
            }
        }
        [DataSourceProperty]
        public PEItemVM Item1
        {
            get => this._item1;
            set
            {
                if (value != this._item1)
                {
                    this._item1 = value;
                    base.OnPropertyChangedWithValue(value, "Item1");
                }
            }
        }
        [DataSourceProperty]
        public PEItemVM Item2
        {
            get => this._item2;
            set
            {
                if (value != this._item2)
                {
                    this._item2 = value;
                    base.OnPropertyChangedWithValue(value, "Item2");
                }
            }
        }
        [DataSourceProperty]
        public PEItemVM Item3
        {
            get => this._item3;
            set
            {
                if (value != this._item3)
                {
                    this._item3 = value;
                    base.OnPropertyChangedWithValue(value, "Item3");
                }
            }
        }
        [DataSourceProperty]
        public MBBindingList<PEItemVM> InventoryItems
        {
            get => this._inventoryItems;
            set
            {
                if (value != this._inventoryItems)
                {
                    this._inventoryItems = value;
                    base.OnPropertyChangedWithValue(value, "InventoryItems");
                }
            }
        }

        private Inventory _currentRequestedInventory;

        [DataSourceProperty]
        public MBBindingList<PEItemVM> RequestedInventoryItems
        {
            get => this._requestedInventoryItems;
            set
            {
                if (value != this._requestedInventoryItems)
                {
                    this._requestedInventoryItems = value;
                    base.OnPropertyChangedWithValue(value, "RequestedInventoryItems");
                    base.OnPropertyChanged("IsRequestedInventory");
                }
            }
        }

        private Inventory _currentPlayerInventory;

        [DataSourceProperty]
        public int MaxSlot
        {
            get => this._maxSlot;
            set
            {
                if (value != this._maxSlot)
                {
                    this._maxSlot = value;
                    base.OnPropertyChangedWithValue(value, "MaxSlot");
                }
            }
        }

        /*public void ExecuteTransfer(PEItemVM draggedItem, int index)
        {
            InformationManager.DisplayMessage(new InformationMessage(targetTag));
        }*/
    }
}

using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    abstract public class PEBaseInventoryScreen : MissionView
    {
        protected PlayerInventoryComponent _playerInventoryComponent;

        protected abstract PEInventoryVM GetInventoryVM();
        public bool IsActive { get; set; }
        protected GauntletLayer _gauntletLayer;
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._playerInventoryComponent = base.Mission.GetMissionBehavior<PlayerInventoryComponent>();
            this._playerInventoryComponent.OnUpdateInventory += this.UpdateInventory;
            this._playerInventoryComponent.OnForceUpdateInventory += this.ForceUpdateInventory;
        }
        protected void UpdateEquipmentInventory(int index, ItemObject item, int count)
        {
            if (!this.IsActive) return;
            if (index == (int)EquipmentIndex.Head)
            {
                this.GetInventoryVM().HelmSlot.Item = item;
                this.GetInventoryVM().HelmSlot.Count = count;
                this.GetInventoryVM().HelmSlot.UpdateFromItem();
            }
            else if (index == (int)EquipmentIndex.Cape)
            {
                this.GetInventoryVM().CapSlot.Item = item;
                this.GetInventoryVM().CapSlot.Count = count;
                this.GetInventoryVM().CapSlot.UpdateFromItem();
            }
            else if (index == (int)EquipmentIndex.Body)
            {
                this.GetInventoryVM().BodySlot.Item = item;
                this.GetInventoryVM().BodySlot.Count = count;
                this.GetInventoryVM().BodySlot.UpdateFromItem();
            }
            else if (index == (int)EquipmentIndex.Gloves)
            {
                this.GetInventoryVM().HandSlot.Item = item;
                this.GetInventoryVM().HandSlot.Count = count;
                this.GetInventoryVM().HandSlot.UpdateFromItem();
            }
            else if (index == (int)EquipmentIndex.Leg)
            {
                this.GetInventoryVM().LegSlot.Item = item;
                this.GetInventoryVM().LegSlot.Count = count;
                this.GetInventoryVM().LegSlot.UpdateFromItem();
            }
            else if (index == (int)EquipmentIndex.Weapon0)
            {
                this.GetInventoryVM().Item0.Item = item;
                this.GetInventoryVM().Item0.Count = count;
                this.GetInventoryVM().Item0.UpdateFromItem();
            }
            else if (index == (int)EquipmentIndex.Weapon1)
            {
                this.GetInventoryVM().Item1.Item = item;
                this.GetInventoryVM().Item1.Count = count;
                this.GetInventoryVM().Item1.UpdateFromItem();
            }
            else if (index == (int)EquipmentIndex.Weapon2)
            {
                this.GetInventoryVM().Item2.Item = item;
                this.GetInventoryVM().Item2.Count = count;
                this.GetInventoryVM().Item2.UpdateFromItem();
            }
            else if (index == (int)EquipmentIndex.Weapon3)
            {
                this.GetInventoryVM().Item3.Item = item;
                this.GetInventoryVM().Item3.Count = count;
                this.GetInventoryVM().Item3.UpdateFromItem();
            }
        }

        public virtual void HandleClickItem(PEItemVM clickedSlot)
        {
            PEInventoryVM inventoryVM = this.GetInventoryVM();
            // InformationManager.DisplayMessage(new InformationMessage(this._gauntletLayer.Input.IsShiftDown().ToString()));
            if (base.MissionScreen.InputManager.IsShiftDown() || this._gauntletLayer.Input.IsShiftDown())
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new InventoryHotkey(clickedSlot.DropTag));
                GameNetwork.EndModuleEventAsClient();
            }
            else if (base.MissionScreen.InputManager.IsControlDown() || this._gauntletLayer.Input.IsControlDown())
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new InventorySplitItem(clickedSlot.DropTag));
                GameNetwork.EndModuleEventAsClient();
            }
        }

        public void ForceUpdateInventory(string UpdateTag, ItemObject item, int count)
        {
            if (!this.IsActive) return;
            string[] updatedTag = UpdateTag.Split('_');
            string inventory0 = string.Join("_", updatedTag.Take(updatedTag.Length - 1));
            int inventory0Index = int.Parse(updatedTag.Last());
            if (inventory0 == this.GetInventoryVM().TargetInventoryId)
            {
                this.GetInventoryVM().RequestedInventoryItems[inventory0Index].Item = item;
                this.GetInventoryVM().RequestedInventoryItems[inventory0Index].Count = count;
                this.GetInventoryVM().RequestedInventoryItems[inventory0Index].UpdateFromItem();
            }
            if (inventory0 == "PlayerInventory")
            {
                this.GetInventoryVM().InventoryItems[inventory0Index].Item = item;
                this.GetInventoryVM().InventoryItems[inventory0Index].Count = count;
                this.GetInventoryVM().InventoryItems[inventory0Index].UpdateFromItem();
            }
            if (inventory0 == "Equipment")
            {
                this.UpdateEquipmentInventory(inventory0Index, item, count);
            }
        }
        public void UpdateInventory(String DraggedSlot, String DroppedSlot, ItemObject item0, ItemObject item1, int item0Count, int item1Count)
        {
            if (!this.IsActive) return;
            string[] draggedSplitted = DraggedSlot.Split('_');
            string[] droppedSplitted = DroppedSlot.Split('_');
            string inventory0 = string.Join("_", draggedSplitted.Take(draggedSplitted.Length - 1));
            int inventory0Index = int.Parse(draggedSplitted.Last());
            string inventory1 = string.Join("_", droppedSplitted.Take(droppedSplitted.Length - 1));
            int inventory1Index = int.Parse(droppedSplitted.Last());

            if (inventory0 == "Equipment")
            {
                this.UpdateEquipmentInventory(inventory0Index, item0, item0Count);
            }
            else if (inventory0 == "PlayerInventory")
            {
                this.GetInventoryVM().InventoryItems[inventory0Index].Item = item0;
                this.GetInventoryVM().InventoryItems[inventory0Index].Count = item0Count;
                this.GetInventoryVM().InventoryItems[inventory0Index].UpdateFromItem();
            }
            else if (inventory0 == this.GetInventoryVM().TargetInventoryId)
            {
                this.GetInventoryVM().RequestedInventoryItems[inventory0Index].Item = item0;
                this.GetInventoryVM().RequestedInventoryItems[inventory0Index].Count = item0Count;
                this.GetInventoryVM().RequestedInventoryItems[inventory0Index].UpdateFromItem();
            }
            if (inventory1 == "Equipment")
            {
                this.UpdateEquipmentInventory(inventory1Index, item1, item1Count);
            }
            else if (inventory1 == "PlayerInventory")
            {
                this.GetInventoryVM().InventoryItems[inventory1Index].Item = item1;
                this.GetInventoryVM().InventoryItems[inventory1Index].Count = item1Count;
                this.GetInventoryVM().InventoryItems[inventory1Index].UpdateFromItem();
            }
            else if (inventory1 == this.GetInventoryVM().TargetInventoryId)
            {
                this.GetInventoryVM().RequestedInventoryItems[inventory1Index].Item = item1;
                this.GetInventoryVM().RequestedInventoryItems[inventory1Index].Count = item1Count;
                this.GetInventoryVM().RequestedInventoryItems[inventory1Index].UpdateFromItem();
            }

        }

    }
}

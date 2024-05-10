using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.Data
{
    public class Inventory
    {
        public List<InventorySlot> Slots = new List<InventorySlot>();
        public string InventoryId = "";
        public bool IsConsumable = false;
        public bool GeneratedViaSpawner = false;
        public List<NetworkCommunicator> CurrentlyOpenedBy;
        public PE_InventoryEntity TiedEntity;
        public Inventory(int slotCount, int stackCount, string inventoryId)
        {
            for (int i = 0; i < slotCount; i++)
            {
                Slots.Add(new InventorySlot(stackCount));
            }
            this.InventoryId = inventoryId;
            this.CurrentlyOpenedBy = new List<NetworkCommunicator>();
        }

        public Inventory(int slotCount, int stackCount, string inventoryId, PE_InventoryEntity tiedEntity)
        {
            for (int i = 0; i < slotCount; i++)
            {
                Slots.Add(new InventorySlot(stackCount));
            }
            this.InventoryId = inventoryId;
            this.CurrentlyOpenedBy = new List<NetworkCommunicator>();
            this.TiedEntity = tiedEntity;
        }

        public bool IsInventoryIncludes(ItemObject item, int count)
        {
            int inventoryIncludesCount = 0;
            foreach (InventorySlot slot in this.Slots)
            {
                if (slot.Item == null) continue;
                if (slot.Item.StringId == item.StringId)
                {
                    inventoryIncludesCount += slot.Count;
                    if (inventoryIncludesCount >= count) return true;
                }
            }
            return false;
        }
        public bool RemoveCountedItem(ItemObject item, int count)
        {
            foreach (InventorySlot slot in this.Slots)
            {
                if (slot.Item == null) continue;
                if (slot.Item.StringId != item.StringId) continue;
                int removeCount = Math.Min(count, slot.Count);
                slot.Count -= removeCount;
                if (slot.Count == 0) slot.Item = null;

                count -= removeCount;
                if (count == 0) return true;
            }
            return false;
        }
        public List<int> RemoveCountedItemSynced(ItemObject item, int count)
        {
            List<int> slots = new List<int>();
            for (int i = 0; i < this.Slots.Count; i++)
            {
                InventorySlot slot = this.Slots[i];
                if (slot.Item == null) continue;
                else if (slot.Item.StringId != item.StringId) continue;
                int removeCount = Math.Min(count, slot.Count);
                slot.Count -= removeCount;
                if (slot.Count == 0) slot.Item = null;

                count -= removeCount;
                slots.Add(i);
                if (count == 0) return slots;
            }
            return slots;
        }
        public bool HasEnoughRoomFor(ItemObject item, int count)
        {
            foreach (InventorySlot slot in this.Slots)
            {
                if (slot.Item == null) count -= Math.Min(slot.MaxStackCount, count);
                else if (slot.Item.StringId == item.StringId) count -= Math.Min(slot.MaxStackCount - slot.Count, count);
                else if (count <= 0) return true;
            }
            return count <= 0;
        }
        public List<int> AddCountedItemSynced(ItemObject item, int count, int ammo)
        {
            List<int> slots = new List<int>();
            for (int i = 0; i < this.Slots.Count; i++)
            {
                InventorySlot slot = this.Slots[i];
                if (slot.Item == null)
                {
                    int maxAddableAmount = item.Type == ItemObject.ItemTypeEnum.Thrown ? slot.MaxStackCount : 1;
                    int addCount = Math.Min(maxAddableAmount, count);
                    slot.Item = item;
                    slot.Count = addCount;
                    slot.Ammo = ammo;
                    count -= addCount;
                    slots.Add(i);
                }
                else if (slot.Item.StringId == item.StringId && slot.Count < slot.MaxStackCount)
                {
                    int maxAddableAmount = slot.MaxStackCount - slot.Count;
                    int addCount = Math.Min(maxAddableAmount, count);
                    slot.Item = item;
                    slot.Count += addCount;
                    count -= addCount;
                    slots.Add(i);
                }
                if (count == 0) return slots;
            }
            return slots;
        }

        public void AddOpenedBy(NetworkCommunicator peer)
        {
            this.CurrentlyOpenedBy.Add(peer);
        }
        public void RemoveOpenedBy(NetworkCommunicator peer)
        {
            this.CurrentlyOpenedBy.Remove(peer);
        }
        public void ClosedBy(NetworkCommunicator peer)
        {
            this.CurrentlyOpenedBy.Remove(peer);
        }

        public bool IsInventoryFull()
        {
            bool oneEmptySlot = this.Slots.Any((slot) => slot.IsEmpty());
            return !oneEmptySlot;
        }
        public bool IsInventoryEmpty()
        {
            bool isEmpty = true;
            foreach (InventorySlot inventorySlot in this.Slots)
            {
                if (inventorySlot.Count > 0) return false;
            }
            return isEmpty;
        }

        public void ExpandInventoryWithItem(ItemObject item, int count, int ammo)
        {
            this.Slots.Add(new InventorySlot(item, count, count, ammo));
        }

        public int AddItem(int slot, ItemObject item, int count, int ammo, InventorySlot itemAddedFrom = null)
        {

            if (this.Slots[slot].MaxStackCount == 0) return count;
            if (count > 200) count = 200;
            if (this.Slots[slot].IsEmpty())
            {
                this.Slots[slot].Item = item;
                if (item.Type == ItemObject.ItemTypeEnum.Arrows ||
                                item.Type == ItemObject.ItemTypeEnum.Bolts ||
                                item.Type == ItemObject.ItemTypeEnum.Bullets ||
                                item.Type == ItemObject.ItemTypeEnum.Shield)
                {
                    this.Slots[slot].Ammo = ammo;
                }
                else
                {
                    this.Slots[slot].Ammo = ItemHelper.GetMaximumAmmo(item);
                }
            }
            if (this.Slots[slot].Item.StringId == item.StringId)
            {
                int maxAddableQuantity = this.Slots[slot].MaxStackCount - this.Slots[slot].Count;
                int addableQuantity = Math.Min(maxAddableQuantity, count);
                this.Slots[slot].Count += addableQuantity;
                count -= addableQuantity;
            }
            else
            {
                return count;
            }
            if (itemAddedFrom != null && count > 0)
            {
                itemAddedFrom.Count = count;
                count = 0;
            }
            else if (itemAddedFrom != null && count <= 0)
            {
                itemAddedFrom.Item = null;
                itemAddedFrom.Count = 0;
            }
            return count; // Return remaining count
        }
        public void EmptyInventory()
        {
            foreach (InventorySlot slot in this.Slots)
            {
                slot.Count = 0;
                slot.Item = null;
            }
        }

        public string Serialize()
        {
            string[] slots = new string[this.Slots.Count];
            for (int i = 0; i < this.Slots.Count; i++)
            {
                slots[i] = this.Slots[i].IsEmpty() ? "" : this.Slots[i].Item.StringId + "*" + this.Slots[i].Count.ToString();
                if (!this.Slots[i].IsEmpty())
                {
                    slots[i] = this.Slots[i].Item.StringId + "*" + this.Slots[i].Count.ToString();
                    if (this.Slots[i].Item.Type == ItemObject.ItemTypeEnum.Arrows ||
                                this.Slots[i].Item.Type == ItemObject.ItemTypeEnum.Bolts ||
                                this.Slots[i].Item.Type == ItemObject.ItemTypeEnum.Bullets)
                    {
                        slots[i] += "*" + this.Slots[i].Ammo.ToString();
                    }
                }
                else
                {
                    slots[i] = "";
                }
            }
            return String.Join("|", slots);
        }

        public static Inventory Deserialize(string serialized, string inventoryId, PE_InventoryEntity entity)
        {
            string[] slots = serialized.Split('|');
            Inventory inventory = new Inventory(slots.Length, 10, inventoryId);
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != "")
                {
                    string[] splitted = slots[i].Split('*');
                    string itemId = splitted[0];
                    int itemCount = int.Parse(splitted[1]);
                    ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
                    int ammo = ItemHelper.GetMaximumAmmo(item);
                    if (splitted.Length > 2)
                    {
                        ammo = int.Parse(splitted[2]);
                    }
                    inventory.Slots[i].Item = item;
                    inventory.Slots[i].Count = itemCount;
                    inventory.Slots[i].Ammo = ammo;
                }
            }
            if (entity != null)
            {
                inventory.TiedEntity = entity;
            }
            return inventory;
        }

        public static void SwapSlots(InventorySlot slotOne, InventorySlot slotTwo)
        {
            ItemObject tempObject = slotOne.Item;
            int tempCount = slotOne.Count;

            slotOne.Item = slotTwo.Item;
            slotOne.Count = slotTwo.Count;

            slotTwo.Item = tempObject;
            slotTwo.Count = tempCount;
        }
    }
}

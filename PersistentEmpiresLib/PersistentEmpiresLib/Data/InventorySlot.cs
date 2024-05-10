using TaleWorlds.Core;

namespace PersistentEmpiresLib.Data
{
    public class InventorySlot
    {
        private int _maxStackCount;
        public ItemObject Item { get; set; }
        public int Count { get; set; }
        public int Ammo { get; set; }
        public int MaxStackCount
        {
            get
            {
                if (this.Item == null || this.Item.Type == ItemObject.ItemTypeEnum.Thrown) return this._maxStackCount;
                else if (this._maxStackCount < 1) return this._maxStackCount;
                return 1;
            }
            set
            {
                this._maxStackCount = value;
            }
        }
        public InventorySlot(ItemObject item, int count, int maxStackCount, int ammo)
        {
            this.Item = item;
            this.Count = count;
            this.MaxStackCount = maxStackCount;
            this.Ammo = ammo;
        }
        public InventorySlot(int maxStackCount)
        {
            this.Item = null;
            this.Count = 0;
            this.Ammo = 0;
            this.MaxStackCount = maxStackCount;
        }
        public void SwapWith(InventorySlot otherSlot)
        {
            ItemObject otherItem = otherSlot.Item;
            int otherCount = otherSlot.Count;

            otherSlot.Item = this.Item;
            otherSlot.Count = this.Count;

            this.Item = otherItem;
            this.Count = otherCount;
        }
        public void EmptySlot()
        {
            this.Item = null;
            this.Count = 0;
        }
        public bool IsEmpty()
        {
            return this.Item == null || this.Count == 0;
        }
    }

}

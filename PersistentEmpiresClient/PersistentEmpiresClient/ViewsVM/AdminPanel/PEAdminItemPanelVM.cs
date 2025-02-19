using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpires.Views.ViewsVM.AdminPanel
{
    public class PEAdminItemPanelVM : ViewModel
    {
        private string _itemId;

        private Action _onCancel;
        private Action<string, int> _onApply;
        private int _count = 1;

        public PEAdminItemPanelVM(Action<string, int> _onApply, Action _onCancel)
        {
            this._onApply = _onApply;
            this._onCancel = _onCancel;
        }

        string tmp = "";
        private void TryToFindItem(string itemId)
        {
            tmp = itemId.Replace("*", "");
            var item = MBObjectManager.Instance.GetObject<ItemObject>(Find);

            if(item != null)
            {
                ItemId = item.StringId;
            }
        }

        private bool Find(ItemObject item)
        {
            return item.StringId.Contains(tmp);
        }

        [DataSourceProperty]
        public string ItemId
        {
            get => this._itemId;
            set
            {
                if (value != this._itemId)
                {
                    this._itemId = value;
                    base.OnPropertyChangedWithValue(value, "ItemId");

                    if(_itemId.StartsWith("*") && _itemId.EndsWith("*"))
                    {
                        TryToFindItem(_itemId);
                    }
                }
            }
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

        public void ExecuteApply()
        {
            this._onApply(this.ItemId, this.Count);
        }
        public void ExecuteCancel()
        {
            this._onCancel();
        }
    }
}

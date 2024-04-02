using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

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

        [DataSourceProperty]
        public string ItemId
        {
            get => this._itemId;
            set {
                if(value != this._itemId)
                {
                    this._itemId = value;
                    base.OnPropertyChangedWithValue(value, "ItemId");
                }
            }
        }
        [DataSourceProperty]
        public int Count
        {
            get => this._count;
            set
            {
                if(value != this._count)
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

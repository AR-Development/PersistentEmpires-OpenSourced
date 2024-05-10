using System;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.AdminPanel
{
    public class PEAdminGoldVM : ViewModel
    {
        private int _gold = 1000;
        private Action _onCancel;
        private Action<int> _onApply;
        public PEAdminGoldVM(Action<int> _onApply, Action _onCancel)
        {
            this._onApply = _onApply;
            this._onCancel = _onCancel;
        }

        public void ExecuteApply()
        {
            this._onApply(this.Gold);
        }
        public void ExecuteCancel()
        {
            this._onCancel();
        }

        [DataSourceProperty]
        public int Gold
        {
            get => this._gold;
            set
            {
                if (value != this._gold)
                {
                    this._gold = value;
                    base.OnPropertyChangedWithValue(value, "Count");
                }
            }
        }
    }
}

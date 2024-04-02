using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.ViewsVM.AdminPanel
{
    public class PEAdminMenuItemVM : ViewModel
    {
        private Action _onExecute;
        private TextObject _itemObj;
        private string _actionText;

        [DataSourceProperty]
        public string ActionText
        {
            get
            {
                return this._actionText;
            }
            set
            {
                if (value != this._actionText)
                {
                    this._actionText = value;
                    base.OnPropertyChangedWithValue(value, "ActionText");
                }
            }
        }
        public PEAdminMenuItemVM(TextObject item, Action onExecute)
        {
            this._onExecute = onExecute;
            this._itemObj = item;
            this.ActionText = this._itemObj.ToString();
        }
        public void ExecuteAction()
        {
            this._onExecute();
        }
    }
}

using System.Collections.Generic;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.AdminPanel
{
    public class PEAdminPanelVM : ViewModel
    {
        private MBBindingList<PEAdminMenuItemVM> _menuItems;

        public PEAdminPanelVM(IEnumerable<PEAdminMenuItemVM> items)
        {
            this.MenuItems = new MBBindingList<PEAdminMenuItemVM>();
            if (items != null)
            {
                foreach (PEAdminMenuItemVM item in items)
                {
                    this.MenuItems.Add(item);
                }
            }
            this.RefreshValues();
        }
        public override void RefreshValues()
        {
            base.RefreshValues();
            this.MenuItems.ApplyActionOnAllItems(delegate (PEAdminMenuItemVM x)
            {
                x.RefreshValues();
            });
        }
        public void RefreshItems(IEnumerable<PEAdminMenuItemVM> items)
        {
            this.MenuItems.Clear();
            foreach (PEAdminMenuItemVM item in items)
            {
                this.MenuItems.Add(item);
            }
        }

        [DataSourceProperty]
        public MBBindingList<PEAdminMenuItemVM> MenuItems
        {
            get
            {
                return this._menuItems;
            }
            set
            {
                if (value != this._menuItems)
                {
                    this._menuItems = value;
                    base.OnPropertyChangedWithValue(value, "MenuItems");
                }
            }
        }
    }
}

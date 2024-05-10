using PersistentEmpires.Views.ViewsVM.FactionManagement.ManagementMenu;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.FactionManagement
{
    public class PEFactionManagementMenuVM : ViewModel
    {
        private MBBindingList<ManagementItemVM> _menuItems;

        public PEFactionManagementMenuVM(IEnumerable<ManagementItemVM> items)
        {
            this.MenuItems = new MBBindingList<ManagementItemVM>();
            if (items != null)
            {
                foreach (ManagementItemVM item in items)
                {
                    this.MenuItems.Add(item);
                }
            }
            this.RefreshValues();
        }
        public override void RefreshValues()
        {
            base.RefreshValues();
            this.MenuItems.ApplyActionOnAllItems(delegate (ManagementItemVM x)
            {
                x.RefreshValues();
            });
        }
        public void RefreshItems(IEnumerable<ManagementItemVM> items)
        {
            this.MenuItems.Clear();
            foreach (ManagementItemVM item in items)
            {
                this.MenuItems.Add(item);
            }
        }

        [DataSourceProperty]
        public MBBindingList<ManagementItemVM> MenuItems
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

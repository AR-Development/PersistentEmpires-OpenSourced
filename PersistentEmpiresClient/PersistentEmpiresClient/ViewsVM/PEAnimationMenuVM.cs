using PersistentEmpires.Views.ViewsVM.AnimationMenu;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEAnimationMenuVM : ViewModel
    {
        MBBindingList<PEAnimationSubMenuVM> _subMenus;
        private PEAnimationSubMenuVM _selectedMenu;

        public delegate void AnimationSelected(string actionId);
        public event AnimationSelected OnAnimationSelected;

        public PEAnimationMenuVM(List<PEAnimationSubMenuVM> categories)
        {
            this.SubMenus = new MBBindingList<PEAnimationSubMenuVM>();
            foreach (PEAnimationSubMenuVM category in categories)
            {

                this.SubMenus.Add(category);
            }
        }

        public void SelectInput(int input)
        {
            if (this.IsSelected)
            {
                if (input == 9)
                {
                    if (this.SelectedMenu.PageNumber == 0)
                    {
                        this.SelectedMenu = null;
                    }
                    else
                    {
                        this.SelectedMenu.PageNumber -= 1;
                    }
                }
                else if (input == 0)
                {
                    if (this.SelectedMenu.PageNumber != this.SelectedMenu.NativePages.Count - 1)
                    {
                        this.SelectedMenu.PageNumber = this.SelectedMenu.PageNumber + 1;
                    }
                }
                else
                {
                    if ((input - 1) < this.SelectedMenu.Page.Count)
                    {
                        if (this.OnAnimationSelected != null)
                        {
                            this.OnAnimationSelected(this.SelectedMenu.Page[input - 1].ActionId);
                        }
                    }
                }
            }
            else
            {
                if (input == 0)
                {
                    this.OnAnimationSelected("act_none");
                    return;
                }
                if (input - 1 >= this._subMenus.Count) return;
                this.SelectedMenu = this._subMenus[input - 1];
            }
        }

        [DataSourceProperty]
        public PEAnimationSubMenuVM SelectedMenu
        {
            get => this._selectedMenu;
            set
            {
                if (value != this._selectedMenu)
                {
                    this._selectedMenu = value;
                    base.OnPropertyChanged("IsSelected");
                    base.OnPropertyChangedWithValue(value, "SelectedMenu");
                }
            }
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get => this.SelectedMenu != null;
        }

        [DataSourceProperty]
        public MBBindingList<PEAnimationSubMenuVM> SubMenus
        {
            get => this._subMenus;
            set
            {
                if (value != this._subMenus)
                {
                    this._subMenus = value;
                    base.OnPropertyChangedWithValue(value, "SubMenus");
                }
            }
        }
    }
}

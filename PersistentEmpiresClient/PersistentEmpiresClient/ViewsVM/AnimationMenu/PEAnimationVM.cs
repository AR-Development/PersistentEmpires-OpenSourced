using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.AnimationMenu
{
    public class PEAnimationVM : ViewModel
    {
        private string _name;
        public string ActionId;
        public PEAnimationVM(string name, string actionId)
        {
            this.Name = name;
            this.ActionId = actionId;
        }

        [DataSourceProperty]
        public string Name
        {
            get => this._name;
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    base.OnPropertyChangedWithValue(value, "Name");
                }
            }
        }
    }
}

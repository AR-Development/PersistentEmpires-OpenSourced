using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PESpawnHudVM : ViewModel
    {
        private string _actionMessage;

        public PESpawnHudVM() { }

        public string ActionMessage
        {
            get => this._actionMessage;
            set
            {
                if (value != this._actionMessage)
                {
                    this._actionMessage = value;
                    base.OnPropertyChangedWithValue(value, "ActionMessage");
                }
            }
        }

    }
}

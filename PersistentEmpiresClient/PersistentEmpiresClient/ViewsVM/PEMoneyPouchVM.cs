using PersistentEmpiresLib;
using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEMoneyPouchVM : ViewModel
    {
        private int _goldInput;
        private PersistentEmpireRepresentative _representative;
        public PEMoneyPouchVM()
        {
        }
        public void RefreshValues(PersistentEmpireRepresentative persistentEmpireRepresentative)
        {
            this._representative = persistentEmpireRepresentative;
            this.GoldInput = this._representative.Gold;
        }
        [DataSourceProperty]
        public int GoldInput
        {
            get => this._goldInput;
            set
            {
                if (value != this._goldInput)
                {
                    this._goldInput = value > this._representative.Gold ? this._representative.Gold : value;
                    base.OnPropertyChangedWithValue(this._goldInput, "GoldInput");
                }
            }
        }
        public void ExecuteDropMoney()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestDropMoney(this.GoldInput));
            GameNetwork.EndModuleEventAsClient();
        }

        public void ExecuteRevealMoneyPouch()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestRevealMoneyPouch());
            GameNetwork.EndModuleEventAsClient();
        }
    }
}

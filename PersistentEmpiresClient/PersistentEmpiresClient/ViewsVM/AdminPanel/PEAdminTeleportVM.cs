using PersistentEmpiresLib.NetworkMessages.Client;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.ViewsVM.AdminPanel
{
    public class PEAdminTeleportVM : ViewModel
    {
        private MBBindingList<PEAdminTpItemVM> _adminTps;
        private Views.AdminPanel.PEAdminTeleportView _pEAdminTeleportView;

        public PEAdminTeleportVM(List<PersistentEmpiresLib.SceneScripts.AdminTp> adminTps, Views.AdminPanel.PEAdminTeleportView pEAdminTeleportView)
        {
            _pEAdminTeleportView = pEAdminTeleportView;
            _adminTps = new MBBindingList<PEAdminTpItemVM>();
            adminTps.OrderBy(x => x.Description).ToList().ForEach(x=> _adminTps.Add(new PEAdminTpItemVM(x, ExecuteSelect)));

            OnPropertyChanged("AdminTps");
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
        }

        public void ExecuteSelect(PEAdminTpItemVM selectedPosition)
        {
            ++selectedPosition.TeleportLocation.SpawnPosition.x;
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestTpToPosition(selectedPosition.TeleportLocation.SpawnPosition));
            GameNetwork.EndModuleEventAsClient();
            _pEAdminTeleportView.OnEscape();
        }

        [DataSourceProperty]
        public MBBindingList<PEAdminTpItemVM> AdminTps
        {
            get => _adminTps;
            set
            {
                if (value != _adminTps)
                {
                    _adminTps = value;
                    OnPropertyChangedWithValue(value, "AdminTps");
                }
            }
        }
    }
}
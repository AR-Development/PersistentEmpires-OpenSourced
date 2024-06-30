using PersistentEmpires.Views.ViewsVM.AdminPanel;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views.AdminPanel
{
    public class PEAdminTeleportView : MissionView
    {
        protected GauntletLayer _gauntletLayer;
        private PEAdminTeleportVM _dataSource;

        public bool IsActive { get; private set; }

        public PEAdminTeleportView()
        {
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            _dataSource = new PEAdminTeleportVM(AdminClientBehavior.AdminTps, this);
        }

        public override bool OnEscape()
        {
            bool result = base.OnEscape();
            CloseManagementMenu();
            return result;
        }
        protected void CloseManagementMenu()
        {
            if (IsActive)
            {
                IsActive = false;
                _gauntletLayer.InputRestrictions.ResetInputRestrictions();
                MissionScreen.RemoveLayer(this._gauntletLayer);
                _gauntletLayer = null;
            }
        }
        public void OnOpen()
        {
            _dataSource.RefreshValues();
            _gauntletLayer = new GauntletLayer(2);
            _gauntletLayer.LoadMovie("PEAdminTeleport", _dataSource);
            _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
            MissionScreen.AddLayer(_gauntletLayer);
            IsActive = true;
        }
    }
}
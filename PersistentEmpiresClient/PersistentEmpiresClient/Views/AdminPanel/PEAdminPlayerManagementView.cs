using PersistentEmpires.Views.ViewsVM.AdminPanel;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views.AdminPanel
{
    public class PEAdminPlayerManagementView : MissionView
    {
        protected AdminClientBehavior _adminBehavior;

        protected GauntletLayer _gauntletLayer;
        private PEAdminPlayerManagementVM _dataSource;

        public bool IsActive { get; private set; }

        public PEAdminPlayerManagementView()
        {
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._adminBehavior = base.Mission.GetMissionBehavior<AdminClientBehavior>();
            _dataSource = new PEAdminPlayerManagementVM();
        }

        public override bool OnEscape()
        {
            bool result = base.OnEscape();
            this.CloseManagementMenu();
            return result;
        }
        protected void CloseManagementMenu()
        {
            if (this.IsActive)
            {
                this.IsActive = false;
                this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
                this._dataSource.SelectedPlayer = null;
                this._dataSource.SearchedPlayerName = "";
                base.MissionScreen.RemoveLayer(this._gauntletLayer);
                this._gauntletLayer = null;
            }
        }
        public void OnOpen()
        {
            this._dataSource.RefreshValues();
            this._gauntletLayer = new GauntletLayer(2);
            this._gauntletLayer.LoadMovie("PEAdminPlayerManagement", this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            this.IsActive = true;
        }
    }
}
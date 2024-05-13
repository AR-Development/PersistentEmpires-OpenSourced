using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views.FactionManagement
{
    public class PEMenuItem : MissionView
    {
        protected FactionUIComponent _factionManagementComponent;
        protected FactionsBehavior _factionsBehavior;
        protected ViewModel _dataSource;
        protected GauntletLayer _gauntletLayer;
        protected string _screenName;
        public bool IsActive { get; protected set; }
        public PEMenuItem(string screenName)
        {
            this._screenName = screenName;
        }

        // protected abstract ViewModel GetDataSource();
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._factionManagementComponent = base.Mission.GetMissionBehavior<FactionUIComponent>();
            this._factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();

            // this._dataSource = this.GetDataSource();
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
                base.MissionScreen.RemoveLayer(this._gauntletLayer);
                this._gauntletLayer = null;
            }
        }
        protected virtual void OnOpen()
        {
            this._dataSource.RefreshValues();
            this._gauntletLayer = new GauntletLayer(2);
            this._gauntletLayer.LoadMovie(this._screenName, this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            this.IsActive = true;
        }
    }
}

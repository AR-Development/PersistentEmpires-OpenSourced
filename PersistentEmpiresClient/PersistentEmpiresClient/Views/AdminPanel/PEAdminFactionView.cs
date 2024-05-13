using PersistentEmpires.Views.ViewsVM.AdminPanel;
using PersistentEmpires.Views.ViewsVM.PETabMenu;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views.AdminPanel
{
    public class PEAdminFactionView : MissionView
    {
        protected AdminClientBehavior _adminBehavior;
        private FactionsBehavior _factionsBehavior;
        protected GauntletLayer _gauntletLayer;
        private PEAdminFactionPanelVM _dataSource;

        public bool IsActive { get; private set; }

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
        public void OnOpen()
        {
            this._dataSource.RefreshValues();
            this._dataSource.RefreshValues(this._factionsBehavior.Factions);
            this._gauntletLayer = new GauntletLayer(2);
            this._gauntletLayer.LoadMovie("PEAdminFactionPanel", this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            this.IsActive = true;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._adminBehavior = base.Mission.GetMissionBehavior<AdminClientBehavior>();
            this._factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            _dataSource = new PEAdminFactionPanelVM(this._factionsBehavior.Factions, this.SetName, this.ResetBanner, this.JoinFaction);
        }

        private void JoinFaction(TabFactionVM obj)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestAdminJoinFaction(obj.FactionIndex));
            GameNetwork.EndModuleEventAsClient();
        }

        private void SetName(TabFactionVM arg1, string arg2)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestAdminSetFactionName(arg1.FactionIndex, arg2));
            GameNetwork.EndModuleEventAsClient();
        }

        private void ResetBanner(TabFactionVM obj)
        {
            this.CloseManagementMenu();
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestAdminResetFactionBanner(obj.FactionIndex));
            GameNetwork.EndModuleEventAsClient();
        }
    }
}

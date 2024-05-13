using PersistentEmpires.Views.ViewsVM.AdminPanel;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views.AdminPanel
{
    public class PEAdminItemPanelView : MissionView
    {
        protected AdminClientBehavior _adminBehavior;

        protected GauntletLayer _gauntletLayer;
        private PEAdminItemPanelVM _dataSource;

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
                this._dataSource.ItemId = "";
                this._dataSource.Count = 1;
                base.MissionScreen.RemoveLayer(this._gauntletLayer);
                this._gauntletLayer = null;
            }
        }
        public void OnOpen()
        {
            this._dataSource.RefreshValues();
            this._gauntletLayer = new GauntletLayer(2);
            this._gauntletLayer.LoadMovie("PEAdminItemPanel", this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            this.IsActive = true;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._adminBehavior = base.Mission.GetMissionBehavior<AdminClientBehavior>();
            _dataSource = new PEAdminItemPanelVM((string ItemId, int count) =>
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new RequestItemSpawn(ItemId, count));
                GameNetwork.EndModuleEventAsClient();
            }, () =>
            {
                this.CloseManagementMenu();
            });
        }
    }
}

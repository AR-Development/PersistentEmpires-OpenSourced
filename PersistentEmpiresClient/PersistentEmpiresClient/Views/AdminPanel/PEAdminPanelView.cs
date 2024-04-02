using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpires.Views.Views.AdminPanel;
using PersistentEmpires.Views.ViewsVM.AdminPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PEAdminPanelView : MissionView
    {
        private GauntletLayer _gauntletLayer;
        private PEAdminPanelVM _dataSource;
        private AdminClientBehavior _adminBehavior;

        public bool IsActive { get; protected set; }
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._adminBehavior = base.Mission.GetMissionBehavior<AdminClientBehavior>();
            this._adminBehavior.OnAdminPanelClick += this.OnAdminPanelClick;
            this._dataSource = new PEAdminPanelVM(null);
        }

        public override bool OnEscape()
        {
            bool result = base.OnEscape();
            this.CloseAdminPanel();
            return result;
        }

        private void CloseAdminPanel()
        {
            if (this.IsActive)
            {
                this.IsActive = false;
                this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
                base.MissionScreen.RemoveLayer(this._gauntletLayer);
                this._gauntletLayer = null;
            }
        }

        private void OpenPanelMenu()
        {
            this._dataSource.RefreshItems(this.GetList());
            this._gauntletLayer = new GauntletLayer(2);
            this._gauntletLayer.LoadMovie("PEAdminPanel", this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            this.IsActive = true;
        }

        private IEnumerable<PEAdminMenuItemVM> GetList()
        {
            List<PEAdminMenuItemVM> menuItemVm = new List<PEAdminMenuItemVM>();
            menuItemVm.Add(new PEAdminMenuItemVM(new TextObject("Player Management"), () =>
            {
                this.CloseAdminPanel();
                base.Mission.GetMissionBehavior<PEAdminPlayerManagementView>().OnOpen();
            }));

            menuItemVm.Add(new PEAdminMenuItemVM(new TextObject("Item Management"), () =>
            {
                this.CloseAdminPanel();
                base.Mission.GetMissionBehavior<PEAdminItemPanelView>().OnOpen();
            }));

            menuItemVm.Add(new PEAdminMenuItemVM(new TextObject("Faction Management"), () =>
            {
                this.CloseAdminPanel();
                base.Mission.GetMissionBehavior<PEAdminFactionView>().OnOpen();
            }));

            menuItemVm.Add(new PEAdminMenuItemVM(new TextObject("Give Yourself Money"), () =>
            {
                this.CloseAdminPanel();
                base.Mission.GetMissionBehavior<PEAdminGoldView>().OnOpen();
            }));

            menuItemVm.Add(new PEAdminMenuItemVM(new TextObject("Became Godlike"), () =>
            {
                this.CloseAdminPanel();
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new RequestBecameGodlike());
                GameNetwork.EndModuleEventAsClient();
            }));

            menuItemVm.Add(new PEAdminMenuItemVM(new TextObject("Remove Unused Boats"), () =>
            {
                this.CloseAdminPanel();
                InformationManager.DisplayMessage(new InformationMessage("Not implemented yet"));
            }));

            menuItemVm.Add(new PEAdminMenuItemVM(new TextObject("Remove Unused Attachable"), () =>
            {
                this.CloseAdminPanel();
                InformationManager.DisplayMessage(new InformationMessage("Not implemented yet"));
            }));

            return menuItemVm;
        }

        private void OnAdminPanelClick()
        {
            this.OpenPanelMenu();
        }
    }
}

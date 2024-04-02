using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpires.Views.ViewsVM.FactionManagement;
using PersistentEmpires.Views.ViewsVM.FactionManagement.ManagementMenu;
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
using PersistentEmpiresLib;
namespace PersistentEmpires.Views.Views.FactionManagement
{
    public class PEFactionManagementMenu : MissionView
    {
        private GauntletLayer _gauntletLayer;

        private PEFactionManagementMenuVM _dataSource;
        private FactionUIComponent _factionManagementComponent;

        public bool IsActive { get; protected set; }
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._factionManagementComponent = base.Mission.GetMissionBehavior<FactionUIComponent>();
            this._factionManagementComponent.OnFactionManagementClick += this.OnFactionManagementClick;
            this._dataSource = new PEFactionManagementMenuVM(null);
        }

        public bool PlayerIsLord()
        {
            PersistentEmpireRepresentative myRepr = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (myRepr == null) return false;
            if (myRepr.GetFaction() == null) return false;
            return myRepr.GetFaction().lordId == GameNetwork.MyPeer.VirtualPlayer.Id.ToString();
        }
        public bool PlayerIsLordOrMarshall()
        {
            PersistentEmpireRepresentative myRepr = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            return this.PlayerIsLord() || myRepr.GetFaction().marshalls.Contains(GameNetwork.MyPeer.VirtualPlayer.Id.ToString());
        }
        public List<ManagementItemVM> GetList() {
            PersistentEmpireRepresentative myRepr = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();

            List<ManagementItemVM> managementItemVM = new List<ManagementItemVM>();
            if(PlayerIsLord())
            {
                managementItemVM.Add(new ManagementItemVM(new TextObject("Change Faction Banner"), () => {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnBannerChangeClickHandler();
                }));
                managementItemVM.Add(new ManagementItemVM(new TextObject("Change Faction Name"), () => {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnNameChangeClickHandler();
                }));
               
                managementItemVM.Add(new ManagementItemVM(new TextObject("Assign Marshall"), () => {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnAssignMarshallClickHandler();
                }));
                managementItemVM.Add(new ManagementItemVM(new TextObject("Transfer Lordship"), () => {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnAssignTransferLordshipClickHandler();
                }));
                
            }
            if(PlayerIsLordOrMarshall())
            {
                managementItemVM.Add(new ManagementItemVM(new TextObject("Kick Someone From Faction"), () => {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnKickSomeoneFromFactionClickHandler();
                }));
                managementItemVM.Add(new ManagementItemVM(new TextObject("Diplomacy Menu"), () => {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnDiplomacyMenuClickHandler();
                }));
                managementItemVM.Add(new ManagementItemVM(new TextObject("Manage door keys"), () => {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnManageDoorKeysClickHandler();
                }));
                managementItemVM.Add(new ManagementItemVM(new TextObject("Manage chest keys"), () => {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnManageChestKeyClickHandler();
                }));
           
                managementItemVM.Add(new ManagementItemVM(new TextObject("Close"), () => {
                    this.CloseManagementMenu();
                }));
            }
            return managementItemVM;
        }
        public override bool OnEscape()
        {
            bool result = base.OnEscape();
            this.CloseManagementMenu();
            return result;
        }

        private void CloseManagementMenu() {
            if (this.IsActive)
            {
                this.IsActive = false;
                this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
                base.MissionScreen.RemoveLayer(this._gauntletLayer);
                this._gauntletLayer = null;
            }
        }
        private void OpenManagementMenu()
        {
            this._dataSource.RefreshItems(this.GetList());
            this._gauntletLayer = new GauntletLayer(2);
            this._gauntletLayer.LoadMovie("PEFactionManagementMenu", this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            this.IsActive = true;
        }

        public void OnFactionManagementClick() {
            this.OpenManagementMenu();
        }
    }
}

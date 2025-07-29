using PersistentEmpires.Views.ViewsVM.FactionManagement;
using PersistentEmpires.Views.ViewsVM.FactionManagement.ManagementMenu;
using PersistentEmpiresLib;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Collections.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using PersistentEmpiresLib.Helpers;
using TaleWorlds.Core;

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
            return myRepr.GetFaction().lordId == GameNetwork.MyPeer.VirtualPlayer.ToPlayerId();
        }
        public bool PlayerIsLordOrMarshall()
        {
            PersistentEmpireRepresentative myRepr = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            return this.PlayerIsLord() || myRepr.GetFaction().marshalls.Contains(GameNetwork.MyPeer.VirtualPlayer.ToPlayerId());
        }
        public List<ManagementItemVM> GetList()
        {
            PersistentEmpireRepresentative myRepr = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();

            List<ManagementItemVM> managementItemVM = new List<ManagementItemVM>();
            if (PlayerIsLord())
            {
                managementItemVM.Add(new ManagementItemVM(GameTexts.FindText("FactionManagementChangeBanner", null), () =>
                {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnBannerChangeClickHandler();
                }));
                managementItemVM.Add(new ManagementItemVM(GameTexts.FindText("FactionManagementChangeFactionName", null), () =>
                {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnNameChangeClickHandler();
                }));

                managementItemVM.Add(new ManagementItemVM(GameTexts.FindText("FactionManagementAssignMarshall", null), () =>
                {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnAssignMarshallClickHandler();
                }));
                managementItemVM.Add(new ManagementItemVM(GameTexts.FindText("FactionManagementTransferLordship", null), () =>
                {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnAssignTransferLordshipClickHandler();
                }));

            }
            if (PlayerIsLordOrMarshall())
            {
                managementItemVM.Add(new ManagementItemVM(GameTexts.FindText("FactionManagementKickFromFaction", null), () =>
                {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnKickSomeoneFromFactionClickHandler();
                }));
                if (myRepr.CanUseDiplomacy)
                {
                    managementItemVM.Add(new ManagementItemVM(GameTexts.FindText("FactionManagementDiplomacy", null), () =>
                    {
                        this.CloseManagementMenu();
                        this._factionManagementComponent.OnDiplomacyMenuClickHandler();
                    }));
                }
                managementItemVM.Add(new ManagementItemVM(GameTexts.FindText("FactionManagementManageDoorKeys", null), () =>
                {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnManageDoorKeysClickHandler();
                }));
                managementItemVM.Add(new ManagementItemVM(GameTexts.FindText("FactionManagementManageChestKeys", null), () =>
                {
                    this.CloseManagementMenu();
                    this._factionManagementComponent.OnManageChestKeyClickHandler();
                }));

                managementItemVM.Add(new ManagementItemVM(GameTexts.FindText("FactionManagementClose", null), () =>
                {
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

        private void CloseManagementMenu()
        {
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

        public void OnFactionManagementClick()
        {
            this.OpenManagementMenu();
        }
    }
}

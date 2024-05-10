using PersistentEmpires.Views.ViewsVM.FactionManagement;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
namespace PersistentEmpires.Views.Views.FactionManagement
{
    public class PEFactionTransferLordship : PEMenuItem
    {
        public PEFactionTransferLordship() : base("PEFactionMembers")
        {
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._factionManagementComponent.OnAssignTransferLordshipClick += this.OnOpen;
            this._dataSource = new PEFactionMembersVM("Transfer Lordship", "Set a new lord.", () =>
            {
                this.CloseManagementMenu();
                this._factionManagementComponent.OnFactionManagementClickHandler();
            },
            (PEFactionMemberItemVM selectedMember) =>
            {
                InquiryData data = new InquiryData("Are you sure ?", "Are you sure to transfer your lordship to another player ?", true, true, "Yes", "No", () =>
                {
                    GameNetwork.BeginModuleEventAsClient();
                    GameNetwork.WriteMessage(new RequestLordshipTransfer(selectedMember.Peer));
                    GameNetwork.EndModuleEventAsClient();
                    this.CloseManagementMenu();
                }, () =>
                {
                    this.CloseManagementMenu();
                });

                InformationManager.ShowInquiry(data);
            },
            () =>
            {
                this.CloseManagementMenu();
            });
        }
        protected override void OnOpen()
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;
            Faction faction = persistentEmpireRepresentative.GetFaction();
            PEFactionMembersVM dataSource = (PEFactionMembersVM)this._dataSource;
            dataSource.RefreshItems(faction, true);
            base.OnOpen();
        }
    }
}

using PersistentEmpires.Views.ViewsVM.FactionManagement;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.Core;
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
                
                InquiryData data = new InquiryData(GameTexts.FindText("TransferLordshipVerifyCaption", null).ToString(), 
                    GameTexts.FindText("TransferLordshipVerify", null).ToString(), true, true,
                    GameTexts.FindText("PE_InquiryData_Yes", null).ToString(),
                    GameTexts.FindText("PE_InquiryData_No", null).ToString(), () =>
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

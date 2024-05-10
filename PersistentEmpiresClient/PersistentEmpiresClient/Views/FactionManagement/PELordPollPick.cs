using PersistentEmpires.Views.ViewsVM.FactionManagement;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.MountAndBlade;
namespace PersistentEmpires.Views.Views.FactionManagement
{
    public class PELordPollPick : PEMenuItem
    {
        private FactionPollComponent _factionPollComponent;
        public PELordPollPick() : base("PEFactionMembers")
        {

        }
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._factionPollComponent = base.Mission.GetMissionBehavior<FactionPollComponent>();
            this._factionManagementComponent.OnFactionLordPollClick += this.OnOpen;
            this._dataSource = new PEFactionMembersVM("Vote A Player Lord", "Vote", () =>
            {
                this.CloseManagementMenu();
            },
            (PEFactionMemberItemVM selectedMember) =>
            {
                this._factionPollComponent.RequestLordPlayerPoll(selectedMember.Peer);
                this.CloseManagementMenu();
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
            ((PEFactionMembersVM)this._dataSource).RefreshItems(faction);
            base.OnOpen();
        }

    }
}

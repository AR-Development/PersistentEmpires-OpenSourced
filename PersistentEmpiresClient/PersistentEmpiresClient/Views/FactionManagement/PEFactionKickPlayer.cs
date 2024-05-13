using PersistentEmpires.Views.ViewsVM.FactionManagement;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using TaleWorlds.MountAndBlade;
namespace PersistentEmpires.Views.Views.FactionManagement
{
    public class PEFactionKickPlayer : PEMenuItem
    {
        public PEFactionKickPlayer() : base("PEFactionMembers")
        {

        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._factionManagementComponent.OnKickSomeoneFromFactionClick += this.OnOpen;
            this._dataSource = new PEFactionMembersVM("Kick Player", "Kick", () =>
            {
                this.CloseManagementMenu();
                this._factionManagementComponent.OnFactionManagementClickHandler();
            },
            (PEFactionMemberItemVM selectedMember) =>
            {
                this._factionsBehavior.RequestKickFromFaction(selectedMember.Peer);
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
            ((PEFactionMembersVM)this._dataSource).RefreshItems(faction, true);
            base.OnOpen();
        }
    }
}

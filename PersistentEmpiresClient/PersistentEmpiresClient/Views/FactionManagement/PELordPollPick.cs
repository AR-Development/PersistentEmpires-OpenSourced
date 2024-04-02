using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpires.Views.Views.FactionManagement;
using PersistentEmpires.Views.ViewsVM.FactionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using PersistentEmpiresLib;
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
            this._dataSource = new PEFactionMembersVM("Vote A Player Lord", "Vote", () => {
                this.CloseManagementMenu();
            },
            (PEFactionMemberItemVM selectedMember) => {
                this._factionPollComponent.RequestLordPlayerPoll(selectedMember.Peer);
                this.CloseManagementMenu();
            },
            () => {
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

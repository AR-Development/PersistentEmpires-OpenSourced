using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpires.Views.ViewsVM.FactionManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresLib;
namespace PersistentEmpires.Views.Views.FactionManagement
{
    public class PEFactionAssignMarshall : PEMenuItem
    {
        public PEFactionAssignMarshall() : base("PEFactionMembers")
        {
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._factionManagementComponent.OnAssignMarshallClick += this.OnOpen;
            this._dataSource = new PEFactionMembersVM("Assign Marshall", "Assign A Marshall", () => {
                this.CloseManagementMenu();
                this._factionManagementComponent.OnFactionManagementClickHandler();
            },
            (PEFactionMemberItemVM selectedMember) => {
                int factionIndex = selectedMember.Peer.GetComponent<PersistentEmpireRepresentative>().GetFactionIndex();
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new FactionAssignMarshall(selectedMember.Peer));
                GameNetwork.EndModuleEventAsClient();
                selectedMember.IsGranted = !selectedMember.IsGranted;

                // this._factionsBehavior.AssignMarshall(selectedMember.Peer, factionIndex);
                // this.CloseManagementMenu();
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
            PEFactionMembersVM dataSource = (PEFactionMembersVM)this._dataSource;
            

            dataSource.RefreshItems(faction, true);
            foreach (var member in dataSource.Members)
            {
                if (faction.marshalls.Contains(member.Peer.VirtualPlayer.Id.ToString()))
                {
                    member.IsGranted = true;
                }
            }
            base.OnOpen();
        }
    }
}

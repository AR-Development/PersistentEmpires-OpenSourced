using PersistentEmpires.Views.ViewsVM.FactionManagement;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresLib.Helpers;

namespace PersistentEmpires.Views.Views.FactionManagement
{
    public class PEFactionDoorKeys : PEMenuItem
    {
        public PEFactionDoorKeys() : base("PEFactionMembers")
        {

        }
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._factionManagementComponent.OnManageDoorKeysClick += this.OnOpen;
            this._factionsBehavior.OnFactionKeyFetched += this.OnKeyFetched;
            this._dataSource = new PEFactionMembersVM("Door Key Management", "Give/Take Key", () =>
            {
                this.CloseManagementMenu();
                this._factionManagementComponent.OnFactionManagementClickHandler();
            },
            (PEFactionMemberItemVM selectedMember) =>
            {
                this._factionsBehavior.RequestDoorKeyForUser(selectedMember.Peer);
                selectedMember.IsGranted = !selectedMember.IsGranted;
                // this.CloseManagementMenu();
            },
            () =>
            {
                this.CloseManagementMenu();
            });

        }

        private void OnKeyFetched(int factionIndex, string playerId, int keyType)
        {
            if (keyType != 0) return;
            if (this.IsActive == false) return;

            foreach (PEFactionMemberItemVM item in ((PEFactionMembersVM)this._dataSource).Members)
            {
                item.IsGranted = item.Peer.VirtualPlayer.ToPlayerId() == playerId;
            }
        }

        protected override void OnOpen()
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;
            Faction faction = persistentEmpireRepresentative.GetFaction();
            PEFactionMembersVM dataSource = (PEFactionMembersVM)this._dataSource;
            dataSource.RefreshItems(faction, true);
            base.OnOpen();
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestFactionKeys(0));
            GameNetwork.EndModuleEventAsClient();
        }
    }
}

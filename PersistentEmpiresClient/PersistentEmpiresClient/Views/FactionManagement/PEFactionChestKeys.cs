using PersistentEmpires.Views.ViewsVM.FactionManagement;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresLib.Helpers;

namespace PersistentEmpires.Views.Views.FactionManagement
{
    public class PEFactionChestKeys : PEMenuItem
    {
        public PEFactionChestKeys() : base("PEFactionMembers")
        {

        }
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._factionManagementComponent.OnManageChestKeyClick += this.OnOpen;
            this._factionsBehavior.OnFactionKeyFetched += this.OnKeyFetched;
            this._dataSource = new PEFactionMembersVM("Chest Key Management", "Give/Take Key", () =>
            {
                this.CloseManagementMenu();
                this._factionManagementComponent.OnFactionManagementClickHandler();
            },
            (PEFactionMemberItemVM selectedMember) =>
            {
                this._factionsBehavior.RequestChestKeyForUser(selectedMember.Peer);
                selectedMember.IsGranted = !selectedMember.IsGranted;
            },
            () =>
            {
                this.CloseManagementMenu();
            });
        }

        private void OnKeyFetched(int factionIndex, string playerId, int keyType)
        {
            if (keyType != 1) return;
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
            GameNetwork.WriteMessage(new RequestFactionKeys(1));
            GameNetwork.EndModuleEventAsClient();
        }
    }
}

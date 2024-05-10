using PersistentEmpires.Views.ViewsVM.FactionManagement;
using PersistentEmpires.Views.ViewsVM.PETabMenu;
using PersistentEmpiresLib;
using TaleWorlds.MountAndBlade;
namespace PersistentEmpires.Views.Views.FactionManagement
{
    public class PEFactionDiplomacy : PEMenuItem
    {
        public PEFactionDiplomacy() : base("PEFactionDiplomacy")
        {

        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._factionManagementComponent.OnDiplomacyMenuClick += this.OnOpen;
            this._factionsBehavior.OnFactionMakePeace += this.OnFactionDeclaredAnything;
            this._factionsBehavior.OnFactionDeclaredWar += this.OnFactionDeclaredAnything;

            this._dataSource = new PEFactionDiplomacyVM(this._factionsBehavior.Factions,
                (TabFactionVM faction) =>
                {
                    this._factionsBehavior.RequestDeclareWar(this._factionsBehavior.Factions[faction.FactionIndex], faction.FactionIndex);
                    //Declare ware
                },
                (TabFactionVM faction) =>
                {
                    this._factionsBehavior.RequestMakePeace(this._factionsBehavior.Factions[faction.FactionIndex], faction.FactionIndex);
                    // Make peace
                });

        }

        private void OnFactionDeclaredAnything(int declarer, int declaredTo)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;
            if (declarer == persistentEmpireRepresentative.GetFactionIndex())
            {
                ((PEFactionDiplomacyVM)this._dataSource).RefreshValues(this._factionsBehavior.Factions, persistentEmpireRepresentative.GetFaction(), persistentEmpireRepresentative.GetFactionIndex());
            }
        }

        protected override void OnOpen()
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            ((PEFactionDiplomacyVM)this._dataSource).RefreshValues(this._factionsBehavior.Factions, persistentEmpireRepresentative.GetFaction(), persistentEmpireRepresentative.GetFactionIndex());
            base.OnOpen();
        }

    }
}

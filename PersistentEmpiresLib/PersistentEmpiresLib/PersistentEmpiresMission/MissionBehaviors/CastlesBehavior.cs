using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class CastlesBehavior : MissionNetwork
    {

        public delegate void CastleAddedHandler(int factionIndex, PE_CastleBanner castle);
        public delegate void CastleUpdatedHandler(PE_CastleBanner castle);

        public event CastleAddedHandler OnCastleAdded;
        public event CastleUpdatedHandler OnCastleUpdated;

        public Dictionary<int, PE_CastleBanner> castles = new Dictionary<int, PE_CastleBanner>();
        private FactionsBehavior factionsBehavior;

        public override void AfterStart()
        {
            base.AfterStart();


            List<GameEntity> gameEntities = new List<GameEntity>();
            base.Mission.Scene.GetAllEntitiesWithScriptComponent<PE_CastleBanner>(ref gameEntities);
            IEnumerable<DBCastle> dbCastles = SaveSystemBehavior.HandleGetCastles();
            Dictionary<int, DBCastle> savedCastles = new Dictionary<int, DBCastle>();
            if (dbCastles != null)
            {
                foreach (DBCastle castle in dbCastles)
                {
                    savedCastles[castle.CastleIndex] = castle;
                }
            }
            foreach (GameEntity gameEntity in gameEntities)
            {
                PE_CastleBanner cb = gameEntity.GetFirstScriptOfType<PE_CastleBanner>();
                if (savedCastles.ContainsKey(cb.CastleIndex)) cb.FactionIndex = savedCastles[cb.CastleIndex].FactionIndex;
                this.AddCastle(cb);
            }

            this.factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            this.factionsBehavior.OnFactionAdded += this.HandleAddFaction;
            foreach (int key in this.factionsBehavior.Factions.Keys) this.HandleAddFaction(this.factionsBehavior.Factions[key], key);
        }

        protected void HandleAddFaction(Faction faction, int factionIndex)
        {
            // this.ReloadCastleBanner(factionIndex);
        }
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
        }
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<UpdateCastle>(this.HandleUpdateCastle);
            }
        }
        public void AddCastle(PE_CastleBanner castleBanner)
        {
            Debug.Print("Adding new Castle " + castleBanner.CastleName);
            castles[castleBanner.CastleIndex] = castleBanner;
            if (this.OnCastleAdded != null)
                this.OnCastleAdded(castleBanner.FactionIndex, castleBanner);

        }

        public void UpdateCastle(int castleIndex, int newFactionIndex)
        {
            this.castles[castleIndex].FactionIndex = newFactionIndex;
            if (GameNetwork.IsClient)
            {
                this.castles[castleIndex].UpdateBannerFromFaction();
            }
            if (GameNetwork.IsServer)
            {
                SaveSystemBehavior.HandleCreateOrSaveCastle(castleIndex, newFactionIndex);
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new UpdateCastle(this.castles[castleIndex]));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
            if (this.OnCastleUpdated != null)
                this.OnCastleUpdated(this.castles[castleIndex]);
        }

        public void ReloadCastleBanner(int factionIndex)
        {
            if (GameNetwork.IsServer) return;
            IEnumerable<PE_CastleBanner> cbs = castles.Values.Where((PE_CastleBanner banner) => banner.FactionIndex == factionIndex);
            foreach (PE_CastleBanner cb in cbs)
            {
                cb.UpdateBannerFromFaction();
            }
        }

        protected void HandleUpdateCastle(UpdateCastle packet)
        {
            if (this.castles.ContainsKey(packet.CastleBanner.FactionIndex) == false) return;
            this.castles[packet.CastleBanner.CastleIndex].FactionIndex = packet.CastleBanner.FactionIndex;
            this.castles[packet.CastleBanner.CastleIndex].UpdateBannerFromFaction();
            if (this.OnCastleUpdated != null)
                this.OnCastleUpdated(this.castles[packet.CastleBanner.CastleIndex]);
        }
    }
}

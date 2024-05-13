using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_SpawnFrame : SynchedMissionObject
    {
        public int CastleIndex = -1;
        public bool SpawnFromCastle = false;
        public int FactionIndex = 0;

        public PE_CastleBanner GetCastleBanner()
        {
            if (!Mission.Current.GetMissionBehavior<CastlesBehavior>().castles.ContainsKey(this.CastleIndex)) return null;
            return Mission.Current.GetMissionBehavior<CastlesBehavior>().castles[this.CastleIndex];
        }

        public bool CanPeerSpawnHere(NetworkCommunicator peer)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            if (persistentEmpireRepresentative.GetFaction() == null)
            {
                return !this.SpawnFromCastle && (this.FactionIndex == 0 || this.FactionIndex == -1);
            }
            if (this.SpawnFromCastle && this.GetCastleBanner() != null)
            {
                return this.GetCastleBanner().FactionIndex == persistentEmpireRepresentative.GetFactionIndex();
            }

            return this.FactionIndex == persistentEmpireRepresentative.GetFactionIndex();
        }

        public PE_SpawnFrame()
        {

        }
    }
}

using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.SpawnBehavior
{
    public class PersistentEmpireSpawnFrameBehavior : SpawnFrameBehaviorBase
    {
        // Token: 0x06002549 RID: 9545 RVA: 0x0008B81D File Offset: 0x00089A1D
        public override MatrixFrame GetSpawnFrame(Team team, bool hasMount, bool isInitialSpawn)
        {
            return base.GetSpawnFrameFromSpawnPoints(this.SpawnPoints.ToList<GameEntity>(), null, hasMount);
        }
    }
}

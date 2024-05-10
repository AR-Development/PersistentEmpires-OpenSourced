using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    abstract public class PE_UsableFromDistance : UsableMissionObject
    {
        public float Distance = 3f;

        public bool IsUsable(Agent user)
        {
            float distance = base.GameEntity.GetGlobalFrame().origin.Distance(user.Position);
            return distance <= this.Distance;
        }
    }
}

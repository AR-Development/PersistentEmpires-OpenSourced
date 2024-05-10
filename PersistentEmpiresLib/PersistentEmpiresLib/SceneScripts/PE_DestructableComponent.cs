using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public abstract class PE_DestructableComponent : SynchedMissionObject
    {
        public float MaxHitPoint = 100f;
        protected float _hitPoint;

        public float HitPoint
        {
            get => this._hitPoint;
            set
            {
                if (!this._hitPoint.Equals(value))
                {
                    this._hitPoint = MathF.Max(value, 0f);
                }
            }
        }

        public abstract void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackBehavior);
        protected override abstract bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage);
    }
}

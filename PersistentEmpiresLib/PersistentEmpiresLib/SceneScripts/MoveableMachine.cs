using PersistentEmpiresLib.SceneScripts.Extensions;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public abstract class PE_MoveableMachine : UsableMachine, IRemoveable, IStray, IMoveable
    {
        public float MaxHitPoint = 100f;
        protected float _hitPoint;

        public UsableMachine GetAttachedObject() => this;

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

        public float AdvanceSpeed = 3f;
        public float RotationalSpeed = 1f;
        public float ElevationSpeed = 1f;
        public bool CanAdvance = true;
        public bool CanRotate = true;
        public bool CanElevate = true;
        public bool AlwaysAlignToTerritory = false;

        public bool IsMovingForward { get; set; }
        public bool IsMovingBackward { get; set; }
        public bool IsTurningRight { get; set; }
        public bool IsTurningLeft { get; set; }
        public bool IsMovingUp { get; set; }
        public bool IsMovingDown { get; set; }


        public float GetAdvanceSpeed()
        {
            return this.AdvanceSpeed;
        }

        public float GetRotationSpeed()
        {
            return this.RotationalSpeed;
        }
        public override void AddStuckMissile(GameEntity missileEntity)
        {
            if (base.GameEntity != null)
            {
                base.AddStuckMissile(missileEntity);
            }
        }
        public float GetElevationSpeed()
        {
            return this.ElevationSpeed;
        }

        public bool GetCanAdvance()
        {
            return this.CanAdvance;
        }

        public bool GetCanRotate()
        {
            return this.CanRotate;
        }

        public bool GetCanElevate()
        {
            return this.CanElevate;
        }

        public bool GetAlwaysAlignToTerritory()
        {
            return this.AlwaysAlignToTerritory;
        }

        /*public void SetFrameAfterTick(MatrixFrame frame)
        {
            this._setFrameAfterTick = frame;
            this._frameSetFlag = true;
        }*/

        protected override void OnInit()
        {
            base.OnInit();
            this.InitiateMoveSynch();
        }

        protected bool CanGoThere(MatrixFrame frame)
        {
            return !base.GameEntity.CheckPointWithOrientedBoundingBox(frame.origin);
        }
        protected override void OnTick(float dt)
        {
            if (base.GameEntity == null) return;
            base.OnTick(dt);
            if (GameNetwork.IsServer)
            {
                MatrixFrame frame = this.MoveObjectTick(dt);
                base.SetFrameSynched(ref frame);
            }
        }

        public void OnEntityRemove()
        {
            if (GameNetwork.IsServer)
            {
                // RemoveableChildrens.OnEntityRemove(base.GameEntity);
            }
        }

        abstract public void SetHitPoint(float hitPoint, Vec3 impactDirection);
        abstract protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage);
        abstract public bool IsStray();

        abstract public void ResetStrayDuration();

        public Agent GetPilotAgent()
        {
            return this.PilotAgent;
        }
    }
}

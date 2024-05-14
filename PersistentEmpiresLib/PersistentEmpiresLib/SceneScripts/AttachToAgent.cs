using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_AttachToAgent : PE_UsableFromDistance, IStray
    {
        // public override ScriptComponentBehavior.TickRequirement GetTickRequirement() => !this.GameEntity.IsVisibleIncludeParents() ? base.GetTickRequirement() : ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel;

        public bool AttachableToHorse = false;
        public string AttachableHorseType = "";
        public int StrayDurationSeconds = 7200;
        private long WillBeDeletedAt = 0;
        public string ParticleEffectOnDestroy = "";
        public string SoundEffectOnDestroy = "";

        public float MaxHitPoint = 500f;
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

        public Agent AttachedTo { get; private set; }

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            if (GameNetwork.IsClientOrReplay)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel;
            }
            else if (GameNetwork.IsServer && this.AttachedTo != null)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel;
            }
            return base.GetTickRequirement();
        }

        protected override void OnTick(float dt)
        {
            if (this.AttachedTo == null) return;
            if (!this.AttachedTo.IsActive())
            {
                this.DetachFromAgentAux();
                return;
            }
            GameEntity parentEntity = base.GameEntity.Parent;

            MatrixFrame frame = parentEntity.GetGlobalFrame();
            frame.rotation = this.AttachedTo.Frame.rotation;
            frame.Rotate(270f * (MBMath.PI / 180), Vec3.Up);
            parentEntity.SetGlobalFrame(frame);


            frame = parentEntity.GetGlobalFrame();
            Vec3 pointPos = base.GameEntity.GetGlobalFrame().origin;
            Vec3 agentPos = this.AttachedTo.Position;

            Vec3 moveVector = agentPos - pointPos;

            frame.origin += moveVector;
            Vec3 normalVector = new Vec3();
            float terrainZ = 0;
            base.Scene.GetTerrainHeightAndNormal(frame.origin.AsVec2, out terrainZ, out normalVector);

            if (frame.origin.z <= terrainZ)
            {
                frame.origin.z = terrainZ;
                frame.rotation.u = normalVector;
                frame.rotation.Orthonormalize();
            }
            else
            {
                frame.rotation.u = new Vec3(0, 0, 1);
                frame.rotation.Orthonormalize();
            }


            parentEntity.SetGlobalFrame(frame);
        }

        public void ResetStrayDuration()
        {
            this.WillBeDeletedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + this.StrayDurationSeconds;
        }

        protected override void OnInit()
        {
            base.OnInit();
            base.ActionMessage = new TextObject("Attach Object");
            TextObject descriptionMessage = new TextObject("Press {KEY} To Attach");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
            this.ResetStrayDuration();
            GameEntity parentEntity = base.GameEntity.Parent;
            SynchedMissionObject synchObject = parentEntity.GetFirstScriptOfType<SynchedMissionObject>();
            var prop = typeof(SynchedMissionObject).GetField("_initialSynchFlags", BindingFlags.NonPublic | BindingFlags.Instance);
            SynchedMissionObject.SynchFlags syncFlags = (SynchedMissionObject.SynchFlags)prop.GetValue(synchObject);
            syncFlags |= SynchFlags.SynchTransform;
            prop.SetValue(synchObject, syncFlags);
            base.IsInstantUse = true;
            this.HitPoint = this.MaxHitPoint;
        }
        public override bool IsDisabledForAgent(Agent agent)
        {
            return this.IsDeactivated || (this.IsDisabledForPlayers && !agent.IsAIControlled) || !agent.IsOnLand();
        }

        public void SetAttachedAgent(Agent attachedTo)
        {
            this.AttachToAgentAux(attachedTo);
        }

        public override void OnUse(Agent userAgent)
        {
            base.OnUse(userAgent);
            Debug.Print("[USING LOG] AGENT USING " + this.GetType().Name);
            if (this.AttachedTo == null)
            {
                if (this.AttachableToHorse)
                {
                    if (!userAgent.HasMount) return;
                    if (this.AttachableHorseType != "" && userAgent.MountAgent.Monster.StringId != this.AttachableHorseType) return;
                    this.AttachToAgentAux(userAgent.MountAgent);
                }
                else
                {
                    if (userAgent.HasMount) return;
                    this.AttachToAgentAux(userAgent);
                }
            }
            else if (this.AttachedTo == userAgent || (userAgent.MountAgent != null && this.AttachedTo == userAgent.MountAgent))
            {

                this.DetachFromAgentAux();
            }
        }

        private void DetachFromAgentAux()
        {
            this.AttachedTo = null;
        }

        private void AttachToAgentAux(Agent attachableAgent)
        {
            this.ResetStrayDuration();
            this.AttachedTo = attachableAgent;
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Attach";
        }

        public bool IsStray()
        {
            if (this.AttachedTo != null) return false;
            if (this.WillBeDeletedAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return true;
            return false;
        }

        public void SetHitPoint(float hitPoint, Vec3 impactDirection)
        {
            this.HitPoint = hitPoint;
            MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
            /*if (this.HitPoint > this.MaxHitPoint) this.HitPoint = this.MaxHitPoint;
            if (this.HitPoint < 0) this.HitPoint = 0;

            if (this.HitPoint == 0)
            {
                if (this.AttachedTo != null)
                {
                    this.AttachedTo = null;
                }
                if (this.ParticleEffectOnDestroy != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(this.ParticleEffectOnDestroy), globalFrame);
                }
                if (this.SoundEffectOnDestroy != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(this.SoundEffectOnDestroy), globalFrame.origin, false, true, -1, -1);
                }
                if(base.GameEntity.Parent != null)
                {
                    base.GameEntity.Parent.Remove(0);
                }
                else
                {
                    base.GameEntity.Remove(0);
                }
            }*/
        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            reportDamage = true;
            MissionWeapon missionWeapon = weapon;
            WeaponComponentData currentUsageItem = missionWeapon.CurrentUsageItem;
            if (impactDirection == null) impactDirection = Vec3.Zero;
            this.SetHitPoint(this.HitPoint - damage, impactDirection);
            if (GameNetwork.IsServer)
            {
                LoggerHelper.LogAnAction(attackerAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerHitToDestructable, null, new object[] { this.GetType().Name });
            }

            return false;
        }
    }
}
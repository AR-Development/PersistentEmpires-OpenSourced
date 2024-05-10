using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.Objects.Siege;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_BatteringRam : SiegeWeapon, IPathHolder, IPrimarySiegeWeapon, IMoveableSiegeWeapon, ISpawnable
    {
        // Token: 0x17000801 RID: 2049
        // (get) Token: 0x06002B88 RID: 11144 RVA: 0x000A8A0A File Offset: 0x000A6C0A
        // (set) Token: 0x06002B89 RID: 11145 RVA: 0x000A8A12 File Offset: 0x000A6C12
        public SiegeWeaponMovementComponent MovementComponent { get; private set; }

        public override BattleSideEnum Side
        {
            get
            {
                return BattleSideEnum.None;
            }
        }

        // Token: 0x17000802 RID: 2050
        // (get) Token: 0x06002B8A RID: 11146 RVA: 0x000A8A1B File Offset: 0x000A6C1B
        public FormationAI.BehaviorSide WeaponSide
        {
            get
            {
                return this._weaponSide;
            }
        }

        // Token: 0x17000803 RID: 2051
        // (get) Token: 0x06002B8B RID: 11147 RVA: 0x000A8A23 File Offset: 0x000A6C23
        public string PathEntity
        {
            get
            {
                return this._pathEntityName;
            }
        }

        // Token: 0x17000804 RID: 2052
        // (get) Token: 0x06002B8C RID: 11148 RVA: 0x000A8A2B File Offset: 0x000A6C2B
        public bool EditorGhostEntityMove
        {
            get
            {
                return this.GhostEntityMove;
            }
        }

        // Token: 0x17000805 RID: 2053
        // (get) Token: 0x06002B8D RID: 11149 RVA: 0x000A8A33 File Offset: 0x000A6C33
        // (set) Token: 0x06002B8E RID: 11150 RVA: 0x000A8A3B File Offset: 0x000A6C3B
        public PE_BatteringRam.RamState State
        {
            get
            {
                return this._state;
            }
            set
            {
                if (this._state != value)
                {
                    this._state = value;
                }
            }
        }

        // Token: 0x17000806 RID: 2054
        // (get) Token: 0x06002B8F RID: 11151 RVA: 0x000A8A4D File Offset: 0x000A6C4D
        public MissionObject TargetCastlePosition
        {
            get
            {
                return this._gate;
            }
        }

        // Token: 0x06002B90 RID: 11152 RVA: 0x000A8A55 File Offset: 0x000A6C55
        public bool HasCompletedAction()
        {
            return this._gate == null || this._gate.IsDestroyed || (this._gate.State == PE_NativeGate.GateState.Open && this.HasArrivedAtTarget);
        }

        // Token: 0x17000807 RID: 2055
        // (get) Token: 0x06002B91 RID: 11153 RVA: 0x000A8A83 File Offset: 0x000A6C83
        public float SiegeWeaponPriority
        {
            get
            {
                return 25f;
            }
        }

        // Token: 0x17000808 RID: 2056
        // (get) Token: 0x06002B92 RID: 11154 RVA: 0x000A8A8A File Offset: 0x000A6C8A
        public int OverTheWallNavMeshID
        {
            get
            {
                return this.GateNavMeshId;
            }
        }

        // Token: 0x17000809 RID: 2057
        // (get) Token: 0x06002B93 RID: 11155 RVA: 0x000A8A92 File Offset: 0x000A6C92
        public bool HoldLadders
        {
            get
            {
                return !this.MovementComponent.HasArrivedAtTarget;
            }
        }

        // Token: 0x1700080A RID: 2058
        // (get) Token: 0x06002B94 RID: 11156 RVA: 0x000A8AA2 File Offset: 0x000A6CA2
        public bool SendLadders
        {
            get
            {
                return this.MovementComponent.HasArrivedAtTarget;
            }
        }

        // Token: 0x1700080B RID: 2059
        // (get) Token: 0x06002B95 RID: 11157 RVA: 0x000A8AAF File Offset: 0x000A6CAF
        // (set) Token: 0x06002B96 RID: 11158 RVA: 0x000A8AB8 File Offset: 0x000A6CB8
        public bool HasArrivedAtTarget
        {
            get
            {
                return this._hasArrivedAtTarget;
            }
            set
            {
                if (!GameNetwork.IsClientOrReplay)
                {
                    this.MovementComponent.SetDestinationNavMeshIdState(!value);
                }
                if (this._hasArrivedAtTarget != value)
                {
                    this._hasArrivedAtTarget = value;
                    if (GameNetwork.IsServerOrRecorder)
                    {
                        GameNetwork.BeginBroadcastModuleEvent();
                        GameNetwork.WriteMessage(new SetPE_BatteringRamHasArrivedAtTarget(base.Id));
                        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                        return;
                    }
                    if (GameNetwork.IsClientOrReplay)
                    {
                        this.MovementComponent.MoveToTargetAsClient();
                    }
                }
            }
        }

        // Token: 0x06002B97 RID: 11159 RVA: 0x000A8B22 File Offset: 0x000A6D22
        public override void Disable()
        {
            base.Disable();
            if (!GameNetwork.IsClientOrReplay)
            {
                if (this.DisabledNavMeshID != 0)
                {
                    base.Scene.SetAbilityOfFacesWithId(this.DisabledNavMeshID, true);
                }
                base.Scene.SetAbilityOfFacesWithId(this.DynamicNavmeshIdStart + 4, false);
            }
        }

        // Token: 0x06002B98 RID: 11160 RVA: 0x000A8B5F File Offset: 0x000A6D5F
        public override SiegeEngineType GetSiegeEngineType()
        {
            return DefaultSiegeEngineTypes.Ram;
        }

        // Token: 0x06002B99 RID: 11161 RVA: 0x000A8B68 File Offset: 0x000A6D68
        protected override void OnInit()
        {
            base.OnInit();
            DestructableComponent destructableComponent = base.GameEntity.GetScriptComponents<DestructableComponent>().FirstOrDefault<DestructableComponent>();
            if (destructableComponent != null)
            {
                destructableComponent.BattleSide = BattleSideEnum.None;
            }
            this._state = PE_BatteringRam.RamState.Stable;
            IEnumerable<GameEntity> enumerable = from ewgt in base.Scene.FindEntitiesWithTag(this._gateTag).ToList<GameEntity>()
                                                 where ewgt.HasScriptOfType<PE_NativeGate>()
                                                 select ewgt;
            if (!enumerable.IsEmpty<GameEntity>())
            {
                this._gate = enumerable.First<GameEntity>().GetFirstScriptOfType<PE_NativeGate>();
                this._gate.AttackerSiegeWeapon = this;
            }
            this.AddRegularMovementComponent();
            this._batteringRamBody = base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.HasTag("body"));
            this._batteringRamBodySkeleton = this._batteringRamBody.Skeleton;
            this._batteringRamBodySkeleton.SetAnimationAtChannel("batteringram_idle", 0, 1f, 0f, 0f);
            this._pullStandingPoints = new List<StandingPoint>();
            this._pullStandingPointLocalIKFrames = new List<MatrixFrame>();
            MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
            if (base.StandingPoints != null)
            {
                foreach (StandingPoint standingPoint in base.StandingPoints)
                {
                    standingPoint.AddComponent(new ResetAnimationOnStopUsageComponent(ActionIndexCache.act_none));
                    if (standingPoint.GameEntity.HasTag("pull"))
                    {
                        standingPoint.IsDeactivated = true;
                        this._pullStandingPoints.Add(standingPoint);
                        this._pullStandingPointLocalIKFrames.Add(standingPoint.GameEntity.GetGlobalFrame().TransformToLocal(globalFrame));
                        standingPoint.AddComponent(new ClearHandInverseKinematicsOnStopUsageComponent());
                    }
                }
            }
            string sideTag = this._sideTag;
            if (!(sideTag == "left"))
            {
                if (!(sideTag == "middle"))
                {
                    if (!(sideTag == "right"))
                    {
                        this._weaponSide = FormationAI.BehaviorSide.Middle;
                    }
                    else
                    {
                        this._weaponSide = FormationAI.BehaviorSide.Right;
                    }
                }
                else
                {
                    this._weaponSide = FormationAI.BehaviorSide.Middle;
                }
            }
            else
            {
                this._weaponSide = FormationAI.BehaviorSide.Left;
            }
            this._ditchFillDebris = base.Scene.FindEntitiesWithTag("ditch_filler").FirstOrDefault((GameEntity df) => df.HasTag(this._sideTag));
            base.SetScriptComponentToTick(this.GetTickRequirement());
            Mission.Current.AddToWeaponListForFriendlyFirePreventing(this);
        }

        // Token: 0x06002B9A RID: 11162 RVA: 0x000A8DD0 File Offset: 0x000A6FD0
        private void AddRegularMovementComponent()
        {
            this.MovementComponent = new SiegeWeaponMovementComponent
            {
                PathEntityName = this.PathEntity,
                MinSpeed = this.MinSpeed,
                MaxSpeed = this.MaxSpeed,
                MainObject = this,
                WheelDiameter = this.WheelDiameter,
                NavMeshIdToDisableOnDestination = this.NavMeshIdToDisableOnDestination,
                MovementSoundCodeID = SoundEvent.GetEventIdFromString("event:/mission/siege/batteringram/move"),
                GhostEntitySpeedMultiplier = this.GhostEntitySpeedMultiplier
            };
            base.AddComponent(this.MovementComponent);
        }

        // Token: 0x06002B9B RID: 11163 RVA: 0x000A8E54 File Offset: 0x000A7054
        protected override void OnDeploymentStateChanged(bool isDeployed)
        {
            base.OnDeploymentStateChanged(isDeployed);
            if (this._ditchFillDebris != null)
            {
                this._ditchFillDebris.SetVisibilityExcludeParents(isDeployed);
                if (!GameNetwork.IsClientOrReplay)
                {
                    if (isDeployed)
                    {
                        Mission.Current.Scene.SetAbilityOfFacesWithId(this._bridgeNavMeshID_1, true);
                        Mission.Current.Scene.SetAbilityOfFacesWithId(this._bridgeNavMeshID_2, true);
                        Mission.Current.Scene.SeparateFacesWithId(this._ditchNavMeshID_1, this._groundToBridgeNavMeshID_1);
                        Mission.Current.Scene.SeparateFacesWithId(this._ditchNavMeshID_2, this._groundToBridgeNavMeshID_2);
                        Mission.Current.Scene.MergeFacesWithId(this._bridgeNavMeshID_1, this._groundToBridgeNavMeshID_1, 0);
                        Mission.Current.Scene.MergeFacesWithId(this._bridgeNavMeshID_2, this._groundToBridgeNavMeshID_2, 0);
                        return;
                    }
                    Mission.Current.Scene.SeparateFacesWithId(this._bridgeNavMeshID_1, this._groundToBridgeNavMeshID_1);
                    Mission.Current.Scene.SeparateFacesWithId(this._bridgeNavMeshID_2, this._groundToBridgeNavMeshID_2);
                    Mission.Current.Scene.SetAbilityOfFacesWithId(this._bridgeNavMeshID_1, false);
                    Mission.Current.Scene.SetAbilityOfFacesWithId(this._bridgeNavMeshID_2, false);
                    Mission.Current.Scene.MergeFacesWithId(this._ditchNavMeshID_1, this._groundToBridgeNavMeshID_1, 0);
                    Mission.Current.Scene.MergeFacesWithId(this._ditchNavMeshID_2, this._groundToBridgeNavMeshID_2, 0);
                }
            }
        }

        // Token: 0x06002B9C RID: 11164 RVA: 0x000A8FCA File Offset: 0x000A71CA
        public MatrixFrame GetInitialFrame()
        {
            if (this.MovementComponent != null)
            {
                return this.MovementComponent.GetInitialFrame();
            }
            return base.GameEntity.GetGlobalFrame();
        }

        // Token: 0x06002B9D RID: 11165 RVA: 0x000A8FEB File Offset: 0x000A71EB
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            if (base.GameEntity.IsVisibleIncludeParents())
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel;
            }
            return base.GetTickRequirement();
        }

        // Token: 0x06002B9E RID: 11166 RVA: 0x000A900C File Offset: 0x000A720C
        protected override void OnTickParallel(float dt)
        {
            base.OnTickParallel(dt);
            if (!base.GameEntity.IsVisibleIncludeParents())
            {
                return;
            }
            this.MovementComponent.TickParallelManually(dt);
            MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
            for (int i = 0; i < this._pullStandingPoints.Count; i++)
            {
                StandingPoint standingPoint = this._pullStandingPoints[i];
                if (standingPoint.HasUser)
                {
                    if (standingPoint.UserAgent.IsInBeingStruckAction)
                    {
                        standingPoint.UserAgent.ClearHandInverseKinematics();
                    }
                    else
                    {
                        Agent userAgent = standingPoint.UserAgent;
                        MatrixFrame matrixFrame = this._pullStandingPointLocalIKFrames[i];
                        userAgent.SetHandInverseKinematicsFrameForMissionObjectUsage(matrixFrame, globalFrame, 0f);
                    }
                }
            }
            if (this.MovementComponent.HasArrivedAtTarget && !this.IsDeactivated)
            {
                int userCountNotInStruckAction = base.UserCountNotInStruckAction;
                if (userCountNotInStruckAction > 0)
                {
                    float animationParameterAtChannel = this._batteringRamBodySkeleton.GetAnimationParameterAtChannel(0);
                    this.UpdateHitAnimationWithProgress((userCountNotInStruckAction - 1) / 2, animationParameterAtChannel);
                }
            }
        }

        // Token: 0x06002B9F RID: 11167 RVA: 0x000A90EC File Offset: 0x000A72EC
        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (!base.GameEntity.IsVisibleIncludeParents())
            {
                return;
            }
            if (!GameNetwork.IsClientOrReplay)
            {
                if (this.MovementComponent.HasArrivedAtTarget && !this.HasArrivedAtTarget)
                {
                    this.HasArrivedAtTarget = true;
                    foreach (StandingPoint standingPoint in base.StandingPoints)
                    {
                        standingPoint.SetIsDeactivatedSynched(standingPoint.GameEntity.HasTag("move"));
                    }
                    if (this.DisabledNavMeshID != 0)
                    {
                        base.GameEntity.Scene.SetAbilityOfFacesWithId(this.DisabledNavMeshID, false);
                    }
                }
                if (this.MovementComponent.HasArrivedAtTarget)
                {
                    if (this._gate == null || this._gate.IsDestroyed || this._gate.IsGateOpen)
                    {
                        if (!this._isAllStandingPointsDisabled)
                        {
                            foreach (StandingPoint standingPoint2 in base.StandingPoints)
                            {
                                standingPoint2.SetIsDeactivatedSynched(true);
                            }
                            this._isAllStandingPointsDisabled = true;
                            return;
                        }
                    }
                    else
                    {
                        if (this._isAllStandingPointsDisabled && !this.IsDeactivated)
                        {
                            foreach (StandingPoint standingPoint3 in base.StandingPoints)
                            {
                                standingPoint3.SetIsDeactivatedSynched(false);
                            }
                            this._isAllStandingPointsDisabled = false;
                        }
                        int userCountNotInStruckAction = base.UserCountNotInStruckAction;
                        switch (this.State)
                        {
                            case PE_BatteringRam.RamState.Stable:
                                if (userCountNotInStruckAction > 0)
                                {
                                    this.State = PE_BatteringRam.RamState.Hitting;
                                    this._usedPower = userCountNotInStruckAction;
                                    this._storedPower = 0f;
                                    this.StartHitAnimationWithProgress((userCountNotInStruckAction - 1) / 2, 0f);
                                    return;
                                }
                                break;
                            case PE_BatteringRam.RamState.Hitting:
                                {
                                    if (userCountNotInStruckAction <= 0 || this._gate == null || this._gate.IsGateOpen)
                                    {
                                        this._batteringRamBody.GetFirstScriptOfType<SynchedMissionObject>().SetAnimationAtChannelSynched("batteringram_idle", 0, 1f);
                                        this.State = PE_BatteringRam.RamState.Stable;
                                        return;
                                    }
                                    int num = (userCountNotInStruckAction - 1) / 2;
                                    float animationParameterAtChannel = this._batteringRamBodySkeleton.GetAnimationParameterAtChannel(0);
                                    if ((this._usedPower - 1) / 2 != num)
                                    {
                                        this.StartHitAnimationWithProgress(num, animationParameterAtChannel);
                                    }
                                    this._usedPower = userCountNotInStruckAction;
                                    this._storedPower += (float)this._usedPower * dt;
                                    float num2 = ((num == 3) ? 0.53f : ((num == 2) ? 0.6f : 0.61f));
                                    string text = ((num == 3) ? "batteringram_fire" : ((num == 2) ? "batteringram_fire_weak" : "batteringram_fire_weakest"));
                                    if (animationParameterAtChannel >= num2)
                                    {
                                        MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
                                        float num3 = this._storedPower * this.DamageMultiplier;
                                        num3 /= animationParameterAtChannel * MBAnimation.GetAnimationDuration(text);
                                        Agent attackerAgent = null;
                                        foreach (StandingPoint standingPoint2 in base.StandingPoints)
                                        {
                                            if (standingPoint2.UserAgent != null)
                                            {
                                                attackerAgent = standingPoint2.UserAgent;
                                                break;
                                            }
                                        }

                                        this._gate.GetPEDestructable().TriggerOnHit(attackerAgent, (int)num3, globalFrame.origin, globalFrame.rotation.f, MissionWeapon.Invalid, this);
                                        this.State = PE_BatteringRam.RamState.AfterHit;
                                        return;
                                    }
                                    break;
                                }
                            case PE_BatteringRam.RamState.AfterHit:
                                if (this._batteringRamBodySkeleton.GetAnimationParameterAtChannel(0) > 0.999f)
                                {
                                    this.State = PE_BatteringRam.RamState.Stable;
                                }
                                break;
                            default:
                                return;
                        }
                    }
                }
            }
        }

        // Token: 0x06002BA0 RID: 11168 RVA: 0x000A9424 File Offset: 0x000A7624
        private void StartHitAnimationWithProgress(int powerStage, float progress)
        {
            string text = ((powerStage == 2) ? "batteringram_fire" : ((powerStage == 1) ? "batteringram_fire_weak" : "batteringram_fire_weakest"));
            this._batteringRamBody.GetFirstScriptOfType<SynchedMissionObject>().SetAnimationAtChannelSynched(text, 0, 1f);
            if (progress > 0f)
            {
                this._batteringRamBody.GetFirstScriptOfType<SynchedMissionObject>().SetAnimationChannelParameterSynched(0, progress);
            }
            foreach (StandingPoint standingPoint in base.StandingPoints)
            {
                if (standingPoint.HasUser && standingPoint.GameEntity.HasTag("pull"))
                {
                    ActionIndexCache actionCodeForStandingPoint = this.GetActionCodeForStandingPoint(standingPoint, powerStage);
                    if (!standingPoint.UserAgent.SetActionChannel(1, actionCodeForStandingPoint, false, 0UL, 0f, 1f, -0.2f, 0.4f, progress, false, -0.2f, 0, true) && standingPoint.UserAgent.Controller == Agent.ControllerType.AI)
                    {
                        standingPoint.UserAgent.StopUsingGameObject(false, Agent.StopUsingGameObjectFlags.AutoAttachAfterStoppingUsingGameObject);
                    }
                }
            }
        }

        // Token: 0x06002BA1 RID: 11169 RVA: 0x000A952C File Offset: 0x000A772C
        private void UpdateHitAnimationWithProgress(int powerStage, float progress)
        {
            foreach (StandingPoint standingPoint in base.StandingPoints)
            {
                if (standingPoint.HasUser && standingPoint.GameEntity.HasTag("pull"))
                {
                    ActionIndexCache actionCodeForStandingPoint = this.GetActionCodeForStandingPoint(standingPoint, powerStage);
                    if (standingPoint.UserAgent.GetCurrentActionValue(1) == actionCodeForStandingPoint)
                    {
                        standingPoint.UserAgent.SetCurrentActionProgress(1, progress);
                    }
                }
            }
        }

        // Token: 0x06002BA2 RID: 11170 RVA: 0x000A95BC File Offset: 0x000A77BC
        private ActionIndexCache GetActionCodeForStandingPoint(StandingPoint standingPoint, int powerStage)
        {
            bool flag = standingPoint.GameEntity.HasTag("right");
            ActionIndexCache actionIndexCache = ActionIndexCache.act_none;
            switch (powerStage)
            {
                case 0:
                    actionIndexCache = (flag ? PE_BatteringRam.act_usage_batteringram_left_slowest : PE_BatteringRam.act_usage_batteringram_right_slowest);
                    break;
                case 1:
                    actionIndexCache = (flag ? PE_BatteringRam.act_usage_batteringram_left_slower : PE_BatteringRam.act_usage_batteringram_right_slower);
                    break;
                case 2:
                    actionIndexCache = (flag ? PE_BatteringRam.act_usage_batteringram_left : PE_BatteringRam.act_usage_batteringram_right);
                    break;
                default:
                    Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Objects\\Siege\\PE_BatteringRam.cs", "GetActionCodeForStandingPoint", 590);
                    break;
            }
            return actionIndexCache;
        }

        // Token: 0x06002BA4 RID: 11172 RVA: 0x000A964C File Offset: 0x000A784C
        protected override void OnMissionReset()
        {
            base.OnMissionReset();
            this._state = PE_BatteringRam.RamState.Stable;
            this._hasArrivedAtTarget = false;
            this._batteringRamBodySkeleton.SetAnimationAtChannel("batteringram_idle", 0, 1f, 0f, 0f);
            foreach (StandingPoint standingPoint in base.StandingPoints)
            {
                standingPoint.IsDeactivated = !standingPoint.GameEntity.HasTag("move");
            }
        }

        // Token: 0x06002BA5 RID: 11173 RVA: 0x000A96E4 File Offset: 0x000A78E4
        public override void WriteToNetwork()
        {
            base.WriteToNetwork();
            GameNetworkMessage.WriteBoolToPacket(this.HasArrivedAtTarget);
            GameNetworkMessage.WriteIntToPacket((int)this.State, CompressionMission.BatteringRamStateCompressionInfo);
            GameNetworkMessage.WriteFloatToPacket(this.MovementComponent.GetTotalDistanceTraveledForPathTracker(), CompressionBasic.PositionCompressionInfo);
        }

        // Token: 0x1700080C RID: 2060
        // (get) Token: 0x06002BA6 RID: 11174 RVA: 0x000A971C File Offset: 0x000A791C
        public override bool IsDeactivated
        {
            get
            {
                return this._gate == null || this._gate.IsDestroyed || (this._gate.State == PE_NativeGate.GateState.Open && this.HasArrivedAtTarget) || base.IsDeactivated;
            }
        }

        // Token: 0x06002BA7 RID: 11175 RVA: 0x000A9750 File Offset: 0x000A7950
        public void HighlightPath()
        {
            this.MovementComponent.HighlightPath();
        }

        // Token: 0x06002BA8 RID: 11176 RVA: 0x000A9760 File Offset: 0x000A7960
        public void SwitchGhostEntityMovementMode(bool isGhostEnabled)
        {
            if (isGhostEnabled)
            {
                if (!this._isGhostMovementOn)
                {
                    base.RemoveComponent(this.MovementComponent);
                    this.SetUpGhostEntity();
                    this.GhostEntityMove = true;
                    SiegeWeaponMovementComponent component = base.GetComponent<SiegeWeaponMovementComponent>();
                    component.GhostEntitySpeedMultiplier *= 3f;
                    component.SetGhostVisibility(true);
                }
                this._isGhostMovementOn = true;
                return;
            }
            if (this._isGhostMovementOn)
            {
                base.RemoveComponent(this.MovementComponent);
                PathLastNodeFixer component2 = base.GetComponent<PathLastNodeFixer>();
                base.RemoveComponent(component2);
                this.AddRegularMovementComponent();
                this.MovementComponent.SetGhostVisibility(false);
            }
            this._isGhostMovementOn = false;
        }

        // Token: 0x06002BA9 RID: 11177 RVA: 0x000A97F4 File Offset: 0x000A79F4
        private void SetUpGhostEntity()
        {
            PathLastNodeFixer pathLastNodeFixer = new PathLastNodeFixer
            {
                PathHolder = this
            };
            base.AddComponent(pathLastNodeFixer);
            this.MovementComponent = new SiegeWeaponMovementComponent
            {
                PathEntityName = this.PathEntity,
                MainObject = this,
                GhostEntitySpeedMultiplier = this.GhostEntitySpeedMultiplier
            };
            base.AddComponent(this.MovementComponent);
            this.MovementComponent.SetupGhostEntity();
        }

        // Token: 0x06002BAA RID: 11178 RVA: 0x000A9856 File Offset: 0x000A7A56
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return new TextObject("{=MaBSSg7I}Battering Ram", null).ToString();
        }

        // Token: 0x06002BAB RID: 11179 RVA: 0x000A9868 File Offset: 0x000A7A68
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            TextObject textObject = (usableGameObject.GameEntity.HasTag("pull") ? new TextObject("{=1cnJtNTt}{KEY} Pull", null) : new TextObject("{=rwZAZSvX}{KEY} Move", null));
            textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            return textObject;
        }

        // Token: 0x06002BAC RID: 11180 RVA: 0x000A98BC File Offset: 0x000A7ABC
        public override OrderType GetOrder(BattleSideEnum side)
        {
            if (base.IsDestroyed)
            {
                return OrderType.None;
            }
            if (side != BattleSideEnum.Attacker)
            {
                return OrderType.AttackEntity;
            }
            if (!this.HasCompletedAction())
            {
                return OrderType.FollowEntity;
            }
            return OrderType.Use;
        }

        // Token: 0x06002BAD RID: 11181 RVA: 0x000A98DC File Offset: 0x000A7ADC
        public override TargetFlags GetTargetFlags()
        {
            TargetFlags targetFlags = TargetFlags.None;
            if (base.UserCountNotInStruckAction > 0)
            {
                targetFlags |= TargetFlags.IsMoving;
            }
            targetFlags |= TargetFlags.IsSiegeEngine;
            targetFlags |= TargetFlags.IsAttacker;
            if (this.Side == BattleSideEnum.Attacker && DebugSiegeBehavior.DebugDefendState == DebugSiegeBehavior.DebugStateDefender.DebugDefendersToRam)
            {
                targetFlags |= TargetFlags.DebugThreat;
            }
            if (this.HasCompletedAction() || base.IsDestroyed || this.IsDeactivated)
            {
                targetFlags |= TargetFlags.NotAThreat;
            }
            return targetFlags;
        }

        // Token: 0x06002BAE RID: 11182 RVA: 0x000A9938 File Offset: 0x000A7B38
        public override float GetTargetValue(List<Vec3> weaponPos)
        {
            return 300f * base.GetUserMultiplierOfWeapon() * this.GetDistanceMultiplierOfWeapon(weaponPos[0]) * base.GetHitPointMultiplierOfWeapon();
        }

        // Token: 0x06002BAF RID: 11183 RVA: 0x000A995C File Offset: 0x000A7B5C
        protected override float GetDistanceMultiplierOfWeapon(Vec3 weaponPos)
        {
            float minimumDistanceBetweenPositions = this.GetMinimumDistanceBetweenPositions(weaponPos);
            if (minimumDistanceBetweenPositions < 100f)
            {
                return 1f;
            }
            if (minimumDistanceBetweenPositions < 625f)
            {
                return 0.8f;
            }
            return 0.6f;
        }

        // Token: 0x06002BB0 RID: 11184 RVA: 0x000A9992 File Offset: 0x000A7B92
        public void SetSpawnedFromSpawner()
        {
            this._spawnedFromSpawner = true;
        }

        // Token: 0x06002BB1 RID: 11185 RVA: 0x000A999C File Offset: 0x000A7B9C
        public void AssignParametersFromSpawner(string gateTag, string sideTag, int bridgeNavMeshID_1, int bridgeNavMeshID_2, int ditchNavMeshID_1, int ditchNavMeshID_2, int groundToBridgeNavMeshID_1, int groundToBridgeNavMeshID_2, string pathEntityName)
        {
            this._gateTag = gateTag;
            this._sideTag = sideTag;
            this._bridgeNavMeshID_1 = bridgeNavMeshID_1;
            this._bridgeNavMeshID_2 = bridgeNavMeshID_2;
            this._ditchNavMeshID_1 = ditchNavMeshID_1;
            this._ditchNavMeshID_2 = ditchNavMeshID_2;
            this._groundToBridgeNavMeshID_1 = groundToBridgeNavMeshID_1;
            this._groundToBridgeNavMeshID_2 = groundToBridgeNavMeshID_2;
            this._pathEntityName = pathEntityName;
        }

        // Token: 0x06002BB2 RID: 11186 RVA: 0x000A99EE File Offset: 0x000A7BEE
        public bool GetNavmeshFaceIds(out List<int> navmeshFaceIds)
        {
            navmeshFaceIds = null;
            return false;
        }

        // Token: 0x06002BB3 RID: 11187 RVA: 0x000A99F4 File Offset: 0x000A7BF4
        public override void OnAfterReadFromNetwork(ValueTuple<BaseSynchedMissionObjectReadableRecord, ISynchedMissionObjectReadableRecord> synchedMissionObjectReadableRecord)
        {
            base.OnAfterReadFromNetwork(synchedMissionObjectReadableRecord);
            PE_BatteringRam.BatteringRamRecord batteringRamRecord = (PE_BatteringRam.BatteringRamRecord)synchedMissionObjectReadableRecord.Item2;
            this.HasArrivedAtTarget = batteringRamRecord.HasArrivedAtTarget;
            this._state = (PE_BatteringRam.RamState)batteringRamRecord.State;
            float num = batteringRamRecord.TotalDistanceTraveled;
            num += 0.05f;
            this.MovementComponent.SetTotalDistanceTraveledForPathTracker(num);
            this.MovementComponent.SetTargetFrameForPathTracker();
        }

        // Token: 0x04001108 RID: 4360
        private static readonly ActionIndexCache act_usage_batteringram_left = ActionIndexCache.Create("act_usage_batteringram_left");

        // Token: 0x04001109 RID: 4361
        private static readonly ActionIndexCache act_usage_batteringram_left_slower = ActionIndexCache.Create("act_usage_batteringram_left_slower");

        // Token: 0x0400110A RID: 4362
        private static readonly ActionIndexCache act_usage_batteringram_left_slowest = ActionIndexCache.Create("act_usage_batteringram_left_slowest");

        // Token: 0x0400110B RID: 4363
        private static readonly ActionIndexCache act_usage_batteringram_right = ActionIndexCache.Create("act_usage_batteringram_right");

        // Token: 0x0400110C RID: 4364
        private static readonly ActionIndexCache act_usage_batteringram_right_slower = ActionIndexCache.Create("act_usage_batteringram_right_slower");

        // Token: 0x0400110D RID: 4365
        private static readonly ActionIndexCache act_usage_batteringram_right_slowest = ActionIndexCache.Create("act_usage_batteringram_right_slowest");

        // Token: 0x0400110F RID: 4367
        private string _pathEntityName = "Path";

        // Token: 0x04001110 RID: 4368
        private const string PullStandingPointTag = "pull";

        // Token: 0x04001111 RID: 4369
        private const string RightStandingPointTag = "right";

        // Token: 0x04001112 RID: 4370
        private const string LeftStandingPointTag = "left";

        // Token: 0x04001113 RID: 4371
        private const string IdleAnimation = "batteringram_idle";

        // Token: 0x04001114 RID: 4372
        private const string KnockAnimation = "batteringram_fire";

        // Token: 0x04001115 RID: 4373
        private const string KnockSlowerAnimation = "batteringram_fire_weak";

        // Token: 0x04001116 RID: 4374
        private const string KnockSlowestAnimation = "batteringram_fire_weakest";

        // Token: 0x04001117 RID: 4375
        private const float KnockAnimationHitProgress = 0.53f;

        // Token: 0x04001118 RID: 4376
        private const float KnockSlowerAnimationHitProgress = 0.6f;

        // Token: 0x04001119 RID: 4377
        private const float KnockSlowestAnimationHitProgress = 0.61f;

        // Token: 0x0400111A RID: 4378
        private const string RoofTag = "roof";

        // Token: 0x0400111B RID: 4379
        private string _gateTag = "gate";

        // Token: 0x0400111C RID: 4380
        public bool GhostEntityMove = true;

        // Token: 0x0400111D RID: 4381
        public float GhostEntitySpeedMultiplier = 1f;

        // Token: 0x0400111E RID: 4382
        private string _sideTag;

        // Token: 0x0400111F RID: 4383
        private FormationAI.BehaviorSide _weaponSide;

        // Token: 0x04001120 RID: 4384
        public float WheelDiameter = 1.3f;

        // Token: 0x04001121 RID: 4385
        public int GateNavMeshId = 7;

        // Token: 0x04001122 RID: 4386
        public int DisabledNavMeshID = 8;

        // Token: 0x04001123 RID: 4387
        private int _bridgeNavMeshID_1 = 8;

        // Token: 0x04001124 RID: 4388
        private int _bridgeNavMeshID_2 = 8;

        // Token: 0x04001125 RID: 4389
        private int _ditchNavMeshID_1 = 9;

        // Token: 0x04001126 RID: 4390
        private int _ditchNavMeshID_2 = 10;

        // Token: 0x04001127 RID: 4391
        private int _groundToBridgeNavMeshID_1 = 12;

        // Token: 0x04001128 RID: 4392
        private int _groundToBridgeNavMeshID_2 = 13;

        // Token: 0x04001129 RID: 4393
        public int NavMeshIdToDisableOnDestination = -1;

        // Token: 0x0400112A RID: 4394
        public float MinSpeed = 0.5f;

        // Token: 0x0400112B RID: 4395
        public float MaxSpeed = 1f;

        // Token: 0x0400112C RID: 4396
        public float DamageMultiplier = 10f;

        // Token: 0x0400112D RID: 4397
        private int _usedPower;

        // Token: 0x0400112E RID: 4398
        private float _storedPower;

        // Token: 0x0400112F RID: 4399
        private List<StandingPoint> _pullStandingPoints;

        // Token: 0x04001130 RID: 4400
        private List<MatrixFrame> _pullStandingPointLocalIKFrames;

        // Token: 0x04001131 RID: 4401
        private GameEntity _ditchFillDebris;

        // Token: 0x04001132 RID: 4402
        private GameEntity _batteringRamBody;

        // Token: 0x04001133 RID: 4403
        private Skeleton _batteringRamBodySkeleton;

        // Token: 0x04001134 RID: 4404
        private bool _isGhostMovementOn;

        // Token: 0x04001135 RID: 4405
        private bool _isAllStandingPointsDisabled;

        // Token: 0x04001136 RID: 4406
        private PE_BatteringRam.RamState _state;

        // Token: 0x04001137 RID: 4407
        private PE_NativeGate _gate;

        // Token: 0x04001138 RID: 4408
        private bool _hasArrivedAtTarget;

        // Token: 0x020005DA RID: 1498
        [DefineSynchedMissionObjectTypeForMod(typeof(PE_BatteringRam))]
        public struct BatteringRamRecord : ISynchedMissionObjectReadableRecord
        {
            // Token: 0x170009D8 RID: 2520
            // (get) Token: 0x06003B72 RID: 15218 RVA: 0x000E8F7B File Offset: 0x000E717B
            // (set) Token: 0x06003B73 RID: 15219 RVA: 0x000E8F83 File Offset: 0x000E7183
            public bool HasArrivedAtTarget { get; private set; }

            // Token: 0x170009D9 RID: 2521
            // (get) Token: 0x06003B74 RID: 15220 RVA: 0x000E8F8C File Offset: 0x000E718C
            // (set) Token: 0x06003B75 RID: 15221 RVA: 0x000E8F94 File Offset: 0x000E7194
            public int State { get; private set; }

            // Token: 0x170009DA RID: 2522
            // (get) Token: 0x06003B76 RID: 15222 RVA: 0x000E8F9D File Offset: 0x000E719D
            // (set) Token: 0x06003B77 RID: 15223 RVA: 0x000E8FA5 File Offset: 0x000E71A5
            public float TotalDistanceTraveled { get; private set; }

            // Token: 0x06003B78 RID: 15224 RVA: 0x000E8FAE File Offset: 0x000E71AE
            public bool ReadFromNetwork(ref bool bufferReadValid)
            {
                this.HasArrivedAtTarget = GameNetworkMessage.ReadBoolFromPacket(ref bufferReadValid);
                this.State = GameNetworkMessage.ReadIntFromPacket(CompressionMission.BatteringRamStateCompressionInfo, ref bufferReadValid);
                this.TotalDistanceTraveled = GameNetworkMessage.ReadFloatFromPacket(CompressionBasic.PositionCompressionInfo, ref bufferReadValid);
                return bufferReadValid;
            }
        }

        // Token: 0x020005DB RID: 1499
        public enum RamState
        {
            // Token: 0x04001EB6 RID: 7862
            Stable,
            // Token: 0x04001EB7 RID: 7863
            Hitting,
            // Token: 0x04001EB8 RID: 7864
            AfterHit,
            // Token: 0x04001EB9 RID: 7865
            NumberOfStates
        }
    }
}

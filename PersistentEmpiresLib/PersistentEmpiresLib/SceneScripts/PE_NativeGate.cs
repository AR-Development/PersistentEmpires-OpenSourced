using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Objects.Siege;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_NativeGate : UsableMachine, IPointDefendable, ICastleKeyPosition, ITargetable
    {
        // Token: 0x1700080D RID: 2061
        // (get) Token: 0x06002BB7 RID: 11191 RVA: 0x000A9B79 File Offset: 0x000A7D79
        // (set) Token: 0x06002BB8 RID: 11192 RVA: 0x000A9B81 File Offset: 0x000A7D81
        public TacticalPosition MiddlePosition { get; private set; }

        public new bool IsDestroyed
        {
            get
            {
                PE_RepairableDestructableComponent destructableComponent = base.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
                if (destructableComponent != null)
                {
                    return destructableComponent.IsBroken;
                }
                return false;
            }
        }

        // Token: 0x1700080E RID: 2062
        // (get) Token: 0x06002BB9 RID: 11193 RVA: 0x000A9B8A File Offset: 0x000A7D8A
        private static int BatteringRamHitSoundIdCache
        {
            get
            {
                if (PE_NativeGate._batteringRamHitSoundId == -1)
                {
                    PE_NativeGate._batteringRamHitSoundId = SoundEvent.GetEventIdFromString("event:/mission/siege/door/hit");
                }
                return PE_NativeGate._batteringRamHitSoundId;
            }
        }

        // Token: 0x1700080F RID: 2063
        // (get) Token: 0x06002BBA RID: 11194 RVA: 0x000A9BA8 File Offset: 0x000A7DA8
        // (set) Token: 0x06002BBB RID: 11195 RVA: 0x000A9BB0 File Offset: 0x000A7DB0
        public TacticalPosition WaitPosition { get; private set; }

        // Token: 0x17000810 RID: 2064
        // (get) Token: 0x06002BBC RID: 11196 RVA: 0x000A9BB9 File Offset: 0x000A7DB9
        public override FocusableObjectType FocusableObjectType
        {
            get
            {
                return FocusableObjectType.Gate;
            }
        }

        // Token: 0x17000811 RID: 2065
        // (get) Token: 0x06002BBD RID: 11197 RVA: 0x000A9BBC File Offset: 0x000A7DBC
        // (set) Token: 0x06002BBE RID: 11198 RVA: 0x000A9BC4 File Offset: 0x000A7DC4
        public PE_NativeGate.GateState State { get; private set; }

        // Token: 0x17000812 RID: 2066
        // (get) Token: 0x06002BBF RID: 11199 RVA: 0x000A9BCD File Offset: 0x000A7DCD
        public bool IsGateOpen
        {
            get
            {
                return this.State == PE_NativeGate.GateState.Open || this.IsDestroyed;
            }
        }

        // Token: 0x17000813 RID: 2067
        // (get) Token: 0x06002BC0 RID: 11200 RVA: 0x000A9BDF File Offset: 0x000A7DDF
        // (set) Token: 0x06002BC1 RID: 11201 RVA: 0x000A9BE7 File Offset: 0x000A7DE7
        public IPrimarySiegeWeapon AttackerSiegeWeapon { get; set; }

        // Token: 0x17000814 RID: 2068
        // (get) Token: 0x06002BC2 RID: 11202 RVA: 0x000A9BF0 File Offset: 0x000A7DF0
        // (set) Token: 0x06002BC3 RID: 11203 RVA: 0x000A9BF8 File Offset: 0x000A7DF8
        public IEnumerable<DefencePoint> DefencePoints { get; protected set; }

        // Token: 0x06002BC4 RID: 11204 RVA: 0x000A9C04 File Offset: 0x000A7E04
        public PE_NativeGate()
        {
            this._attackOnlyDoorColliders = new List<GameEntity>();
        }

        // Token: 0x06002BC5 RID: 11205 RVA: 0x000A9CC2 File Offset: 0x000A7EC2
        public Vec3 GetPosition()
        {
            return base.GameEntity.GlobalPosition;
        }

        // Token: 0x06002BC6 RID: 11206 RVA: 0x000A9CCF File Offset: 0x000A7ECF
        public override OrderType GetOrder(BattleSideEnum side)
        {
            if (side != BattleSideEnum.Attacker)
            {
                return OrderType.Use;
            }
            return OrderType.AttackEntity;
        }

        // Token: 0x17000815 RID: 2069
        // (get) Token: 0x06002BC7 RID: 11207 RVA: 0x000A9CE4 File Offset: 0x000A7EE4
        // (set) Token: 0x06002BC8 RID: 11208 RVA: 0x000A9CEC File Offset: 0x000A7EEC
        public FormationAI.BehaviorSide DefenseSide { get; private set; }

        // Token: 0x17000816 RID: 2070
        // (get) Token: 0x06002BC9 RID: 11209 RVA: 0x000A9CF5 File Offset: 0x000A7EF5
        public WorldFrame MiddleFrame
        {
            get
            {
                return this._middleFrame;
            }
        }

        // Token: 0x17000817 RID: 2071
        // (get) Token: 0x06002BCA RID: 11210 RVA: 0x000A9CFD File Offset: 0x000A7EFD
        public WorldFrame DefenseWaitFrame
        {
            get
            {
                return this._defenseWaitFrame;
            }
        }

        public PE_RepairableDestructableComponent GetPEDestructable()
        {
            return base.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
        }

        // Token: 0x06002BCB RID: 11211 RVA: 0x000A9D08 File Offset: 0x000A7F08
        protected override void OnInit()
        {
            base.OnInit();
            PE_RepairableDestructableComponent destructableComponent = base.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
            if (destructableComponent != null)
            {
                destructableComponent.OnDestroyed += this.OnDestroyed;
                destructableComponent.OnRepaired += this.OnRepaired;
                destructableComponent.OnHitTaken += this.OnHitTaken;
            }
            this.CollectGameEntities(true);
            base.GameEntity.SetAnimationSoundActivation(true);
            if (GameNetwork.IsClientOrReplay)
            {
                return;
            }
            List<GameEntity> list = base.GameEntity.CollectChildrenEntitiesWithTag("middle_pos");
            if (list.Count > 0)
            {
                GameEntity gameEntity2 = list.FirstOrDefault<GameEntity>();
                this.MiddlePosition = gameEntity2.GetFirstScriptOfType<TacticalPosition>();
                MatrixFrame globalFrame = gameEntity2.GetGlobalFrame();
                this._middleFrame = new WorldFrame(globalFrame.rotation, globalFrame.origin.ToWorldPosition());
                this._middleFrame.Origin.GetGroundVec3();
            }
            else
            {
                MatrixFrame globalFrame2 = base.GameEntity.GetGlobalFrame();
                this._middleFrame = new WorldFrame(globalFrame2.rotation, globalFrame2.origin.ToWorldPosition());
            }
            List<GameEntity> list2 = base.GameEntity.CollectChildrenEntitiesWithTag("wait_pos");
            if (list2.Count > 0)
            {
                GameEntity gameEntity3 = list2.FirstOrDefault<GameEntity>();
                this.WaitPosition = gameEntity3.GetFirstScriptOfType<TacticalPosition>();
                MatrixFrame globalFrame3 = gameEntity3.GetGlobalFrame();
                this._defenseWaitFrame = new WorldFrame(globalFrame3.rotation, globalFrame3.origin.ToWorldPosition());
                this._defenseWaitFrame.Origin.GetGroundVec3();
            }
            else
            {
                this._defenseWaitFrame = this._middleFrame;
            }
            this._openingAnimationIndex = MBAnimation.GetAnimationIndexWithName(this.OpeningAnimationName);
            this._closingAnimationIndex = MBAnimation.GetAnimationIndexWithName(this.ClosingAnimationName);
            base.SetScriptComponentToTick(this.GetTickRequirement());
            this.OnCheckForProblems();
        }

        private void OnRepaired()
        {
            PE_RepairableDestructableComponent comp = base.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();

            foreach (GameEntity entity in base.GameEntity.GetChildren().ToList())
            {
                if (entity != comp.BrokenState()) entity.SetVisibilityExcludeParents(true);
            }
        }

        // Token: 0x06002BCD RID: 11213 RVA: 0x000AA0CC File Offset: 0x000A82CC
        public override void AfterMissionStart()
        {
            this._afterMissionStartTriggered = true;
            base.AfterMissionStart();
            this.SetInitialStateOfGate();
            this.InitializeExtraColliderPositions();
            this._pathChecker = new AgentPathNavMeshChecker(Mission.Current, base.GameEntity.GetGlobalFrame(), 2f, this.NavigationMeshId, BattleSideEnum.Defender, AgentPathNavMeshChecker.Direction.BothDirections, 14f, 3f);
        }

        // Token: 0x06002BCE RID: 11214 RVA: 0x000AA170 File Offset: 0x000A8370
        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            PE_RepairableDestructableComponent destructableComponent = base.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
            if (destructableComponent != null)
            {
                destructableComponent.OnDestroyed -= this.OnDestroyed;
                destructableComponent.OnRepaired -= this.OnRepaired;
                destructableComponent.OnHitTaken -= this.OnHitTaken;
            }
        }

        // Token: 0x06002BCF RID: 11215 RVA: 0x000AA1F7 File Offset: 0x000A83F7
        protected override void OnEditorInit()
        {
            base.OnEditorInit();
            if (base.GameEntity.HasTag("outer_gate") && base.GameEntity.HasTag("inner_gate"))
            {
                MBDebug.ShowWarning("PE_NativeGate gate has both the outer gate tag and the inner gate tag.");
            }
        }

        // Token: 0x06002BD0 RID: 11216 RVA: 0x000AA22D File Offset: 0x000A842D
        protected override void OnMissionReset()
        {
            this.CollectGameEntities(false);
            base.OnMissionReset();
            this.SetInitialStateOfGate();
            this._previousAnimationProgress = -1f;
        }

        // Token: 0x06002BD1 RID: 11217 RVA: 0x000AA260 File Offset: 0x000A8460
        private void SetInitialStateOfGate()
        {
            if (!GameNetwork.IsClientOrReplay && this.NavigationMeshIdToDisableOnOpen != -1)
            {
                this._openNavMeshIdDisabled = false;
                base.Scene.SetAbilityOfFacesWithId(this.NavigationMeshIdToDisableOnOpen, true);
            }
            if (!this._civilianMission)
            {
                this._doorSkeleton.SetAnimationAtChannel(this._closingAnimationIndex, 0, 1f, -1f, 0f);
                this._doorSkeleton.SetAnimationParameterAtChannel(0, 0.99f);
                this._doorSkeleton.Freeze(false);
                this.State = PE_NativeGate.GateState.Closed;
                return;
            }
            this.OpenDoor();
            if (this._doorSkeleton != null)
            {
                this._door.SetAnimationChannelParameterSynched(0, 1f);
            }
            this.SetGateNavMeshState(true);
            base.SetDisabled(true);
            DestructableComponent firstScriptOfType = base.GameEntity.GetFirstScriptOfType<DestructableComponent>();
            if (firstScriptOfType == null)
            {
                return;
            }
            firstScriptOfType.SetDisabled(false);
        }

        // Token: 0x06002BD2 RID: 11218 RVA: 0x000AA32D File Offset: 0x000A852D
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return new TextObject("{=6wZUG0ev}Gate", null).ToString();
        }

        // Token: 0x06002BD3 RID: 11219 RVA: 0x000AA340 File Offset: 0x000A8540
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            TextObject textObject = new TextObject(usableGameObject.GameEntity.HasTag("open") ? "{=5oozsaIb}{KEY} Open" : "{=TJj71hPO}{KEY} Close", null);
            textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            return textObject;
        }


        // Token: 0x06002BD5 RID: 11221 RVA: 0x000AA396 File Offset: 0x000A8596
        public void OpenDoorAndDisableGateForCivilianMission()
        {
            this._civilianMission = true;
        }

        public PE_CastleBanner GetCastleBanner()
        {
            // FactionsBehavior factionBehavior = Mission.Current.GetMissionBehavior<FactionsBehavior>();
            CastlesBehavior castleBehaviors = Mission.Current.GetMissionBehavior<CastlesBehavior>();
            if (castleBehaviors.castles.ContainsKey(this.CastleId))
            {
                return castleBehaviors.castles[this.CastleId];
            }
            return null;
        }

        public bool CanPlayerUse(NetworkCommunicator player)
        {
            bool canPlayerUse = true;
            if (this.CastleId > -1)
            {
                canPlayerUse = false;
                Faction f = this.GetCastleBanner().GetOwnerFaction();
                if (f.doorManagers.Contains(player.VirtualPlayer.ToPlayerId()) || f.marshalls.Contains(player.VirtualPlayer.ToPlayerId()) || f.lordId == player.VirtualPlayer.ToPlayerId()) canPlayerUse = true;
                PE_RepairableDestructableComponent destructComponent = base.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
                if (destructComponent != null && destructComponent.IsBroken) canPlayerUse = true;
            }
            return canPlayerUse;
        }

        // Token: 0x06002BD6 RID: 11222 RVA: 0x000AA3A0 File Offset: 0x000A85A0
        public void OpenDoor()
        {
            if (!base.IsDisabled)
            {
                this.State = PE_NativeGate.GateState.Open;
                int animationIndexAtChannel = this._doorSkeleton.GetAnimationIndexAtChannel(0);
                float animationParameterAtChannel = this._doorSkeleton.GetAnimationParameterAtChannel(0);
                this._door.SetAnimationAtChannelSynched(this._openingAnimationIndex, 0, 1f);
                if (animationIndexAtChannel == this._closingAnimationIndex)
                {
                    this._door.SetAnimationChannelParameterSynched(0, 1f - animationParameterAtChannel);
                }
                SynchedMissionObject plank = this._plank;
                if (plank == null)
                {
                    return;
                }
                plank.SetVisibleSynched(false, false);
            }
        }

        // Token: 0x06002BD7 RID: 11223 RVA: 0x000AA434 File Offset: 0x000A8634
        public void CloseDoor()
        {
            if (!base.IsDisabled)
            {
                this.State = PE_NativeGate.GateState.Closed;
                if (!this.AutoOpen)
                {
                    this.SetGateNavMeshState(false);
                }
                else
                {
                    //
                }
                int animationIndexAtChannel = this._doorSkeleton.GetAnimationIndexAtChannel(0);
                float animationParameterAtChannel = this._doorSkeleton.GetAnimationParameterAtChannel(0);
                this._door.SetAnimationAtChannelSynched(this._closingAnimationIndex, 0, 1f);
                if (animationIndexAtChannel == this._openingAnimationIndex)
                {
                    this._door.SetAnimationChannelParameterSynched(0, 1f - animationParameterAtChannel);
                }
            }
        }

        // Token: 0x06002BD8 RID: 11224 RVA: 0x000AA4B4 File Offset: 0x000A86B4
        public void UpdateDoorBodies(bool updateAnyway)
        {
            if (this.IsDestroyed) { return; }
            if (this._attackOnlyDoorColliders.Count == 2)
            {
                float animationParameterAtChannel = this._doorSkeleton.GetAnimationParameterAtChannel(0);
                if (this._previousAnimationProgress != animationParameterAtChannel || updateAnyway)
                {
                    this._previousAnimationProgress = animationParameterAtChannel;
                    MatrixFrame matrixFrame = this._doorSkeleton.GetBoneEntitialFrameWithIndex(this._leftDoorBoneIndex);
                    MatrixFrame matrixFrame2 = this._doorSkeleton.GetBoneEntitialFrameWithIndex(this._rightDoorBoneIndex);
                    this._attackOnlyDoorColliders[0].SetFrame(ref matrixFrame2);
                    this._attackOnlyDoorColliders[1].SetFrame(ref matrixFrame);
                    GameEntity agentColliderLeft = this._agentColliderLeft;
                    if (agentColliderLeft != null)
                    {
                        agentColliderLeft.SetFrame(ref matrixFrame);
                    }
                    GameEntity agentColliderRight = this._agentColliderRight;
                    if (agentColliderRight != null)
                    {
                        agentColliderRight.SetFrame(ref matrixFrame2);
                    }
                    if (this._extraColliderLeft != null && this._extraColliderRight != null)
                    {
                        if (this.State == PE_NativeGate.GateState.Closed)
                        {
                            if (!this._leftExtraColliderDisabled)
                            {
                                this._extraColliderLeft.SetBodyFlags(this._extraColliderLeft.BodyFlag | BodyFlags.Disabled);
                                this._leftExtraColliderDisabled = true;
                            }
                            if (!this._rightExtraColliderDisabled)
                            {
                                this._extraColliderRight.SetBodyFlags(this._extraColliderRight.BodyFlag | BodyFlags.Disabled);
                                this._rightExtraColliderDisabled = true;
                                return;
                            }
                        }
                        else
                        {
                            float num = (matrixFrame2.origin - matrixFrame.origin).Length * 0.5f;
                            float num2 = Vec3.DotProduct(matrixFrame2.rotation.s, Vec3.Side) / (matrixFrame2.rotation.s.Length * 1f);
                            float num3 = MathF.Sqrt(1f - num2 * num2);
                            float num4 = num * 1.1f;
                            float num5 = MBMath.Map(num2, 0.3f, 1f, 0f, 1f) * (num * 0.2f);
                            this._extraColliderLeft.SetLocalPosition(matrixFrame.origin - new Vec3(num4 - num + num5, num * num3, 0f, -1f));
                            this._extraColliderRight.SetLocalPosition(matrixFrame2.origin - new Vec3(-(num4 - num) - num5, num * num3, 0f, -1f));
                            float num6;
                            if (num2 < 0f)
                            {
                                num6 = num;
                                num6 += num * -num2;
                            }
                            else
                            {
                                num6 = num - num * num2;
                            }
                            num6 = (num4 - num6) / num;
                            if (num6 <= 0.0001f)
                            {
                                if (!this._leftExtraColliderDisabled)
                                {
                                    this._extraColliderLeft.SetBodyFlags(this._extraColliderLeft.BodyFlag | BodyFlags.Disabled);
                                    this._leftExtraColliderDisabled = true;
                                }
                            }
                            else
                            {
                                if (this._leftExtraColliderDisabled)
                                {
                                    this._extraColliderLeft.SetBodyFlags(this._extraColliderLeft.BodyFlag & ~BodyFlags.Disabled);
                                    this._leftExtraColliderDisabled = false;
                                }
                                matrixFrame = this._extraColliderLeft.GetFrame();
                                matrixFrame.rotation.Orthonormalize();
                                matrixFrame.origin -= new Vec3(num4 - num4 * num6, 0f, 0f, -1f);
                                this._extraColliderLeft.SetFrame(ref matrixFrame);
                            }
                            matrixFrame2 = this._extraColliderRight.GetFrame();
                            matrixFrame2.rotation.Orthonormalize();
                            float num7;
                            if (num2 < 0f)
                            {
                                num7 = num;
                                num7 += num * -num2;
                            }
                            else
                            {
                                num7 = num - num * num2;
                            }
                            num7 = (num4 - num7) / num;
                            if (num7 > 0.0001f)
                            {
                                if (this._rightExtraColliderDisabled)
                                {
                                    this._extraColliderRight.SetBodyFlags(this._extraColliderRight.BodyFlag & ~BodyFlags.Disabled);
                                    this._rightExtraColliderDisabled = false;
                                }
                                matrixFrame2.origin += new Vec3(num4 - num4 * num7, 0f, 0f, -1f);
                                this._extraColliderRight.SetFrame(ref matrixFrame2);
                                return;
                            }
                            if (!this._rightExtraColliderDisabled)
                            {
                                this._extraColliderRight.SetBodyFlags(this._extraColliderRight.BodyFlag | BodyFlags.Disabled);
                                this._rightExtraColliderDisabled = true;
                                return;
                            }
                        }
                    }
                }
            }
            else if (this._attackOnlyDoorColliders.Count == 1)
            {
                MatrixFrame boneEntitialFrameWithName = this._doorSkeleton.GetBoneEntitialFrameWithName(this.RightDoorBoneName);
                this._attackOnlyDoorColliders[0].SetFrame(ref boneEntitialFrameWithName);
                GameEntity agentColliderRight2 = this._agentColliderRight;
                if (agentColliderRight2 == null)
                {
                    return;
                }
                agentColliderRight2.SetFrame(ref boneEntitialFrameWithName);
            }
        }

        // Token: 0x06002BD9 RID: 11225 RVA: 0x000AA8EC File Offset: 0x000A8AEC
        private void SetGateNavMeshState(bool isEnabled)
        {
            if (!GameNetwork.IsClientOrReplay)
            {
                base.Scene.SetAbilityOfFacesWithId(this.NavigationMeshId, isEnabled);
                if (this._queueManager != null)
                {
                    this._queueManager.Activate();
                    base.Scene.SetAbilityOfFacesWithId(this._queueManager.ManagedNavigationFaceId, isEnabled);
                }
            }
        }

        // Token: 0x06002BDB RID: 11227 RVA: 0x000AA9B0 File Offset: 0x000A8BB0
        public void SetAutoOpenState(bool isEnabled)
        {
            this.AutoOpen = isEnabled;
            if (this.AutoOpen)
            {
                this.SetGateNavMeshState(true);
                return;
            }
            if (this.State == PE_NativeGate.GateState.Open)
            {
                this.CloseDoor();
            }
            else
            {
                this.SetGateNavMeshState(false);
            }
        }

        // Token: 0x06002BDC RID: 11228 RVA: 0x000AAA01 File Offset: 0x000A8C01
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            if (base.GameEntity.IsVisibleIncludeParents())
            {
                return ScriptComponentBehavior.TickRequirement.Tick | base.GetTickRequirement();
            }
            return base.GetTickRequirement();
        }

        // Token: 0x06002BDD RID: 11229 RVA: 0x000AAA20 File Offset: 0x000A8C20
        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (!base.GameEntity.IsVisibleIncludeParents())
            {
                return;
            }
            if (this._afterMissionStartTriggered)
            {
                this.UpdateDoorBodies(false);
            }
            if (!GameNetwork.IsClientOrReplay)
            {
                this.ServerTick(dt);
            }
        }

        // Token: 0x06002BDE RID: 11230 RVA: 0x000AABBC File Offset: 0x000A8DBC
        protected override bool IsAgentOnInconvenientNavmesh(Agent agent, StandingPoint standingPoint)
        {
            int currentNavigationFaceId = agent.GetCurrentNavigationFaceId();
            return false;
        }

        // Token: 0x06002BDF RID: 11231 RVA: 0x000AACE4 File Offset: 0x000A8EE4
        private void ServerTick(float dt)
        {
            if (!this.IsDeactivated)
            {
                foreach (StandingPoint standingPoint in base.StandingPoints)
                {
                    if (standingPoint.HasUser)
                    {
                        if (standingPoint.GameEntity.HasTag("open"))
                        {
                            if (this.CanPlayerUse(standingPoint.UserAgent.MissionPeer.GetNetworkPeer()))
                            {
                                this.OpenDoor();
                                if (this.AutoOpen)
                                {
                                    this.SetAutoOpenState(false);
                                }
                            }
                            else
                            {
                                Faction f = this.GetCastleBanner().GetOwnerFaction();
                                InformationComponent.Instance.SendMessage("This door is locked by " + f.name, 0x0606c2d9, standingPoint.UserAgent.MissionPeer.GetNetworkPeer());
                                Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/movement/foley/door_close"), base.GameEntity.GetGlobalFrame().origin, false, true, -1, -1);
                                standingPoint.UserAgent.StopUsingGameObjectMT(false);
                            }
                        }
                        else
                        {
                            if (this.CanPlayerUse(standingPoint.UserAgent.MissionPeer.GetNetworkPeer()))
                            {
                                this.CloseDoor();
                                if (Mission.Current.IsSallyOutBattle)
                                {
                                    this.SetAutoOpenState(true);
                                }
                            }
                            else
                            {
                                Faction f = this.GetCastleBanner().GetOwnerFaction();
                                InformationComponent.Instance.SendMessage("This door is locked by " + f.name, 0x0606c2d9, standingPoint.UserAgent.MissionPeer.GetNetworkPeer());
                                Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/movement/foley/door_close"), base.GameEntity.GetGlobalFrame().origin, false, true, -1, -1);
                                standingPoint.UserAgent.StopUsingGameObjectMT(false);
                            }
                        }
                    }
                }
                if (this.AutoOpen && this._pathChecker != null)
                {
                    this._pathChecker.Tick(dt);
                    if (this._pathChecker.HasAgentsUsingPath())
                    {
                        if (this.State != PE_NativeGate.GateState.Open)
                        {
                            this.OpenDoor();
                        }
                    }
                    else if (this.State != PE_NativeGate.GateState.Closed)
                    {
                        this.CloseDoor();
                    }
                }
                if (this._doorSkeleton != null && !this.IsDestroyed)
                {
                    float animationParameterAtChannel = this._doorSkeleton.GetAnimationParameterAtChannel(0);
                    foreach (StandingPoint standingPoint2 in base.StandingPoints)
                    {
                        bool flag = animationParameterAtChannel < 1f || standingPoint2.GameEntity.HasTag((this.State == PE_NativeGate.GateState.Open) ? "open" : "close");
                        standingPoint2.SetIsDeactivatedSynched(flag);
                    }
                    if (animationParameterAtChannel >= 1f && this.State == PE_NativeGate.GateState.Open)
                    {
                        if (this._extraColliderRight != null)
                        {
                            this._extraColliderRight.SetBodyFlags(this._extraColliderRight.BodyFlag | BodyFlags.Disabled);
                            this._rightExtraColliderDisabled = true;
                        }
                        if (this._extraColliderLeft != null)
                        {
                            this._extraColliderLeft.SetBodyFlags(this._extraColliderLeft.BodyFlag | BodyFlags.Disabled);
                            this._leftExtraColliderDisabled = true;
                        }
                    }
                    if (this._plank != null && this.State == PE_NativeGate.GateState.Closed && animationParameterAtChannel > 0.9f)
                    {
                        this._plank.SetVisibleSynched(true, false);
                    }
                }
            }
        }

        // Token: 0x06002BE0 RID: 11232 RVA: 0x000AAEF8 File Offset: 0x000A90F8
        public TargetFlags GetTargetFlags()
        {
            TargetFlags targetFlags = TargetFlags.None;
            targetFlags |= TargetFlags.IsStructure;
            if (DebugSiegeBehavior.DebugAttackState == DebugSiegeBehavior.DebugStateAttacker.DebugAttackersToBattlements)
            {
                targetFlags |= TargetFlags.DebugThreat;
            }
            return targetFlags;
        }

        // Token: 0x06002BE1 RID: 11233 RVA: 0x000AAF29 File Offset: 0x000A9129
        public float GetTargetValue(List<Vec3> weaponPos)
        {
            return 10f;
        }

        // Token: 0x06002BE2 RID: 11234 RVA: 0x000AAF30 File Offset: 0x000A9130
        public GameEntity GetTargetEntity()
        {
            return base.GameEntity;
        }

        // Token: 0x06002BE3 RID: 11235 RVA: 0x000AAF38 File Offset: 0x000A9138
        public BattleSideEnum GetSide()
        {
            return BattleSideEnum.Defender;
        }

        // Token: 0x06002BE4 RID: 11236 RVA: 0x000AAF3B File Offset: 0x000A913B
        public GameEntity Entity()
        {
            return base.GameEntity;
        }

        // Token: 0x06002BE5 RID: 11237 RVA: 0x000AAF44 File Offset: 0x000A9144
        protected void CollectGameEntities(bool calledFromOnInit)
        {
            this.CollectDynamicGameEntities(calledFromOnInit);
            if (!GameNetwork.IsClientOrReplay)
            {
                List<GameEntity> list = base.GameEntity.CollectChildrenEntitiesWithTag("plank");
                if (list.Count > 0)
                {
                    this._plank = list.FirstOrDefault<GameEntity>().GetFirstScriptOfType<SynchedMissionObject>();
                }
            }
        }

        // Token: 0x06002BE7 RID: 11239 RVA: 0x000AAF9C File Offset: 0x000A919C
        protected void CollectDynamicGameEntities(bool calledFromOnInit)
        {
            this._attackOnlyDoorColliders.Clear();
            List<GameEntity> list;
            if (calledFromOnInit)
            {
                list = base.GameEntity.CollectChildrenEntitiesWithTag("gate").ToList<GameEntity>();
                this._leftExtraColliderDisabled = false;
                this._rightExtraColliderDisabled = false;
                this._agentColliderLeft = base.GameEntity.GetFirstChildEntityWithTag("collider_agent_l");
                this._agentColliderRight = base.GameEntity.GetFirstChildEntityWithTag("collider_agent_r");
            }
            else
            {
                list = (from x in base.GameEntity.CollectChildrenEntitiesWithTag("gate")
                        where x.IsVisibleIncludeParents()
                        select x).ToList<GameEntity>();
            }
            if (list.Count == 0)
            {
                return;
            }
            if (list.Count > 1)
            {
                int num = int.MinValue;
                int num2 = int.MaxValue;
                GameEntity gameEntity = null;
                GameEntity gameEntity2 = null;
                foreach (GameEntity gameEntity3 in list)
                {
                    int num3 = int.Parse(gameEntity3.Tags.FirstOrDefault((string x) => x.Contains("state_")).Split(new char[] { '_' }).Last<string>());
                    if (num3 > num)
                    {
                        num = num3;
                        gameEntity = gameEntity3;
                    }
                    if (num3 < num2)
                    {
                        num2 = num3;
                        gameEntity2 = gameEntity3;
                    }
                }
                this._door = (calledFromOnInit ? gameEntity2.GetFirstScriptOfType<SynchedMissionObject>() : gameEntity.GetFirstScriptOfType<SynchedMissionObject>());
            }
            else
            {
                this._door = list[0].GetFirstScriptOfType<SynchedMissionObject>();
            }
            this._doorSkeleton = this._door.GameEntity.Skeleton;
            GameEntity gameEntity4 = this._door.GameEntity.CollectChildrenEntitiesWithTag("collider_r").FirstOrDefault<GameEntity>();
            if (gameEntity4 != null)
            {
                this._attackOnlyDoorColliders.Add(gameEntity4);
            }
            GameEntity gameEntity5 = this._door.GameEntity.CollectChildrenEntitiesWithTag("collider_l").FirstOrDefault<GameEntity>();
            if (gameEntity5 != null)
            {
                this._attackOnlyDoorColliders.Add(gameEntity5);
            }
            if (gameEntity4 == null || gameEntity5 == null)
            {
                GameEntity agentColliderLeft = this._agentColliderLeft;
                if (agentColliderLeft != null)
                {
                    agentColliderLeft.SetVisibilityExcludeParents(false);
                }
                GameEntity agentColliderRight = this._agentColliderRight;
                if (agentColliderRight != null)
                {
                    agentColliderRight.SetVisibilityExcludeParents(false);
                }
            }
            GameEntity gameEntity6 = this._door.GameEntity.CollectChildrenEntitiesWithTag(this.ExtraCollisionObjectTagLeft).FirstOrDefault<GameEntity>();
            if (gameEntity6 != null)
            {
                if (!this.ActivateExtraColliders)
                {
                    gameEntity6.RemovePhysics(false);
                }
                else
                {
                    if (!calledFromOnInit)
                    {
                        MatrixFrame matrixFrame = ((this._extraColliderLeft != null) ? this._extraColliderLeft.GetFrame() : this._doorSkeleton.GetBoneEntitialFrameWithName(this.LeftDoorBoneName));
                        this._extraColliderLeft = gameEntity6;
                        this._extraColliderLeft.SetFrame(ref matrixFrame);
                    }
                    else
                    {
                        this._extraColliderLeft = gameEntity6;
                    }
                    if (this._leftExtraColliderDisabled)
                    {
                        this._extraColliderLeft.SetBodyFlags(this._extraColliderLeft.BodyFlag | BodyFlags.Disabled);
                    }
                    else
                    {
                        this._extraColliderLeft.SetBodyFlags(this._extraColliderLeft.BodyFlag & ~BodyFlags.Disabled);
                    }
                }
            }
            GameEntity gameEntity7 = this._door.GameEntity.CollectChildrenEntitiesWithTag(this.ExtraCollisionObjectTagRight).FirstOrDefault<GameEntity>();
            if (gameEntity7 != null)
            {
                if (!this.ActivateExtraColliders)
                {
                    gameEntity7.RemovePhysics(false);
                }
                else
                {
                    if (!calledFromOnInit)
                    {
                        MatrixFrame matrixFrame2 = ((this._extraColliderRight != null) ? this._extraColliderRight.GetFrame() : this._doorSkeleton.GetBoneEntitialFrameWithName(this.RightDoorBoneName));
                        this._extraColliderRight = gameEntity7;
                        this._extraColliderRight.SetFrame(ref matrixFrame2);
                    }
                    else
                    {
                        this._extraColliderRight = gameEntity7;
                    }
                    if (this._rightExtraColliderDisabled)
                    {
                        this._extraColliderRight.SetBodyFlags(this._extraColliderRight.BodyFlag | BodyFlags.Disabled);
                    }
                    else
                    {
                        this._extraColliderRight.SetBodyFlags(this._extraColliderRight.BodyFlag & ~BodyFlags.Disabled);
                    }
                }
            }
            if (this._door != null && this._doorSkeleton != null)
            {
                this._leftDoorBoneIndex = Skeleton.GetBoneIndexFromName(this._doorSkeleton.GetName(), this.LeftDoorBoneName);
                this._rightDoorBoneIndex = Skeleton.GetBoneIndexFromName(this._doorSkeleton.GetName(), this.RightDoorBoneName);
            }
        }

        // Token: 0x06002BE8 RID: 11240 RVA: 0x000AB3C4 File Offset: 0x000A95C4
        private void InitializeExtraColliderPositions()
        {
            if (this._extraColliderLeft != null)
            {
                MatrixFrame boneEntitialFrameWithName = this._doorSkeleton.GetBoneEntitialFrameWithName(this.LeftDoorBoneName);
                this._extraColliderLeft.SetFrame(ref boneEntitialFrameWithName);
                this._extraColliderLeft.SetVisibilityExcludeParents(true);
            }
            if (this._extraColliderRight != null)
            {
                MatrixFrame boneEntitialFrameWithName2 = this._doorSkeleton.GetBoneEntitialFrameWithName(this.RightDoorBoneName);
                this._extraColliderRight.SetFrame(ref boneEntitialFrameWithName2);
                this._extraColliderRight.SetVisibilityExcludeParents(true);
            }
            this.UpdateDoorBodies(true);
            foreach (GameEntity gameEntity in this._attackOnlyDoorColliders)
            {
                gameEntity.SetVisibilityExcludeParents(true);
            }
            if (this._agentColliderLeft != null)
            {
                this._agentColliderLeft.SetVisibilityExcludeParents(true);
            }
            if (this._agentColliderRight != null)
            {
                this._agentColliderRight.SetVisibilityExcludeParents(true);
            }
        }

        // Token: 0x06002BE9 RID: 11241 RVA: 0x000AB4C4 File Offset: 0x000A96C4
        private void OnHitTaken(Agent hitterAgent, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, int inflictedDamage)
        {
            if (!GameNetwork.IsClientOrReplay && inflictedDamage >= 200 && this.State == PE_NativeGate.GateState.Closed && attackerScriptComponentBehavior is PE_BatteringRam)
            {
                SynchedMissionObject plank = this._plank;
                if (plank != null)
                {
                    plank.SetAnimationAtChannelSynched(this.PlankHitAnimationName, 0, 1f);
                }
                this._door.SetAnimationAtChannelSynched(this.HitAnimationName, 0, 1f);
                Mission.Current.MakeSound(PE_NativeGate.BatteringRamHitSoundIdCache, base.GameEntity.GlobalPosition, false, true, -1, -1);
            }
        }

        // Token: 0x06002BEA RID: 11242 RVA: 0x000AB548 File Offset: 0x000A9748
        private void OnDestroyed(ScriptComponentBehavior attackerScriptComponentBehavior)
        {

            if (!GameNetwork.IsClientOrReplay)
            {

                foreach (StandingPoint standingPoint in base.StandingPoints)
                {
                    standingPoint.SetIsDeactivatedSynched(true);
                }
                if (attackerScriptComponentBehavior is BatteringRam)
                {
                    this._door.SetAnimationAtChannelSynched(this.DestroyAnimationName, 0, 1f);
                }
                this.SetGateNavMeshState(true);
            }

            PE_RepairableDestructableComponent comp = base.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();

            foreach (GameEntity entity in base.GameEntity.GetChildren().ToList())
            {
                if (entity != comp.BrokenState()) entity.SetVisibilityExcludeParents(false);
            }

        }

        // Token: 0x06002BEC RID: 11244 RVA: 0x000AB5F4 File Offset: 0x000A97F4
        protected override bool OnCheckForProblems()
        {
            bool flag = base.OnCheckForProblems();
            if (base.GameEntity.HasTag("outer_gate") && base.GameEntity.HasTag("inner_gate"))
            {
                MBEditor.AddEntityWarning(base.GameEntity, "This castle gate has both outer and inner tag at the same time.");
                flag = true;
            }
            if (base.GameEntity.CollectChildrenEntitiesWithTag("wait_pos").Count != 1)
            {
                MBEditor.AddEntityWarning(base.GameEntity, "There must be one entity with wait position tag under castle gate.");
                flag = true;
            }
            if (base.GameEntity.HasTag("outer_gate"))
            {
                uint visibilityMask = base.GameEntity.GetVisibilityLevelMaskIncludingParents();
                GameEntity gameEntity = base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.HasTag("middle_pos") && x.GetVisibilityLevelMaskIncludingParents() == visibilityMask);
                if (gameEntity != null)
                {
                    GameEntity gameEntity2 = base.Scene.FindEntitiesWithTag("inner_gate").FirstOrDefault((GameEntity x) => x.GetVisibilityLevelMaskIncludingParents() == visibilityMask);
                    if (gameEntity2 != null)
                    {
                        if (gameEntity2.HasScriptOfType<CastleGate>())
                        {
                            Vec2 vec = gameEntity2.GlobalPosition.AsVec2 - gameEntity.GlobalPosition.AsVec2;
                            Vec2 vec2 = base.GameEntity.GlobalPosition.AsVec2 - gameEntity.GlobalPosition.AsVec2;
                            if (Vec2.DotProduct(vec, vec2) <= 0f)
                            {
                                MBEditor.AddEntityWarning(base.GameEntity, "Outer gate's middle position must not be between outer and inner gate.");
                                flag = true;
                            }
                        }
                        else
                        {
                            MBEditor.AddEntityWarning(base.GameEntity, gameEntity2.Name + " this entity has inner gate tag but doesn't have castle gate script.");
                            flag = true;
                        }
                    }
                    else
                    {
                        MBEditor.AddEntityWarning(base.GameEntity, "There is no entity with inner gate tag.");
                        flag = true;
                    }
                }
                else
                {
                    MBEditor.AddEntityWarning(base.GameEntity, "Outer gate doesn't have any middle positions");
                    flag = true;
                }
            }
            Vec3 scaleVector = base.GameEntity.GetGlobalFrame().rotation.GetScaleVector();
            if (MathF.Abs(scaleVector.x - scaleVector.y) > 1E-05f || MathF.Abs(scaleVector.x - scaleVector.z) > 1E-05f || MathF.Abs(scaleVector.y - scaleVector.z) > 1E-05f)
            {
                MBEditor.AddEntityWarning(base.GameEntity, "$$$ Non uniform scale on CastleGate at scene " + base.GameEntity.Scene.GetName());
                flag = true;
            }
            return flag;
        }

        // Token: 0x06002BED RID: 11245 RVA: 0x000AB83B File Offset: 0x000A9A3B
        public Vec3 GetTargetingOffset()
        {
            return Vec3.Zero;
        }

        // Token: 0x04001139 RID: 4409
        public const string OuterGateTag = "outer_gate";

        // Token: 0x0400113A RID: 4410
        public const string InnerGateTag = "inner_gate";

        // Token: 0x04001146 RID: 4422
        private static int _batteringRamHitSoundId = -1;

        // Token: 0x04001149 RID: 4425
        public string OpeningAnimationName = "castle_gate_a_opening";

        // Token: 0x0400114A RID: 4426
        public string ClosingAnimationName = "castle_gate_a_closing";

        // Token: 0x0400114B RID: 4427
        public string HitAnimationName = "castle_gate_a_hit";

        // Token: 0x0400114C RID: 4428
        public string PlankHitAnimationName = "castle_gate_a_plank_hit";

        // Token: 0x0400114D RID: 4429
        public string HitMeleeAnimationName = "castle_gate_a_hit_melee";

        // Token: 0x0400114E RID: 4430
        public string DestroyAnimationName = "castle_gate_a_break";

        // Token: 0x0400114F RID: 4431
        public int NavigationMeshId = 1000;

        // Token: 0x04001150 RID: 4432
        public int NavigationMeshIdToDisableOnOpen = -1;

        // Token: 0x04001151 RID: 4433
        public string LeftDoorBoneName = "bn_bottom_l";

        // Token: 0x04001152 RID: 4434
        public string RightDoorBoneName = "bn_bottom_r";

        // Token: 0x04001153 RID: 4435
        public string ExtraCollisionObjectTagRight = "extra_collider_r";

        // Token: 0x04001154 RID: 4436
        public string ExtraCollisionObjectTagLeft = "extra_collider_l";

        // Token: 0x04001155 RID: 4437
        private int _openingAnimationIndex = -1;

        // Token: 0x04001156 RID: 4438
        private int _closingAnimationIndex = -1;

        // Token: 0x04001157 RID: 4439
        public bool _leftExtraColliderDisabled;

        // Token: 0x04001158 RID: 4440
        public bool _rightExtraColliderDisabled;

        // Token: 0x04001159 RID: 4441
        private bool _civilianMission;

        // Token: 0x0400115A RID: 4442
        public bool ActivateExtraColliders = true;

        // Token: 0x0400115B RID: 4443
        public string SideTag;

        // Token: 0x0400115D RID: 4445
        private bool _openNavMeshIdDisabled;

        // Token: 0x0400115E RID: 4446
        private SynchedMissionObject _door;

        // Token: 0x0400115F RID: 4447
        public Skeleton _doorSkeleton;

        // Token: 0x04001160 RID: 4448
        public GameEntity _extraColliderRight;

        // Token: 0x04001161 RID: 4449
        public GameEntity _extraColliderLeft;

        // Token: 0x04001162 RID: 4450
        private readonly List<GameEntity> _attackOnlyDoorColliders;

        // Token: 0x04001163 RID: 4451
        private float _previousAnimationProgress = -1f;

        // Token: 0x04001164 RID: 4452
        private GameEntity _agentColliderRight;

        // Token: 0x04001165 RID: 4453
        private GameEntity _agentColliderLeft;

        // Token: 0x04001166 RID: 4454
        private LadderQueueManager _queueManager;

        // Token: 0x04001167 RID: 4455
        public bool _afterMissionStartTriggered;

        // Token: 0x04001168 RID: 4456
        private sbyte _rightDoorBoneIndex;

        // Token: 0x04001169 RID: 4457
        private sbyte _leftDoorBoneIndex;

        // Token: 0x0400116C RID: 4460
        public AgentPathNavMeshChecker _pathChecker;

        // Token: 0x0400116D RID: 4461
        public bool AutoOpen;

        // Token: 0x0400116E RID: 4462
        public SynchedMissionObject _plank;

        // Token: 0x04001170 RID: 4464
        private WorldFrame _middleFrame;

        // Token: 0x04001171 RID: 4465
        private WorldFrame _defenseWaitFrame;

        public int CastleId = -1;

        // Token: 0x020005DE RID: 1502
        public enum GateState
        {
            // Token: 0x04001EC1 RID: 7873
            Open,
            // Token: 0x04001EC2 RID: 7874
            Closed
        }
    }
}

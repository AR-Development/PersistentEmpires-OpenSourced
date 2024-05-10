using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_AnimationPoint : StandingPoint
    {
        // Token: 0x1700003A RID: 58
        // (get) Token: 0x0600021D RID: 541 RVA: 0x0000E482 File Offset: 0x0000C682
        public override bool PlayerStopsUsingWhenInteractsWithOther
        {
            get
            {
                return false;
            }
        }

        // Token: 0x1700003B RID: 59
        // (get) Token: 0x0600021E RID: 542 RVA: 0x0000E485 File Offset: 0x0000C685
        // (set) Token: 0x0600021F RID: 543 RVA: 0x0000E48D File Offset: 0x0000C68D
        public bool IsArriveActionFinished { get; private set; }

        // Token: 0x1700003C RID: 60
        // (get) Token: 0x06000220 RID: 544 RVA: 0x0000E496 File Offset: 0x0000C696
        // (set) Token: 0x06000221 RID: 545 RVA: 0x0000E4A0 File Offset: 0x0000C6A0
        protected string SelectedRightHandItem
        {
            get
            {
                return this._selectedRightHandItem;
            }
            set
            {
                if (value != this._selectedRightHandItem)
                {
                    PE_AnimationPoint.ItemForBone newItem = new PE_AnimationPoint.ItemForBone(this.RightHandItemBone, value, false);
                    this.AssignItemToBone(newItem);
                    this._selectedRightHandItem = value;
                }
            }
        }

        // Token: 0x1700003D RID: 61
        // (get) Token: 0x06000222 RID: 546 RVA: 0x0000E4D8 File Offset: 0x0000C6D8
        // (set) Token: 0x06000223 RID: 547 RVA: 0x0000E4E0 File Offset: 0x0000C6E0
        protected string SelectedLeftHandItem
        {
            get
            {
                return this._selectedLeftHandItem;
            }
            set
            {
                if (value != this._selectedLeftHandItem)
                {
                    PE_AnimationPoint.ItemForBone newItem = new PE_AnimationPoint.ItemForBone(this.LeftHandItemBone, value, false);
                    this.AssignItemToBone(newItem);
                    this._selectedLeftHandItem = value;
                }
            }
        }

        // Token: 0x1700003E RID: 62
        // (get) Token: 0x06000224 RID: 548 RVA: 0x0000E518 File Offset: 0x0000C718
        // (set) Token: 0x06000225 RID: 549 RVA: 0x0000E520 File Offset: 0x0000C720
        public bool IsActive { get; private set; } = true;

        // Token: 0x06000226 RID: 550 RVA: 0x0000E52C File Offset: 0x0000C72C
        public PE_AnimationPoint()
        {
            this._greetingTimer = null;
        }

        // Token: 0x1700003F RID: 63
        // (get) Token: 0x06000227 RID: 551 RVA: 0x0000E699 File Offset: 0x0000C899
        public override bool DisableCombatActionsOnUse
        {
            get
            {
                return !base.IsInstantUse;
            }
        }

        // Token: 0x06000228 RID: 552 RVA: 0x0000E6A4 File Offset: 0x0000C8A4
        private void CreateVisualizer()
        {
            if (this.PairLoopStartActionCode != ActionIndexCache.act_none || this.LoopStartActionCode != ActionIndexCache.act_none)
            {
                this._animatedEntity = GameEntity.CreateEmpty(base.GameEntity.Scene, false);
                this._animatedEntity.EntityFlags = (this._animatedEntity.EntityFlags | EntityFlags.DontSaveToScene);
                this._animatedEntity.Name = "ap_visual_entity";
                MBActionSet actionSet = MBActionSet.GetActionSetWithIndex(0);
                ActionIndexCache actionIndexCache = ActionIndexCache.act_none;
                int numberOfActionSets = MBActionSet.GetNumberOfActionSets();
                for (int i = 0; i < numberOfActionSets; i++)
                {
                    MBActionSet actionSetWithIndex = MBActionSet.GetActionSetWithIndex(i);
                    if (this.ArriveActionCode == ActionIndexCache.act_none || MBActionSet.CheckActionAnimationClipExists(actionSetWithIndex, this.ArriveActionCode))
                    {
                        if (this.PairLoopStartActionCode != ActionIndexCache.act_none && MBActionSet.CheckActionAnimationClipExists(actionSetWithIndex, this.PairLoopStartActionCode))
                        {
                            actionSet = actionSetWithIndex;
                            actionIndexCache = this.PairLoopStartActionCode;
                            break;
                        }
                        if (this.LoopStartActionCode != ActionIndexCache.act_none && MBActionSet.CheckActionAnimationClipExists(actionSetWithIndex, this.LoopStartActionCode))
                        {
                            actionSet = actionSetWithIndex;
                            actionIndexCache = this.LoopStartActionCode;
                            break;
                        }
                    }
                }
                if (actionIndexCache == null || actionIndexCache == ActionIndexCache.act_none)
                {
                    actionIndexCache = ActionIndexCache.Create("act_jump_loop");
                }
                this._animatedEntity.CreateAgentSkeleton("human_skeleton", true, actionSet, "human", MBObjectManager.Instance.GetObject<Monster>("human"));
                this._animatedEntity.Skeleton.SetAgentActionChannel(0, actionIndexCache, 0f, -0.2f, true);
                this._animatedEntity.AddMultiMeshToSkeleton(MetaMesh.GetCopy("roman_cloth_tunic_a", true, false));
                this._animatedEntity.AddMultiMeshToSkeleton(MetaMesh.GetCopy("casual_02_boots", true, false));
                this._animatedEntity.AddMultiMeshToSkeleton(MetaMesh.GetCopy("hands_male_a", true, false));
                this._animatedEntity.AddMultiMeshToSkeleton(MetaMesh.GetCopy("head_male_a", true, false));
                this._animatedEntityDisplacement = Vec3.Zero;
                if (this.ArriveActionCode != ActionIndexCache.act_none && (MBActionSet.GetActionAnimationFlags(actionSet, this.ArriveActionCode) & AnimFlags.anf_displace_position) != (AnimFlags)0UL)
                {
                    this._animatedEntityDisplacement = MBActionSet.GetActionDisplacementVector(actionSet, this.ArriveActionCode);
                }
                this.UpdateAnimatedEntityFrame();
            }
        }

        // Token: 0x06000229 RID: 553 RVA: 0x0000E8D0 File Offset: 0x0000CAD0
        private void UpdateAnimatedEntityFrame()
        {
            MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
            MatrixFrame matrixFrame = globalFrame.TransformToParent(new MatrixFrame
            {
                rotation = Mat3.Identity,
                origin = this._animatedEntityDisplacement
            });
            globalFrame.origin = matrixFrame.origin;
            this._animatedEntity.SetFrame(ref globalFrame);
        }

        // Token: 0x0600022A RID: 554 RVA: 0x0000E92D File Offset: 0x0000CB2D
        protected override void OnEditModeVisibilityChanged(bool currentVisibility)
        {
            if (this._animatedEntity != null)
            {
                this._animatedEntity.SetVisibilityExcludeParents(currentVisibility);
                if (!base.GameEntity.IsGhostObject())
                {
                    this._resyncAnimations = true;
                }
            }
        }

        // Token: 0x0600022B RID: 555 RVA: 0x0000E960 File Offset: 0x0000CB60
        protected override void OnEditorTick(float dt)
        {
            if (this._animatedEntity != null)
            {
                if (this._resyncAnimations)
                {
                    this.ResetAnimations();
                    this._resyncAnimations = false;
                }
                bool flag = this._animatedEntity.IsVisibleIncludeParents();
                if (flag && !MBEditor.HelpersEnabled())
                {
                    this._animatedEntity.SetVisibilityExcludeParents(false);
                    flag = false;
                }
                if (flag)
                {
                    this.UpdateAnimatedEntityFrame();
                }
            }
        }

        // Token: 0x0600022C RID: 556 RVA: 0x0000E9BD File Offset: 0x0000CBBD
        protected override void OnEditorInit()
        {
            this._itemsForBones = new List<PE_AnimationPoint.ItemForBone>();
            this.SetActionCodes();
            this.InitParameters();
            if (!base.GameEntity.IsGhostObject())
            {
                this.CreateVisualizer();
            }
        }

        // Token: 0x0600022D RID: 557 RVA: 0x0000E9EC File Offset: 0x0000CBEC
        protected override void OnRemoved(int removeReason)
        {
            base.OnRemoved(removeReason);
            if (this._animatedEntity != null && this._animatedEntity.Scene == base.GameEntity.Scene)
            {
                this._animatedEntity.Remove(removeReason);
                this._animatedEntity = null;
            }
        }

        // Token: 0x0600022E RID: 558 RVA: 0x0000EA40 File Offset: 0x0000CC40
        protected void ResetAnimations()
        {
            ActionIndexCache actionIndexCache = ActionIndexCache.act_none;
            int numberOfActionSets = MBActionSet.GetNumberOfActionSets();
            for (int i = 0; i < numberOfActionSets; i++)
            {
                MBActionSet actionSetWithIndex = MBActionSet.GetActionSetWithIndex(i);
                if (this.ArriveActionCode == ActionIndexCache.act_none || MBActionSet.CheckActionAnimationClipExists(actionSetWithIndex, this.ArriveActionCode))
                {
                    if (this.PairLoopStartActionCode != ActionIndexCache.act_none && MBActionSet.CheckActionAnimationClipExists(actionSetWithIndex, this.PairLoopStartActionCode))
                    {
                        actionIndexCache = this.PairLoopStartActionCode;
                        break;
                    }
                    if (this.LoopStartActionCode != ActionIndexCache.act_none && MBActionSet.CheckActionAnimationClipExists(actionSetWithIndex, this.LoopStartActionCode))
                    {
                        actionIndexCache = this.LoopStartActionCode;
                        break;
                    }
                }
            }
            if (actionIndexCache != null && actionIndexCache != ActionIndexCache.act_none)
            {
                ActionIndexCache actionIndex = ActionIndexCache.Create("act_jump_loop");
                this._animatedEntity.Skeleton.SetAgentActionChannel(0, actionIndex, 0f, -0.2f, true);
                this._animatedEntity.Skeleton.SetAgentActionChannel(0, actionIndexCache, 0f, -0.2f, true);
            }
        }

        // Token: 0x0600022F RID: 559 RVA: 0x0000EB3E File Offset: 0x0000CD3E
        protected override void OnEditorVariableChanged(string variableName)
        {
            if (this.ShouldUpdateOnEditorVariableChanged(variableName))
            {
                if (this._animatedEntity != null)
                {
                    this._animatedEntity.Remove(91);
                }
                this.SetActionCodes();
                this.CreateVisualizer();
            }
        }

        // Token: 0x06000230 RID: 560 RVA: 0x0000EB70 File Offset: 0x0000CD70
        public void RequestResync()
        {
            this._resyncAnimations = true;
        }

        // Token: 0x06000231 RID: 561 RVA: 0x0000EB79 File Offset: 0x0000CD79
        public override void AfterMissionStart()
        {
            if (Agent.Main != null && this.LoopStartActionCode != ActionIndexCache.act_none && !MBActionSet.CheckActionAnimationClipExists(Agent.Main.ActionSet, this.LoopStartActionCode))
            {
                base.IsDisabledForPlayers = true;
            }
        }

        // Token: 0x06000232 RID: 562 RVA: 0x0000EBB2 File Offset: 0x0000CDB2
        protected virtual bool ShouldUpdateOnEditorVariableChanged(string variableName)
        {
            return variableName == "ArriveAction" || variableName == "LoopStartAction" || variableName == "PairLoopStartAction";
        }

        // Token: 0x06000233 RID: 563 RVA: 0x0000EBDB File Offset: 0x0000CDDB
        protected void ClearAssignedItems()
        {
            this.SetAgentItemsVisibility(false);
            this._itemsForBones.Clear();
        }

        // Token: 0x06000234 RID: 564 RVA: 0x0000EBEF File Offset: 0x0000CDEF
        protected void AssignItemToBone(PE_AnimationPoint.ItemForBone newItem)
        {
            if (!string.IsNullOrEmpty(newItem.ItemPrefabName) && !this._itemsForBones.Contains(newItem))
            {
                this._itemsForBones.Add(newItem);
            }
        }

        // Token: 0x06000235 RID: 565 RVA: 0x0000EC18 File Offset: 0x0000CE18
        public override bool IsDisabledForAgent(Agent agent)
        {
            if (base.HasUser && base.UserAgent == agent)
            {
                return !this.IsActive || base.IsDeactivated;
            }
            if (!this.IsActive || agent.MountAgent != null || base.IsDeactivated || !agent.IsOnLand() || (!agent.IsAIControlled && (base.IsDisabledForPlayers || base.HasUser)))
            {
                return true;
            }
            GameEntity parent = base.GameEntity.Parent;
            if (parent == null || !parent.HasScriptOfType<UsableMachine>() || !base.GameEntity.HasTag("alternative"))
            {
                return base.IsDisabledForAgent(agent);
            }
            if (agent.IsAIControlled && parent.HasTag("reserved"))
            {
                return true;
            }
            return false;
        }

        // Token: 0x06000236 RID: 566 RVA: 0x0000EDA4 File Offset: 0x0000CFA4
        protected override void OnInit()
        {
            base.OnInit();
            this._itemsForBones = new List<PE_AnimationPoint.ItemForBone>();
            this.SetActionCodes();
            this.InitParameters();
            base.SetScriptComponentToTick(this.GetTickRequirement());
        }

        // Token: 0x06000237 RID: 567 RVA: 0x0000EDD0 File Offset: 0x0000CFD0
        private void InitParameters()
        {
            this._greetingTimer = null;
            this._pointRotation = Vec3.Zero;
            this._state = PE_AnimationPoint.State.NotUsing;
            // this._pairPoints = this.GetPairs(this.PairEntity);
            if (this.ActivatePairs)
            {
                this.SetPairsActivity(false);
            }
            this.LockUserPositions = true;
        }

        // Token: 0x06000238 RID: 568 RVA: 0x0000EE20 File Offset: 0x0000D020
        protected virtual void SetActionCodes()
        {
            this.ArriveActionCode = ActionIndexCache.Create(this.ArriveAction);
            this.LoopStartActionCode = ActionIndexCache.Create(this.LoopStartAction);
            this.PairLoopStartActionCode = ActionIndexCache.Create(this.PairLoopStartAction);
            this.LeaveActionCode = ActionIndexCache.Create(this.LeaveAction);
            this.SelectedRightHandItem = this.RightHandItem;
            this.SelectedLeftHandItem = this.LeftHandItem;
        }

        // Token: 0x06000239 RID: 569 RVA: 0x0000EE89 File Offset: 0x0000D089
        protected override bool DoesActionTypeStopUsingGameObject(Agent.ActionCodeType actionType)
        {
            return false;
        }

        // Token: 0x0600023A RID: 570 RVA: 0x0000EE8C File Offset: 0x0000D08C
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            if (base.HasUser)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick;
            }
            return base.GetTickRequirement();
        }

        // Token: 0x0600023B RID: 571 RVA: 0x0000EEA5 File Offset: 0x0000D0A5
        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            this.Tick(dt, false);
        }

        // Token: 0x0600023C RID: 572 RVA: 0x0000EEB8 File Offset: 0x0000D0B8
        private List<PE_AnimationPoint> GetPairs(GameEntity entity)
        {
            List<PE_AnimationPoint> list = new List<PE_AnimationPoint>();
            if (entity != null)
            {
                if (entity.HasScriptOfType<PE_AnimationPoint>())
                {
                    PE_AnimationPoint firstScriptOfType = entity.GetFirstScriptOfType<PE_AnimationPoint>();
                    list.Add(firstScriptOfType);
                }
                else
                {
                    foreach (GameEntity entity2 in entity.GetChildren())
                    {
                        list.AddRange(this.GetPairs(entity2));
                    }
                }
            }
            if (list.Contains(this))
            {
                list.Remove(this);
            }
            return list;
        }

        // Token: 0x0600023D RID: 573 RVA: 0x0000EF44 File Offset: 0x0000D144
        public override WorldFrame GetUserFrameForAgent(Agent agent)
        {
            WorldFrame userFrameForAgent = base.GetUserFrameForAgent(agent);
            float agentScale = agent.AgentScale;
            userFrameForAgent.Origin.SetVec2(userFrameForAgent.Origin.AsVec2 + (userFrameForAgent.Rotation.f.AsVec2 * -this.ForwardDistanceToPivotPoint + userFrameForAgent.Rotation.s.AsVec2 * this.SideDistanceToPivotPoint) * (1f - agentScale));
            return userFrameForAgent;
        }

        // Token: 0x0600023E RID: 574 RVA: 0x0000EFC8 File Offset: 0x0000D1C8
        private void Tick(float dt, bool isSimulation = false)
        {
            if (base.HasUser)
            {
                if (Game.Current != null && Game.Current.IsDevelopmentMode)
                {
                    base.UserAgent.GetTargetPosition().IsNonZero();
                }
                ActionIndexValueCache currentActionValue = base.UserAgent.GetCurrentActionValue(0);
                switch (this._state)
                {
                    case PE_AnimationPoint.State.NotUsing:
                        if (this.IsTargetReached() && base.UserAgent.MovementVelocity.LengthSquared < 0.1f && base.UserAgent.IsOnLand())
                        {
                            if (this.ArriveActionCode != ActionIndexCache.act_none)
                            {
                                Agent userAgent = base.UserAgent;
                                int channelNo = 0;
                                ActionIndexCache arriveActionCode = this.ArriveActionCode;
                                bool ignorePriority = false;
                                ulong additionalFlags = 0UL;
                                float blendWithNextActionFactor = 0f;
                                float blendInPeriod = (float)(isSimulation ? 0 : 0);
                                userAgent.SetActionChannel(channelNo, arriveActionCode, ignorePriority, additionalFlags, blendWithNextActionFactor, MBRandom.RandomFloatRanged(0.8f, 1f), blendInPeriod, 0.4f, 0f, false, -0.2f, 0, true);
                            }
                            this._state = PE_AnimationPoint.State.StartToUse;
                            return;
                        }
                        break;
                    case PE_AnimationPoint.State.StartToUse:
                        if (this.ArriveActionCode != ActionIndexCache.act_none && isSimulation)
                        {
                            this.SimulateAnimations(0.1f);
                        }
                        if (this.ArriveActionCode == ActionIndexCache.act_none || currentActionValue == this.ArriveActionCode || base.UserAgent.ActionSet.AreActionsAlternatives(currentActionValue, this.ArriveActionCode))
                        {
                            base.UserAgent.ClearTargetFrame();
                            WorldFrame userFrameForAgent = this.GetUserFrameForAgent(base.UserAgent);
                            this._pointRotation = userFrameForAgent.Rotation.f;
                            this._pointRotation.Normalize();
                            if (base.UserAgent != Agent.Main)
                            {
                                base.UserAgent.SetScriptedPositionAndDirection(ref userFrameForAgent.Origin, userFrameForAgent.Rotation.f.AsVec2.RotationInRadians, false, Agent.AIScriptedFrameFlags.DoNotRun);
                            }
                            this._state = PE_AnimationPoint.State.Using;
                            return;
                        }
                        break;
                    case PE_AnimationPoint.State.Using:
                        if (isSimulation)
                        {
                            float dt2 = 0.1f;
                            if (currentActionValue != this.ArriveActionCode)
                            {
                                dt2 = 0.01f + MBRandom.RandomFloat * 0.09f;
                            }
                            this.SimulateAnimations(dt2);
                        }
                        if (!this.IsArriveActionFinished && (this.ArriveActionCode == ActionIndexCache.act_none || base.UserAgent.GetCurrentActionValue(0) != this.ArriveActionCode))
                        {
                            this.IsArriveActionFinished = true;
                            this.AddItemsToAgent();
                        }
                        if (this.IsRotationCorrectDuringUsage())
                        {
                            base.UserAgent.SetActionChannel(0, this.LoopStartActionCode, false, 0UL, 0f, (this.ActionSpeed < 0.8f) ? this.ActionSpeed : MBRandom.RandomFloatRanged(0.8f, this.ActionSpeed), (float)(isSimulation ? 0 : 0), 0.4f, isSimulation ? MBRandom.RandomFloatRanged(0f, 0.5f) : 0f, false, -0.2f, 0, true);
                        }
                        if (this.IsArriveActionFinished && base.UserAgent != Agent.Main)
                        {
                            // this.PairTick(isSimulation);
                        }
                        break;
                    default:
                        return;
                }
            }
        }

        // Token: 0x0600023F RID: 575 RVA: 0x0000F2A4 File Offset: 0x0000D4A4
        /*private void PairTick(bool isSimulation)
        {
            if (this._pairState != PE_AnimationPoint.PairState.NoPair && pairEntityUsers.Count < this.MinUserToStartInteraction)
            {
                this._pairState = PE_AnimationPoint.PairState.NoPair;
                base.UserAgent.SetActionChannel(0, this._lastAction, false, (ulong)((long)base.UserAgent.GetCurrentActionPriority(0)), 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                base.UserAgent.ResetLookAgent();
                this._greetingTimer = null;
            }
            else if (this._pairState == PE_AnimationPoint.PairState.NoPair && pairEntityUsers.Count >= this.MinUserToStartInteraction && this.IsRotationCorrectDuringUsage())
            {
                this._lastAction = base.UserAgent.GetCurrentActionValue(0);
                if (this._startPairAnimationWithGreeting)
                {
                    this._pairState = PE_AnimationPoint.PairState.BecomePair;
                    this._greetingTimer = new Timer(Mission.Current.CurrentTime, (float)MBRandom.RandomInt(5) * 0.3f, true);
                }
                else
                {
                    this._pairState = PE_AnimationPoint.PairState.StartPairAnimation;
                }
            }
            else if (this._pairState == PE_AnimationPoint.PairState.BecomePair && this._greetingTimer.Check(Mission.Current.CurrentTime))
            {
                this._greetingTimer = null;
                this._pairState = PE_AnimationPoint.PairState.Greeting;
                Vec3 eyeGlobalPosition = pairEntityUsers.GetRandomElement<Agent>().GetEyeGlobalPosition();
                Vec3 eyeGlobalPosition2 = base.UserAgent.GetEyeGlobalPosition();
                Vec3 v = eyeGlobalPosition - eyeGlobalPosition2;
                v.Normalize();
                Mat3 rotation = base.UserAgent.Frame.rotation;
                if (Vec3.DotProduct(rotation.f, v) > 0f)
                {
                    ActionIndexCache greetingActionId = this.GetGreetingActionId(eyeGlobalPosition2, eyeGlobalPosition, rotation);
                    base.UserAgent.SetActionChannel(1, greetingActionId, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                }
            }
            else if (this._pairState == PE_AnimationPoint.PairState.Greeting && base.UserAgent.GetCurrentActionValue(1) == ActionIndexCache.act_none)
            {
                this._pairState = PE_AnimationPoint.PairState.StartPairAnimation;
            }
            if (this._pairState == PE_AnimationPoint.PairState.StartPairAnimation)
            {
                this._pairState = PE_AnimationPoint.PairState.Pair;
                base.UserAgent.SetActionChannel(0, this.PairLoopStartActionCode, false, 0UL, 0f, MBRandom.RandomFloatRanged(0.8f, this.ActionSpeed), (float)(isSimulation ? 0 : 0), 0.4f, isSimulation ? MBRandom.RandomFloatRanged(0f, 0.5f) : 0f, false, -0.2f, 0, true);
            }
            if (this._pairState == PE_AnimationPoint.PairState.Pair && this.IsRotationCorrectDuringUsage())
            {
                base.UserAgent.SetActionChannel(0, this.PairLoopStartActionCode, false, 0UL, 0f, MBRandom.RandomFloatRanged(0.8f, this.ActionSpeed), (float)(isSimulation ? 0 : 0), 0.4f, isSimulation ? MBRandom.RandomFloatRanged(0f, 0.5f) : 0f, false, -0.2f, 0, true);
            }
        }*/

        // Token: 0x06000240 RID: 576 RVA: 0x0000F5A8 File Offset: 0x0000D7A8
        private ActionIndexCache GetGreetingActionId(Vec3 userAgentGlobalEyePoint, Vec3 lookTarget, Mat3 userAgentRot)
        {
            Vec3 vec = lookTarget - userAgentGlobalEyePoint;
            vec.Normalize();
            float num = Vec3.DotProduct(userAgentRot.f, vec);
            if (num > 0.8f)
            {
                return this._greetingFrontActions[MBRandom.RandomInt(this._greetingFrontActions.Length)];
            }
            if (num <= 0f)
            {
                return ActionIndexCache.act_none;
            }
            if (Vec3.DotProduct(Vec3.CrossProduct(vec, userAgentRot.f), userAgentRot.u) > 0f)
            {
                return this._greetingRightActions[MBRandom.RandomInt(this._greetingRightActions.Length)];
            }
            return this._greetingLeftActions[MBRandom.RandomInt(this._greetingLeftActions.Length)];
        }

        // Token: 0x06000242 RID: 578 RVA: 0x0000F6D0 File Offset: 0x0000D8D0
        private void SetPairsActivity(bool isActive)
        {
            foreach (PE_AnimationPoint animationPoint in this._pairPoints)
            {
                animationPoint.IsActive = isActive;
                if (!isActive)
                {
                    if (animationPoint.HasAIUser)
                    {
                        animationPoint.UserAgent.StopUsingGameObject(true, Agent.StopUsingGameObjectFlags.AutoAttachAfterStoppingUsingGameObject);
                    }
                    Agent movingAgent = animationPoint.MovingAgent;
                    if (movingAgent != null)
                    {
                        movingAgent.StopUsingGameObject(true, Agent.StopUsingGameObjectFlags.AutoAttachAfterStoppingUsingGameObject);
                    }
                }
            }
        }

        // Token: 0x06000243 RID: 579 RVA: 0x0000F750 File Offset: 0x0000D950
        public override bool IsUsableByAgent(Agent userAgent)
        {
            return this.IsActive && base.IsUsableByAgent(userAgent);
        }

        // Token: 0x06000244 RID: 580 RVA: 0x0000F764 File Offset: 0x0000D964
        public override void OnUse(Agent userAgent)
        {
            base.OnUse(userAgent);
            this._equipmentIndexMainHand = base.UserAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            this._equipmentIndexOffHand = base.UserAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            this._state = PE_AnimationPoint.State.NotUsing;
            if (this.ActivatePairs)
            {
                this.SetPairsActivity(true);
            }
        }

        // Token: 0x06000245 RID: 581 RVA: 0x0000F7B4 File Offset: 0x0000D9B4
        private void RevertWeaponWieldSheathState()
        {
            if (this._equipmentIndexMainHand != EquipmentIndex.None && this.AutoSheathWeapons)
            {
                base.UserAgent.TryToWieldWeaponInSlot(this._equipmentIndexMainHand, Agent.WeaponWieldActionType.WithAnimation, false);
            }
            else if (this._equipmentIndexMainHand == EquipmentIndex.None && this.AutoWieldWeapons)
            {
                base.UserAgent.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.WithAnimation);
            }
            if (this._equipmentIndexOffHand != EquipmentIndex.None && this.AutoSheathWeapons)
            {
                base.UserAgent.TryToWieldWeaponInSlot(this._equipmentIndexOffHand, Agent.WeaponWieldActionType.WithAnimation, false);
                return;
            }
            if (this._equipmentIndexOffHand == EquipmentIndex.None && this.AutoWieldWeapons)
            {
                base.UserAgent.TryToSheathWeaponInHand(Agent.HandIndex.OffHand, Agent.WeaponWieldActionType.WithAnimation);
            }
        }

        // Token: 0x06000246 RID: 582 RVA: 0x0000F848 File Offset: 0x0000DA48
        public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
        {
            this.SetAgentItemsVisibility(false);
            this.RevertWeaponWieldSheathState();
            if (base.UserAgent.IsActive())
            {
                if (this.LeaveActionCode == ActionIndexCache.act_none)
                {
                    base.UserAgent.SetActionChannel(0, this.LeaveActionCode, false, (ulong)((long)base.UserAgent.GetCurrentActionPriority(0)), 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                }
                else if (this.IsArriveActionFinished)
                {
                    ActionIndexValueCache currentActionValue = base.UserAgent.GetCurrentActionValue(0);
                    if (currentActionValue != this.LeaveActionCode && !base.UserAgent.ActionSet.AreActionsAlternatives(currentActionValue, this.LeaveActionCode))
                    {
                        base.UserAgent.SetActionChannel(0, this.LeaveActionCode, false, (ulong)((long)base.UserAgent.GetCurrentActionPriority(0)), 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                    }
                }
                else
                {
                    ActionIndexValueCache currentActionValue2 = userAgent.GetCurrentActionValue(0);
                    if (currentActionValue2 == this.ArriveActionCode && this.ArriveActionCode != ActionIndexCache.act_none)
                    {
                        MBActionSet actionSet = userAgent.ActionSet;
                        float currentActionProgress = userAgent.GetCurrentActionProgress(0);
                        float actionBlendOutStartProgress = MBActionSet.GetActionBlendOutStartProgress(actionSet, currentActionValue2);
                        if (currentActionProgress < actionBlendOutStartProgress)
                        {
                            float num = (actionBlendOutStartProgress - currentActionProgress) / actionBlendOutStartProgress;
                            MBActionSet.GetActionBlendOutStartProgress(actionSet, this.LeaveActionCode);
                        }
                    }
                }
            }
            this._pairState = PE_AnimationPoint.PairState.NoPair;
            this._lastAction = ActionIndexValueCache.act_none;
            if (base.UserAgent.GetLookAgent() != null)
            {
                base.UserAgent.ResetLookAgent();
            }
            this.IsArriveActionFinished = false;
            base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
            if (this.ActivatePairs)
            {
                this.SetPairsActivity(false);
            }
        }

        // Token: 0x06000247 RID: 583 RVA: 0x0000F9FE File Offset: 0x0000DBFE
        public override void SimulateTick(float dt)
        {
            this.Tick(dt, true);
        }

        // Token: 0x06000248 RID: 584 RVA: 0x0000FA08 File Offset: 0x0000DC08
        public override bool HasAlternative()
        {
            return this.GroupId >= 0;
        }

        // Token: 0x06000249 RID: 585 RVA: 0x0000FA18 File Offset: 0x0000DC18
        public float GetRandomWaitInSeconds()
        {
            if (this.MinWaitinSeconds < 0f || this.MaxWaitInSeconds < 0f)
            {
                return -1f;
            }
            if (MathF.Abs(this.MinWaitinSeconds - this.MaxWaitInSeconds) >= 1E-45f)
            {
                return this.MinWaitinSeconds + MBRandom.RandomFloat * (this.MaxWaitInSeconds - this.MinWaitinSeconds);
            }
            return this.MinWaitinSeconds;
        }

        // Token: 0x0600024A RID: 586 RVA: 0x0000FA80 File Offset: 0x0000DC80
        public List<PE_AnimationPoint> GetAlternatives()
        {
            List<PE_AnimationPoint> list = new List<PE_AnimationPoint>();
            IEnumerable<GameEntity> children = base.GameEntity.Parent.GetChildren();
            if (children != null)
            {
                foreach (GameEntity gameEntity in children)
                {
                    PE_AnimationPoint firstScriptOfType = gameEntity.GetFirstScriptOfType<PE_AnimationPoint>();
                    if (firstScriptOfType != null && firstScriptOfType.HasAlternative() && this.GroupId == firstScriptOfType.GroupId)
                    {
                        list.Add(firstScriptOfType);
                    }
                }
            }
            return list;
        }

        // Token: 0x0600024B RID: 587 RVA: 0x0000FB04 File Offset: 0x0000DD04
        private void SimulateAnimations(float dt)
        {
            base.UserAgent.TickActionChannels(dt);
            Vec3 v = base.UserAgent.ComputeAnimationDisplacement(dt);
            if (v.LengthSquared > 0f)
            {
                base.UserAgent.TeleportToPosition(base.UserAgent.Position + v);
            }
            base.UserAgent.AgentVisuals.GetSkeleton().TickAnimations(dt, base.UserAgent.AgentVisuals.GetGlobalFrame(), true);
        }

        // Token: 0x0600024C RID: 588 RVA: 0x0000FB7C File Offset: 0x0000DD7C
        private bool IsTargetReached()
        {
            float num = Vec2.DotProduct(base.UserAgent.GetTargetDirection().AsVec2, base.UserAgent.GetMovementDirection());
            return (base.UserAgent.Position.AsVec2 - base.UserAgent.GetTargetPosition()).LengthSquared < 0.040000003f && num > 0.99f;
        }

        // Token: 0x0600024D RID: 589 RVA: 0x0000FBE9 File Offset: 0x0000DDE9
        public bool IsRotationCorrectDuringUsage()
        {
            return this._pointRotation.IsNonZero && Vec2.DotProduct(this._pointRotation.AsVec2, base.UserAgent.GetMovementDirection()) > 0.99f;
        }

        // Token: 0x0600024E RID: 590 RVA: 0x0000FC1C File Offset: 0x0000DE1C
        protected bool CanAgentUseItem(Agent agent)
        {
            return true;
        }

        // Token: 0x0600024F RID: 591 RVA: 0x0000FC38 File Offset: 0x0000DE38
        protected void AddItemsToAgent()
        {
            if (this.CanAgentUseItem(base.UserAgent) && this.IsArriveActionFinished)
            {
                if (this._itemsForBones.Count != 0)
                {
                    // base.UserAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.HoldAndHideRecentlyUsedMeshes();
                }
                foreach (PE_AnimationPoint.ItemForBone itemForBone in this._itemsForBones)
                {
                    ItemObject @object = Game.Current.ObjectManager.GetObject<ItemObject>(itemForBone.ItemPrefabName);
                    if (@object != null)
                    {
                        EquipmentIndex equipmentIndex = this.FindProperSlot(@object);
                        if (!base.UserAgent.Equipment[equipmentIndex].IsEmpty)
                        {
                            base.UserAgent.DropItem(equipmentIndex, WeaponClass.Undefined);
                        }
                        ItemObject item = @object;
                        ItemModifier itemModifier = null;
                        IAgentOriginBase origin = base.UserAgent.Origin;
                        MissionWeapon missionWeapon = new MissionWeapon(item, itemModifier, (origin != null) ? origin.Banner : null);
                        base.UserAgent.EquipWeaponWithNewEntity(equipmentIndex, ref missionWeapon);
                        base.UserAgent.TryToWieldWeaponInSlot(equipmentIndex, Agent.WeaponWieldActionType.Instant, false);
                    }
                    else
                    {
                        sbyte realBoneIndex = base.UserAgent.AgentVisuals.GetRealBoneIndex(itemForBone.HumanBone);
                    }
                }
            }
        }

        // Token: 0x06000252 RID: 594 RVA: 0x0000FEC8 File Offset: 0x0000E0C8
        public void SetAgentItemsVisibility(bool isVisible)
        {
            if (!base.UserAgent.IsMainAgent)
            {
                foreach (PE_AnimationPoint.ItemForBone itemForBone in this._itemsForBones)
                {
                    sbyte realBoneIndex = base.UserAgent.AgentVisuals.GetRealBoneIndex(itemForBone.HumanBone);
                    //base.UserAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SetPrefabVisibility(realBoneIndex, itemForBone.ItemPrefabName, isVisible);
                    PE_AnimationPoint.ItemForBone itemForBone2 = itemForBone;
                    itemForBone2.IsVisible = isVisible;
                }
            }
        }

        // Token: 0x06000253 RID: 595 RVA: 0x0000FF60 File Offset: 0x0000E160
        private void SetAgentItemVisibility(PE_AnimationPoint.ItemForBone item, bool isVisible)
        {
            sbyte realBoneIndex = base.UserAgent.AgentVisuals.GetRealBoneIndex(item.HumanBone);
            // base.UserAgent.GetComponent<CampaignAgentComponent>().AgentNavigator.SetPrefabVisibility(realBoneIndex, item.ItemPrefabName, isVisible);
            item.IsVisible = isVisible;
        }

        // Token: 0x06000254 RID: 596 RVA: 0x0000FFAC File Offset: 0x0000E1AC
        private EquipmentIndex FindProperSlot(ItemObject item)
        {
            EquipmentIndex result = EquipmentIndex.Weapon3;
            for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex <= EquipmentIndex.Weapon3; equipmentIndex++)
            {
                if (base.UserAgent.Equipment[equipmentIndex].IsEmpty)
                {
                    result = equipmentIndex;
                }
                else if (base.UserAgent.Equipment[equipmentIndex].Item == item)
                {
                    return equipmentIndex;
                }
            }
            return result;
        }

        // Token: 0x040000F2 RID: 242
        private const string AlternativeTag = "alternative";

        // Token: 0x040000F3 RID: 243
        private const float RangeThreshold = 0.2f;

        // Token: 0x040000F4 RID: 244
        private const float RotationScoreThreshold = 0.99f;

        // Token: 0x040000F5 RID: 245
        private const float ActionSpeedRandomMinValue = 0.8f;

        // Token: 0x040000F6 RID: 246
        private const float AnimationRandomProgressMaxValue = 0.5f;

        // Token: 0x040000F7 RID: 247
        public string ArriveAction = "";

        // Token: 0x040000F8 RID: 248
        public string LoopStartAction = "";

        // Token: 0x040000F9 RID: 249
        public string PairLoopStartAction = "";

        // Token: 0x040000FA RID: 250
        public string LeaveAction = "";

        // Token: 0x040000FB RID: 251
        public int GroupId = -1;

        // Token: 0x040000FC RID: 252
        public string RightHandItem = "";

        // Token: 0x040000FD RID: 253
        public HumanBone RightHandItemBone = HumanBone.ItemR;

        // Token: 0x040000FE RID: 254
        public string LeftHandItem = "";

        // Token: 0x040000FF RID: 255
        public HumanBone LeftHandItemBone = HumanBone.ItemL;

        // Token: 0x04000101 RID: 257
        public int MinUserToStartInteraction = 1;

        // Token: 0x04000102 RID: 258
        public bool ActivatePairs;

        // Token: 0x04000103 RID: 259
        public float MinWaitinSeconds = 30f;

        // Token: 0x04000104 RID: 260
        public float MaxWaitInSeconds = 120f;

        // Token: 0x04000105 RID: 261
        public float ForwardDistanceToPivotPoint;

        // Token: 0x04000106 RID: 262
        public float SideDistanceToPivotPoint;

        // Token: 0x04000107 RID: 263
        private bool _startPairAnimationWithGreeting;

        // Token: 0x04000108 RID: 264
        protected float ActionSpeed = 1f;

        // Token: 0x04000109 RID: 265
        public bool KeepOldVisibility;

        // Token: 0x0400010B RID: 267
        private ActionIndexCache ArriveActionCode;

        // Token: 0x0400010C RID: 268
        protected ActionIndexCache LoopStartActionCode;

        // Token: 0x0400010D RID: 269
        protected ActionIndexCache PairLoopStartActionCode;

        // Token: 0x0400010E RID: 270
        private ActionIndexCache LeaveActionCode;

        // Token: 0x0400010F RID: 271
        protected ActionIndexCache DefaultActionCode;

        // Token: 0x04000110 RID: 272
        private bool _resyncAnimations;

        // Token: 0x04000111 RID: 273
        private string _selectedRightHandItem;

        // Token: 0x04000112 RID: 274
        private string _selectedLeftHandItem;

        // Token: 0x04000113 RID: 275
        private PE_AnimationPoint.State _state;

        // Token: 0x04000114 RID: 276
        private PE_AnimationPoint.PairState _pairState;

        // Token: 0x04000115 RID: 277
        private Vec3 _pointRotation;

        // Token: 0x04000116 RID: 278
        private List<PE_AnimationPoint> _pairPoints;

        // Token: 0x04000117 RID: 279
        private List<PE_AnimationPoint.ItemForBone> _itemsForBones;

        // Token: 0x04000118 RID: 280
        private ActionIndexValueCache _lastAction;

        // Token: 0x04000119 RID: 281
        private Timer _greetingTimer;

        // Token: 0x0400011A RID: 282
        private GameEntity _animatedEntity;

        // Token: 0x0400011B RID: 283
        private Vec3 _animatedEntityDisplacement = Vec3.Zero;

        // Token: 0x0400011C RID: 284
        private EquipmentIndex _equipmentIndexMainHand;

        // Token: 0x0400011D RID: 285
        private EquipmentIndex _equipmentIndexOffHand;

        // Token: 0x0400011F RID: 287
        private readonly ActionIndexCache[] _greetingFrontActions = new ActionIndexCache[]
        {
            ActionIndexCache.Create("act_greeting_front_1"),
            ActionIndexCache.Create("act_greeting_front_2"),
            ActionIndexCache.Create("act_greeting_front_3"),
            ActionIndexCache.Create("act_greeting_front_4")
        };

        // Token: 0x04000120 RID: 288
        private readonly ActionIndexCache[] _greetingRightActions = new ActionIndexCache[]
        {
            ActionIndexCache.Create("act_greeting_right_1"),
            ActionIndexCache.Create("act_greeting_right_2"),
            ActionIndexCache.Create("act_greeting_right_3"),
            ActionIndexCache.Create("act_greeting_right_4")
        };

        // Token: 0x04000121 RID: 289
        private readonly ActionIndexCache[] _greetingLeftActions = new ActionIndexCache[]
        {
            ActionIndexCache.Create("act_greeting_left_1"),
            ActionIndexCache.Create("act_greeting_left_2"),
            ActionIndexCache.Create("act_greeting_left_3"),
            ActionIndexCache.Create("act_greeting_left_4")
        };

        // Token: 0x0200010A RID: 266
        protected struct ItemForBone
        {
            // Token: 0x06000CB9 RID: 3257 RVA: 0x00061EB3 File Offset: 0x000600B3
            public ItemForBone(HumanBone bone, string name, bool isVisible)
            {
                this.HumanBone = bone;
                this.ItemPrefabName = name;
                this.IsVisible = isVisible;
                this.OldVisibility = isVisible;
            }

            // Token: 0x04000532 RID: 1330
            public HumanBone HumanBone;

            // Token: 0x04000533 RID: 1331
            public string ItemPrefabName;

            // Token: 0x04000534 RID: 1332
            public bool IsVisible;

            // Token: 0x04000535 RID: 1333
            public bool OldVisibility;
        }

        // Token: 0x0200010B RID: 267
        private enum State
        {
            // Token: 0x04000537 RID: 1335
            NotUsing,
            // Token: 0x04000538 RID: 1336
            StartToUse,
            // Token: 0x04000539 RID: 1337
            Using
        }

        // Token: 0x0200010C RID: 268
        private enum PairState
        {
            // Token: 0x0400053B RID: 1339
            NoPair,
            // Token: 0x0400053C RID: 1340
            BecomePair,
            // Token: 0x0400053D RID: 1341
            Greeting,
            // Token: 0x0400053E RID: 1342
            StartPairAnimation,
            // Token: 0x0400053F RID: 1343
            Pair
        }
    }
}

using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_ArrowBarrel : UsableMachine
    {
        public int RequiredGold = 500;
        // Token: 0x170008A9 RID: 2217
        // (get) Token: 0x06003042 RID: 12354 RVA: 0x000C66D0 File Offset: 0x000C48D0
        private static int _pickupArrowSoundFromBarrelCache
        {
            get
            {
                if (PE_ArrowBarrel._pickupArrowSoundFromBarrel == -1)
                {
                    PE_ArrowBarrel._pickupArrowSoundFromBarrel = SoundEvent.GetEventIdFromString("event:/mission/combat/pickup_arrows");
                }
                return PE_ArrowBarrel._pickupArrowSoundFromBarrel;
            }
        }

        // Token: 0x06003043 RID: 12355 RVA: 0x000C66EE File Offset: 0x000C48EE
        protected PE_ArrowBarrel()
        {
        }

        // Token: 0x06003044 RID: 12356 RVA: 0x000C6700 File Offset: 0x000C4900
        protected override void OnInit()
        {
            base.OnInit();
            foreach (StandingPoint standingPoint in base.StandingPoints)
            {
                (standingPoint as StandingPointWithWeaponRequirement).InitRequiredWeaponClasses(WeaponClass.Arrow, WeaponClass.Bolt);
            }
            base.SetScriptComponentToTick(this.GetTickRequirement());
            this.MakeVisibilityCheck = false;
            this._isVisible = base.GameEntity.IsVisibleIncludeParents();
        }

        // Token: 0x06003045 RID: 12357 RVA: 0x000C6784 File Offset: 0x000C4984
        public override void AfterMissionStart()
        {
            if (base.StandingPoints != null)
            {
                foreach (StandingPoint standingPoint in base.StandingPoints)
                {
                    // 
                }
            }
        }

        // Token: 0x06003046 RID: 12358 RVA: 0x000C67E0 File Offset: 0x000C49E0
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            TextObject textObject = new TextObject("{=bNYm3K6b}{KEY} Pick Up", null);
            textObject.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            return textObject;
        }

        // Token: 0x06003047 RID: 12359 RVA: 0x000C680A File Offset: 0x000C4A0A
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return new TextObject("{=bWi4aMO9}Arrow Barrel", null).ToString();
        }

        // Token: 0x06003048 RID: 12360 RVA: 0x000C681C File Offset: 0x000C4A1C
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            if (GameNetwork.IsClientOrReplay)
            {
                return base.GetTickRequirement();
            }
            return ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel | base.GetTickRequirement();
        }

        // Token: 0x06003049 RID: 12361 RVA: 0x000C6834 File Offset: 0x000C4A34
        protected override void OnTickParallel(float dt)
        {
            this.TickAux(true);
        }

        // Token: 0x0600304A RID: 12362 RVA: 0x000C683D File Offset: 0x000C4A3D
        protected override void OnTick(float dt)
        {
            base.OnTick(dt);
            if (this._needsSingleThreadTickOnce)
            {
                this._needsSingleThreadTickOnce = false;
                this.TickAux(false);
            }
        }

        // Token: 0x0600304B RID: 12363 RVA: 0x000C685C File Offset: 0x000C4A5C
        private void TickAux(bool isParallel)
        {
            if (this._isVisible && !GameNetwork.IsClientOrReplay)
            {
                foreach (StandingPoint standingPoint in base.StandingPoints)
                {
                    if (standingPoint.HasUser)
                    {
                        Agent userAgent = standingPoint.UserAgent;
                        ActionIndexValueCache currentActionValue = userAgent.GetCurrentActionValue(0);
                        ActionIndexValueCache currentActionValue2 = userAgent.GetCurrentActionValue(1);
                        if (!(currentActionValue2 == ActionIndexValueCache.act_none) || (!(currentActionValue == PE_ArrowBarrel.act_pickup_down_begin) && !(currentActionValue == PE_ArrowBarrel.act_pickup_down_begin_left_stance)))
                        {
                            if (currentActionValue2 == ActionIndexValueCache.act_none && (currentActionValue == PE_ArrowBarrel.act_pickup_down_end || currentActionValue == PE_ArrowBarrel.act_pickup_down_end_left_stance))
                            {
                                if (isParallel)
                                {
                                    this._needsSingleThreadTickOnce = true;
                                }
                                else
                                {
                                    PersistentEmpireRepresentative persistentEmpireRepresentative = userAgent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>();
                                    if (persistentEmpireRepresentative != null)
                                    {
                                        for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                                        {
                                            if (!userAgent.Equipment[equipmentIndex].IsEmpty && (userAgent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Arrow || userAgent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Bolt) && userAgent.Equipment[equipmentIndex].Amount < userAgent.Equipment[equipmentIndex].ModifiedMaxAmount && persistentEmpireRepresentative.ReduceIfHaveEnoughGold(this.RequiredGold))
                                            {
                                                userAgent.SetWeaponAmountInSlot(equipmentIndex, userAgent.Equipment[equipmentIndex].ModifiedMaxAmount, true);
                                                Mission.Current.MakeSoundOnlyOnRelatedPeer(PE_ArrowBarrel._pickupArrowSoundFromBarrelCache, userAgent.Position, userAgent.Index);
                                            }
                                        }
                                    }
                                    userAgent.StopUsingGameObject(true, Agent.StopUsingGameObjectFlags.AutoAttachAfterStoppingUsingGameObject);
                                }
                            }
                            else if (currentActionValue2 != ActionIndexValueCache.act_none || !userAgent.SetActionChannel(0, userAgent.GetIsLeftStance() ? PE_ArrowBarrel.act_pickup_down_begin_left_stance : PE_ArrowBarrel.act_pickup_down_begin, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true))
                            {
                                if (isParallel)
                                {
                                    this._needsSingleThreadTickOnce = true;
                                }
                                else
                                {
                                    userAgent.StopUsingGameObject(true, Agent.StopUsingGameObjectFlags.AutoAttachAfterStoppingUsingGameObject);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Token: 0x0600304C RID: 12364 RVA: 0x000C6AA4 File Offset: 0x000C4CA4
        public override OrderType GetOrder(BattleSideEnum side)
        {
            return OrderType.None;
        }

        // Token: 0x0400140A RID: 5130
        private static readonly ActionIndexCache act_pickup_down_begin = ActionIndexCache.Create("act_pickup_down_begin");

        // Token: 0x0400140B RID: 5131
        private static readonly ActionIndexCache act_pickup_down_begin_left_stance = ActionIndexCache.Create("act_pickup_down_begin_left_stance");

        // Token: 0x0400140C RID: 5132
        private static readonly ActionIndexCache act_pickup_down_end = ActionIndexCache.Create("act_pickup_down_end");

        // Token: 0x0400140D RID: 5133
        private static readonly ActionIndexCache act_pickup_down_end_left_stance = ActionIndexCache.Create("act_pickup_down_end_left_stance");

        // Token: 0x0400140E RID: 5134
        private static int _pickupArrowSoundFromBarrel = -1;

        // Token: 0x0400140F RID: 5135
        private bool _isVisible = true;

        // Token: 0x04001410 RID: 5136
        private bool _needsSingleThreadTickOnce;
    }
}

using PersistentEmpiresLib.Helpers;
using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_ItemConverter : PE_UsableFromDistance
    {
        public int AnimationDurationInSeconds = 5;
        public string Animation = "act_pe_use_smithing_machine_loop";
        public string Name = "Poison Pool";

        private long UseStartedAt = 0;
        private long UseWillEndAt = 0;

        public string InputItemId = "pe_stealing_dagger";
        public string OutputItemId = "pe_poison_dagger";
        public string RelevantSkill = "Chemist";
        public int RequiredSkill = 10;
        public bool HideItemOnAnimation = false;
        private EquipmentIndex HiddenItemIndex = EquipmentIndex.None;
        protected override bool LockUserFrames
        {
            get
            {
                return true;
            }
        }
        protected override bool LockUserPositions
        {
            get
            {
                return true;
            }
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return this.Name;
        }

        protected override void OnInit()
        {
            base.OnInit();
            base.ActionMessage = new TextObject(this.Name);
            TextObject descriptionMessage = new TextObject("Press {KEY} To Use");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
        }
        protected override void OnTick(float currentFrameDeltaTime)
        {
            this.OnTickParallel2(currentFrameDeltaTime);
        }
        protected override void OnTickOccasionally(float currentFrameDeltaTime)
        {
            this.OnTickParallel2(currentFrameDeltaTime);
        }
        protected override void OnTickParallel2(float dt)
        {
            base.OnTickParallel2(dt);
            if (GameNetwork.IsServer)
            {
                if (base.HasUser)
                {
                    if (this.UseWillEndAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    {
                        base.UserAgent.StopUsingGameObjectMT(base.UserAgent.CanUseObject(this));
                    }
                }
            }
        }
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            if (GameNetwork.IsServer)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel2;
            }
            return base.GetTickRequirement();
        }

        public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
        {
            base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
            userAgent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloatRanged(1f), false, -0.2f, 0, true);
            if (isSuccessful)
            {
                if (GameNetwork.IsServer)
                {
                    EquipmentIndex mainHandIndex;
                    if (HideItemOnAnimation)
                    {
                        mainHandIndex = HiddenItemIndex;
                    }
                    else
                    {
                        mainHandIndex = userAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                    }

                    bool flag = true;
                    if (mainHandIndex == EquipmentIndex.None)
                    {
                        flag = false;
                    }
                    if (userAgent.Equipment[mainHandIndex].IsEmpty)
                    {
                        flag = false;
                    }
                    if (userAgent.Equipment[mainHandIndex].Item.StringId != this.InputItemId)
                    {
                        flag = false;
                    }

                    if (flag)
                    {
                        ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(this.OutputItemId);
                        MissionWeapon weapon = new MissionWeapon(item, null, null);
                        userAgent.RemoveEquippedWeapon(mainHandIndex);
                        userAgent.EquipWeaponWithNewEntity(mainHandIndex, ref weapon);
                        userAgent.TryToWieldWeaponInSlot(mainHandIndex, Agent.WeaponWieldActionType.Instant, true);
                    }
                }
            }
            if (userAgent.IsMine)
            {
                PEInformationManager.StopCounter();
            }
            userAgent.ClearTargetFrame();
        }

        public override void OnUse(Agent userAgent)
        {
            if (GameNetwork.IsServer)
            {
                HiddenItemIndex = EquipmentIndex.None;
                if (base.HasUser)
                {
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }

                EquipmentIndex mainHandIndex = userAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                if (mainHandIndex == EquipmentIndex.None)
                {
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                if (userAgent.Equipment[mainHandIndex].IsEmpty)
                {
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                if (userAgent.Equipment[mainHandIndex].Item.StringId != this.InputItemId)
                {
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                HiddenItemIndex = mainHandIndex;

                SkillObject relevantSkill = MBObjectManager.Instance.GetObject<SkillObject>(this.RelevantSkill);
                int skillValue = userAgent.Character.GetSkillValue(relevantSkill);
                if (skillValue < this.RequiredSkill)
                {
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
            }
            if (this.Animation != "")
            {
                if (HideItemOnAnimation)
                {
                    userAgent.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.Instant);
                }

                ActionIndexCache actionIndexCache = ActionIndexCache.Create(this.Animation);
                userAgent.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            }
            this.UseStartedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this.UseWillEndAt = this.UseStartedAt + this.AnimationDurationInSeconds;
            userAgent.SetTargetPosition(userAgent.GetWorldFrame().Origin.AsVec2);
            if (userAgent.IsMine)
            {
                PEInformationManager.StartCounter("Converting...", this.AnimationDurationInSeconds);
            }
            base.OnUse(userAgent);
        }
    }
}

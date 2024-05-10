using PersistentEmpiresLib.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_MoneyBag : UsableMissionObject
    {
        private int _amount;
        private int _usedChannelIndex;
        private ActionIndexCache _progressActionIndex;
        private ActionIndexCache _successActionIndex;
        private static readonly ActionIndexCache act_pickup_down_begin = ActionIndexCache.Create("act_pickup_down_begin");
        private static readonly ActionIndexCache act_pickup_down_end = ActionIndexCache.Create("act_pickup_down_end");
        private static readonly ActionIndexCache act_pickup_down_begin_left_stance = ActionIndexCache.Create("act_pickup_down_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_down_end_left_stance = ActionIndexCache.Create("act_pickup_down_end_left_stance");
        private static readonly ActionIndexCache act_pickup_down_left_begin = ActionIndexCache.Create("act_pickup_down_left_begin");
        private static readonly ActionIndexCache act_pickup_down_left_end = ActionIndexCache.Create("act_pickup_down_left_end");
        private static readonly ActionIndexCache act_pickup_down_left_begin_left_stance = ActionIndexCache.Create("act_pickup_down_left_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_down_left_end_left_stance = ActionIndexCache.Create("act_pickup_down_left_end_left_stance");
        private static readonly ActionIndexCache act_pickup_middle_begin = ActionIndexCache.Create("act_pickup_middle_begin");
        private static readonly ActionIndexCache act_pickup_middle_end = ActionIndexCache.Create("act_pickup_middle_end");
        private static readonly ActionIndexCache act_pickup_middle_begin_left_stance = ActionIndexCache.Create("act_pickup_middle_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_middle_end_left_stance = ActionIndexCache.Create("act_pickup_middle_end_left_stance");
        private static readonly ActionIndexCache act_pickup_middle_left_begin = ActionIndexCache.Create("act_pickup_middle_left_begin");
        private static readonly ActionIndexCache act_pickup_middle_left_end = ActionIndexCache.Create("act_pickup_middle_left_end");
        private static readonly ActionIndexCache act_pickup_middle_left_begin_left_stance = ActionIndexCache.Create("act_pickup_middle_left_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_middle_left_end_left_stance = ActionIndexCache.Create("act_pickup_middle_left_end_left_stance");
        private static readonly ActionIndexCache act_pickup_up_begin = ActionIndexCache.Create("act_pickup_up_begin");
        private static readonly ActionIndexCache act_pickup_up_end = ActionIndexCache.Create("act_pickup_up_end");
        private static readonly ActionIndexCache act_pickup_up_begin_left_stance = ActionIndexCache.Create("act_pickup_up_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_up_end_left_stance = ActionIndexCache.Create("act_pickup_up_end_left_stance");
        private static readonly ActionIndexCache act_pickup_up_left_begin = ActionIndexCache.Create("act_pickup_up_left_begin");
        private static readonly ActionIndexCache act_pickup_up_left_end = ActionIndexCache.Create("act_pickup_up_left_end");
        private static readonly ActionIndexCache act_pickup_up_left_begin_left_stance = ActionIndexCache.Create("act_pickup_up_left_begin_left_stance");
        private static readonly ActionIndexCache act_pickup_up_left_end_left_stance = ActionIndexCache.Create("act_pickup_up_left_end_left_stance");
        private static readonly ActionIndexCache act_pickup_from_right_down_horseback_begin = ActionIndexCache.Create("act_pickup_from_right_down_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_right_down_horseback_end = ActionIndexCache.Create("act_pickup_from_right_down_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_right_down_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_right_down_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_right_down_horseback_left_end = ActionIndexCache.Create("act_pickup_from_right_down_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_begin = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_end = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_right_middle_horseback_left_end = ActionIndexCache.Create("act_pickup_from_right_middle_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_right_up_horseback_begin = ActionIndexCache.Create("act_pickup_from_right_up_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_right_up_horseback_end = ActionIndexCache.Create("act_pickup_from_right_up_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_right_up_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_right_up_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_right_up_horseback_left_end = ActionIndexCache.Create("act_pickup_from_right_up_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_left_down_horseback_begin = ActionIndexCache.Create("act_pickup_from_left_down_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_left_down_horseback_end = ActionIndexCache.Create("act_pickup_from_left_down_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_left_down_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_left_down_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_left_down_horseback_left_end = ActionIndexCache.Create("act_pickup_from_left_down_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_begin = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_end = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_left_middle_horseback_left_end = ActionIndexCache.Create("act_pickup_from_left_middle_horseback_left_end");
        private static readonly ActionIndexCache act_pickup_from_left_up_horseback_begin = ActionIndexCache.Create("act_pickup_from_left_up_horseback_begin");
        private static readonly ActionIndexCache act_pickup_from_left_up_horseback_end = ActionIndexCache.Create("act_pickup_from_left_up_horseback_end");
        private static readonly ActionIndexCache act_pickup_from_left_up_horseback_left_begin = ActionIndexCache.Create("act_pickup_from_left_up_horseback_left_begin");
        private static readonly ActionIndexCache act_pickup_from_left_up_horseback_left_end = ActionIndexCache.Create("act_pickup_from_left_up_horseback_left_end");
        protected override bool LockUserFrames { get => false; }
        protected override bool LockUserPositions { get => false; }
        protected override void OnInit()
        {
            base.OnInit();
            base.ActionMessage = new TextObject("Money Bag");
            TextObject descriptionMessage = new TextObject("Press {KEY} To Loot");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Money Bag";
        }
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            if (GameNetwork.IsServer && base.HasUser)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel2;
            }
            return base.GetTickRequirement();
        }
        protected override void OnTickOccasionally(float currentFrameDeltaTime)
        {
            this.OnTickParallel2(currentFrameDeltaTime);
        }
        public void SetAmount(int amount)
        {
            this._amount = amount;
        }
        public int GetAmount()
        {
            return this._amount;
        }
        protected override void OnTickParallel2(float dt)
        {
            base.OnTickParallel2(dt);
            if (GameNetwork.IsServer)
            {
                if (base.HasUser)
                {
                    ActionIndexCache currentAction = base.UserAgent.GetCurrentAction(this._usedChannelIndex);
                    if (currentAction == this._successActionIndex)
                    {
                        base.UserAgent.StopUsingGameObjectMT(base.UserAgent.CanUseObject(this));
                    }
                    else if (currentAction != this._progressActionIndex)
                    {
                        base.UserAgent.StopUsingGameObjectMT(false);
                    }
                }
            }
        }
        public override void OnUse(Agent userAgent)
        {
            if (base.HasUser) return;
            base.OnUse(userAgent);
            // userAgent.StopUsingGameObjectMT(true, true, false);
            if (GameNetwork.IsServer)
            {
                Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);

                MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
                float num = globalFrame.origin.z;
                float eyeGlobalHeight = userAgent.GetEyeGlobalHeight();
                bool isLeftStance = userAgent.GetIsLeftStance();
                if (userAgent.HasMount)
                {
                    this._usedChannelIndex = 1;
                    MatrixFrame frame = userAgent.Frame;
                    bool flag = Vec2.DotProduct(frame.rotation.f.AsVec2.LeftVec(), (base.GameEntity.GetGlobalFrame().origin - frame.origin).AsVec2) > 0f;
                    if (num < eyeGlobalHeight * 0.7f + userAgent.Position.z)
                    {

                        this._progressActionIndex = (flag ? PE_MoneyBag.act_pickup_from_left_down_horseback_begin : PE_MoneyBag.act_pickup_from_right_down_horseback_begin);
                        this._successActionIndex = (flag ? PE_MoneyBag.act_pickup_from_left_down_horseback_end : PE_MoneyBag.act_pickup_from_right_down_horseback_end);
                    }
                    else if (num < eyeGlobalHeight * 1.1f + userAgent.Position.z)
                    {

                        this._progressActionIndex = (flag ? PE_MoneyBag.act_pickup_from_left_middle_horseback_begin : PE_MoneyBag.act_pickup_from_right_middle_horseback_begin);
                        this._successActionIndex = (flag ? PE_MoneyBag.act_pickup_from_left_middle_horseback_end : PE_MoneyBag.act_pickup_from_right_middle_horseback_end);
                    }
                    else
                    {
                        this._progressActionIndex = (flag ? PE_MoneyBag.act_pickup_from_left_up_horseback_begin : PE_MoneyBag.act_pickup_from_right_up_horseback_begin);
                        this._successActionIndex = (flag ? PE_MoneyBag.act_pickup_from_left_up_horseback_end : PE_MoneyBag.act_pickup_from_right_up_horseback_end);
                    }
                }
                else if (num < eyeGlobalHeight * 0.4f + userAgent.Position.z)
                {
                    this._usedChannelIndex = 0;

                    this._progressActionIndex = (isLeftStance ? PE_MoneyBag.act_pickup_down_begin_left_stance : PE_MoneyBag.act_pickup_down_begin);
                    this._successActionIndex = (isLeftStance ? PE_MoneyBag.act_pickup_down_end_left_stance : PE_MoneyBag
                        .act_pickup_down_end);

                }
                else if (num < eyeGlobalHeight * 1.1f + userAgent.Position.z)
                {
                    this._usedChannelIndex = 1;
                    this._progressActionIndex = (isLeftStance ? PE_MoneyBag.act_pickup_middle_begin_left_stance : PE_MoneyBag.act_pickup_middle_begin);
                    this._successActionIndex = (isLeftStance ? PE_MoneyBag.act_pickup_middle_end_left_stance : PE_MoneyBag.act_pickup_middle_end);
                }
                else
                {
                    this._usedChannelIndex = 1;
                    this._progressActionIndex = (isLeftStance ? PE_MoneyBag.act_pickup_up_begin_left_stance : PE_MoneyBag.act_pickup_up_begin);
                    this._successActionIndex = (isLeftStance ? PE_MoneyBag.act_pickup_up_end_left_stance : PE_MoneyBag.act_pickup_up_end);
                }
                userAgent.SetActionChannel(this._usedChannelIndex, this._progressActionIndex, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            }
        }

        public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
        {
            base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
            Debug.Print("[USING LOG] AGENT USE STOPPED " + this.GetType().Name);

            if (isSuccessful)
            {
                if (GameNetwork.IsServer)
                {
                    PersistentEmpireRepresentative representative = userAgent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>();
                    representative.GoldGain(this._amount);
                    LoggerHelper.LogAnAction(userAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerPickedUpGold, null, new object[] { this._amount });
                    // Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_positive"), userAgent.Frame.origin, false, true, -1, -1);
                }
                base.GameEntity.Remove(80);
            }
        }
    }
}

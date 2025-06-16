using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_Bed : PE_UsableFromDistance
    {
        public int AnimationDurationInSeconds = 5;
        public string Animation = "act_npc_farmer_bush_cutting_while_stand";

        public bool ForMount = false;

        public long UseStartedAt = 0;
        public long UseWillEndAt = 0;
        protected override bool LockUserFrames
        {
            get
            {
                return false;
            }
        }
        protected override bool LockUserPositions
        {
            get
            {
                return false;
            }
        }

        public override bool IsDisabledForAgent(Agent agent)
        {
            if (this.ForMount == false)
            {
                return this.IsDeactivated || agent.MountAgent != null || (this.IsDisabledForPlayers && !agent.IsAIControlled) || !agent.IsOnLand();
            }
            return this.IsDeactivated || agent.MountAgent == null || (this.IsDisabledForPlayers && !agent.IsAIControlled) || !agent.IsOnLand();
        }

        protected override void OnInit()
        {
            base.OnInit();
            base.ActionMessage = new TextObject("Bed");
            TextObject descriptionMessage = new TextObject("Press {KEY} To Heal");
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
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Bed";
        }

        public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
        {
            base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
            userAgent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloatRanged(1f), false, -0.2f, 0, true);
            if (isSuccessful)
            {
                if (GameNetwork.IsServer)
                {
                    NetworkCommunicator peer = userAgent.MissionPeer.GetNetworkPeer();
                    PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                    if (this.ForMount == false)
                    {
                        int missingHealth = (int)(userAgent.HealthLimit - userAgent.Health);
                        int hunger = persistentEmpireRepresentative.GetHunger();
                        int refillAmount = Math.Min(hunger, missingHealth);
                        persistentEmpireRepresentative.SetHunger(hunger - refillAmount);
                        userAgent.Health += refillAmount;
                    }
                    else
                    {
                        userAgent.MountAgent.Health = userAgent.MountAgent.HealthLimit;
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
                if (base.HasUser)
                {
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                NetworkCommunicator peer = userAgent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                if (persistentEmpireRepresentative.GetHunger() < 15 && this.ForMount == false)
                {
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Bed1", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                if (this.ForMount && userAgent.MountAgent.Health < 20)
                {
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Bed2", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
            }
            if (this.Animation != "")
            {
                ActionIndexCache actionIndexCache = ActionIndexCache.Create(this.Animation);
                userAgent.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            }
            this.UseStartedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this.UseWillEndAt = this.UseStartedAt + this.AnimationDurationInSeconds;
            userAgent.SetTargetPosition(userAgent.GetWorldFrame().Origin.AsVec2);
            if (userAgent.IsMine)
            {
                PEInformationManager.StartCounter("Sleeping...", this.AnimationDurationInSeconds);
            }
            base.OnUse(userAgent);
        }
    }
}

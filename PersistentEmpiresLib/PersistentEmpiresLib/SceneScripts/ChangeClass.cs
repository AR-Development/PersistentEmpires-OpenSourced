using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_ChangeClass : UsableMissionObject
    {
        public int ChangingDurationAsSeconds = 10;
        public string Animation = "act_main_story_become_king_crowd_06";
        public String ClassId = PersistentEmpireBehavior.DefaultClass;
        public int CastleId = 0;
        public int DinarCost = 0;
        public bool JoinCastleFaction = true;
        public int FactionId = 0;
        public bool Mercenary = false;

        public long UseStartedAt { get; private set; }
        public long UseWillEndAt { get; private set; }

        protected override bool LockUserFrames
        {
            get => false;
        }
        protected override bool LockUserPositions
        {
            get => false;
        }

        protected override void OnInit()
        {
            base.OnInit();
            MultiplayerClassDivisions.MPHeroClass heroClass = this.GetHeroClass();
            TextObject actionMessage = new TextObject("Become a " + heroClass.HeroCharacter.GetName().ToString() + " for {FACTION}");
            PE_CastleBanner castleBanner = this.GetCastleBanner();
            if (castleBanner != null)
            {
                actionMessage.SetTextVariable("FACTION", castleBanner.GetOwnerFaction().name);
            }
            else
            {
                actionMessage.SetTextVariable("FACTION", "your own sake");
            }
            base.ActionMessage = actionMessage;
            TextObject descriptionMessage = new TextObject("Press {KEY} To Become");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
            if (this.FactionId < 0) this.FactionId = 0;
        }

        public override void OnFocusGain(Agent userAgent)
        {
            base.OnFocusGain(userAgent);
            if (GameNetwork.IsClient)
            {
                if (this.JoinCastleFaction)
                {
                    PE_CastleBanner castleBanner = this.GetCastleBanner();
                    if (castleBanner != null)
                    {
                        base.ActionMessage.SetTextVariable("FACTION", castleBanner.GetOwnerFaction().name);
                    }
                    else
                    {
                        base.ActionMessage.SetTextVariable("FACTION", "your own sake");
                    }
                }
                else
                {
                    FactionsBehavior factionsBehavior = Mission.Current.GetMissionBehavior<FactionsBehavior>();
                    if (factionsBehavior.Factions.ContainsKey(FactionId))
                    {
                        base.ActionMessage.SetTextVariable("FACTION", factionsBehavior.Factions[FactionId].name);
                    }
                    else
                    {
                        base.ActionMessage.SetTextVariable("FACTION", "your own sake");
                    }
                }

                MultiplayerClassDivisions.MPHeroClass heroClass = this.GetHeroClass();
                InformationManager.ShowTooltip(typeof(BasicCharacterObject), new object[] {
                    heroClass.HeroCharacter,
                    this.DinarCost
                });
            }
        }
        public override void OnFocusLose(Agent userAgent)
        {
            base.OnFocusLose(userAgent);
            if (GameNetwork.IsClient)
            {
                InformationManager.HideTooltip();
            }
        }
        public MultiplayerClassDivisions.MPHeroClass GetHeroClass()
        {
            return MBObjectManager.Instance.GetObjectTypeList<MultiplayerClassDivisions.MPHeroClass>().FirstOrDefault((mpHeroClass) => mpHeroClass.HeroCharacter.StringId == this.ClassId);
        }
        public int GetHeroClassIndex()
        {
            return MBObjectManager.Instance.GetObjectTypeList<MultiplayerClassDivisions.MPHeroClass>().Select((value, index) => new { value, index }).First((a) => a.value.HeroCharacter.StringId == this.ClassId).index;
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Change Class";
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
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            /*if (GameNetwork.IsClientOrReplay && base.HasUser)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel2;
            }*/
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
        public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
        {
            base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
            if (GameNetwork.IsServer)
            {
                Debug.Print("[USING LOG] AGENT USE STOPPED " + this.GetType().Name);
                MultiplayerClassDivisions.MPHeroClass heroClass = this.GetHeroClass();
                if (heroClass == null)
                {
                    Debug.Print("** PE ERROR** " + this.ClassId + " CANNOT FOUND ON MPHERO XML FILE.");
                    return;
                }
                NetworkCommunicator player = userAgent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();


                if (isSuccessful)
                {
                    if (player.ControlledAgent.Character.StringId != heroClass.HeroCharacter.StringId && !persistentEmpireRepresentative.ReduceIfHaveEnoughGold(this.DinarCost))
                    {
                        InformationComponent.Instance.SendMessage("You need " + this.DinarCost + " gold.", (new Color(1f, 0, 0)).ToUnsignedInteger(), player);
                        return;
                    }
                    persistentEmpireRepresentative.SetClass(this.ClassId);
                    int joinedFrom = persistentEmpireRepresentative.GetFactionIndex();
                    PE_CastleBanner banner = this.GetCastleBanner();
                    int joinedTo = 0;
                    if (banner != null && this.JoinCastleFaction)
                    {
                        joinedTo = banner.FactionIndex;
                    }
                    else
                    {
                        joinedTo = this.FactionId;
                    }

                    FactionsBehavior factionsBehavior = Mission.Current.GetMissionBehavior<FactionsBehavior>();

                    AgentHelpers.RespawnAgentOnPlaceForFaction(userAgent, joinedTo == -1 ? null : factionsBehavior.Factions[joinedTo], null, heroClass.HeroCharacter);
                    LoggerHelper.LogAnAction(userAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerClassChange, null, new object[] { heroClass.HeroCharacter });
                    factionsBehavior.SetPlayerFaction(player, joinedTo, joinedFrom);
                }
            }

            ActionIndexCache actionIndexCache = ActionIndexCache.act_none;
            userAgent.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
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
                Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);

                if (base.HasUser)
                {
                    return;
                }
                MultiplayerClassDivisions.MPHeroClass heroClass = this.GetHeroClass();
                if (heroClass == null)
                {
                    Debug.Print("** PE ERROR** " + this.ClassId + " CANNOT FOUND ON MPHERO XML FILE.");
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                NetworkCommunicator player = userAgent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
                PE_CastleBanner banner = this.GetCastleBanner();

                if (persistentEmpireRepresentative == null) return;
                bool switchingToSameClass = false;
                if (player.ControlledAgent.Character.StringId == heroClass.HeroCharacter.StringId)
                {
                    if (banner != null && persistentEmpireRepresentative.GetFactionIndex() == banner.FactionIndex && this.JoinCastleFaction)
                    {
                        userAgent.StopUsingGameObjectMT(false);
                        return;
                    }
                    else if ((banner == null || !this.JoinCastleFaction) && persistentEmpireRepresentative.GetFactionIndex() == this.FactionId)
                    {
                        userAgent.StopUsingGameObjectMT(false);
                        return;
                    }
                    switchingToSameClass = true;
                }
                if (this.Mercenary)
                {
                    CastlesBehavior castleBehaviors = Mission.Current.GetMissionBehavior<CastlesBehavior>();
                    PE_CastleBanner[] castleBanners = castleBehaviors.castles.Values.ToArray();
                    foreach (PE_CastleBanner castleBanner in castleBanners)
                    {
                        if (castleBanner.FactionIndex == this.FactionId)
                        {
                            userAgent.StopUsingGameObjectMT(false);
                            return;
                        }
                    }
                }
                if (!persistentEmpireRepresentative.HaveEnoughGold(this.DinarCost) && !switchingToSameClass)
                {
                    InformationComponent.Instance.SendMessage("You need " + this.DinarCost + " gold.", (new Color(1f, 0, 0)).ToUnsignedInteger(), player);
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                ActionIndexCache actionIndexCache = ActionIndexCache.Create(this.Animation);
                userAgent.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                this.UseStartedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                this.UseWillEndAt = this.UseStartedAt + this.ChangingDurationAsSeconds;
                userAgent.SetTargetPosition(userAgent.GetWorldFrame().Origin.AsVec2);
            }
            else
            {
                if (userAgent.IsMine)
                {
                    PEInformationManager.StartCounter("Changing class...", this.ChangingDurationAsSeconds);
                    userAgent.SetTargetPosition(userAgent.GetWorldFrame().Origin.AsVec2);
                }
            }
            base.OnUse(userAgent);

        }
    }
}

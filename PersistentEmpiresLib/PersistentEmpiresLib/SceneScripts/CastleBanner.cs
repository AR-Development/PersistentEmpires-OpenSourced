using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Server;
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
    public class PE_CastleBanner : UsableMissionObject
    {
        public int FactionIndex = 0;
        public int CastleIndex = 0;
        public String CastleName = "Montibeil Castle";
        private string BannerKey = "11.45.126.4345.4345.764.764.1.0.0.462.0.13.512.512.764.764.1.0.0";

        public string CastleCaptureAnimation = "act_main_story_become_king_crowd_06";
        public int CaptureDuration = 5;
        public string CaptureItem = "pe_banner";

        protected override bool LockUserFrames { get => false; }
        protected override bool LockUserPositions { get => false; }

        public long UseStartedAt { get; private set; }
        public long UseWillEndAt { get; private set; }

        public String GetBannerKey()
        {
            return this.BannerKey;
        }

        protected override void OnInit()
        {
            base.OnInit();
            base.ActionMessage = new TextObject(this.CastleName + "'s Banner");
            TextObject descriptionMessage = new TextObject("Press {KEY} To Capture");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
        }

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            /*if (GameNetwork.IsClientOrReplay && base.HasUser)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel2;
            }else*/
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
                        base.UserAgent.StopUsingGameObjectMT(true);
                    }
                }
            }
        }
        public void UpdateBannerFromFaction()
        {
            if (GameNetwork.IsClient)
            {
                // this.SetBannerKey(this.GetOwnerFaction().team.Banner.Serialize());
                Banner banner = this.GetOwnerFaction().banner;
                BannerRenderer.RequestRenderBanner(banner, base.GameEntity);
                /*((BannerVisual)banner.BannerVisual).GetTableauTextureLarge(delegate (Texture t)
                {
                    BannerRenderer.OnBannerTableauRenderDone(base.GameEntity, t);
                }, true);*/
            }
        }

        public void SetBannerKey(string BannerKey)
        {
            this.BannerKey = BannerKey;
            if (GameNetwork.IsClient)
            {
                Banner banner = new Banner(BannerKey);
                BannerRenderer.RequestRenderBanner(banner, base.GameEntity);
                /*((BannerVisual)banner.BannerVisual).GetTableauTextureLarge(delegate (Texture t)
                {
                    BannerRenderer.OnBannerTableauRenderDone(base.GameEntity, t);
                }, true);*/
            }

        }
        public Faction GetOwnerFaction()
        {
            FactionsBehavior factionBehavior = Mission.Current.GetMissionBehavior<FactionsBehavior>();
            if (factionBehavior == null) return null;
            if (!factionBehavior.Factions.ContainsKey(this.FactionIndex))
            {
                return factionBehavior.Factions[0];
            }
            return factionBehavior.Factions[this.FactionIndex];

        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Castle Banner";
        }

        public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
        {
            base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
            if (GameNetwork.IsServer)
            {
                Debug.Print("USINGLOG " + isSuccessful.ToString());
                Debug.Print("[USING LOG] AGENT USE STOPPED " + this.GetType().Name);
                if (isSuccessful)
                {
                    EquipmentIndex wieldedEquipmentIndex = userAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                    if (wieldedEquipmentIndex == EquipmentIndex.None) return;
                    if (userAgent.Equipment[wieldedEquipmentIndex].Item.StringId != this.CaptureItem) return;

                    NetworkCommunicator peer = userAgent.MissionPeer.GetNetworkPeer();
                    PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                    Faction capturerFaction = persistentEmpireRepresentative.GetFaction();
                    if (persistentEmpireRepresentative.GetFactionIndex() == 0 || persistentEmpireRepresentative.GetFactionIndex() == 1) return;
                    int factionIndex = persistentEmpireRepresentative.GetFactionIndex();
                    this.FactionIndex = factionIndex;
                    SaveSystemBehavior.HandleCreateOrSaveCastle(this.CastleIndex, factionIndex);
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new UpdateCastle(this));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                    InformationComponent.Instance.BroadcastQuickInformation(this.CastleName + " have been captured by " + capturerFaction.name, Colors.Red.ToUnsignedInteger());
                    userAgent.RemoveEquippedWeapon(userAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand));
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
                Debug.Print("[USING LOG] AGENT USING " + this.GetType().Name);
                if (base.HasUser)
                {
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                EquipmentIndex wieldedItemIndex = userAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                if (wieldedItemIndex == EquipmentIndex.None)
                {
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }

                NetworkCommunicator peer = userAgent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                Faction capturerFaction = persistentEmpireRepresentative.GetFaction();
                int capturerFactionIndex = persistentEmpireRepresentative.GetFactionIndex();
                Faction castleFaction = this.GetOwnerFaction();

                if (castleFaction == null || this.FactionIndex == 0 || this.FactionIndex == 1)
                {
                    // Cannot be capturable.
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                if (capturerFactionIndex == 0 || capturerFactionIndex == 1 || capturerFaction == null || capturerFactionIndex == this.FactionIndex)
                {
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                if (!capturerFaction.warDeclaredTo.Contains(this.FactionIndex) && !castleFaction.warDeclaredTo.Contains(capturerFactionIndex))
                {
                    // No war decleration no.
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                foreach (NetworkCommunicator membersOfBeingCaptured in castleFaction.members)
                {
                    InformationComponent.Instance.SendMessage(this.CastleName + " BEING CAPTURED. YOU NEED TO STOP IT.", new Color(1f, 0f, 0f).ToUnsignedInteger(), membersOfBeingCaptured);
                }
                ActionIndexCache actionIndexCache = ActionIndexCache.Create(this.CastleCaptureAnimation);
                userAgent.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            }
            if (GameNetwork.IsClient)
            {
                if (userAgent.IsMine)
                {
                    PEInformationManager.StartCounter("Capturing the " + this.CastleName, this.CaptureDuration);
                }
            }

            this.UseStartedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this.UseWillEndAt = this.UseStartedAt + this.CaptureDuration;
            userAgent.SetTargetPosition(userAgent.Position.AsVec2);
            base.OnUse(userAgent);
        }
    }
}

using PersistentEmpiresLib.Data;
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
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_ItemGathering : PE_UsableFromDistance
    {
        public string Name = "Berry";
        public int RespawnTime = 10;
        public int ItemCount = 10;
        public int AnimationDurationInSeconds = 5;
        public string Animation = "act_npc_farmer_bush_cutting_while_stand";
        public string NeededItem = "";
        public string RequiredSkillId = "Gathering";
        public int RequiredSkill = 15;
        public string DropsItem = "";
        public int DropCount = 1;
        public bool RotateWhenUsage = false;
        public string LookPointTag = "lookpoint";
        public bool RandomizedRespawn = false;
        public int RandomRespawnOffset = 0;

        public bool IsDestroyed = false;
        private long DestroyedAt = 0;
        private long UseStartedAt = 0;
        private long UseWillEndAt = 0;
        private int CurrentCount = 0;
        private ItemObject DropsItemObject;

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

        protected override void OnInit()
        {
            base.OnInit();
            base.ActionMessage = new TextObject(this.Name);
            TextObject descriptionMessage = new TextObject("Press {KEY} To Gather");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
            this.DropsItemObject = MBObjectManager.Instance.GetObject<ItemObject>(this.DropsItem);
            this.CurrentCount = this.ItemCount;
            if (RandomizedRespawn)
            {
                this.RespawnTime += MBRandom.RandomInt(this.RandomRespawnOffset);
            }
            if (this.DropsItemObject == null)
            {
                Debug.Print(this.DropsItem + " CANNOT BE FOUND ON PE_ITEMGATHERING", 0, Debug.DebugColor.Red);
            }
        }
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            /*if (GameNetwork.IsClientOrReplay && base.HasUser)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel2;
            }*/
            if (GameNetwork.IsServer)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel2;
            }
            return base.GetTickRequirement();
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
            if (this.IsDestroyed && this.DestroyedAt + this.RespawnTime < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                this.UpdateIsDestroyed(false);
            }
        }

        public void UpdateIsDestroyed(bool isDestroyed)
        {
            if (!isDestroyed)
            {
                this.CurrentCount = this.ItemCount;
                base.GameEntity.SetVisibilityExcludeParents(true);
                this.IsDestroyed = false;
            }
            else
            {
                this.IsDestroyed = true;
                base.GameEntity.SetVisibilityExcludeParents(false);
                this.DestroyedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }


            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new UpdateItemGatheringDestroyed(this, isDestroyed));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
        }

        public override void OnUseStopped(Agent userAgent, bool isSuccessful, int preferenceIndex)
        {
            base.OnUseStopped(userAgent, isSuccessful, preferenceIndex);
            Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);

            userAgent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloatRanged(1f), false, -0.2f, 0, true);
            if (isSuccessful)
            {
                if (GameNetwork.IsServer)
                {
                    this.CurrentCount--;
                    NetworkCommunicator peer = userAgent.MissionPeer.GetNetworkPeer();
                    PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                    Inventory playerInventory = persistentEmpireRepresentative.GetInventory();
                    playerInventory.AddCountedItemSynced(this.DropsItemObject, this.DropCount, ItemHelper.GetMaximumAmmo(this.DropsItemObject));
                    LoggerHelper.LogAnAction(peer, LogAction.PlayerItemGathers, null, new object[] { this.DropsItemObject });
                    if (this.CurrentCount == 0)
                    {
                        this.UpdateIsDestroyed(true);
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
                Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);

                if (base.HasUser)
                {
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                SkillObject requiredSkillObject = MBObjectManager.Instance.GetObject<SkillObject>(this.RequiredSkillId);
                if (userAgent.Character.GetSkillValue(requiredSkillObject) < this.RequiredSkill)
                {
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Not_Qualified", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                EquipmentIndex wieldedItemIndex = userAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                // MissionWeapon wieldedItem = userAgent.Equipment[wieldedItemIndex];
                if (this.NeededItem == "" && wieldedItemIndex != EquipmentIndex.None)
                {
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Empty_Your_Hands", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                else if (this.NeededItem != "")
                {
                    if (wieldedItemIndex == EquipmentIndex.None)
                    {
                        ItemObject need = MBObjectManager.Instance.GetObject<ItemObject>(this.NeededItem);
                        InformationComponent.Instance.SendMessage("You need a " + need.Name.ToString() + " to do this.", new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                        userAgent.StopUsingGameObjectMT(false);
                        return;
                    }
                    else
                    {
                        MissionWeapon wieldedItem = userAgent.Equipment[wieldedItemIndex];
                        if (wieldedItem.Item.StringId != this.NeededItem)
                        {
                            ItemObject need = MBObjectManager.Instance.GetObject<ItemObject>(this.NeededItem);
                            InformationComponent.Instance.SendMessage("You need a " + need.Name.ToString() + " to do this.", new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                            userAgent.StopUsingGameObjectMT(false);
                            return;
                        }
                    }
                }
                if (this.IsDestroyed)
                {
                    InformationComponent.Instance.SendMessage("This object is disabled please try later.", new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }

                NetworkCommunicator peer = userAgent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                Inventory playerInventory = persistentEmpireRepresentative.GetInventory();
                if (!playerInventory.HasEnoughRoomFor(this.DropsItemObject, this.DropCount))
                {
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Not_Enough_Space", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
            }
            ActionIndexCache actionIndexCache = ActionIndexCache.Create(this.Animation);
            userAgent.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            this.UseStartedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this.UseWillEndAt = this.UseStartedAt + this.AnimationDurationInSeconds;
            if (this.RotateWhenUsage)
            {
                GameEntity entity = base.GameEntity.GetFirstChildEntityWithTag(this.LookPointTag);
                GameEntityWithWorldPosition gameEntityWithWorldPosition = new GameEntityWithWorldPosition(entity);
                WorldFrame userFrameLook = gameEntityWithWorldPosition.WorldFrame;
                WorldFrame userFrameForAgent = this.GetUserFrameForAgent(userAgent);
                userAgent.SetTargetPositionAndDirection(userFrameForAgent.Origin.AsVec2, -userFrameLook.Rotation.f);
            }
            else
            {
                userAgent.SetTargetPosition(userAgent.GetWorldFrame().Origin.AsVec2);
            }
            if (userAgent.IsMine)
            {
                PEInformationManager.StartCounter("Gathering " + this.DropsItemObject.Name.ToString() + "...", this.AnimationDurationInSeconds);
            }
            base.OnUse(userAgent);
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Item Gathering";
        }

    }
}

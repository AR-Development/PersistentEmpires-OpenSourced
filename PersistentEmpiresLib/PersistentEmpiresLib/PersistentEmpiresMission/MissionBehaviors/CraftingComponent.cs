using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class CraftingComponent : MissionNetwork
    {
        private class CraftingAction
        {
            public CraftingAction(PE_CraftingStation craftingStation, Craftable craftable, long startedAt)
            {
                this.craftingStation = craftingStation;
                this.craftable = craftable;
                this.startedAt = startedAt;
            }
            public PE_CraftingStation craftingStation;
            public Craftable craftable;
            public long startedAt;
        }
        Dictionary<NetworkCommunicator, CraftingAction> craftings;

        public delegate void CraftingStationUseHandler(PE_CraftingStation craftingStation, Inventory playerInventory);
        public event CraftingStationUseHandler OnCraftingUse;

        public delegate void CraftingStartedHandler(PE_CraftingStation craftingStation, int craftIndex, NetworkCommunicator player);
        public event CraftingStartedHandler OnCraftingStarted;

        public delegate void CraftingCompletedHandler();
        public event CraftingCompletedHandler OnCraftingCompleted;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            this.craftings = new Dictionary<NetworkCommunicator, CraftingAction>();
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
#if SERVER
            if (this.craftings == null) return;

            foreach (NetworkCommunicator player in craftings.Keys.ToList())
            {
                PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
                if (!player.IsConnectionActive)
                {
                    this.craftings.Remove(player);
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new CraftingCompleted(player));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                    continue;
                }
                if (persistentEmpireRepresentative == null)
                {
                    this.craftings.Remove(player);

                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new CraftingCompleted(player));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                    continue;
                }
                CraftingAction craftingAction = this.craftings[player];
                if (player.ControlledAgent != null && craftingAction.craftingStation.Animation != "")
                {
                    if (player.ControlledAgent.GetCurrentAction(0).Name == "act_none")
                    {
                        ActionIndexCache action = ActionIndexCache.Create(craftingAction.craftingStation.Animation);
                        player.ControlledAgent.SetActionChannel(0, action, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloatRanged(1f), false, -0.2f, 0, true);
                    }
                }
                if (craftingAction.startedAt + craftingAction.craftable.CraftTime <= DateTimeOffset.Now.ToUnixTimeSeconds())
                {
                    // Crafting complete bra.
                    bool hasEveryItem = craftingAction.craftable.Recipe.All((r) => persistentEmpireRepresentative.GetInventory().IsInventoryIncludes(r.Item, r.NeededCount));
                    if (!hasEveryItem)
                    {
                        InformationComponent.Instance.SendMessage(GameTexts.FindText("CraftingComponent1", null).ToString(), new Color(1f, 0f, 0f).ToUnsignedInteger(), player);
                        this.craftings.Remove(player);
                        GameNetwork.BeginBroadcastModuleEvent();
                        GameNetwork.WriteMessage(new CraftingCompleted(player));
                        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                        continue;
                    }
                    List<int> updatedSlots;
                    foreach (CraftingRecipe r in craftingAction.craftable.Recipe)
                    {
                        updatedSlots = persistentEmpireRepresentative.GetInventory().RemoveCountedItemSynced(r.Item, r.NeededCount);
                        foreach (int i in updatedSlots)
                        {
                            GameNetwork.BeginModuleEventAsServer(player);
                            GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                            GameNetwork.EndModuleEventAsServer();
                        }
                    }
                    updatedSlots = persistentEmpireRepresentative.GetInventory().AddCountedItemSynced(craftingAction.craftable.Item, craftingAction.craftable.OutputCount, ItemHelper.GetMaximumAmmo(craftingAction.craftable.Item));
                    foreach (int i in updatedSlots)
                    {
                        GameNetwork.BeginModuleEventAsServer(player);
                        GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                        GameNetwork.EndModuleEventAsServer();
                    }
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new CraftingCompleted(player));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                    if (player.ControlledAgent != null)
                    {

                        player.ControlledAgent.StopUsingGameObjectMT(true);
                        // player.ControlledAgent.StopUsingGameObjectMT(true, true, false);
                        ActionIndexCache ac = ActionIndexCache.act_none;
                        player.ControlledAgent.SetActionChannel(0, ac, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0, false, -0.2f, 0, true);

                    }
                    this.craftings.Remove(player);
                }
            }
#endif
        }
        public void OnUsedCrafting(Inventory playerInventory, PE_CraftingStation craftingStation)
        {
            if (this.OnCraftingUse != null)
            {
                this.OnCraftingUse(craftingStation, playerInventory);
            }
        }
        public void AgentRequestCrafting(Agent agent, PE_CraftingStation craftingStation)
        {
            if (!agent.IsPlayerControlled) return;
            NetworkCommunicator peer = agent.MissionPeer.GetNetworkPeer();
            if (GameNetwork.IsServer)
            {
                PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new OpenCraftingStation(craftingStation, persistentEmpireRepresentative.GetInventory()));
                GameNetwork.EndModuleEventAsServer();
            }
        }
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<OpenCraftingStation>(this.HandleOpenCraftingStationFromServer);
                networkMessageHandlerRegisterer.Register<CraftingStarted>(this.HandleCraftingStartedFromServer);
                networkMessageHandlerRegisterer.Register<CraftingCompleted>(this.HandleCraftingCompletedFromServer);
                // networkMessageHandlerRegisterer.Register<UpdateCastle>(this.HandleUpdateCastle);
            }
            else
            {
                networkMessageHandlerRegisterer.Register<RequestExecuteCraft>(this.HandleRequestExecuteCraftFromClient);

            }
        }

        private void HandleCraftingCompletedFromServer(CraftingCompleted message)
        {
            // Stop the animation here
            if (message.Player.ControlledAgent != null)
            {
                message.Player.ControlledAgent.StopUsingGameObjectMT(true);
                ActionIndexCache ac = ActionIndexCache.act_none;
                message.Player.ControlledAgent.SetActionChannel(0, ac, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0, false, -0.2f, 0, true);
            }
            if (message.Player.IsMine)
            {
                if (this.OnCraftingCompleted != null)
                {
                    this.OnCraftingCompleted();
                }
            }
        }

        private void HandleCraftingStartedFromServer(CraftingStarted message)
        {
            // Start an animation here
            PE_CraftingStation craftingStation = (PE_CraftingStation)message.CraftingStation;
            if (message.Player.IsMine)
            {
                if (this.OnCraftingStarted != null)
                {
                    this.OnCraftingStarted((PE_CraftingStation)message.CraftingStation, message.CraftIndex, message.Player);
                }
            }
        }

        private bool HandleRequestExecuteCraftFromClient(NetworkCommunicator peer, RequestExecuteCraft message)
        {
            if (peer.ControlledAgent == null) return false;
            PE_CraftingStation craftingStation = (PE_CraftingStation)message.CraftingStation;
            Craftable requestedCraft = craftingStation.Craftables[message.CraftIndex];
            /* if(this.craftings.Values.ToList().Any(c => c != null && c.craftingStation.Id == craftingStation.Id))
            {
                InformationComponent.Instance.SendMessage("This station is being used.", new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                return false;
            }*/
            if (craftingStation.upgradeableBuilding != null && craftingStation.upgradeableBuilding.CurrentTier < requestedCraft.Tier)
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("CraftingComponent2", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                return false;
            }
            if (this.craftings.ContainsKey(peer))
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("CraftingComponent3", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                return false;
            }
            if (requestedCraft.RequiredEngineering > peer.ControlledAgent.Character.GetSkillValue(requestedCraft.RelevantSkill))
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Not_Qualified", null).ToString(), new Color(1f, 0f, 0f).ToUnsignedInteger(), peer);
                return false;
            }
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            if (!persistentEmpireRepresentative.GetInventory().HasEnoughRoomFor(requestedCraft.Item, requestedCraft.OutputCount))
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Not_Enough_Space", null).ToString(), new Color(1f, 0f, 0f).ToUnsignedInteger(), peer);
                return false;
            }
            bool hasEveryItem = requestedCraft.Recipe.All((r) => persistentEmpireRepresentative.GetInventory().IsInventoryIncludes(r.Item, r.NeededCount));
            if (!hasEveryItem)
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("CraftingComponent6", null).ToString(), new Color(1f, 0f, 0f).ToUnsignedInteger(), peer);
                return false;
            }
            // Start crafting...
            this.craftings[peer] = new CraftingAction(craftingStation, requestedCraft, DateTimeOffset.Now.ToUnixTimeSeconds());
            if (peer.ControlledAgent != null && craftingStation.Animation != "")
            {
                ActionIndexCache action = ActionIndexCache.Create(craftingStation.Animation);
                peer.ControlledAgent.SetActionChannel(0, action, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloatRanged(1f), false, -0.2f, 0, true);
            }
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new CraftingStarted(craftingStation, peer, message.CraftIndex));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            return true;
        }

        private void HandleOpenCraftingStationFromServer(OpenCraftingStation message)
        {
            this.OnUsedCrafting(message.PlayerInventory, (PE_CraftingStation)message.Station);
        }
    }
}

using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class PlayerInventoryComponent : MissionNetwork
    {
        public delegate void OpenInventoryHandler(Inventory playerInventory, Inventory targetInventory);
        public event OpenInventoryHandler OnOpenInventory;

        public delegate void ForceUpdateInventoryHandler(string updatedSlotTag, ItemObject item, int count);
        public event ForceUpdateInventoryHandler OnForceUpdateInventory;

        public delegate void ForceCloseInventoryHandler();
        public event ForceCloseInventoryHandler OnForceCloseInventory;

        public delegate void UpdateInventoryHandler(String DraggedSlot, String DroppedSlot, ItemObject DraggedSlotItem, ItemObject DroppedSlotItem, int DraggedSlotCount, int DroppedSlotCount);
        public event UpdateInventoryHandler OnUpdateInventory;

        public Dictionary<string, Inventory> CustomInventories;
        public Dictionary<string, PE_InventoryEntity> LootableObjects;
        public Dictionary<string, long> LootableCreatedAt;
        public Dictionary<NetworkCommunicator, Inventory> OpenedByPeerInventory;

        public int DestroyChance { get; private set; }

        public Dictionary<NetworkCommunicator, long> LastRevealed = new Dictionary<NetworkCommunicator, long>();
        public Dictionary<Agent, bool> IgnoreAgentDropLoot = new Dictionary<Agent, bool>();
        private Random random = new Random();
        private static float _distanceForAutoCloseInventory = 4f;

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        #region MissionNetwork
#if CLIENT
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }
#endif
#if SERVER
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);

            List<DBInventory> savedInventoriesAsList = SaveSystemBehavior.HandleGetAllInventories().ToList();
            this.CustomInventories = new Dictionary<string, Inventory>();
            List<GameEntity> gameEntities = new List<GameEntity>();
            // This is done to ensure derived scripts from PE_InventoryEntity are included
            Mission.Scene.GetEntities(ref gameEntities);
            var tmp = gameEntities.Select(x => x.GetScriptComponents().OfType<PE_InventoryEntity>()).SelectMany(y => y).Where(z => z != null);

            foreach (PE_InventoryEntity inventoryEntity in tmp)//gameEntities.Select((g) => g.GetFirstScriptOfType<PE_InventoryEntity>()))
            {
                if (inventoryEntity.InteractionEntity == null)
                {
                    Debug.Print("INVENTORY ERROR " + inventoryEntity.InventoryId);
                }

                var savedInventory = savedInventoriesAsList.FirstOrDefault(x => x.InventoryId == inventoryEntity.InventoryId);
                if (savedInventory != null)
                {
                    CustomInventories[inventoryEntity.InventoryId] = Inventory.Deserialize(savedInventory.InventorySerialized, savedInventory.InventoryId, inventoryEntity);
                }
                else
                {
                    CustomInventories[inventoryEntity.InventoryId] = new Inventory(inventoryEntity.Slot, inventoryEntity.StackCount, inventoryEntity.InventoryId, inventoryEntity);
                }
            }
            this.LootableObjects = new Dictionary<string, PE_InventoryEntity>();
            this.LootableCreatedAt = new Dictionary<string, long>();
            this.OpenedByPeerInventory = new Dictionary<NetworkCommunicator, Inventory>();
            this.DestroyChance = ConfigManager.GetIntConfig("ItemDestroyChanceOnDeath", 5);
        }

        private static int _counter = 0;
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (++_counter < 10)
                return;
            // Reset counter
            _counter = 0;

            foreach (string inventoryId in this.LootableCreatedAt.Keys.ToArray())
            {
                if (this.LootableCreatedAt[inventoryId] + 600 < DateTimeOffset.UtcNow.ToUnixTimeSeconds() || (this.CustomInventories.ContainsKey(inventoryId) && this.CustomInventories[inventoryId].IsInventoryEmpty()))
                {
                    this.LootableCreatedAt.Remove(inventoryId);
                    if (this.CustomInventories.ContainsKey(inventoryId) && this.LootableObjects.ContainsKey(inventoryId))
                    {
                        this.LootableObjects[inventoryId].GameEntity.Remove(0);
                    }
                }
            }

            foreach (var x in OpenedByPeerInventory)
            {
                if (x.Value != null && x.Value.TiedEntity?.GameEntity.GetGlobalFrame().origin.Distance(x.Key.ControlledAgent.Position) > _distanceForAutoCloseInventory)
                {
                    ClosedInventoryOnServer(x.Key, x.Value.InventoryId);

                    // Send signal to client to close inventory
                    GameNetwork.BeginModuleEventAsServer(x.Key);
                    GameNetwork.WriteMessage(new ForceCloseInventory());
                    GameNetwork.EndModuleEventAsServer();
                }
            }
        }

        public override void OnPlayerDisconnectedFromServer(NetworkCommunicator player)
        {
            var persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();

            if (persistentEmpireRepresentative != null)
            {
                persistentEmpireRepresentative.GetInventory().EmptyInventory();
            }

            if (this.OpenedByPeerInventory.ContainsKey(player))
            {
                if (this.OpenedByPeerInventory[player] != null)
                {
                    this.OpenedByPeerInventory[player].RemoveOpenedBy(player);
                }
                this.OpenedByPeerInventory.Remove(player);
            }

            foreach (var x in OpenedByPeerInventory)
            {
                if (x.Value != null && x.Value.TiedEntity?.GameEntity.GetGlobalFrame().origin.Distance(x.Key.ControlledAgent.Position) > 2f)
                {
                    ClosedInventoryOnServer(x.Key, x.Value.InventoryId);
                }
            }
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }
#endif
        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            if (GameNetwork.IsServer && affectedAgent.IsHuman && affectedAgent.IsPlayerControlled && agentState == AgentState.Killed)
            {
                affectedAgent.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.Instant);
                EquipmentIndex shieldIndex = affectedAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
                if (shieldIndex != EquipmentIndex.None)
                {
                    MissionWeapon weapon = new MissionWeapon(affectedAgent.Equipment[shieldIndex].Item, null, null, affectedAgent.Equipment[shieldIndex].Ammo);
                    affectedAgent.RemoveEquippedWeapon(shieldIndex);
                    affectedAgent.EquipWeaponWithNewEntity(shieldIndex, ref weapon);
                }
            }

            if (GameNetwork.IsServer && affectedAgent.MissionPeer != null && affectedAgent.IsHuman && affectedAgent.IsPlayerControlled)
            {
                NetworkCommunicator player = affectedAgent.MissionPeer.GetNetworkPeer();

                if (agentState == AgentState.Killed &&
                        player.QuitFromMission == false &&
                        player.IsConnectionActive)
                {
                    // Save inventory on KO
                    SaveSystemBehavior.HandleCreateOrSavePlayerInventory(player);
                }

                if (this.OpenedByPeerInventory.ContainsKey(player))
                {
                    if (this.OpenedByPeerInventory[player] != null)
                    {

                        this.OpenedByPeerInventory[player].RemoveOpenedBy(player);
                    }
                    this.OpenedByPeerInventory.Remove(player);
                }
                GameNetwork.BeginModuleEventAsServer(player);
                GameNetwork.WriteMessage(new ForceCloseInventory());
                GameNetwork.EndModuleEventAsServer();
            }
            
            // Possibly used on client, thats why I dont move this whole function just to Server side code
            if (this.IgnoreAgentDropLoot.ContainsKey(affectedAgent))
            {
                this.IgnoreAgentDropLoot.Remove(affectedAgent);
                return;
            }

            if (WoundingBehavior.Instance.WoundingEnabled)
            {
                // NO ITEM DROPS ON WOUNDING.
                return;
            }

            if (
                GameNetwork.IsServer &&
                affectedAgent.IsHuman &&
                affectedAgent.IsPlayerControlled &&
                agentState == AgentState.Killed &&
                affectedAgent.MissionPeer != null &&
                (
                    (affectedAgent.MissionPeer.GetNetworkPeer().QuitFromMission == false && affectedAgent.MissionPeer.GetNetworkPeer().IsConnectionActive) //||
                                                                                                                                                           // (affectedAgent.MissionPeer.GetNetworkPeer().QuitFromMission == true && CombatlogBehavior.Instance != null && CombatlogBehavior.Instance.IsPlayerInCombatState(affectedAgent.MissionPeer.GetNetworkPeer()))
                )
            )
            {
                NetworkCommunicator player = affectedAgent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
                if (persistentEmpireRepresentative == null) return;
                Inventory playerInventory = persistentEmpireRepresentative.GetInventory();
                String lootInventoryId = "Loot" + player.UserName + this.RandomString(6);
                Inventory lootInventory = new Inventory(0, 0, lootInventoryId);
                lootInventory.IsConsumable = true;
                Equipment equipments = AgentHelpers.GetCurrentAgentEquipment(affectedAgent);
                EquipmentIndex mainHand = affectedAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                EquipmentIndex offHand = affectedAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);

                bool killedByFriendly = false;

                if (affectorAgent != null && affectorAgent.IsPlayerControlled && affectorAgent.MissionPeer != null)
                {
                    PersistentEmpireRepresentative attackerRepresentative = affectorAgent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>();
                    if (persistentEmpireRepresentative.GetFactionIndex() == attackerRepresentative.GetFactionIndex()) killedByFriendly = true;
                }

                for (EquipmentIndex i = EquipmentIndex.Weapon0; i < EquipmentIndex.ArmorItemEndSlot; i++)
                {

                    if (i != mainHand && i != offHand)
                    {
                        if (!equipments[i].IsEmpty)
                        {
                            int ammo = ItemHelper.GetMaximumAmmo(equipments[i].Item);
                            if ((equipments[i].Item.Type == ItemObject.ItemTypeEnum.Arrows ||
                                equipments[i].Item.Type == ItemObject.ItemTypeEnum.Bolts ||
                                equipments[i].Item.Type == ItemObject.ItemTypeEnum.Bullets) && (i >= EquipmentIndex.Weapon0 && i <= EquipmentIndex.Weapon3))
                            {
                                ammo = affectedAgent.Equipment[i].Amount;
                            }
                            if (random.Next(100) > this.DestroyChance || killedByFriendly)
                            {
                                lootInventory.ExpandInventoryWithItem(equipments[i].Item, 1, ammo);
                            }
                            else
                            {
                                InformationComponent.Instance.SendMessage("Your item " + equipments[i].Item.Name.ToString() + " was destroyed when you died", Color.ConvertStringToColor("#ff0000ff").ToUnsignedInteger(), player);
                            }
                        }
                    }
                }

                foreach (InventorySlot inventorySlot in playerInventory.Slots)
                {
                    if (inventorySlot.Item == null || inventorySlot.Count == 0) continue;

                    if (random.Next(100) > 5 || killedByFriendly)  // % 10 remove item
                    {
                        lootInventory.ExpandInventoryWithItem(inventorySlot.Item, inventorySlot.Count, inventorySlot.Ammo);
                    }
                    else
                    {
                        InformationComponent.Instance.SendMessage("Your item " + inventorySlot.Item.Name.ToString() + " was destroyed when you died", Color.ConvertStringToColor("#ff0000ff").ToUnsignedInteger(), player);
                    }
                }

                playerInventory.EmptyInventory();
                MatrixFrame frame = affectedAgent.Frame;
                PE_InventoryEntity droppedLoot = (PE_InventoryEntity)base.Mission.CreateMissionObjectFromPrefab("pe_loot", frame);
                lootInventory.TiedEntity = droppedLoot;
                droppedLoot.InventoryId = lootInventory.InventoryId;
                droppedLoot.InventoryName = "Loot " + player.UserName + "'s Body";
                droppedLoot.Slot = lootInventory.Slots.Count;

                this.CustomInventories[lootInventory.InventoryId] = lootInventory;
                this.LootableObjects[lootInventory.InventoryId] = droppedLoot;
                this.LootableCreatedAt[lootInventory.InventoryId] = DateTimeOffset.Now.ToUnixTimeSeconds();

                droppedLoot.AddPhysicsSynchedPE(new Vec3(0, 0, 1), Vec3.Zero, "wood");


                if (this.OpenedByPeerInventory.ContainsKey(player))
                {
                    if (this.OpenedByPeerInventory[player] != null)
                    {
                        // LoggerHelper.LogAnAction(player, LogAction.PlayerClosesChest, null, new object[] { this.OpenedByPeerInventory[player] });
                        this.OpenedByPeerInventory[player].RemoveOpenedBy(player);
                    }
                    this.OpenedByPeerInventory.Remove(player);
                }

                if (player.ControlledAgent != null && persistentEmpireRepresentative.IsFirstAgentSpawned)
                {
                    SaveSystemBehavior.HandleCreateOrSavePlayer(player);
                }

                LoggerHelper.LogAnAction(player, LogAction.PlayerDroppedLoot, null, new object[] { lootInventory });

                GameNetwork.BeginModuleEventAsServer(player);
                GameNetwork.WriteMessage(new ForceCloseInventory());
                GameNetwork.EndModuleEventAsServer();
            }
        }

        #endregion

        #region Handlers
#if CLIENT
        private void HandleForceCloseInventoryFromServer(ForceCloseInventory message)
        {
            if (this.OnForceCloseInventory != null)
            {
                this.OnForceCloseInventory();
            }
        }

        private void HandleUpdateInventorySlotFromServer(UpdateInventorySlot message)
        {
            if (this.OnForceUpdateInventory != null)
            {
                this.OnForceUpdateInventory(message.Slot, message.Item, message.Count);
            }
        }

        private void HandleResetAgentArmorFromServer(ResetAgentArmor message)
        {
            AgentHelpers.ResetAgentArmor(message.agent, message.equipment);
        }

        private void HandleExecuteInventoryTransferFromServer(ExecuteInventoryTransfer message)
        {
            if (this.OnUpdateInventory != null)
            {
                this.OnUpdateInventory(message.DraggedSlot, message.DroppedSlot, message.DraggedSlotItem, message.DroppedSlotItem, message.DraggedSlotCount, message.DroppedSlotCount);
            }
        }

        private void HandleOpenInventoryFromServer(OpenInventory message)
        {
            if (this.OnOpenInventory != null)
            {
                this.OnOpenInventory(message.PlayerInventory, message.RequestedInventory);
            }
        }
#endif
#if SERVER
        private bool HandleRequestInventoryTransferFromClient(NetworkCommunicator player, RequestInventoryTransfer message)
        {
            return this.TransferInventoryItem(player, message.DroppedTag, message.DraggedTag);
        }

        private bool HandleRequestOpenInventoryFromClient(NetworkCommunicator player, RequestOpenInventory message)
        {
            this.OpenInventoryForPeer(player, message.InventoryId);
            return true;
        }

        private bool HandleClosedInventoryFromClient(NetworkCommunicator player, ClosedInventory message)
        {
            return ClosedInventoryOnServer(player, message.InventoryId);
        }

        private bool HandleRequestDropItemFromInventory(NetworkCommunicator player, RequestDropItemFromInventory message)
        {
            if (player.ControlledAgent == null || player.ControlledAgent.IsActive() == false) return false;
            string[] dropTag = message.DropTag.Split('_');
            string inventory = string.Join("_", dropTag.Take(dropTag.Length - 1));
            int draggedIndex = int.Parse(dropTag.Last());
            ItemObject droppedItem = null;
            int droppedCount = 0;
            int droppedAmmo = 0;
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            if (inventory == "Equipment")
            {
                Equipment currentEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                if (currentEquipment[draggedIndex].IsEmpty)
                {
                    return false;

                }
                droppedItem = currentEquipment[draggedIndex].Item;
                droppedCount = 1;
                droppedAmmo = ItemHelper.GetMaximumAmmo(currentEquipment[draggedIndex].Item);
                if (droppedItem.ItemType == ItemObject.ItemTypeEnum.Arrows || droppedItem.ItemType == ItemObject.ItemTypeEnum.Bolts || droppedItem.ItemType == ItemObject.ItemTypeEnum.Bullets)
                {
                    droppedAmmo = player.ControlledAgent.Equipment[draggedIndex].Amount;
                }
                if (draggedIndex < (int)EquipmentIndex.NumAllWeaponSlots)
                {
                    player.ControlledAgent.RemoveEquippedWeapon((EquipmentIndex)draggedIndex);
                }
                else
                {
                    currentEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                    currentEquipment[draggedIndex] = new EquipmentElement();
                    AgentHelpers.ResetAgentArmor(player.ControlledAgent, currentEquipment.Clone(false));
                }
                GameNetwork.BeginModuleEventAsServer(player);
                GameNetwork.WriteMessage(new UpdateInventorySlot("Equipment_" + draggedIndex, null, 0));
                GameNetwork.EndModuleEventAsServer();
            }
            else if (inventory == "PlayerInventory")
            {
                Inventory sourceInventory = persistentEmpireRepresentative.GetInventory();
                InventorySlot slot = sourceInventory.Slots[draggedIndex];
                if (slot.Item == null || slot.Count == 0)
                {
                    return false;
                }
                droppedItem = slot.Item;
                droppedCount = slot.Count;
                droppedAmmo = slot.Ammo;
                slot.Count = 0;
                slot.Item = null;

                GameNetwork.BeginModuleEventAsServer(player);
                GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + draggedIndex, slot.Item, slot.Count));
                GameNetwork.EndModuleEventAsServer();
            }
            else if (this.CustomInventories.ContainsKey(inventory))
            {
                Inventory sourceInventory = this.CustomInventories[inventory];
                InventorySlot slot = sourceInventory.Slots[draggedIndex];
                if (slot.Item == null || slot.Count == 0)
                {
                    return false;
                }
                droppedItem = slot.Item;
                droppedCount = slot.Count;
                droppedAmmo = slot.Ammo;
                slot.Count = 0;
                slot.Item = null;
                if (!sourceInventory.GeneratedViaSpawner && !sourceInventory.IsConsumable)
                {
                    SaveSystemBehavior.HandleCreateOrSaveInventory(sourceInventory.InventoryId);
                }
                foreach (NetworkCommunicator otherPlayer in sourceInventory.CurrentlyOpenedBy)
                {
                    if (otherPlayer.IsConnectionActive == false) continue;
                    GameNetwork.BeginModuleEventAsServer(otherPlayer);
                    GameNetwork.WriteMessage(new UpdateInventorySlot(sourceInventory.InventoryId + "_" + draggedIndex, slot.Item, slot.Count));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
            if (droppedItem == null || droppedCount == 0) return false;
            // Find or create a loot entity
            PE_InventoryEntity droppedLoot = null;
            foreach (PE_InventoryEntity entity in this.LootableObjects.Values.ToList())
            {
                float distance = entity.GameEntity.GetGlobalFrame().origin.Distance(player.ControlledAgent.Position);
                if (distance <= 5)
                {
                    droppedLoot = entity;
                    Debug.Print("!!!!!!!!!!!!!!! DISTANCE IS " + distance);
                    break;
                }
            }
            if (droppedLoot == null)
            {
                String lootInventoryId = "Drop" + player.UserName + this.RandomString(6);
                Inventory lootInventory = new Inventory(0, 0, lootInventoryId);
                lootInventory.IsConsumable = true;
                MatrixFrame frame = player.ControlledAgent.Frame;
                droppedLoot = (PE_InventoryEntity)base.Mission.CreateMissionObjectFromPrefab("pe_loot", frame);
                droppedLoot.InventoryId = lootInventory.InventoryId;
                droppedLoot.InventoryName = "Dropped Items";
                droppedLoot.Slot = lootInventory.Slots.Count;
                lootInventory.TiedEntity = droppedLoot;
                this.CustomInventories[lootInventory.InventoryId] = lootInventory;
                this.LootableObjects[lootInventory.InventoryId] = droppedLoot;
                this.LootableCreatedAt[lootInventory.InventoryId] = DateTimeOffset.Now.ToUnixTimeSeconds();

                droppedLoot.AddPhysicsSynchedPE(new Vec3(0, 0, 1), Vec3.Zero, "wood");
            }
            this.CustomInventories[droppedLoot.InventoryId].ExpandInventoryWithItem(droppedItem, droppedCount, droppedAmmo);
            // player.ControlledAgent.Get
            LoggerHelper.LogAnAction(player, LogAction.PlayerDroppedItem, null, new object[] {
                droppedLoot.InventoryId,
                inventory,
                droppedItem,
                droppedCount
            });

            return true;
        }

        private bool HandleRequestRevealItemBag(NetworkCommunicator player, RequestRevealItemBag message)
        {
            if (player.ControlledAgent == null) return true;
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return true;

            if (LastRevealed.ContainsKey(player) && LastRevealed[player] + 3 > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return false;
            }
            LastRevealed[player] = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            Vec3 position = player.ControlledAgent.Position;

            List<AffectedPlayer> affectedPlayers = new List<AffectedPlayer>();
            Inventory playerInventory = persistentEmpireRepresentative.GetInventory();



            foreach (NetworkCommunicator otherPlayer in GameNetwork.NetworkPeers)
            {
                if (otherPlayer.ControlledAgent == null) continue;
                Vec3 otherPlayerPosition = otherPlayer.ControlledAgent.Position;
                float d = position.Distance(otherPlayerPosition);
                if (d < 30)
                {
                    InformationComponent.Instance.SendMessage(player.UserName + " revealed his item bag.", Color.ConvertStringToColor("#FFEB3BFF").ToUnsignedInteger(), otherPlayer);
                    if (playerInventory.IsInventoryEmpty())
                    {
                        InformationComponent.Instance.SendMessage("Bag is empty", Color.ConvertStringToColor("#FFEB3BFF").ToUnsignedInteger(), otherPlayer);
                    }
                    else
                    {
                        foreach (InventorySlot inventorySlot in playerInventory.Slots)
                        {
                            if (inventorySlot.Item != null && inventorySlot.Count > 0)
                            {
                                InformationComponent.Instance.SendMessage(player.UserName + " is carrying " + inventorySlot.Count + " amount of " + inventorySlot.Item.Name.ToString(), Color.ConvertStringToColor("#FFEB3BFF").ToUnsignedInteger(), otherPlayer);
                            }
                        }
                    }

                    if (otherPlayer != player)
                    {
                        affectedPlayers.Add(new AffectedPlayer(otherPlayer));
                    }
                }
            }
            LoggerHelper.LogAnAction(player, LogAction.PlayerRevealedItemBag, affectedPlayers.ToArray());
            return true;
        }
#endif
        #endregion

        #region Functions
#if CLIENT
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            networkMessageHandlerRegisterer.Register<OpenInventory>(this.HandleOpenInventoryFromServer);
            networkMessageHandlerRegisterer.Register<ExecuteInventoryTransfer>(this.HandleExecuteInventoryTransferFromServer);
            networkMessageHandlerRegisterer.Register<ResetAgentArmor>(this.HandleResetAgentArmorFromServer);
            networkMessageHandlerRegisterer.Register<UpdateInventorySlot>(this.HandleUpdateInventorySlotFromServer);
            networkMessageHandlerRegisterer.Register<ForceCloseInventory>(this.HandleForceCloseInventoryFromServer);
        }
#endif
#if SERVER
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            networkMessageHandlerRegisterer.Register<RequestOpenInventory>(this.HandleRequestOpenInventoryFromClient);
            networkMessageHandlerRegisterer.Register<RequestInventoryTransfer>(this.HandleRequestInventoryTransferFromClient);
            networkMessageHandlerRegisterer.Register<ClosedInventory>(this.HandleClosedInventoryFromClient);
            networkMessageHandlerRegisterer.Register<InventoryHotkey>(this.HandleRequestInventoryHotkey);
            networkMessageHandlerRegisterer.Register<InventorySplitItem>(this.HandleRequestSplit);
            networkMessageHandlerRegisterer.Register<RequestDropItemFromInventory>(this.HandleRequestDropItemFromInventory);
            networkMessageHandlerRegisterer.Register<RequestRevealItemBag>(this.HandleRequestRevealItemBag);
        }

        private bool ClosedInventoryOnServer(NetworkCommunicator player, string inventoryId)
        {
            Inventory targetInventory;
            if (this.OpenedByPeerInventory.ContainsKey(player))
            {
                this.OpenedByPeerInventory.Remove(player);
            }
            if (!this.CustomInventories.TryGetValue(inventoryId, out targetInventory)) return false;
            if (targetInventory == null) return false;
            targetInventory.RemoveOpenedBy(player);
            // LoggerHelper.LogAnAction(player, LogAction.PlayerClosesChest, null, new object[] { targetInventory });
            return true;
        }

        public void OpenInventoryForPeer(NetworkCommunicator player, string inventoryId)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;
            Inventory requestedInventory = null;
            Inventory playerInventory = persistentEmpireRepresentative.GetInventory();
            if (inventoryId != "" && this.CustomInventories.ContainsKey(inventoryId))
            {
                requestedInventory = this.CustomInventories[inventoryId];
                requestedInventory.CurrentlyOpenedBy.Add(player);
            }
            this.OpenedByPeerInventory[player] = requestedInventory;

            GameNetwork.BeginModuleEventAsServer(player);
            GameNetwork.WriteMessage(new OpenInventory(inventoryId, playerInventory, requestedInventory));
            GameNetwork.EndModuleEventAsServer();

            if (inventoryId != "" && this.CustomInventories.ContainsKey(inventoryId))
            {
                PENetworkModule.WriteInventorySlots(requestedInventory, player);
            }
            if (requestedInventory != null)
            {
                // LoggerHelper.LogAnAction(player, LogAction.PlayerOpensChest, null, new object[] { requestedInventory });
            }
        }

        public void CleanUpInventory(Inventory targetInventory)
        {
            if (targetInventory == null) return;
            foreach (NetworkCommunicator otherPlayer in targetInventory.CurrentlyOpenedBy)
            {
                if (otherPlayer.IsConnectionActive == false) continue;
                GameNetwork.BeginModuleEventAsServer(otherPlayer);
                GameNetwork.WriteMessage(new ForceCloseInventory());
                GameNetwork.EndModuleEventAsServer();
                if (this.OpenedByPeerInventory.ContainsKey(otherPlayer))
                {
                    this.OpenedByPeerInventory.Remove(otherPlayer);
                }
            }
            targetInventory.CurrentlyOpenedBy.Clear();
            if (this.CustomInventories.ContainsKey(targetInventory.InventoryId)) this.CustomInventories.Remove(targetInventory.InventoryId);
            if (this.LootableObjects.ContainsKey(targetInventory.InventoryId))
            {
                if (this.LootableObjects[targetInventory.InventoryId].GameEntity != null)
                {
                    // this.LootableObjects[targetInventory.InventoryId].GameEntity.Remove(0);
                }
                this.LootableObjects.Remove(targetInventory.InventoryId);
            }
        }

        private bool HandleRequestSplit(NetworkCommunicator player, InventorySplitItem message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            string[] inventoryTag = message.ClickedTag.Split('_');
            string inventory = string.Join("_", inventoryTag.Take(inventoryTag.Length - 1));
            int slot = int.Parse(inventoryTag.Last());
            if (inventory == "PlayerInventory")
            {
                Inventory sourceInventory = persistentEmpireRepresentative.GetInventory();
                ItemObject item = sourceInventory.Slots[slot].Item;
                int itemCount = sourceInventory.Slots[slot].Count;
                if (item == null || itemCount == 0) return false;
                int splittedCount = itemCount / 2;
                if (splittedCount == 0) return false;
                int distributedCount = splittedCount;
                for (int i = slot + 1; i < sourceInventory.Slots.Count; i++)
                {
                    if (i == slot) continue;
                    InventorySlot inventorySlot = sourceInventory.Slots[i];
                    if (inventorySlot.Item == null || inventorySlot.Item.StringId == item.StringId)
                    {
                        int maxAddableQuantity = inventorySlot.MaxStackCount - inventorySlot.Count;
                        int addableQuantity = Math.Min(maxAddableQuantity, distributedCount);
                        inventorySlot.Item = item;
                        inventorySlot.Count += addableQuantity;
                        distributedCount -= addableQuantity;
                        if (addableQuantity > 0)
                        {
                            GameNetwork.BeginModuleEventAsServer(player);
                            GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, inventorySlot.Item, inventorySlot.Count));
                            GameNetwork.EndModuleEventAsServer();
                        }
                        if (distributedCount == 0) break;
                    }
                }
                sourceInventory.Slots[slot].Count = itemCount - (splittedCount - distributedCount);
                GameNetwork.BeginModuleEventAsServer(player);
                GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + slot, sourceInventory.Slots[slot].Item, sourceInventory.Slots[slot].Count));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }
            else if (this.CustomInventories.ContainsKey(inventory))
            {
                Inventory sourceInventory = this.CustomInventories[inventory];
                ItemObject item = sourceInventory.Slots[slot].Item;
                int itemCount = sourceInventory.Slots[slot].Count;
                if (item == null || itemCount == 0) return false;
                int splittedCount = itemCount / 2;
                if (splittedCount == 0) return false;
                int distributedCount = splittedCount;
                for (int i = slot + 1; i < sourceInventory.Slots.Count; i++)
                {
                    if (i == slot) continue;
                    InventorySlot inventorySlot = sourceInventory.Slots[i];
                    if (inventorySlot.Item == null || inventorySlot.Item.StringId == item.StringId)
                    {

                        int maxAddableQuantity = inventorySlot.MaxStackCount - inventorySlot.Count;
                        int addableQuantity = Math.Min(maxAddableQuantity, distributedCount);
                        inventorySlot.Item = item;
                        inventorySlot.Count += addableQuantity;
                        distributedCount -= addableQuantity;
                        if (addableQuantity > 0)
                        {
                            foreach (NetworkCommunicator otherPlayer in sourceInventory.CurrentlyOpenedBy)
                            {
                                if (otherPlayer.IsConnectionActive == false) continue;
                                GameNetwork.BeginModuleEventAsServer(otherPlayer);
                                GameNetwork.WriteMessage(new UpdateInventorySlot(sourceInventory.InventoryId + "_" + i, inventorySlot.Item, inventorySlot.Count));
                                GameNetwork.EndModuleEventAsServer();
                            }
                        }
                        if (distributedCount == 0) break;
                    }
                }
                sourceInventory.Slots[slot].Count = itemCount - (splittedCount - distributedCount);
                if (!sourceInventory.GeneratedViaSpawner && !sourceInventory.IsConsumable)
                {
                    SaveSystemBehavior.HandleCreateOrSaveInventory(sourceInventory.InventoryId);
                }
                foreach (NetworkCommunicator otherPlayer in sourceInventory.CurrentlyOpenedBy)
                {
                    if (otherPlayer.IsConnectionActive == false) continue;
                    GameNetwork.BeginModuleEventAsServer(otherPlayer);
                    GameNetwork.WriteMessage(new UpdateInventorySlot(sourceInventory.InventoryId + "_" + slot, sourceInventory.Slots[slot].Item, sourceInventory.Slots[slot].Count));
                    GameNetwork.EndModuleEventAsServer();
                }
                return true;
            }
            return true;
        }

        private bool HandleRequestInventoryHotkey(NetworkCommunicator player, InventoryHotkey message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;

            string[] inventoryTag = message.ClickedTag.Split('_');
            string inventory = string.Join("_", inventoryTag.Take(inventoryTag.Length - 1));
            int slot = int.Parse(inventoryTag.Last());
            // If clicked in Equipment Inventory
            if (inventory == "Equipment")
            {
                if (player.ControlledAgent == null) return false;
                Equipment agentEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                Inventory targetInventory = persistentEmpireRepresentative.GetInventory();
                ItemObject item = agentEquipment[slot].IsEmpty ? null : agentEquipment[slot].Item;
                int itemCount = agentEquipment[slot].IsEmpty ? 0 : 1;
                if (item == null || itemCount == 0) return false;

                for (int i = 0; i < targetInventory.Slots.Count; i++)
                {
                    InventorySlot inventorySlot = targetInventory.Slots[i];
                    if (inventorySlot.Item == null && inventorySlot.Count + 1 <= inventorySlot.MaxStackCount)
                    {
                        return this.TransferInventoryItem(player, "PlayerInventory_" + i, message.ClickedTag);
                    }
                    else if (inventorySlot.Item.StringId == item.StringId && inventorySlot.Count + 1 <= inventorySlot.MaxStackCount)
                    {
                        return this.TransferInventoryItem(player, "PlayerInventory_" + i, message.ClickedTag);
                    }
                }
            }
            else if (inventory == "PlayerInventory")
            {
                if (!this.OpenedByPeerInventory.ContainsKey(player)) return false;
                Inventory targetInventory = this.OpenedByPeerInventory[player];
                Inventory sourceInventory = persistentEmpireRepresentative.GetInventory();
                ItemObject item = sourceInventory.Slots[slot].Item;
                int itemCount = sourceInventory.Slots[slot].Count;
                if (item == null || itemCount == 0) return false;
                for (int i = 0; i < targetInventory.Slots.Count; i++)
                {
                    InventorySlot inventorySlot = targetInventory.Slots[i];
                    if (inventorySlot.Item == null)
                    {
                        int maxAddableQuantity = inventorySlot.MaxStackCount - inventorySlot.Count;
                        int addableQuantity = Math.Min(maxAddableQuantity, itemCount);
                        this.TransferInventoryItem(player, targetInventory.InventoryId + "_" + i, message.ClickedTag);
                        itemCount -= addableQuantity;
                        if (itemCount == 0) break;
                    }
                    else if (inventorySlot.Item.StringId == item.StringId)
                    {
                        int maxAddableQuantity = inventorySlot.MaxStackCount - inventorySlot.Count;
                        int addableQuantity = Math.Min(maxAddableQuantity, itemCount);
                        this.TransferInventoryItem(player, targetInventory.InventoryId + "_" + i, message.ClickedTag);
                        itemCount -= addableQuantity;
                        if (itemCount == 0) break;
                    }
                }
                return true;
            }
            else if (this.CustomInventories.ContainsKey(inventory))
            {
                if (!this.OpenedByPeerInventory.ContainsKey(player)) return false;
                Inventory targetInventory = persistentEmpireRepresentative.GetInventory();
                Inventory sourceInventory = this.CustomInventories[inventory];
                ItemObject item = sourceInventory.Slots[slot].Item;
                int itemCount = sourceInventory.Slots[slot].Count;
                if (item == null || itemCount == 0) return false;
                for (int i = 0; i < targetInventory.Slots.Count; i++)
                {
                    InventorySlot inventorySlot = targetInventory.Slots[i];
                    if (inventorySlot.Item == null)
                    {
                        int maxAddableQuantity = inventorySlot.MaxStackCount - inventorySlot.Count;
                        int addableQuantity = Math.Min(maxAddableQuantity, itemCount);
                        this.TransferInventoryItem(player, targetInventory.InventoryId + "_" + i, message.ClickedTag);
                        itemCount -= addableQuantity;
                        if (itemCount == 0) break;
                    }
                    else if (inventorySlot.Item.StringId == item.StringId)
                    {
                        int maxAddableQuantity = inventorySlot.MaxStackCount - inventorySlot.Count;
                        int addableQuantity = Math.Min(maxAddableQuantity, itemCount);
                        this.TransferInventoryItem(player, targetInventory.InventoryId + "_" + i, message.ClickedTag);
                        itemCount -= addableQuantity;
                        if (itemCount == 0) break;
                    }
                }
                return true;
            }
            return true;
        }

        private bool TransferInventoryItem(NetworkCommunicator player, string DroppedTag, string DraggedTag)
        {
            if (DroppedTag == DraggedTag) return false;
            if (player.ControlledAgent == null || player.ControlledAgent.IsActive() == false) return false;


            string[] draggedTag = DraggedTag.Split('_');
            string[] droppedTag = DroppedTag.Split('_');
            string draggedFromInventory = string.Join("_", draggedTag.Take(draggedTag.Length - 1));
            string droppedToInventory = string.Join("_", droppedTag.Take(droppedTag.Length - 1));
            int draggedIndex = int.Parse(draggedTag.Last());
            int droppedIndex = int.Parse(droppedTag.Last());
            ItemObject draggedItem = null;
            InventorySlot itemAddedFrom = null;
            int draggedAmmo = 0;
            int draggedCount = 0;
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            if (player.ControlledAgent == null) return false;

            if (this.CustomInventories.ContainsKey(draggedFromInventory) && this.CustomInventories[draggedFromInventory].TiedEntity != null && this.CustomInventories[draggedFromInventory].TiedEntity.GameEntity != null && this.CustomInventories[draggedFromInventory].TiedEntity.GameEntity.GetGlobalFrame().origin.Distance(player.ControlledAgent.Position) > 10f)
            {
                return false;
            }

            if (draggedFromInventory == "Equipment")
            {
                if (player.ControlledAgent == null) return false;
                Equipment agentEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                draggedItem = agentEquipment[draggedIndex].IsEmpty ? null : agentEquipment[draggedIndex].Item;
                if (draggedItem.ItemType == ItemObject.ItemTypeEnum.Arrows ||
                    draggedItem.ItemType == ItemObject.ItemTypeEnum.Bolts ||
                    draggedItem.ItemType == ItemObject.ItemTypeEnum.Bullets)
                {
                    draggedAmmo = player.ControlledAgent.Equipment[draggedIndex].Amount;
                }
                draggedCount = agentEquipment[draggedIndex].IsEmpty ? 0 : 1;
            }
            else if (draggedFromInventory == "PlayerInventory")
            {
                itemAddedFrom = persistentEmpireRepresentative.GetInventory().Slots[draggedIndex];
                draggedItem = persistentEmpireRepresentative.GetInventory().Slots[draggedIndex].Item;
                draggedCount = persistentEmpireRepresentative.GetInventory().Slots[draggedIndex].Count;
                draggedAmmo = persistentEmpireRepresentative.GetInventory().Slots[draggedIndex].Ammo;
            }
            else if (this.CustomInventories.ContainsKey(draggedFromInventory))
            {
                Inventory transferredFrom = this.CustomInventories[draggedFromInventory];
                itemAddedFrom = transferredFrom.Slots[draggedIndex];
                draggedItem = transferredFrom.Slots[draggedIndex].Item;
                draggedCount = transferredFrom.Slots[draggedIndex].Count;
                draggedAmmo = transferredFrom.Slots[draggedIndex].Ammo;
            }

            if (draggedItem == null) return false;
            if (draggedCount == 0) return false;

            if (droppedToInventory == "Equipment")
            {
                if (droppedIndex == (int)EquipmentIndex.Head && draggedItem.ItemType == ItemObject.ItemTypeEnum.HeadArmor && player.ControlledAgent.SpawnEquipment[droppedIndex].IsEmpty)
                {
                    // Equip Head Armor Re-Render Agent, Send Inventory Update Data.
                    itemAddedFrom.Count -= 1;
                    if (itemAddedFrom.Count == 0) itemAddedFrom.Item = null;
                    Equipment currentEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                    currentEquipment[EquipmentIndex.Head] = new EquipmentElement(draggedItem);
                    AgentHelpers.ResetAgentArmor(player.ControlledAgent, currentEquipment.Clone(false));
                    GameNetwork.BeginModuleEventAsServer(player);
                    GameNetwork.WriteMessage(new ExecuteInventoryTransfer(DraggedTag, DroppedTag, itemAddedFrom.Item, draggedItem, itemAddedFrom.Count, 1));
                    GameNetwork.EndModuleEventAsServer();
                }
                else if (droppedIndex == (int)EquipmentIndex.Cape && draggedItem.ItemType == ItemObject.ItemTypeEnum.Cape && player.ControlledAgent.SpawnEquipment[droppedIndex].IsEmpty)
                {
                    // Equip Head Armor Re-Render Agent, Send Inventory Update Data.
                    itemAddedFrom.Count -= 1;
                    if (itemAddedFrom.Count == 0) itemAddedFrom.Item = null;
                    Equipment currentEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                    currentEquipment[EquipmentIndex.Cape] = new EquipmentElement(draggedItem);
                    AgentHelpers.ResetAgentArmor(player.ControlledAgent, currentEquipment.Clone(false));
                    GameNetwork.BeginModuleEventAsServer(player);
                    GameNetwork.WriteMessage(new ExecuteInventoryTransfer(DraggedTag, DroppedTag, itemAddedFrom.Item, draggedItem, itemAddedFrom.Count, 1));
                    GameNetwork.EndModuleEventAsServer();
                }
                else if (droppedIndex == (int)EquipmentIndex.Body && draggedItem.ItemType == ItemObject.ItemTypeEnum.BodyArmor && player.ControlledAgent.SpawnEquipment[droppedIndex].IsEmpty)
                {
                    // Equip Head Armor Re-Render Agent, Send Inventory Update Data.
                    itemAddedFrom.Count -= 1;
                    if (itemAddedFrom.Count == 0) itemAddedFrom.Item = null;
                    Equipment currentEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                    currentEquipment[EquipmentIndex.Body] = new EquipmentElement(draggedItem);
                    AgentHelpers.ResetAgentArmor(player.ControlledAgent, currentEquipment.Clone(false));
                    GameNetwork.BeginModuleEventAsServer(player);
                    GameNetwork.WriteMessage(new ExecuteInventoryTransfer(DraggedTag, DroppedTag, itemAddedFrom.Item, draggedItem, itemAddedFrom.Count, 1));
                    GameNetwork.EndModuleEventAsServer();
                }
                else if (droppedIndex == (int)EquipmentIndex.Leg && draggedItem.ItemType == ItemObject.ItemTypeEnum.LegArmor && player.ControlledAgent.SpawnEquipment[droppedIndex].IsEmpty)
                {
                    // Equip Head Armor Re-Render Agent, Send Inventory Update Data.
                    itemAddedFrom.Count -= 1;
                    if (itemAddedFrom.Count == 0) itemAddedFrom.Item = null;
                    Equipment currentEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                    currentEquipment[EquipmentIndex.Leg] = new EquipmentElement(draggedItem);
                    AgentHelpers.ResetAgentArmor(player.ControlledAgent, currentEquipment.Clone(false));
                    GameNetwork.BeginModuleEventAsServer(player);
                    GameNetwork.WriteMessage(new ExecuteInventoryTransfer(DraggedTag, DroppedTag, itemAddedFrom.Item, draggedItem, itemAddedFrom.Count, 1));
                    GameNetwork.EndModuleEventAsServer();
                }
                else if (droppedIndex == (int)EquipmentIndex.Gloves && draggedItem.ItemType == ItemObject.ItemTypeEnum.HandArmor && player.ControlledAgent.SpawnEquipment[droppedIndex].IsEmpty)
                {
                    // Equip Head Armor Re-Render Agent, Send Inventory Update Data.
                    itemAddedFrom.Count -= 1;
                    if (itemAddedFrom.Count == 0) itemAddedFrom.Item = null;
                    Equipment currentEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                    currentEquipment[EquipmentIndex.Gloves] = new EquipmentElement(draggedItem);
                    AgentHelpers.ResetAgentArmor(player.ControlledAgent, currentEquipment.Clone(false));
                    GameNetwork.BeginModuleEventAsServer(player);
                    GameNetwork.WriteMessage(new ExecuteInventoryTransfer(DraggedTag, DroppedTag, itemAddedFrom.Item, draggedItem, itemAddedFrom.Count, 1));
                    GameNetwork.EndModuleEventAsServer();
                }
                else if (droppedIndex >= (int)EquipmentIndex.Weapon0 && droppedIndex <= (int)EquipmentIndex.Weapon3 &&
                    (
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.OneHandedWeapon ||
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.TwoHandedWeapon ||
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.Polearm ||
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.Pistol ||
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.Shield ||
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.Arrows ||
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.Bow ||
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.Musket ||
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.Crossbow ||
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.Bullets ||
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.Banner ||
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.Thrown ||
                        draggedItem.ItemType == ItemObject.ItemTypeEnum.Bolts
                    ) && player.ControlledAgent.Equipment[droppedIndex].IsEmpty
                )
                {
                    Faction f = persistentEmpireRepresentative.GetFaction();
                    Banner b = f == null ? null : f.banner;
                    if (itemAddedFrom != null)
                    {

                        itemAddedFrom.Count -= 1;
                        if (itemAddedFrom.Count == 0) itemAddedFrom.Item = null;
                        MissionWeapon weapon;
                        if (draggedItem.Type == ItemObject.ItemTypeEnum.Arrows ||
                                draggedItem.Type == ItemObject.ItemTypeEnum.Bolts ||
                                draggedItem.Type == ItemObject.ItemTypeEnum.Bullets)
                        {
                            weapon = new MissionWeapon(draggedItem, null, b, (short)itemAddedFrom.Ammo);
                        }
                        else
                        {
                            weapon = new MissionWeapon(draggedItem, null, b);
                        }

                        player.ControlledAgent.EquipWeaponWithNewEntity((EquipmentIndex)droppedIndex, ref weapon);
                        GameNetwork.BeginModuleEventAsServer(player);
                        GameNetwork.WriteMessage(new ExecuteInventoryTransfer(DraggedTag, DroppedTag, itemAddedFrom.Item, draggedItem, itemAddedFrom.Count, 1));
                        GameNetwork.EndModuleEventAsServer();
                    }
                    else
                    {
                        MissionWeapon weapon;
                        // Equipment oldEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                        if (draggedItem.Type == ItemObject.ItemTypeEnum.Arrows ||
                                draggedItem.Type == ItemObject.ItemTypeEnum.Bolts ||
                                draggedItem.Type == ItemObject.ItemTypeEnum.Bullets)
                        {
                            //weapon = new MissionWeapon(draggedItem, null, b, (short)draggedAmmo);
                            weapon = new MissionWeapon(draggedItem, null, b, player.ControlledAgent.Equipment[draggedIndex].Amount);

                        }
                        else
                        {
                            weapon = new MissionWeapon(draggedItem, null, b);
                        }
                        player.ControlledAgent.EquipWeaponWithNewEntity((EquipmentIndex)droppedIndex, ref weapon);
                        player.ControlledAgent.RemoveEquippedWeapon((EquipmentIndex)draggedIndex);
                        Equipment currentEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                        GameNetwork.BeginModuleEventAsServer(player);
                        GameNetwork.WriteMessage(new ExecuteInventoryTransfer(DraggedTag, DroppedTag, currentEquipment[draggedIndex].Item, draggedItem, 0, 1));
                        GameNetwork.EndModuleEventAsServer();
                    }
                }

                if (this.CustomInventories.ContainsKey(draggedFromInventory))
                {
                    ItemObject item = draggedItem;
                    LoggerHelper.LogAnAction(player, LogAction.PlayerEquipedItemFromChest, null, new object[] {
                        draggedFromInventory,
                        item,
                        1
                    });
                }
                else if (droppedToInventory.StartsWith("Equipment") && draggedFromInventory.StartsWith("PlayerInventory"))
                {

                    ItemObject item = draggedItem;
                    LoggerHelper.LogAnAction(player, LogAction.PlayerEquiptedFromInventory, null, new object[] {
                        draggedFromInventory,
                        item,
                        1
                    });
                }
            }
            else if (droppedToInventory == "PlayerInventory" || this.CustomInventories.ContainsKey(droppedToInventory))
            {
                Inventory playerInventory = persistentEmpireRepresentative.GetInventory();
                Inventory targetInventory = droppedToInventory == "PlayerInventory" ? playerInventory : this.CustomInventories[droppedToInventory];
                int returnedAmount = targetInventory.AddItem(droppedIndex, draggedItem, draggedCount, draggedAmmo, itemAddedFrom);
                Equipment currentEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                if (draggedFromInventory == "Equipment")
                {
                    if (draggedIndex < (int)EquipmentIndex.NumAllWeaponSlots && returnedAmount == 0)
                    {
                        player.ControlledAgent.RemoveEquippedWeapon((EquipmentIndex)draggedIndex);
                    }
                    else if (returnedAmount == 0)
                    {
                        currentEquipment = AgentHelpers.GetCurrentAgentEquipment(player.ControlledAgent);
                        currentEquipment[draggedIndex] = new EquipmentElement();
                        AgentHelpers.ResetAgentArmor(player.ControlledAgent, currentEquipment.Clone(false));
                    }
                }

                GameNetwork.BeginModuleEventAsServer(player);
                GameNetwork.WriteMessage(new ExecuteInventoryTransfer(
                    DraggedTag,
                    DroppedTag,
                    draggedFromInventory == "Equipment" ? (returnedAmount == 0 ? null : currentEquipment[draggedIndex].Item) : itemAddedFrom.Item,
                    targetInventory.Slots[droppedIndex].Item,
                    draggedFromInventory == "Equipment" ? returnedAmount : itemAddedFrom.Count, targetInventory.Slots[droppedIndex].Count));
                GameNetwork.EndModuleEventAsServer();

                if (this.CustomInventories.ContainsKey(droppedToInventory) && (draggedFromInventory.StartsWith("PlayerInventory") || draggedFromInventory.StartsWith("Equipment")))
                {
                    ItemObject item = draggedItem;

                    LoggerHelper.LogAnAction(player, LogAction.PlayerTransferredItemToChest, null, new object[] {
                        droppedToInventory,
                        item,
                        draggedCount - returnedAmount,
                        draggedFromInventory
                    });
                }
                else if (droppedToInventory.StartsWith("PlayerInventory") && this.CustomInventories.ContainsKey(draggedFromInventory))
                {

                    ItemObject item = draggedItem;
                    LoggerHelper.LogAnAction(player, LogAction.PlayerTransferredItemFromChest, null, new object[] {
                        draggedFromInventory,
                        item,
                        draggedCount - returnedAmount
                    });
                }
                else if (droppedToInventory.StartsWith("PlayerInventory") && draggedFromInventory.StartsWith("Equipment"))
                {

                    ItemObject item = draggedItem;
                    LoggerHelper.LogAnAction(player, LogAction.PlayerTransferredItemToInventory, null, new object[] {
                        draggedFromInventory,
                        item,
                        draggedCount - returnedAmount
                    });
                }
            }
            // Update other peers
            if (this.CustomInventories.ContainsKey(droppedToInventory))
            {
                Inventory targetInventory = this.CustomInventories[droppedToInventory];
                if (!targetInventory.GeneratedViaSpawner && !targetInventory.IsConsumable && targetInventory.InventoryId != "PlayerInventory")
                {
                    SaveSystemBehavior.HandleCreateOrSaveInventory(targetInventory.InventoryId);
                }
                foreach (NetworkCommunicator otherPlayer in targetInventory.CurrentlyOpenedBy)
                {
                    if (otherPlayer.IsConnectionActive == false) continue;
                    if (otherPlayer == player) continue;
                    GameNetwork.BeginModuleEventAsServer(otherPlayer);
                    GameNetwork.WriteMessage(new UpdateInventorySlot(DroppedTag, targetInventory.Slots[droppedIndex].Item, targetInventory.Slots[droppedIndex].Count));
                    GameNetwork.EndModuleEventAsServer();
                }
                if (targetInventory.IsConsumable && targetInventory.IsInventoryEmpty())
                {
                    this.LootableObjects[targetInventory.InventoryId].GameEntity.Remove(0);
                }

            }
            if (this.CustomInventories.ContainsKey(draggedFromInventory))
            {
                Inventory targetInventory = this.CustomInventories[draggedFromInventory];
                if (!targetInventory.GeneratedViaSpawner && !targetInventory.IsConsumable && targetInventory.InventoryId != "PlayerInventory")
                {
                    SaveSystemBehavior.HandleCreateOrSaveInventory(targetInventory.InventoryId);
                }
                foreach (NetworkCommunicator otherPlayer in targetInventory.CurrentlyOpenedBy)
                {
                    if (otherPlayer == player || otherPlayer.IsConnectionActive == false) continue;
                    GameNetwork.BeginModuleEventAsServer(otherPlayer);
                    GameNetwork.WriteMessage(new UpdateInventorySlot(DraggedTag, targetInventory.Slots[draggedIndex].Item, targetInventory.Slots[draggedIndex].Count));
                    GameNetwork.EndModuleEventAsServer();
                }
                if (targetInventory.IsConsumable && targetInventory.IsInventoryEmpty())
                {
                    this.LootableObjects[targetInventory.InventoryId].GameEntity.Remove(0);
                }
            }
            return true;
        }
#endif
        #endregion
    }
}

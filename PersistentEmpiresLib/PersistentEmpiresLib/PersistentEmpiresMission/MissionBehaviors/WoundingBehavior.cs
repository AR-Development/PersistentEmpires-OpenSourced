using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresLib.NetworkMessages.Client;
using System.Linq;
using TaleWorlds.ObjectSystem;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using TaleWorlds.PlayerServices;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class WoundingBehavior : MissionNetwork
    {
        public static WoundingBehavior Instance;
        public bool WoundingEnabled = false;
        public int WoundingTime = 60;
        public string ItemId = "pe_doctorscalpel";
        public int RequiredMedicineSkillForHealing = 50;

        public Dictionary<string, KeyValuePair<bool, long>> WoundedUntil = new Dictionary<string, KeyValuePair<bool, long>>();
        public Dictionary<string, Vec3> DeathPlace = new Dictionary<string, Vec3>();
        public Dictionary<string, bool> IsWounded = new Dictionary<string, bool>();
        public Dictionary<string, Tuple<bool, Equipment>> DeathEquipment = new Dictionary<string, Tuple<bool, Equipment>>();
        public Dictionary<string, MissionEquipment> DeathWeaponEquipment = new Dictionary<string, MissionEquipment>();
#if SERVER
        private static List<string> ItemsWhichCanBeUsedByWounded = new List<string>();
#endif

            #region MissionNetwork
#if CLIENT
        public override void OnBehaviorInitialize()
        {        
            base.OnBehaviorInitialize();
            if (Instance == null)
            {
                Instance = this;
            }
            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            if (affectedAgent.IsHuman && affectedAgent.IsPlayerControlled
                                    && agentState == AgentState.Killed
                                    && affectedAgent.MissionPeer != null
                                    && affectedAgent.MissionPeer.GetNetworkPeer().QuitFromMission == false)
            {
                var spawnEquipment = affectedAgent.SpawnEquipment.Clone(true);
                for (var equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                {
                    spawnEquipment[equipmentIndex] = new EquipmentElement(affectedAgent.Equipment[equipmentIndex].Item);
                }

                var equipments = new List<string>();
                equipments.Add(spawnEquipment[EquipmentIndex.Weapon0].Item?.StringId);
                equipments.Add(spawnEquipment[EquipmentIndex.Weapon1].Item?.StringId);
                equipments.Add(spawnEquipment[EquipmentIndex.Weapon2].Item?.StringId);
                equipments.Add(spawnEquipment[EquipmentIndex.Weapon3].Item?.StringId);

                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new RegisterClientEquipmentOnWound(equipments));
                GameNetwork.EndModuleEventAsClient();
            }
        }
#endif
#if SERVER
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            if (Instance == null)
            {
                Instance = this;
            }

            WoundingEnabled = ConfigManager.GetBoolConfig("WoundingEnabled", false);
            WoundingTime = ConfigManager.GetIntConfig("WoundingTimeMinutes", 60);
            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
        }

        public override void OnPlayerConnectedToServer(NetworkCommunicator networkPeer)
        {
            base.OnPlayerConnectedToServer(networkPeer);

            if (WoundingEnabled == false) return;

            var woundedUntil = SaveSystemBehavior.HandleGetWoundedUntil(networkPeer);

            if(woundedUntil.HasValue)
            {
                if(!WoundedUntil.ContainsKey(networkPeer.VirtualPlayer?.ToPlayerId()))
                {
                    WoundedUntil.Add(networkPeer.VirtualPlayer?.ToPlayerId(), new KeyValuePair<bool, long>(true, woundedUntil.Value));
                }

                if (!IsWounded.ContainsKey(networkPeer.VirtualPlayer?.ToPlayerId()))
                {
                    IsWounded.Add(networkPeer.VirtualPlayer?.ToPlayerId(), true);
                }
            }

            foreach (string playerId in IsWounded.Keys.ToList())
            {
                if (IsWounded.ContainsKey(playerId) && IsWounded[playerId])
                {
                    var peer = GameNetwork.NetworkPeers.Where(x => x.VirtualPlayer?.ToPlayerId() == playerId).FirstOrDefault();
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new UpdateWoundedPlayer(peer, true));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (!WoundingEnabled) return;

            foreach (string player in WoundedUntil.Keys.ToList())
            {
                if (!WoundedUntil.ContainsKey(player)) continue;

                if (WoundedUntil[player].Value < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {                    
                    HealPlayer(player);
                }
                else
                {
                    var peer = GameNetwork.NetworkPeers.Where(x => x.VirtualPlayer?.ToPlayerId() == player).FirstOrDefault();
                    if (peer?.ControlledAgent != null)
                    {
                        if (!ItemsWhichCanBeUsedByWounded.Contains(peer.ControlledAgent.WieldedWeapon.Item?.StringId) || !AgentHungerBehavior.Instance.Eatables.Any(x=> x.Item == peer.ControlledAgent.WieldedWeapon.Item))
                        {
                            peer.ControlledAgent?.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.Instant);
                        }
                    }
                }
            }
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            if (affectedAgent.IsHuman && affectedAgent.IsPlayerControlled 
                && agentState == AgentState.Killed && WoundingEnabled 
                && affectedAgent.MissionPeer != null 
                && affectedAgent.MissionPeer.GetNetworkPeer().QuitFromMission == false && affectedAgent.MissionPeer.GetNetworkPeer().IsConnectionActive
            )
            {
                var player = affectedAgent.MissionPeer.GetNetworkPeer();
                DeathPlace[player.VirtualPlayer?.ToPlayerId()] = affectedAgent.Position;
                var spawnEquipment = affectedAgent.SpawnEquipment.Clone(true);
                DeathEquipment[player.VirtualPlayer?.ToPlayerId()] = new Tuple<bool, Equipment>(false, spawnEquipment);
                // this.DeathWeaponEquipment[player] = affectedAgent.Equipment[Equip]
                for (var equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                {
                    DeathEquipment[player.VirtualPlayer?.ToPlayerId()].Item2[equipmentIndex] = new EquipmentElement(affectedAgent.Equipment[equipmentIndex].Item);
                }

                //if your wounded AND your not healed yet then keep old timer
                if (WoundedUntil.ContainsKey(player.VirtualPlayer?.ToPlayerId()) && WoundedUntil[player.VirtualPlayer?.ToPlayerId()].Value < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    // Recalculate stats
                    player.ControlledAgent.UpdateAgentStats();
                    return;
                }

                // otherwise get new heal time
                var woundTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (WoundingTime * 60);
                WoundedUntil[player.VirtualPlayer?.ToPlayerId()] = new KeyValuePair<bool, long>(false, woundTime);
                IsWounded[player.VirtualPlayer?.ToPlayerId()] = true;
                var persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
                persistentEmpireRepresentative.SetWounded(woundTime);
                // Make sure it get saved in db
                SaveSystemBehavior.HandleUpdateWoundedUntil(player, woundTime);

                // Recalculate stats
                player.ControlledAgent.UpdateAgentStats();

                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new UpdateWoundedPlayer(player, true));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

                InformationComponent.Instance.SendMessage("You are now wounded.", Color.ConvertStringToColor("#F44336FF").ToUnsignedInteger(), player);
            }
        }
#endif
        #endregion

        #region Handlers
#if CLIENT
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            networkMessageHandlerRegisterer.Register<UpdateWoundedPlayer>(this.HandleUpdateWoundedPlayer);
        }

        private void HandleUpdateWoundedPlayer(UpdateWoundedPlayer message)
        {
            IsWounded[message.Player.VirtualPlayer.ToPlayerId()] = message.IsWounded;
        }
#endif
#if SERVER
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            networkMessageHandlerRegisterer.Register<RegisterClientEquipmentOnWound>(HandleRegisterClientEquipmentOnWound);
        }

        private bool HandleRegisterClientEquipmentOnWound(NetworkCommunicator player, RegisterClientEquipmentOnWound message)
        {
            if (DeathEquipment.ContainsKey(player.VirtualPlayer?.ToPlayerId()))
            {
                var playerEquipment = DeathEquipment[player.VirtualPlayer?.ToPlayerId()].Item2;

                if (!DeathEquipment[player.VirtualPlayer?.ToPlayerId()].Item1)
                {
                    if (playerEquipment[EquipmentIndex.Weapon0].Item?.StringId != (string.IsNullOrEmpty(message.Equipments[0]) ? null : message.Equipments[0]))
                    {
                        playerEquipment[EquipmentIndex.Weapon0] = new EquipmentElement();
                    }
                    if (playerEquipment[EquipmentIndex.Weapon1].Item?.StringId != (string.IsNullOrEmpty(message.Equipments[1]) ? null : message.Equipments[1]))
                    {
                        playerEquipment[EquipmentIndex.Weapon1] = new EquipmentElement();
                    }
                    if (playerEquipment[EquipmentIndex.Weapon2].Item?.StringId != (string.IsNullOrEmpty(message.Equipments[2]) ? null : message.Equipments[2]))
                    {
                        playerEquipment[EquipmentIndex.Weapon2] = new EquipmentElement();
                    }
                    if (playerEquipment[EquipmentIndex.Weapon3].Item?.StringId != (string.IsNullOrEmpty(message.Equipments[3]) ? null : message.Equipments[3]))
                    {
                        playerEquipment[EquipmentIndex.Weapon3] = new EquipmentElement();
                    }
                    DeathEquipment[player.VirtualPlayer?.ToPlayerId()] = new Tuple<bool, Equipment>(true, playerEquipment);
                }
            }

            return true;
        }
#endif
        #endregion

        #region Functions
#if CLIENT
#endif
#if SERVER        
        public void HealPlayer(string playerId)
        {
            var player = GameNetwork.NetworkPeers.Where(x => x.VirtualPlayer?.ToPlayerId() == playerId).FirstOrDefault();
            var persistentEmpireRepresentative = player?.GetComponent<PersistentEmpireRepresentative>();
            if (!WoundedUntil.ContainsKey(playerId) || persistentEmpireRepresentative == null) return;

            InformationComponent.Instance.SendMessage("You are no longer wounded.", Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), player);
            WoundedUntil.Remove(playerId);
            IsWounded[playerId] = false;
            persistentEmpireRepresentative.UnWound();
            player.ControlledAgent.UpdateAgentStats();

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new UpdateWoundedPlayer(player, false));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }

        public static void AddItmemToItemsWhichCanBeUsedByWoundedList(string itemId)
        {
            ItemsWhichCanBeUsedByWounded.Add(itemId);
        }

        // This should be not allowed to make wounded man heal other man.....
        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            if (!affectorAgent.IsHuman) return;
            if (affectorWeapon.Item == null) return;
            if (affectorWeapon.Item != null && affectorWeapon.Item.StringId != this.ItemId) return;
            SkillObject medicineSkill = MBObjectManager.Instance.GetObject<SkillObject>("Medicine");
            if (affectorAgent.Character.GetSkillValue(medicineSkill) < RequiredMedicineSkillForHealing) return;

            var communicator = affectedAgent.MissionPeer?.GetNetworkPeer();
            if (communicator == null) return;
            if (!WoundedUntil.ContainsKey(communicator.VirtualPlayer?.ToPlayerId()) || !WoundedUntil[communicator.VirtualPlayer?.ToPlayerId()].Key) return;

            var deductedTime = WoundedUntil[communicator.VirtualPlayer?.ToPlayerId()].Value / 2;
            WoundedUntil[communicator.VirtualPlayer?.ToPlayerId()] = new KeyValuePair<bool, long>(true, deductedTime);
            var persistentEmpireRepresentative = communicator.GetComponent<PersistentEmpireRepresentative>();
            persistentEmpireRepresentative.SetWounded(deductedTime);
            // Make sure it get saved in db
            SaveSystemBehavior.HandleUpdateWoundedUntil(communicator, deductedTime);
        }
#endif


        public bool IsAgentWounded(Agent agent)
        {
            if (agent.MissionPeer == null) return false;
            if (agent.IsActive() == false) return false;
            return this.IsPlayerWounded(agent.MissionPeer.GetNetworkPeer());
        }

        public bool IsPlayerWounded(NetworkCommunicator player)
        {
            if (this.IsWounded.ContainsKey(player.VirtualPlayer?.ToPlayerId()) == false) return false;
            return this.IsWounded[player.VirtualPlayer?.ToPlayerId()];
        }

        public DateTime? GetPlayerWoundedUntil(NetworkCommunicator player)
        {
            if (this.WoundedUntil.ContainsKey(player.VirtualPlayer?.ToPlayerId()) == false) return null;
            return new DateTime(WoundedUntil[player.VirtualPlayer?.ToPlayerId()].Value);
        }
        #endregion
    }
}
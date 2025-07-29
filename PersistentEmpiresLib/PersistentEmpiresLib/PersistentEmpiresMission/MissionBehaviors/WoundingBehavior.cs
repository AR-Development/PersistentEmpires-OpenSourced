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
        private static object _lock = new object();
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

            if (affectedAgent.IsHuman 
                && affectedAgent == Mission.Current.MainAgent 
                && affectedAgent.IsPlayerControlled
                && agentState == AgentState.Killed
                && affectedAgent.MissionPeer != null)
            {
                var spawnEquipment = affectedAgent.SpawnEquipment.Clone(true);
                for (var equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                {
                    spawnEquipment[equipmentIndex] = new EquipmentElement(affectedAgent.Equipment[equipmentIndex].Item);
                }

                var equipments = new List<string>
                {
                    spawnEquipment[EquipmentIndex.Weapon0].Item?.StringId,
                    spawnEquipment[EquipmentIndex.Weapon1].Item?.StringId,
                    spawnEquipment[EquipmentIndex.Weapon2].Item?.StringId,
                    spawnEquipment[EquipmentIndex.Weapon3].Item?.StringId
                };

                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new RegisterClientEquipmentOnWound(equipments, affectedAgent.MissionPeer.GetNetworkPeer()?.VirtualPlayer?.ToPlayerId()));
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
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new UpdateWoundedPlayer(playerId, true));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
        }

        private static int _counter = 0;
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (!WoundingEnabled) return;

            if (++_counter < 10)
                return;
            // Reset counter
            _counter = 0;

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
                        if (!string.IsNullOrEmpty(peer.ControlledAgent.WieldedWeapon.Item?.StringId) && !ItemsWhichCanBeUsedByWounded.Contains(peer.ControlledAgent.WieldedWeapon.Item?.StringId) && !AgentHungerBehavior.Instance.Eatables.Any(x=> x.Item.StringId == peer.ControlledAgent.WieldedWeapon.Item?.StringId))
                        {
                            peer.ControlledAgent?.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.Instant);
                        }
                    }
                }
            }
        }

        private static List<string> trainingWeapons = new List<string>() { "PE_wooden_sword_t1", "PE_wooden_sword_t2" };
        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            if (WoundingEnabled && affectedAgent.IsHuman && affectedAgent.IsPlayerControlled
                && agentState == AgentState.Killed && affectedAgent.MissionPeer != null
                //&& affectedAgent.MissionPeer.GetNetworkPeer().QuitFromMission == false && affectedAgent.MissionPeer.GetNetworkPeer().IsConnectionActive
            )
            {
                var player = affectedAgent.MissionPeer.GetNetworkPeer();
                DeathPlace[player.VirtualPlayer?.ToPlayerId()] = affectedAgent.Position;
                var spawnEquipment = affectedAgent.SpawnEquipment.Clone(true);
                lock (_lock)
                {
                    DeathEquipment[player.VirtualPlayer?.ToPlayerId()] = new Tuple<bool, Equipment>(false, spawnEquipment);
                    // this.DeathWeaponEquipment[player] = affectedAgent.Equipment[Equip]
                    for (var equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                    {
                        DeathEquipment[player.VirtualPlayer?.ToPlayerId()].Item2[equipmentIndex] = new EquipmentElement(affectedAgent.Equipment[equipmentIndex].Item);
                    }
                }

                //if your wounded AND your not healed yet then keep old timer
                if (WoundedUntil.ContainsKey(player.VirtualPlayer?.ToPlayerId()) && WoundedUntil[player.VirtualPlayer?.ToPlayerId()].Value < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    // Recalculate stats
                    player.ControlledAgent.UpdateAgentStats();
                    return;
                }

                if (trainingWeapons.Contains(affectorAgent.WieldedWeapon.Item?.StringId))
                {
                    return;
                }

                // check if we should put player in wounded mode
                if (blow.WeaponRecordWeaponFlags != 0 || blow.OverrideKillInfo == Agent.KillInfo.Gravity)
                {
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
                    GameNetwork.WriteMessage(new UpdateWoundedPlayer(player.VirtualPlayer?.ToPlayerId(), true));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

                    InformationComponent.Instance.SendMessage(GameTexts.FindText("WoundingBehavior1", null).ToString(), Color.ConvertStringToColor("#F44336FF").ToUnsignedInteger(), player);
                }
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
            IsWounded[message.PlayerId] = message.IsWounded;
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
            lock (_lock)
            {
                if (DeathEquipment.ContainsKey(message.PlayerId))
                {
                    var playerEquipment = DeathEquipment[message.PlayerId].Item2;

                    if (!DeathEquipment[message.PlayerId].Item1)
                    {
                        DeathEquipment[message.PlayerId] = new Tuple<bool, Equipment>(true, playerEquipment);

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
                        
                        var weapon0 = new MissionWeapon(playerEquipment[EquipmentIndex.Weapon0].Item, null, null);
                        player.ControlledAgent?.EquipWeaponWithNewEntity(EquipmentIndex.Weapon0, ref weapon0);

                        var weapon1 = new MissionWeapon(playerEquipment[EquipmentIndex.Weapon1].Item, null, null);
                        player.ControlledAgent?.EquipWeaponWithNewEntity(EquipmentIndex.Weapon1, ref weapon1);

                        var weapon2 = new MissionWeapon(playerEquipment[EquipmentIndex.Weapon2].Item, null, null);
                        player.ControlledAgent?.EquipWeaponWithNewEntity(EquipmentIndex.Weapon2, ref weapon2);

                        var weapon3 = new MissionWeapon(playerEquipment[EquipmentIndex.Weapon3].Item, null, null);
                        player.ControlledAgent?.EquipWeaponWithNewEntity(EquipmentIndex.Weapon3, ref weapon3);

                        var persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
                        // Save Current equipment, 
                        if (player.ControlledAgent != null && persistentEmpireRepresentative.IsFirstAgentSpawned)
                        {
                            SaveSystemBehavior.HandleCreateOrSavePlayer(player);
                        }
                        // Update items in db
                        //SaveSystemBehavior.HandleSavePlayerEquipmentOnDeath(message.PlayerId, playerEquipment);
                    }
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

            InformationComponent.Instance.SendMessage(GameTexts.FindText("WoundingBehavior2", null).ToString(), Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), player);
            WoundedUntil.Remove(playerId);
            IsWounded[playerId] = false;
            persistentEmpireRepresentative.UnWound();
            player.ControlledAgent?.UpdateAgentStats();

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new UpdateWoundedPlayer(playerId, false));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }

        public void UpdatePlayerWoundTime(NetworkCommunicator player, long newTime)
        {
            var playerId = player.VirtualPlayer?.ToPlayerId();

            if (!WoundedUntil.ContainsKey(playerId) || WoundedUntil[playerId].Key) return;

            WoundedUntil[playerId] = new KeyValuePair<bool, long>(true, newTime);
            
            var persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();

            persistentEmpireRepresentative.SetWounded(newTime);
            SaveSystemBehavior.HandleUpdateWoundedUntil(player, newTime);
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
            if (!WoundedUntil.ContainsKey(communicator.VirtualPlayer?.ToPlayerId()) || WoundedUntil[communicator.VirtualPlayer?.ToPlayerId()].Key) return;

            var deductedTime = WoundedUntil[communicator.VirtualPlayer?.ToPlayerId()].Value - (WoundingTime * 30);
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
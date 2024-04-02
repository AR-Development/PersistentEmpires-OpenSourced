using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class WoundingBehavior : MissionNetwork
    {
        public static WoundingBehavior Instance;
        public bool WoundingEnabled = false;
        public int WoundingTime = 60;

        public Dictionary<NetworkCommunicator, long> WoundedUntil = new Dictionary<NetworkCommunicator, long>();
        public Dictionary<NetworkCommunicator, Vec3> DeathPlace = new Dictionary<NetworkCommunicator, Vec3>();
        public Dictionary<NetworkCommunicator, bool> IsWounded = new Dictionary<NetworkCommunicator, bool>();
        public Dictionary<NetworkCommunicator, Equipment> DeathEquipment = new Dictionary<NetworkCommunicator, Equipment>();
        public Dictionary<NetworkCommunicator, MissionEquipment> DeathWeaponEquipment = new Dictionary<NetworkCommunicator, MissionEquipment>();
#if SERVER
        private static List<string> ItemsWhichCanBeUsedByWounded = new List<string>();
#endif

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            if (Instance == null)
            {
                Instance = this;
            }
#if CLIENT
            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
#endif
#if SERVER
            WoundingEnabled = ConfigManager.GetBoolConfig("WoundingEnabled", false);
            WoundingTime = ConfigManager.GetIntConfig("WoundingTimeMinutes", 60);
#endif
        }

#if CLIENT
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            networkMessageHandlerRegisterer.Register<UpdateWoundedPlayer>(this.HandleUpdateWoundedPlayer);
        }
#endif

        private void HandleUpdateWoundedPlayer(UpdateWoundedPlayer message)
        {
            IsWounded[message.Player] = message.IsWounded;
        }

        public bool IsAgentWounded(Agent agent)
        {
            if (agent.MissionPeer == null) return false;
            if (agent.IsActive() == false) return false;
            return this.IsPlayerWounded(agent.MissionPeer.GetNetworkPeer());
        } 

        public bool IsPlayerWounded(NetworkCommunicator player) {
            if (this.IsWounded.ContainsKey(player) == false) return false;
            return this.IsWounded[player];
        }

#if SERVER
        public override void OnPlayerConnectedToServer(NetworkCommunicator networkPeer)
        {
            base.OnPlayerConnectedToServer(networkPeer);
            
            if (WoundingEnabled == false) return;

            foreach(NetworkCommunicator player in  IsWounded.Keys.ToList())
            {
                if(IsWounded.ContainsKey(player) && IsWounded[player])
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new UpdateWoundedPlayer(player, true));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
        }

        public void HealPlayer(NetworkCommunicator player)
        {
            if (!WoundedUntil.ContainsKey(player)) return;

            InformationComponent.Instance.SendMessage("You are no longer wounded.", Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), player);
            WoundedUntil.Remove(player);

            IsWounded[player] = false;
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new UpdateWoundedPlayer(player, false));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (!WoundingEnabled) return;

            foreach(NetworkCommunicator player in WoundedUntil.Keys.ToList())
            {
                if (!WoundedUntil.ContainsKey(player)) continue;

                if(WoundedUntil[player] < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    HealPlayer(player);
                }
                else
                {
                    if(player.ControlledAgent != null)
                    {
                        if(!ItemsWhichCanBeUsedByWounded.Contains(player.ControlledAgent.WieldedWeapon.Item?.StringId))
                        {
                            player.ControlledAgent?.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.Instant);
                        }
                    }
                }
            }
        }

        public static void AddItmemToItemsWhichCanBeUsedByWoundedList(string itemId)
        {
            ItemsWhichCanBeUsedByWounded.Add(itemId);
        }

        // This should be not allowed to make wounded man heal other man.....
        //public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        //{
        //    base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);

        //    if(IsAgentWounded(affectorAgent) && affectedAgent != null && affectedAgent != affectorAgent)
        //    {
        //        affectedAgent.Health += blow.InflictedDamage;
        //    }
        //}

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            if (affectedAgent.IsHuman && affectedAgent.IsPlayerControlled && agentState == AgentState.Killed && WoundingEnabled &&
               affectedAgent.MissionPeer != null &&
               (
                   (affectedAgent.MissionPeer.GetNetworkPeer().QuitFromMission == false && affectedAgent.MissionPeer.GetNetworkPeer().IsConnectionActive)
               )
            )
            {
                var player = affectedAgent.MissionPeer.GetNetworkPeer();
                DeathPlace[player] = affectedAgent.Position;
                var spawnEquipment = affectedAgent.SpawnEquipment.Clone(true);
                DeathEquipment[player] = spawnEquipment;
                // this.DeathWeaponEquipment[player] = affectedAgent.Equipment[Equip]
                for (var equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
                {
                    DeathEquipment[player][equipmentIndex] = new EquipmentElement(affectedAgent.Equipment[equipmentIndex].Item);
                }

                //if your wounded AND your not healed yet then keep old timer
                if (WoundedUntil.ContainsKey(player) && WoundedUntil[player] < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return;
                
                // otherwise get new heal time
                WoundedUntil[player] = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (WoundingTime * 60);
                
                IsWounded[player] = true;
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new UpdateWoundedPlayer(player, true));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

                InformationComponent.Instance.SendMessage("You are now wounded.", Color.ConvertStringToColor("#F44336FF").ToUnsignedInteger(), player);
            }
        }
#endif
    }
}
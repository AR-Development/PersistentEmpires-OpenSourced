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
    public class CombatlogBehavior : MissionLogic
    {
        public static CombatlogBehavior Instance { get; private set; }
        public Dictionary<NetworkCommunicator, long> CombatLogTimer = new Dictionary<NetworkCommunicator, long>();
        public int Duration = 5; // ConfigManager.GetInt("CombatlogDuration");

        public override void OnBehaviorInitialize() {
            CombatlogBehavior.Instance = this;
            if(GameNetwork.IsServer)
            {
                this.Duration = ConfigManager.GetIntConfig("CombatlogDuration", 5);
            }
        }
        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);
            if(affectorAgent != null && affectorAgent != affectedAgent && affectorAgent.IsActive() && affectorAgent.IsHuman && affectedAgent != null && affectedAgent.MissionPeer != null && affectedAgent.MissionPeer.GetNetworkPeer().QuitFromMission == false && affectedAgent.IsHuman && affectedAgent.IsActive() && affectorWeapon.Item != null && affectorWeapon.Item.StringId != "pe_doctorscalpel")
            {
                NetworkCommunicator player = affectedAgent.MissionPeer.GetNetworkPeer();
                this.WarnPlayer(player);
                this.CombatLogTimer[player] = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + Duration;
            }
        }
        public override void OnMissionTick(float dt)
        {
            foreach(NetworkCommunicator player in this.CombatLogTimer.Keys.ToList())
            {
                if(!this.IsPlayerInCombatState(player))
                {
                    if(player.IsConnectionActive)
                    {
                        InformationComponent.Instance.SendMessage("You can be healed now.", new Color(0f, 1f, 0f).ToUnsignedInteger(), player);
                    }
                    this.CombatLogTimer.Remove(player);
                }
            }
        }
        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
            /*if(affectedAgent != null && affectedAgent.MissionPeer != null)
            {
                if(this.CombatLogTimer.ContainsKey(affectedAgent.MissionPeer.GetNetworkPeer()))
                {
                    this.CombatLogTimer.Remove(affectedAgent.MissionPeer.GetNetworkPeer());
                }
            }*/
        }

        public void WarnPlayer(NetworkCommunicator player)
        {
            bool shouldWarn = true;
            if (this.CombatLogTimer.ContainsKey(player) && this.CombatLogTimer[player] > DateTimeOffset.UtcNow.ToUnixTimeSeconds()) shouldWarn = false;

            if(shouldWarn)
            {
                InformationComponent.Instance.SendMessage("You are in combat ! You will not be healed for " + this.Duration + " seconds", new Color(1f,0f,0f).ToUnsignedInteger(), player);
            }
        }

        public bool IsPlayerInCombatState(NetworkCommunicator player) { 
            if(this.CombatLogTimer.ContainsKey(player))
            {
                long endTime = this.CombatLogTimer[player];
                return endTime > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class PoisoningBehavior : MissionLogic
    {
        public string PoisionItemId = "pe_poison_dagger";
        public string AntidoteItemId = "pe_antidote";
        public int DamageIntervalSeconds = 10;
        public long LastCheckedAt = 0;

        public Dictionary<NetworkCommunicator, bool> Poisioned = new Dictionary<NetworkCommunicator, bool>();
        public Dictionary<Agent, int> pickedAgents = new Dictionary<Agent, int>();
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
#if SERVER

            this.PoisionItemId = ConfigManager.GetStrConfig("PoisonItemId", "pe_poison_dagger");
            this.AntidoteItemId = ConfigManager.GetStrConfig("AntidoteItemId", "pe_antidote");
#endif
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (this.LastCheckedAt + this.DamageIntervalSeconds < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                this.LastCheckedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                foreach (NetworkCommunicator player in this.Poisioned.Keys.ToList())
                {
                    if (player.ControlledAgent == null && this.Poisioned.ContainsKey(player))
                    {
                        this.Poisioned.Remove(player);
                    }
                    else
                    {
                        player.ControlledAgent.Health -= 1;
                    }
                }
            }
            foreach (Agent a in pickedAgents.Keys.ToList())
            {
                if (pickedAgents.ContainsKey(a) == false) continue;
                pickedAgents[a]--;
                if (pickedAgents[a] == 0)
                {
                    pickedAgents.Remove(a);
                    EquipmentIndex index = a.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                    if (index != EquipmentIndex.None)
                    {
                        a.RemoveEquippedWeapon(index);
                    }
                }
            }

        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
            if (affectedAgent.MissionPeer != null && affectedAgent.MissionPeer.GetNetworkPeer() != null && this.Poisioned.ContainsKey(affectedAgent.MissionPeer.GetNetworkPeer()))
            {
                this.Poisioned.Remove(affectedAgent.MissionPeer.GetNetworkPeer());
            }
        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);

            if (affectorWeapon.IsEmpty) return;
            if (affectorWeapon.Item == null) return;
            if (affectedAgent.MissionPeer == null) return;
            if (affectorAgent.MissionPeer == null) return;
            if (blow.VictimBodyPart == BoneBodyPartType.None) return;

            if (affectorWeapon.Item.StringId == this.PoisionItemId)
            {
                EquipmentIndex mainHandIndex = affectorAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                if (mainHandIndex != EquipmentIndex.None && affectorAgent.Equipment[mainHandIndex].Item.StringId == this.PoisionItemId)
                {
                    pickedAgents[affectorAgent] = 5;
                    // affectorAgent.TryToSheathWeaponInHand(Agent.HandIndex.MainHand, Agent.WeaponWieldActionType.Instant);
                    // affectorAgent.RemoveEquippedWeapon(mainHandIndex);
                }
                this.Poisioned[affectedAgent.MissionPeer.GetNetworkPeer()] = true;
            }
            else if (affectorWeapon.Item.StringId == this.AntidoteItemId && this.Poisioned.ContainsKey(affectedAgent.MissionPeer.GetNetworkPeer()))
            {
                EquipmentIndex mainHandIndex = affectorAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
                if (mainHandIndex != EquipmentIndex.None && affectorAgent.Equipment[mainHandIndex].Item.StringId == this.AntidoteItemId)
                {
                    pickedAgents[affectorAgent] = 5;
                }
                this.Poisioned.Remove(affectedAgent.MissionPeer.GetNetworkPeer());
            }
        }

    }
}

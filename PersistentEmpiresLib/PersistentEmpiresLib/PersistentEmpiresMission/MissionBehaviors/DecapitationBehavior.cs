using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class DecapitationBehavior : MissionNetwork
    {
        int dropChance = 0;
        Random random = new Random();
        Dictionary<Agent, Blow> registeredBlows = new Dictionary<Agent, Blow>();


        Dictionary<Agent, int> bodyAgentDict = new Dictionary<Agent, int>();
        Dictionary<Agent, int> headAgentDict = new Dictionary<Agent, int>();
        PlayerInventoryComponent inventoryComponent;
        MoneyPouchBehavior moneyPouchBehavior;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            inventoryComponent = base.Mission.GetMissionBehavior<PlayerInventoryComponent>();
            moneyPouchBehavior = base.Mission.GetMissionBehavior<MoneyPouchBehavior>();
#if SERVER
            dropChance = ConfigManager.GetIntConfig("DecapitationChance", 25);
#endif
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsServer)
            {
            }
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<BehadeAgentPacket>(this.HandleBehadeAgentPacketFromServer);
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
#if SERVER
            foreach (Agent headAgent in this.headAgentDict.Keys.ToList())
            {
                if (headAgent.AgentVisuals == null || headAgent.AgentVisuals.GetSkeleton() == null)
                {
                    if (this.headAgentDict.ContainsKey(headAgent))
                    {
                        this.headAgentDict.Remove(headAgent);
                    }
                    continue;
                }
                foreach (Mesh mesh in headAgent.AgentVisuals.GetSkeleton().GetAllMeshes()) // Remove head of the victim
                {
                    EquipmentElement headItem = headAgent.SpawnEquipment[EquipmentIndex.Head];
                    string headItemMeshName = "head";
                    if (headItem.IsEmpty == false)
                    {
                        headItemMeshName = headItem.Item.MultiMeshName;
                    }
                    if (
                        mesh.Name.Contains("head") == false &&
                        mesh.Name.Contains("hair") == false &&
                        mesh.Name.Contains("beard") == false &&
                        mesh.Name.Contains("eyebrow") == false &&
                        mesh.Name.Contains(headItemMeshName) == false
                       )
                    {

                        mesh.SetVisibilityMask(VisibilityMaskFlags.EditModeShadows);
                        mesh.ClearMesh();
                    }
                }
                headAgent.CreateBloodBurstAtLimb(1, 0.5f);
                if (this.headAgentDict.ContainsKey(headAgent))
                {
                    this.headAgentDict[headAgent]--;
                    if (this.headAgentDict[headAgent] == 0)
                    {
                        this.headAgentDict.Remove(headAgent);
                    }
                }

            }

            foreach (Agent bodyAgent in this.bodyAgentDict.Keys.ToList())
            {

                foreach (Mesh mesh in bodyAgent.AgentVisuals.GetSkeleton().GetAllMeshes()) // Remove head of the victim
                {
                    EquipmentElement headItem = bodyAgent.SpawnEquipment[EquipmentIndex.Head];
                    string headItemName = "head";
                    if (headItem.IsEmpty == false)
                    {
                        headItemName = headItem.Item.MultiMeshName;
                    }
                    if (
                        mesh.Name.Contains("head") ||
                        mesh.Name.Contains("hair") ||
                        mesh.Name.Contains("beard") ||
                        mesh.Name.Contains("eyebrow") ||
                        mesh.Name.Contains(headItemName)
                    )
                    {
                        mesh.SetVisibilityMask(VisibilityMaskFlags.EditModeShadows);
                        mesh.ClearMesh();
                    }
                }
                bodyAgent.CreateBloodBurstAtLimb(1, 0.5f);
                if (this.bodyAgentDict.ContainsKey(bodyAgent))
                {
                    this.bodyAgentDict[bodyAgent]--;
                    if (this.bodyAgentDict[bodyAgent] == 0)
                    {
                        this.bodyAgentDict.Remove(bodyAgent);
                    }
                }
            }
#endif
        }

        private void HandleBehadeAgentPacketFromServer(BehadeAgentPacket message)
        {
            this.bodyAgentDict[message.BodyAgent] = 100;
            this.headAgentDict[message.HeadAgent] = 100;
        }

        private Agent SpawnCopyAgent(Agent agent)
        {
            AgentBuildData agentBuildData = new AgentBuildData(agent.Character);
            agentBuildData.ClothingColor1(agent.ClothingColor1);
            agentBuildData.ClothingColor2(agent.ClothingColor2);

            Equipment equipment = new Equipment();
            for (EquipmentIndex i = EquipmentIndex.Head; i < EquipmentIndex.ArmorItemEndSlot; i++)
            {
                equipment[i] = agent.SpawnEquipment[i];
            }


            agentBuildData
                // .Index(agent.Index)
                .MissionPeer(agent.MissionPeer)
                .Equipment(equipment)
                .InitialPosition(agent.Position)
                .InitialDirection(agent.LookDirection.AsVec2)
                .Team(agent.Team)
                .IsFemale(agent.IsFemale)
                .BodyProperties(agent.BodyPropertiesValue)
                .Age((int)agent.Age);
            agentBuildData.InitialPosition(agent.Position);
            agentBuildData.InitialDirection(agent.GetMovementDirection());
            Agent copy = Mission.Current.SpawnAgent(agentBuildData);
            copy.AgentVisuals.SetVoiceDefinitionIndex(-1, 0f);
            return copy;
        }

        public void BehadeAgent(Agent affectedAgent, Blow b)
        {
            Agent body = this.SpawnCopyAgent(affectedAgent);
            body.SetActionChannel(0, ActionIndexCache.act_none, false, 0UL, 0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            // this.KillAgent(body);
            inventoryComponent.IgnoreAgentDropLoot[body] = true;
            moneyPouchBehavior.IgnoreAgentDropLoot[body] = true;
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new BehadeAgentPacket(body, affectedAgent));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            body.Die(b);
        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);
            registeredBlows[affectedAgent] = blow;
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
            if (GameNetwork.IsClient)
            {
                return;
            }

            if (
                affectorAgent == null ||
                agentState != AgentState.Killed ||
                blow.VictimBodyPart > BoneBodyPartType.Neck ||
                blow.DamageType != DamageTypes.Cut
             ) return;


            if (affectedAgent.IsAIControlled || affectedAgent.IsPlayerControlled == false) return;
            if (affectedAgent.State != AgentState.Killed) return;

            if (random.Next(100) > 100 - dropChance)
            { // FOR TESTING PURPOSES

                if (registeredBlows.ContainsKey(affectedAgent))
                {
                    this.BehadeAgent(affectedAgent, registeredBlows[affectedAgent]);
                }
            }
        }
    }
}

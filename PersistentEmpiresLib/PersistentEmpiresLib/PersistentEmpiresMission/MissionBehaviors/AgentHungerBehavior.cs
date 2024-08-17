using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Library.Debug;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public struct Food
    {
        public ItemObject Item;
        public int Hunger;
        public int RefillHealth;
        public ActionIndexCache Animation;
        public float EatDuration;
        public string EatingSound;
        public Food(string itemId, int hunger, int refillHealth, string animation, float eatDuration, string eatingSound)
        {
            this.Item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            this.Hunger = hunger;
            this.Animation = ActionIndexCache.Create(animation);
            this.EatDuration = eatDuration;
            this.EatingSound = eatingSound;
            this.RefillHealth = refillHealth;
        }
    }
    public class AgentHungerBehavior : MissionNetwork
    {
        public delegate void AgentHungerChangedDelegate(int hunger);
        public event AgentHungerChangedDelegate OnAgentHungerChanged;

        private class EatingAction
        {
            public Agent EaterAgent;
            public Food Food;
            public long EatingStartedAt;
            public long EatingEndsAt;

            public EatingAction(Agent eater, Food food)
            {
                this.EaterAgent = eater;
                this.Food = food;
                this.EatingStartedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                this.EatingEndsAt = this.EatingStartedAt + (int)(this.Food.EatDuration * 1000);
            }
        }

        public List<Food> Eatables = new List<Food>();
        Dictionary<Agent, EatingAction> AgentsEating = new Dictionary<Agent, EatingAction>();
        private int HungerInterval = 72; // ConfigManager.GetIntConfig("HungerInterval", 72); // 60 secs
        private int HungerReduceAmount = 1;// ConfigManager.GetIntConfig("HungerInterval", 1);
        private int HungerRefillHealthLowerBoundary = 25;// ConfigManager.GetIntConfig("HungerRefillHealthLowerBoundary", 25);
        private int HungerHealingAmount = 10; //  ConfigManager.GetIntConfig("HungerHealingAmount", 10);
        private int HungerHealingReduceAmount = 5; // ConfigManager.GetIntConfig("HungerHealingReduceAmount", 5);
        private float HungerStartHealingUnderHealthPct = 75 / 100; // ConfigManager.GetIntConfig("HungerStartHealingUnderHealthPct", 75) / 100;
        private long LastHungerCheckedAt = 0;

        private int StarvingInternal = 10;
        private long LastStarvingCheckedAt = 0;
        private void ReduceHungerLoop()
        {
            if (this.LastHungerCheckedAt + this.HungerInterval < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
                {
                    if (!peer.IsConnectionActive) continue;
                    if (peer.ControlledAgent == null) continue;
                    PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                    if (persistentEmpireRepresentative == null) continue;

                    if (peer.ControlledAgent.Health < (peer.ControlledAgent.Health) * HungerStartHealingUnderHealthPct)
                    {
                        persistentEmpireRepresentative.SetHunger(persistentEmpireRepresentative.GetHunger() - HungerHealingReduceAmount);
                        if (persistentEmpireRepresentative.GetHunger() > HungerRefillHealthLowerBoundary)
                        {
                            peer.ControlledAgent.Health += HungerHealingAmount;
                        }
                    }
                    else
                    {
                        persistentEmpireRepresentative.SetHunger(persistentEmpireRepresentative.GetHunger() - HungerReduceAmount); // Reduce hunger
                    }

                }
                this.LastHungerCheckedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }
        private void StarvingCheckLoop()
        {
            if (this.LastStarvingCheckedAt + this.StarvingInternal < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
                {
                    if (!peer.IsConnectionActive) continue;
                    if (peer.ControlledAgent == null) continue;
                    PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                    if (persistentEmpireRepresentative == null) continue;
                    if (persistentEmpireRepresentative.GetHunger() > 0) continue;
                    if (peer.ControlledAgent.Health <= 10) continue;
                    float reduceAmount = peer.ControlledAgent.Health - 10 > 10 ? 10 : 10 - peer.ControlledAgent.Health;
                    peer.ControlledAgent.Health -= reduceAmount;
                }
                this.LastStarvingCheckedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }
        private void EatingActionLoop()
        {
            foreach (Agent agent in this.AgentsEating.Keys.ToList())
            {
                if (!agent.IsActive())
                {
                    this.AgentsEating.Remove(agent);
                    continue;
                }
                EatingAction action = this.AgentsEating[agent];

                if (agent.GetWieldedItemIndex(Agent.HandIndex.MainHand) == EquipmentIndex.None)
                {
                    agent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                    this.AgentsEating.Remove(agent);
                    continue;
                }
                else if (agent.GetCurrentAction(0).Name == "act_none")
                {
                    agent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                    this.AgentsEating.Remove(agent);
                    continue;
                }
                else if (action.EatingEndsAt < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() && agent.GetWieldedItemIndex(Agent.HandIndex.MainHand) != EquipmentIndex.None)
                {
                    agent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                    NetworkCommunicator peer = agent.MissionPeer.GetNetworkPeer();
                    PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                    persistentEmpireRepresentative.SetHunger(persistentEmpireRepresentative.GetHunger() + action.Food.Hunger);

                    agent.Health += action.Food.RefillHealth;

                    if (agent.Health > agent.HealthLimit) agent.Health = agent.HealthLimit;
                    agent.RemoveEquippedWeapon(agent.GetWieldedItemIndex(Agent.HandIndex.MainHand));
                    this.AgentsEating.Remove(agent);
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
            if (GameNetwork.IsServer)
            {
                this.ReduceHungerLoop();
                this.StarvingCheckLoop();
                this.EatingActionLoop();
            }
        }
        public bool HasRefillHealthNode(XmlNode node)
        {
            return node.ChildNodes.Cast<XmlNode>().Any(x => x.Name == "RefillHealth");
        }
        private void LoadEatables(string modulePath)
        {
            string FoodPath = ModuleHelper.GetXmlPath(modulePath, "Food");
            Debug.Print("[PE] Trying Loading " + FoodPath, 0, DebugColor.Cyan);
            if (File.Exists(FoodPath) == false) return;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(FoodPath);
            foreach (XmlNode node in xmlDocument.SelectNodes("/Foods/Food"))
            {
                string ItemId = node["ItemId"].InnerText;
                int hunger = int.Parse(node["Hunger"].InnerText);
                string animation = node["Animation"].InnerText;
                float duration = float.Parse(node["Duration"].InnerText);
                string eatingSound = node["EatingSound"].InnerText;
                int refillHealth = 0;
                if (HasRefillHealthNode(node)) refillHealth = int.Parse(node["RefillHealth"].InnerText);
                Food food = new Food(ItemId, hunger, refillHealth, animation, duration, eatingSound);
                this.Eatables.Add(food);
            }
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            if (GameNetwork.IsServer)
            {
                this.HungerInterval = ConfigManager.GetIntConfig("HungerInterval", 72); // 60 secs
                this.HungerReduceAmount = ConfigManager.GetIntConfig("HungerReduceAmount", 1);
                this.HungerRefillHealthLowerBoundary = ConfigManager.GetIntConfig("HungerRefillHealthLowerBoundary", 25);
                this.HungerHealingAmount = ConfigManager.GetIntConfig("HungerHealingAmount", 10);
                this.HungerHealingReduceAmount = ConfigManager.GetIntConfig("HungerHealingReduceAmount", 5);
                this.HungerStartHealingUnderHealthPct = ConfigManager.GetIntConfig("HungerStartHealingUnderHealthPct", 75) / 100;
            }
            Debug.Print("[PE] LOADING EATABLES...");
            this.LoadEatables("PersistentEmpires");

            foreach (ModuleInfo module in ModuleHelper.GetModules())
            {
                if (module.Id == "PersistentEmpires") continue;
                this.LoadEatables(module.Id);
            }

        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
            this.Eatables.Clear();
        }

        private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<SetHunger>(this.HandleSetHungerFromServer);
            }
            else if (GameNetwork.IsServer)
            {
                networkMessageHandlerRegisterer.Register<RequestStartEat>(this.HandleRequestStartEatFromClient);
                networkMessageHandlerRegisterer.Register<RequestStopEat>(this.HandleRequestStopEatFromClient);
            }

        }

        public bool RequestStartEat()
        {
            Agent myAgent = GameNetwork.MyPeer.ControlledAgent;
            if (myAgent == null) return false;

            EquipmentIndex wieldedIndex = myAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            if (wieldedIndex == EquipmentIndex.None) return false;

            MissionWeapon equipment = myAgent.Equipment[wieldedIndex];
            if (equipment.IsEmpty) return false;

            Food food = this.Eatables.FirstOrDefault(f => f.Item != null && f.Item.StringId == equipment.Item.StringId);
            if (food.Item == null) return false;

            if (myAgent.HasMount) return false;

            if (food.EatingSound != "")
            {
                base.Mission.MakeSound(SoundEvent.GetEventIdFromString(food.EatingSound), myAgent.Position, false, true, -1, -1);
            }
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestStartEat());
            GameNetwork.EndModuleEventAsClient();
            myAgent.SetActionChannel(0, food.Animation, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            return true;
        }

        public void RequestStopEat()
        {
            Agent myAgent = GameNetwork.MyPeer.ControlledAgent;
            if (myAgent == null) return;

            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestStopEat());
            GameNetwork.EndModuleEventAsClient();

            myAgent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
        }
        private bool HandleRequestStartEatFromClient(NetworkCommunicator peer, RequestStartEat message)
        {
            if (peer.ControlledAgent == null) return false;
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;

            EquipmentIndex index = peer.ControlledAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            if (index == EquipmentIndex.None) return false;
            MissionWeapon equipmentElement = peer.ControlledAgent.Equipment[index];

            Food eatable = this.Eatables.FirstOrDefault(f => f.Item.Id == equipmentElement.Item.Id);
            if (eatable.Item == null) return false;
            EatingAction eatingAction = new EatingAction(peer.ControlledAgent, eatable);
            this.AgentsEating[peer.ControlledAgent] = eatingAction;
            peer.ControlledAgent.SetActionChannel(0, eatable.Animation, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);

            return true;
        }

        private bool HandleRequestStopEatFromClient(NetworkCommunicator peer, RequestStopEat message)
        {
            if (peer.ControlledAgent == null) return false;
            if (this.AgentsEating.ContainsKey(peer.ControlledAgent))
            {
                peer.ControlledAgent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            }

            return true;
        }

        private void HandleSetHungerFromServer(SetHunger message)
        {
            PersistentEmpireRepresentative myRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            myRepresentative.SetHunger(message.Hunger);

            if (this.OnAgentHungerChanged != null)
            {
                this.OnAgentHungerChanged(message.Hunger);
            }
        }
    }
}

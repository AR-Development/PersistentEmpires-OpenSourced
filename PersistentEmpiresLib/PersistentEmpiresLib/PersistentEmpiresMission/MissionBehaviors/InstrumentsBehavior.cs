using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public struct Instrument
    {
        public ItemObject Item;
        public ActionIndexCache Animation;
        public int SoundIndex;
        public Instrument(string itemId, string animation, string musicId)
        {
            this.Item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            this.Animation = ActionIndexCache.Create(animation);
            this.SoundIndex = SoundEvent.GetEventIdFromString(musicId);
        }
    }
    public class InstrumentsBehavior : MissionNetwork
    {
        private class PlayingAction
        {
            public Agent PlayerAgent;
            public Instrument Instrument;
            public long PlayingStartedAt;

            public PlayingAction(Agent player, Instrument instrument)
            {
                this.PlayerAgent = player;
                this.Instrument = instrument;
                this.PlayingStartedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        public List<Instrument> Instruments = new List<Instrument>();
        private Dictionary<Agent, PlayingAction> AgentsPlaying = new Dictionary<Agent, PlayingAction>();
        private Dictionary<Agent, SoundEvent> AgentsPlayingSound = new Dictionary<Agent, SoundEvent>();
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            foreach (ModuleInfo module in ModuleHelper.GetModules())
            {
                /*if (module.IsSelected || GameNetwork.IsServer)
                {*/
                this.LoadInstruments(module.Id);
                //}
            }
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
            this.Instruments.Clear();
        }

        private void LoadInstruments(string moduleId)
        {
            string FoodPath = ModuleHelper.GetXmlPath(moduleId, "Instruments");
            if (File.Exists(FoodPath) == false) return;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(FoodPath);
            foreach (XmlNode node in xmlDocument.SelectNodes("/Instruments/Instrument"))
            {
                string ItemId = node["ItemId"].InnerText;
                string animation = node["Animation"].InnerText;
                string musicId = node["MusicId"].InnerText;
                Instrument instrument = new Instrument(ItemId, animation, musicId);
                this.Instruments.Add(instrument);
            }
        }

        public bool RequestStartPlaying()
        {
            Agent myAgent = GameNetwork.MyPeer.ControlledAgent;
            if (myAgent == null) return false;

            EquipmentIndex wieldedIndex = myAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            if (wieldedIndex == EquipmentIndex.None) return false;

            MissionWeapon equipment = myAgent.Equipment[wieldedIndex];
            if (equipment.IsEmpty) return false;

            Instrument instrument = this.Instruments.FirstOrDefault(f => f.Item != null && f.Item.StringId == equipment.Item.StringId);
            if (instrument.Item == null) return false;

            if (myAgent.HasMount) return false;

            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestStartPlaying());
            GameNetwork.EndModuleEventAsClient();

            return true;
        }

        private static int _counter = 0;
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

#if SERVER
            if (++_counter < 10)
                return;
            // Reset counter
            _counter = 0;

            foreach (Agent key in this.AgentsPlayingSound.Keys.ToList())
            {
                if (key != null && key.IsActive())
                {
                    this.AgentsPlayingSound[key].SetPosition(key.Position);
                }
            }
#endif
        }
        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            if (GameNetwork.IsClient)
            {
                this.StopAgentPlaying(affectedAgent);
            }
        }
        public void RequestStopEat()
        {
            Agent myAgent = GameNetwork.MyPeer.ControlledAgent;
            if (myAgent == null) return;

            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestStopPlaying());
            GameNetwork.EndModuleEventAsClient();

        }

        private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<AgentPlayingInstrument>(this.HandleAgentPlayingInstrumentFromServer);
            }
            else if (GameNetwork.IsServer)
            {
                networkMessageHandlerRegisterer.Register<RequestStartPlaying>(this.HandleRequestStartPlayingFromClient);
                networkMessageHandlerRegisterer.Register<RequestStopPlaying>(this.HandleRequestStopPlayingFromClient);
            }

        }
        private void StopAgentPlaying(Agent agent)
        {
            if (this.AgentsPlayingSound.ContainsKey(agent) == false) return;
            if (this.AgentsPlayingSound[agent].IsValid && this.AgentsPlayingSound[agent].IsPlaying())
            {
                this.AgentsPlayingSound[agent].Stop();
                AnimationSystemData animationSystemData = agent.Monster.FillAnimationSystemData(MBGlobals.GetActionSet("as_human_warrior"), agent.Character.GetStepSize(), false);
                agent.SetActionSet(ref animationSystemData);
                agent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            }
            this.AgentsPlayingSound.Remove(agent);
        }
        private void PlayAgentSound(Agent agent, Instrument instrument)
        {
            SoundEvent eventRef = SoundEvent.CreateEvent(instrument.SoundIndex, base.Mission.Scene);//get a reference to sound and update parameters later.
            eventRef.SetPosition(agent.Position);
            eventRef.Play();
            this.AgentsPlayingSound[agent] = eventRef;
            AnimationSystemData animationSystemData = agent.Monster.FillAnimationSystemData(MBGlobals.GetActionSet("as_human_musician"), agent.Character.GetStepSize(), false);
            agent.SetActionSet(ref animationSystemData);
            agent.SetActionChannel(0, instrument.Animation, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
        }
        private void HandleAgentPlayingInstrumentFromServer(AgentPlayingInstrument message)
        {
            if (message.PlayerAgent == null || message.PlayerAgent.IsActive() == false) return;
            if (message.IsPlaying)
            {
                this.StopAgentPlaying(message.PlayerAgent);
                if (this.Instruments.Count > message.PlayingInstrumentIndex)
                {
                    this.PlayAgentSound(message.PlayerAgent, this.Instruments[message.PlayingInstrumentIndex]);
                }
            }
            else
            {
                this.StopAgentPlaying(message.PlayerAgent);
            }
        }

        private bool HandleRequestStopPlayingFromClient(NetworkCommunicator peer, RequestStopPlaying message)
        {
            if (peer.ControlledAgent == null) return false;
            if (this.AgentsPlaying.ContainsKey(peer.ControlledAgent))
            {
                // peer.ControlledAgent.SetActionChannel(0, ActionIndexCache.act_none, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new AgentPlayingInstrument(peer.ControlledAgent, 0, false));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                // peer.ControlledAgent.ClearTargetFrame();
                this.AgentsPlaying.Remove(peer.ControlledAgent);
            }
            return true;
        }
        private bool HandleRequestStartPlayingFromClient(NetworkCommunicator peer, RequestStartPlaying message)
        {
            if (peer.ControlledAgent == null) return false;
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;

            EquipmentIndex index = peer.ControlledAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            if (index == EquipmentIndex.None) return false;
            MissionWeapon equipmentElement = peer.ControlledAgent.Equipment[index];

            var instrumentWithIndex = this.Instruments.Select((instr, instrIndex) => new { Instrument = instr, Index = instrIndex }).FirstOrDefault(f => f.Instrument.Item.Id == equipmentElement.Item.Id);
            if (instrumentWithIndex.Instrument.Item == null) return false;
            PlayingAction playingAction = new PlayingAction(peer.ControlledAgent, instrumentWithIndex.Instrument);
            this.AgentsPlaying[peer.ControlledAgent] = playingAction;
            // peer.ControlledAgent.SetActionChannel(0, instrumentWithIndex.Instrument.Animation, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new AgentPlayingInstrument(peer.ControlledAgent, instrumentWithIndex.Index, true));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            // peer.ControlledAgent.SetTargetPosition(peer.ControlledAgent.Position.AsVec2);
            return true;
        }
    }
}

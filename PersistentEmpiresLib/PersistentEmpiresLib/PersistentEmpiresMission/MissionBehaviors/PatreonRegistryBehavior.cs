using PersistentEmpiresLib.NetworkMessages.Server;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public struct PatreonData
    {
        public string Title;
        public Color Color;
        public PatreonData(string Title, Color color)
        {
            this.Title = Title;
            this.Color = color;
        }
        public PatreonData(string Title, uint color)
        {
            this.Title = Title;
            this.Color = Color.FromUint(color);
        }
    }
    public class PatreonRegistryBehavior : MissionNetwork
    {
        public Dictionary<NetworkCommunicator, PatreonData> PatreonRegistry;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            this.PatreonRegistry = new Dictionary<NetworkCommunicator, PatreonData>();

        }

        protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator player)
        {
            if (GameNetwork.IsClient)
            {
                return;
            }


            foreach (NetworkCommunicator peer in this.PatreonRegistry.Keys.ToList())
            {
                if (peer.IsConnectionActive == false) continue;
                PatreonData data = this.PatreonRegistry[peer];

                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new PatreonRegister(peer, data.Title, data.Color.ToUnsignedInteger()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }

        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<PatreonRegister>(this.HandlePatreonRegisterFromServer);
            }
        }
        public bool IsPlayerPatreon(NetworkCommunicator player)
        {
            return this.PatreonRegistry.ContainsKey(player);
        }
        private void HandlePatreonRegisterFromServer(PatreonRegister message)
        {
            this.PatreonRegistry[message.Player] = new PatreonData(message.Tier, message.Color);
        }
    }
}

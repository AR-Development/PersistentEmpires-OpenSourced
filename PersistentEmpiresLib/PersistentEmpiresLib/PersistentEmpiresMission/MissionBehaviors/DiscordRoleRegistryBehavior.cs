using NetworkMessages.Server;
using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresMission.MissionBehaviors
{ 
    public struct DiscordData
    {
        public string Title;
        public Color Color;
        public DiscordData(string Title, Color color)
        {
            this.Title = Title;
            this.Color = color;
        }
        public DiscordData(string Title, uint color)
        {
            this.Title = Title;
            this.Color = Color.FromUint(color);
        }
    }
    public class DiscordRoleRegistryBehavior : MissionNetwork
    {
        public Dictionary<NetworkCommunicator, DiscordData> DiscordRegistry;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            this.DiscordRegistry = new Dictionary<NetworkCommunicator, DiscordData>();

        }

        protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator player)
        {
            if (GameNetwork.IsClient)
            {
                return;
            }


            foreach (NetworkCommunicator peer in this.DiscordRegistry.Keys.ToList())
            {
                if (peer.IsConnectionActive == false) continue;
                DiscordData data = this.DiscordRegistry[peer];

                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new DiscordRoleRegister(peer, data.Title, data.Color.ToUnsignedInteger()));
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
                networkMessageHandlerRegisterer.Register<DiscordRoleRegister>(this.HandleDiscordRegisterFromServer);
            }
        }
        public bool IsPlayerDiscord(NetworkCommunicator player)
        {
            return this.DiscordRegistry.ContainsKey(player);
        }
        private void HandleDiscordRegisterFromServer(DiscordRoleRegister message)
        {
            this.DiscordRegistry[message.Player] = new DiscordData(message.Role, message.Color);
        }
    }
} 

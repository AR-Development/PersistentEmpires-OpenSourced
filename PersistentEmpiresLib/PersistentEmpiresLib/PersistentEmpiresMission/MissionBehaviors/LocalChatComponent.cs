using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class LocalChatComponent : MissionNetwork
    {
#if SERVER
        public delegate bool PrefixHandleLocalChatFromClient(NetworkCommunicator Sender, String Message, bool shout);
        public event PrefixHandleLocalChatFromClient OnPrefixHandleLocalChatFromClient;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            networkMessageHandlerRegisterer.Register<LocalMessage>(HandleLocalMessageFromClient);
            networkMessageHandlerRegisterer.Register<ShoutMessage>(HandleShoutMessageFromClient);
            networkMessageHandlerRegisterer.Register<PlayerIsTypingMessage>(HandlePlayerIsTypingMessageFromClient);
        }

        private bool HandleLocalMessageFromClient(NetworkCommunicator player, LocalMessage message)
        {

            if (player.ControlledAgent == null) return false;
            if (this.OnPrefixHandleLocalChatFromClient != null)
            {
                if (!this.OnPrefixHandleLocalChatFromClient(player, message.Text, false)) return true;
            }
            Vec3 position = player.ControlledAgent.Position;
            List<AffectedPlayer> affectedPlayers = new List<AffectedPlayer>();
            foreach (NetworkCommunicator otherPlayer in GameNetwork.NetworkPeers)
            {
                if (otherPlayer.ControlledAgent == null) continue;
                Vec3 otherPlayerPosition = otherPlayer.ControlledAgent.Position;
                float d = position.Distance(otherPlayerPosition);
                if (d < 30)
                {
                    // InformationComponent.Instance.SendMessage("(LOCAL)[" + player.UserName + "] " + message.Text, new Color(0.96f, 0.64f, 0.0078f).ToUnsignedInteger(), otherPlayer);
                    GameNetwork.BeginModuleEventAsServer(otherPlayer);
                    GameNetwork.WriteMessage(new LocalMessageServer(message.Text, player));
                    GameNetwork.EndModuleEventAsServer();
                    if (otherPlayer != player)
                    {
                        affectedPlayers.Add(new AffectedPlayer(otherPlayer));
                    }
                }
            }
            LoggerHelper.LogAnAction(player, LogAction.LocalChat, affectedPlayers.ToArray(), new object[] {
                message.Text
            });
            return true;
        }

        private bool HandlePlayerIsTypingMessageFromClient(NetworkCommunicator player, PlayerIsTypingMessage message)
        {
            if (player.ControlledAgent == null) return false;
            
            Vec3 position = player.ControlledAgent.Position;
            
            foreach (NetworkCommunicator otherPlayer in GameNetwork.NetworkPeers)
            {
                if (otherPlayer.ControlledAgent == null) continue;
            
                Vec3 otherPlayerPosition = otherPlayer.ControlledAgent.Position;
                float d = position.Distance(otherPlayerPosition);
                
                if (d < 30)
                {
                    GameNetwork.BeginModuleEventAsServer(otherPlayer);
                    GameNetwork.WriteMessage(new PlayerIsTypingMessageServer(player));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
            return true;
        }
        
        private bool HandleShoutMessageFromClient(NetworkCommunicator player, ShoutMessage message)
        {
            if (player.ControlledAgent == null) return false;
            if (this.OnPrefixHandleLocalChatFromClient != null)
            {
                if (!this.OnPrefixHandleLocalChatFromClient(player, message.Text, true)) return true;
            }
            Vec3 position = player.ControlledAgent.Position;

            List<AffectedPlayer> affectedPlayers = new List<AffectedPlayer>();

            foreach (NetworkCommunicator otherPlayer in GameNetwork.NetworkPeers)
            {
                if (otherPlayer.ControlledAgent == null) continue;
                Vec3 otherPlayerPosition = otherPlayer.ControlledAgent.Position;
                float d = position.Distance(otherPlayerPosition);
                if (d < 50)
                {
                    // InformationComponent.Instance.SendMessage("(SHOUT)[" + player.UserName + "] " + message.Text, new Color(0.69f, 0.43f, 0f).ToUnsignedInteger(), otherPlayer);
                    GameNetwork.BeginModuleEventAsServer(otherPlayer);
                    GameNetwork.WriteMessage(new ShoutMessageServer(message.Text, player));
                    GameNetwork.EndModuleEventAsServer();
                    if (otherPlayer != player)
                    {
                        affectedPlayers.Add(new AffectedPlayer(otherPlayer));
                    }
                }
            }
            LoggerHelper.LogAnAction(player, LogAction.LocalChat, affectedPlayers.ToArray(), new object[] {
                message.Text
            });
            return true;
        }
#endif
#if CLIENT
        public delegate void OnPlayerIsTypingMessageDelegate(NetworkCommunicator sender);
        public delegate void LocalChatMessageDelegate(NetworkCommunicator Sender, String Message, bool shout);
        public delegate void CustomBubbleMessageDelegate(NetworkCommunicator Sender, String Message, bool shout);
        public delegate void CustomBubbleMessageDelegate2(NetworkCommunicator Sender, String Message, string color);

        public event OnPlayerIsTypingMessageDelegate OnPlayerIsTypingMessage;
        public event LocalChatMessageDelegate OnLocalChatMessage;
        public event CustomBubbleMessageDelegate OnCustomBubbleMessage;
        public event CustomBubbleMessageDelegate2 OnCustomBubbleMessage2;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            networkMessageHandlerRegisterer.Register<LocalMessageServer>(this.HandleLocalMessageFromServer);
            networkMessageHandlerRegisterer.Register<ShoutMessageServer>(this.HandleShoutMessageFromServer);
            networkMessageHandlerRegisterer.Register<CustomBubbleMessage>(this.HandleCustomBubbleMessageFromServer);
            networkMessageHandlerRegisterer.Register<PlayerIsTypingMessageServer>(HandlePlayerIsTypingMessageServer);
        }

        private void HandleCustomBubbleMessageFromServer(CustomBubbleMessage message)
        {
            InformationManager.DisplayMessage(new InformationMessage(message.Message, Color.ConvertStringToColor("#ab47bcFF")));
            if (this.OnCustomBubbleMessage != null)
            {
                this.OnCustomBubbleMessage(message.Sender, message.Message, true);
            }
        }

        private void HandleShoutMessageFromServer(ShoutMessageServer message)
        {
            InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("LocalChatComponent1", null).ToString() + message.Sender.UserName + ": " + message.Message, Color.ConvertStringToColor("#AFAFAFFF")));
            if (this.OnLocalChatMessage != null)
            {
                this.OnLocalChatMessage(message.Sender, message.Message, true);
            }
        }

        private void HandleLocalMessageFromServer(LocalMessageServer message)
        {
            InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("LocalChatComponent2", null).ToString() + message.Sender.UserName + ": " + message.Message, Color.ConvertStringToColor("#DADADAFF")));
            if (this.OnLocalChatMessage != null)
            {
                this.OnLocalChatMessage(message.Sender, message.Message, false);
            }
        }

        private void HandlePlayerIsTypingMessageServer(PlayerIsTypingMessageServer message)
        {
            if (OnPlayerIsTypingMessage!= null)
            {
                OnPlayerIsTypingMessage(message.Sender);
            }
        }
#endif        
    }
}
/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class LocalChatComponent : MissionNetwork
    {
        public delegate void LocalChatMessageDelegate(NetworkCommunicator Sender, String Message, bool shout);
        public delegate void CustomBubbleMessageDelegate(NetworkCommunicator Sender, String Message, bool shout);

        public delegate bool PrefixHandleLocalChatFromClient(NetworkCommunicator Sender, String Message, bool shout);

        public event LocalChatMessageDelegate OnLocalChatMessage;
        public event CustomBubbleMessageDelegate OnCustomBubbleMessage;
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
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<LocalMessageServer>(this.HandleLocalMessageFromServer);
                networkMessageHandlerRegisterer.Register<ShoutMessageServer>(this.HandleShoutMessageFromServer);
                networkMessageHandlerRegisterer.Register<CustomBubbleMessage>(this.HandleCustomBubbleMessageFromServer);
            }
            if (GameNetwork.IsServer)
            {
                networkMessageHandlerRegisterer.Register<LocalMessage>(this.HandleLocalMessageFromClient);
                networkMessageHandlerRegisterer.Register<ShoutMessage>(this.HandleShoutMessageFromClient);
            }
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
            InformationManager.DisplayMessage(new InformationMessage("[SHOUT] " + message.Sender.UserName + ": " + message.Message, Color.ConvertStringToColor("#AFAFAFFF")));
            if (this.OnLocalChatMessage != null)
            {
                this.OnLocalChatMessage(message.Sender, message.Message, true);
            }
        }

        private void HandleLocalMessageFromServer(LocalMessageServer message)
        {
            InformationManager.DisplayMessage(new InformationMessage("[LOCAL] " + message.Sender.UserName + ": " + message.Message, Color.ConvertStringToColor("#DADADAFF")));
            if (this.OnLocalChatMessage != null)
            {
                this.OnLocalChatMessage(message.Sender, message.Message, false);
            }
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
    }
}

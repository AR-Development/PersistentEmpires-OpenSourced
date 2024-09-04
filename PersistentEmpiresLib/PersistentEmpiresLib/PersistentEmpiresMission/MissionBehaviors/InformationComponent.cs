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

using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class InformationComponent : MissionNetwork
    {
        public static InformationComponent Instance;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            InformationComponent.Instance = this;
        }

        public void BroadcastQuickInformation(String text)
        {
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new QuickInformation(text));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }
        public void SendQuickInformationToPlayer(String text, NetworkCommunicator player)
        {
            GameNetwork.BeginModuleEventAsServer(player);
            GameNetwork.WriteMessage(new QuickInformation(text));
            GameNetwork.EndModuleEventAsServer();
        }
        public void SendAnnouncementToPlayer(String text, NetworkCommunicator player)
        {
            GameNetwork.BeginModuleEventAsServer(player);
            GameNetwork.WriteMessage(new Announcement(text));
            GameNetwork.EndModuleEventAsServer();
        }
        public void SendMessage(String text, uint color, NetworkCommunicator player)
        {
            GameNetwork.BeginModuleEventAsServer(player);
            GameNetwork.WriteMessage(new PEInformationMessage(text, color));
            GameNetwork.EndModuleEventAsServer();
        }
        public void BroadcastMessage(String text, uint color)
        {
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new PEInformationMessage(text, color));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }
        public void BroadcastAnnouncement(String text)
        {
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new Announcement(text));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }

        public void HandleOnAnnouncementFromServer(Announcement announcement)
        {
            InformationManager.AddSystemNotification(announcement.Message);
        }

        public void HandleOnQuickInformationFromServer(QuickInformation quickInfo)
        {
            MBInformationManager.AddQuickInformation(new TaleWorlds.Localization.TextObject(quickInfo.Message));
        }

        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<QuickInformation>(this.HandleOnQuickInformationFromServer);
                networkMessageHandlerRegisterer.Register<Announcement>(this.HandleOnAnnouncementFromServer);
                networkMessageHandlerRegisterer.Register<PEInformationMessage>(this.HandleInformationMessageFromServer);
            }
        }

        private void HandleInformationMessageFromServer(PEInformationMessage message)
        {
            InformationManager.DisplayMessage(new InformationMessage(message.Message, Color.FromUint(message.Color)));
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }
    }
}

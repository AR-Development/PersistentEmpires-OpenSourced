using PersistentEmpiresLib.NetworkMessages.Server;
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
    public class InformationComponent : MissionNetwork
    {
        public static InformationComponent Instance;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            InformationComponent.Instance = this;
        }

        public void BroadcastQuickInformation(String text) {
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
            GameNetwork.WriteMessage(new PEInformationMessage(text,color));
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

        public void HandleOnAnnouncementFromServer(Announcement announcement) {
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

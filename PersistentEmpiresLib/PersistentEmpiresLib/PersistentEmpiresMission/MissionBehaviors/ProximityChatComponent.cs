using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class ProximityChatComponent : MissionNetwork
    {

        public delegate void HandleVoicePlayMessage(SendBatchVoiceToPlay message);
        public event HandleVoicePlayMessage OnVoicePlayMessage;

        public delegate void HandleVoicePlayerJoined(NetworkCommunicator player);
        public event HandleVoicePlayerJoined OnVoicePlayerJoined;

        public delegate void HandleVoicePlayerLeaved(NetworkCommunicator player);
        public event HandleVoicePlayerLeaved OnVoicePlayerLeaved;

        public delegate void HandleOptionsClicked();
        public event HandleOptionsClicked OnOptionsClicked;

        public Dictionary<NetworkCommunicator, bool> GlobalMuted;

        protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
        {
            if (GameNetwork.IsClient)
            {
                registerer.Register<SendBatchVoiceToPlay>(this.HandleServerEventSendVoiceToPlay);
                return;
            }
            if (GameNetwork.IsServer)
            {
                registerer.Register<SendBatchVoice>(this.HandleClientEventSendVoiceRecord);
            }
        }

        // Token: 0x060025D6 RID: 9686 RVA: 0x0008CF30 File Offset: 0x0008B130
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            GlobalMuted = new Dictionary<NetworkCommunicator, bool>();
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
        }
        public void HandleOption()
        {
            if (this.OnOptionsClicked != null) this.OnOptionsClicked();
        }


        public void SendEncodedVoiceToServer(byte[][] data, int[] length)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new SendBatchVoice(data, length));
            GameNetwork.EndModuleEventAsClient();
        }
        // Token: 0x060025DA RID: 9690 RVA: 0x0008D084 File Offset: 0x0008B284
        private bool HandleClientEventSendVoiceRecord(NetworkCommunicator peer, SendBatchVoice message)
        {
            MissionPeer component = peer.GetComponent<MissionPeer>();
            Agent speakerAgent = peer.ControlledAgent;
            if (this.GlobalMuted.ContainsKey(peer)) return true;
            if (speakerAgent == null || speakerAgent.IsActive() == false) return true;
            if (message.BufferLens[0] > 0 && component != null && component.Team != null)
            {
                foreach (NetworkCommunicator networkCommunicator in GameNetwork.NetworkPeers)
                {
                    if (networkCommunicator == peer) continue;
                    MissionPeer component2 = networkCommunicator.GetComponent<MissionPeer>();
                    if (component2 == null) continue;
                    Agent hearingAgent = component2.ControlledAgent;

                    if (networkCommunicator.IsSynchronized && hearingAgent != null && hearingAgent.IsActive() && hearingAgent.Position.Distance(speakerAgent.Position) <= 31f)
                    {
                        GameNetwork.BeginModuleEventAsServerUnreliable(component2.Peer);
                        GameNetwork.WriteMessage(new SendBatchVoiceToPlay(peer, message.PackedBuffer, message.BufferLens));
                        GameNetwork.EndModuleEventAsServerUnreliable();
                    }
                }
            }
            return true;
        }

        // Token: 0x060025DB RID: 9691 RVA: 0x0008D150 File Offset: 0x0008B350
        private void HandleServerEventSendVoiceToPlay(SendBatchVoiceToPlay message)
        {
            if (this.OnVoicePlayMessage != null)
            {
                this.OnVoicePlayMessage(message);
            }
        }
        public override void OnPlayerConnectedToServer(NetworkCommunicator networkPeer)
        {
            base.OnPlayerConnectedToServer(networkPeer);
            if (this.OnVoicePlayerJoined != null) this.OnVoicePlayerJoined(networkPeer);
        }
        public override void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
        {
            base.OnPlayerDisconnectedFromServer(networkPeer);

            if (this.OnVoicePlayerLeaved != null) this.OnVoicePlayerLeaved(networkPeer);
        }
    }
}

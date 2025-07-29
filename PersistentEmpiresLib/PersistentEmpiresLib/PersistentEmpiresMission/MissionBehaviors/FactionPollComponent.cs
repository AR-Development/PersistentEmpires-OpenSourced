using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class FactionPollComponent : MissionNetwork
    {
        private InformationComponent _informationComponent;
        private FactionsBehavior _factionsBehavior;

        private int LordPollRequiredGold = 1000; /// ConfigManager.GetIntConfig("LordPollRequiredGold", 1000);
        private int LordPollTimeOut = 60; /// ConfigManager.GetIntConfig("LordPollTimeOut", 60);
        internal static bool LordPollEnabled = true;

        public class FactionPoll
        {
            private List<NetworkCommunicator> _participantsToVote;
            public Action<FactionPoll> OnClosedOnServer;
            public Action<FactionPoll> OnCancelledOnServer;

            public Type PollType { get; }
            public bool IsOpen { get; private set; }
            public NetworkCommunicator TargetPlayer { get; private set; }
            private int OpenTime { get; set; }
            public int CloseTime { get; set; }
            public int AcceptedCount { get; set; }
            public int RejectedCount { get; set; }
            public Faction Faction { get; private set; }
            public int FactionIndex { get; }

            public enum Type
            {
                Lord
            }
            public List<NetworkCommunicator> ParticipantsToVote
            {
                get
                {
                    return this._participantsToVote;
                }
            }


            public FactionPoll(Type pollType, int factionIndex, Faction f, NetworkCommunicator targetPlayer)
            {
                this._participantsToVote = new List<NetworkCommunicator>();
                this.PollType = pollType;
                this.OpenTime = Environment.TickCount;
                this.CloseTime = 0;
                this.AcceptedCount = 0;
                this.RejectedCount = 0;
                this.IsOpen = true;
                this.TargetPlayer = targetPlayer;
                this.Faction = f;
                this.FactionIndex = factionIndex;
                this._participantsToVote = f.members.GetRange(0, f.members.Count);

            }

            public virtual bool IsCancelled()
            {
                return false;
            }
            public void Tick()
            {
                if (GameNetwork.IsServer)
                {
                    for (int i = this._participantsToVote.Count - 1; i >= 0; i--)
                    {
                        if (!this._participantsToVote[i].IsConnectionActive)
                        {
                            this._participantsToVote.RemoveAt(i);
                        }
                    }
                    if (this.IsCancelled())
                    {
                        Action<FactionPoll> onCancelledOnServer = this.OnCancelledOnServer;
                        if (onCancelledOnServer == null)
                        {
                            return;
                        }
                        onCancelledOnServer(this);
                        return;
                    }
                    else if (this.OpenTime < Environment.TickCount - 30000 || this.ResultsFinalized())
                    {
                        Action<FactionPoll> onClosedOnServer = this.OnClosedOnServer;
                        if (onClosedOnServer == null)
                        {
                            return;
                        }
                        onClosedOnServer(this);
                    }
                }
            }
            private bool ResultsFinalized()
            {
                return this.GotEnoughAcceptVotesToEnd() || this.GotEnoughRejectVotesToEnd() || this._participantsToVote.Count == 0;
            }
            public bool GotEnoughAcceptVotesToEnd()
            {
                bool result;
                result = this.AcceptedByMajority();
                return result;
            }
            public bool GotEnoughRejectVotesToEnd()
            {
                bool result;
                result = this.RejectedByMajority();
                return result;
            }
            private bool AcceptedByAllParticipants()
            {
                return this.AcceptedCount == this.GetPollParticipantCount();
            }
            private bool AcceptedByMajority()
            {
                return (float)this.AcceptedCount / (float)this.GetPollParticipantCount() > 0.50001f;
            }
            private bool RejectedByAtLeastOneParticipant()
            {
                return this.RejectedCount > 0;
            }

            private bool RejectedByMajority()
            {
                return (float)this.RejectedCount / (float)this.GetPollParticipantCount() > 0.50001f;
            }
            private int GetPollParticipantCount()
            {
                return this._participantsToVote.Count + this.AcceptedCount + this.RejectedCount;
            }
            public virtual List<NetworkCommunicator> GetPollProgressReceivers()
            {
                return this.Faction.members;
            }
            public void Close()
            {
                this.CloseTime = Environment.TickCount;
                this.IsOpen = false;
            }

            // Token: 0x06003BE0 RID: 15328 RVA: 0x000EC093 File Offset: 0x000EA293
            public void Cancel()
            {
                this.Close();
            }
            public bool ApplyVote(NetworkCommunicator peer, bool accepted)
            {
                bool result = false;
                if (this._participantsToVote.Contains(peer))
                {
                    if (accepted)
                    {
                        this.AcceptedCount++;
                    }
                    else
                    {
                        this.RejectedCount++;
                    }
                    this._participantsToVote.Remove(peer);
                    result = true;
                }
                return result;
            }
        }


        private Dictionary<int, FactionPoll> _ongoingPolls;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            _ongoingPolls = new Dictionary<int, FactionPoll>();
            _informationComponent = base.Mission.GetMissionBehavior<InformationComponent>();
            _factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
#if SERVER

            LordPollRequiredGold = ConfigManager.GetIntConfig("LordPollRequiredGold", 1000);
            LordPollTimeOut = ConfigManager.GetIntConfig("LordPollTimeOut", 60);
            LordPollEnabled = ConfigManager.GetBoolConfig("LordPollEnabled", true);
#endif
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        public delegate void OnLordPollDelegate(MissionPeer pollStarter, MissionPeer lordCandidate);
        public event OnLordPollDelegate OnStartLordPoll;

        public delegate void OnClosedOnServerDelegate(FactionPoll factionPoll);
        public event OnClosedOnServerDelegate OnClosedOnServer;

        public delegate void OnCancelledOnServerDelegate(FactionPoll factionPoll);
        public event OnCancelledOnServerDelegate OnCancelledOnServer;

        public delegate void OnPollClosedDelegate(bool result, NetworkCommunicator subject);
        public event OnPollClosedDelegate OnPollClosed;

        public delegate void OnPollCancelledDelegate();
        public event OnPollCancelledDelegate OnPollCancelled;

        public delegate void OnPollUpdateDelegate(int accepted, int rejected);
        public event OnPollUpdateDelegate OnPollUpdate;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            
            if (this._ongoingPolls == null) return;

            foreach (FactionPoll ongoingPolls in this._ongoingPolls.Values.ToList())
            {
                if (ongoingPolls.IsOpen)
                {
                    ongoingPolls.Tick();
                }
            }
        }
        public void OpenLordPollServer(NetworkCommunicator pollCreatorPeer, NetworkCommunicator targetPeer)
        {
            if(!LordPollEnabled)
            {
                _informationComponent.SendAnnouncementToPlayer(GameTexts.FindText("FactionPollComponent1", null).ToString(), pollCreatorPeer, Colors.Red.ToUnsignedInteger());
                return;
            }

            if (pollCreatorPeer == null)
            {
                // Reject return
                return;
            }
            if (targetPeer == null)
            {
                this._informationComponent.SendAnnouncementToPlayer(GameTexts.FindText("FactionPollComponent2", null).ToString(), pollCreatorPeer, Colors.Red.ToUnsignedInteger());
                return;
            }
            if (!pollCreatorPeer.IsConnectionActive)
            {
                // Reject return
                return;
            }
            if (!targetPeer.IsConnectionActive)
            {
                this._informationComponent.SendAnnouncementToPlayer(GameTexts.FindText("FactionPollComponent3", null).ToString(), pollCreatorPeer, Colors.Red.ToUnsignedInteger());
                return;
            }
            MissionPeer creatorMPeer = pollCreatorPeer.GetComponent<MissionPeer>();
            MissionPeer targetMPeer = pollCreatorPeer.GetComponent<MissionPeer>();
            if (creatorMPeer == null || targetMPeer == null) return;
            if (targetPeer.IsSynchronized == false || pollCreatorPeer.IsSynchronized == false) return;
            PersistentEmpireRepresentative targetRepresentative = targetPeer.GetComponent<PersistentEmpireRepresentative>();
            PersistentEmpireRepresentative pollCreatorRepresentative = pollCreatorPeer.GetComponent<PersistentEmpireRepresentative>();
            if (targetRepresentative == null || pollCreatorRepresentative == null) return;

            Faction f = targetRepresentative.GetFaction();
            if (pollCreatorPeer.Index == targetPeer.Index && f.lordId != targetPeer.VirtualPlayer.ToPlayerId() && targetRepresentative.IsAdmin == false)
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("FactionPollComponent4", null).ToString(), Color.ConvertStringToColor("#d32f2fff").ToUnsignedInteger(), pollCreatorPeer);
                return;
            }
            if (targetRepresentative.GetFactionIndex() <= 1 || pollCreatorRepresentative.GetFactionIndex() <= 1)
            {
                this._informationComponent.SendAnnouncementToPlayer(GameTexts.FindText("FactionPollComponent5", null).ToString(), pollCreatorPeer, Colors.Red.ToUnsignedInteger());
                return;
            }
            if (targetRepresentative.GetFactionIndex() != pollCreatorRepresentative.GetFactionIndex())
            {
                this._informationComponent.SendAnnouncementToPlayer(GameTexts.FindText("FactionPollComponent6", null).ToString(), pollCreatorPeer, Colors.Red.ToUnsignedInteger());
                return;
            }

            if (f.pollUnlockedAt > DateTimeOffset.UtcNow.ToUnixTimeSeconds() && f.lordId != targetPeer.VirtualPlayer.ToPlayerId())
            {
                this._informationComponent.SendMessage(GameTexts.FindText("FactionPollComponent7", null).ToString(), Color.ConvertStringToColor("#FF0000FF").ToUnsignedInteger(), pollCreatorPeer);
                return;
            }
            if (f.lordId == targetPeer.VirtualPlayer.ToPlayerId() && f.pollUnlockedAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds() > 86400)//24 * 60 * 60)
            {
                this._informationComponent.SendMessage(GameTexts.FindText("FactionPollComponent8", null).ToString(), Color.ConvertStringToColor("#FF0000FF").ToUnsignedInteger(), pollCreatorPeer);
                return;
            }
            if (!pollCreatorRepresentative.ReduceIfHaveEnoughGold(LordPollRequiredGold))
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("FactionPollComponent9", null).ToString() + LordPollRequiredGold.ToString() + GameTexts.FindText("FactionPollComponent10", null).ToString(), (new Color(1f, 0, 0)).ToUnsignedInteger(), pollCreatorPeer);
                return;
            }
            if (this._ongoingPolls.ContainsKey(targetRepresentative.GetFactionIndex()))
            {
                if (this._ongoingPolls[targetRepresentative.GetFactionIndex()].IsOpen)
                {
                    this._informationComponent.SendAnnouncementToPlayer(GameTexts.FindText("FactionPollComponent11", null).ToString(), pollCreatorPeer, Colors.Red.ToUnsignedInteger());
                    return;
                }
                if (Environment.TickCount - this._ongoingPolls[targetRepresentative.GetFactionIndex()].CloseTime < LordPollTimeOut * 1000)
                {
                    this._informationComponent.SendAnnouncementToPlayer(GameTexts.FindText("FactionPollComponent12", null).ToString(), pollCreatorPeer, Colors.Red.ToUnsignedInteger());
                    return;
                }
            }
            this.OpenLordPoll(targetPeer, pollCreatorPeer);
            foreach (NetworkCommunicator player in this._ongoingPolls[targetRepresentative.GetFactionIndex()].ParticipantsToVote)
            {
                GameNetwork.BeginModuleEventAsServer(player);
                GameNetwork.WriteMessage(new FactionLordPollOpened(pollCreatorPeer, targetPeer));
                GameNetwork.EndModuleEventAsServer();
            }
        }
        public void RequestLordPlayerPoll(NetworkCommunicator peer)
        {
            if (GameNetwork.IsServer)
            {
                if (GameNetwork.MyPeer != null)
                {
                    this.OpenLordPollServer(GameNetwork.MyPeer, peer);
                    return;
                }
            }
            else
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new FactionLordPollRequest(peer));
                GameNetwork.EndModuleEventAsClient();
            }
        }

        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer registerer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                registerer.Register<FactionPollProgress>(this.HandleServerEventUpdatePollProgress);
                registerer.Register<FactionLordPollOpened>(this.HandleServerFactionLordPollOpened);
                registerer.Register<FactionPollCancelled>(this.HandleServerEventPollCancelled);
                registerer.Register<FactionLordPollClosed>(this.HandleServerEventLordPollClosed);
                return;
            }
            if (GameNetwork.IsServer)
            {
                registerer.Register<FactionPollResponse>(this.HandleClientFactionPollResponse);
                registerer.Register<FactionLordPollRequest>(this.HandleClientFactionLordPollRequested);
            }
        }

        private void HandleServerEventLordPollClosed(FactionLordPollClosed message)
        {
            this.CloseLordPoll(message.Accepted, message.TargetPlayer, message.FactionIndex);
        }

        protected void HandleServerEventPollCancelled(FactionPollCancelled factionPollCancelled)
        {
            this.CancelPoll(factionPollCancelled.FactionIndex);
        }
        protected void HandleServerFactionLordPollOpened(FactionLordPollOpened factionLordPollOpened)
        {
            this.OpenLordPoll(factionLordPollOpened.LordCandidate, factionLordPollOpened.PollCreator);
        }
        protected void HandleServerEventUpdatePollProgress(FactionPollProgress pollProgress)
        {
            this.UpdatePollProgress(pollProgress.VotesAccepted, pollProgress.VotesRejected);
        }
        protected bool HandleClientFactionLordPollRequested(NetworkCommunicator sender, FactionLordPollRequest factionLordPollRequested)
        {
            this.OpenLordPollServer(sender, factionLordPollRequested.Player);
            return true;
        }

        protected bool HandleClientFactionPollResponse(NetworkCommunicator sender, FactionPollResponse factionPollResponse)
        {
            this.ApplyVote(sender, factionPollResponse.Accepted);
            return true;
        }
        private void ApplyVote(NetworkCommunicator peer, bool accepted)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (this._ongoingPolls.ContainsKey(persistentEmpireRepresentative.GetFactionIndex()) == false) return;
            if (this._ongoingPolls[persistentEmpireRepresentative.GetFactionIndex()].ApplyVote(peer, accepted) == false) return;
            FactionPoll ongoingPoll = this._ongoingPolls[persistentEmpireRepresentative.GetFactionIndex()];
            List<NetworkCommunicator> pollProgressReceivers = ongoingPoll.GetPollProgressReceivers();
            int count = pollProgressReceivers.Count;
            for (int i = 0; i < count; i++)
            {
                GameNetwork.BeginModuleEventAsServer(pollProgressReceivers[i]);
                GameNetwork.WriteMessage(new FactionPollProgress(ongoingPoll.AcceptedCount, ongoingPoll.RejectedCount));
                GameNetwork.EndModuleEventAsServer();
            }
            this.UpdatePollProgress(ongoingPoll.AcceptedCount, ongoingPoll.RejectedCount);
        }
        private void UpdatePollProgress(int votesAccepted, int votesRejected)
        {
            if (this.OnPollUpdate == null)
            {
                return;
            }
            this.OnPollUpdate(votesAccepted, votesRejected);
        }
        public void OpenLordPoll(NetworkCommunicator targetPeer, NetworkCommunicator pollCreatorPeer)
        {
            PersistentEmpireRepresentative targetRepresentative = targetPeer.GetComponent<PersistentEmpireRepresentative>();
            PersistentEmpireRepresentative pollCreatorRepresentative = pollCreatorPeer.GetComponent<PersistentEmpireRepresentative>();
            this._ongoingPolls[targetRepresentative.GetFactionIndex()] = new FactionPoll(FactionPoll.Type.Lord, targetRepresentative.GetFactionIndex(), targetRepresentative.GetFaction(), targetPeer);
            MissionPeer component = pollCreatorPeer.GetComponent<MissionPeer>();
            MissionPeer component2 = targetPeer.GetComponent<MissionPeer>();

            if (GameNetwork.IsServer)
            {
                this._ongoingPolls[targetRepresentative.GetFactionIndex()].OnClosedOnServer = this.OnLordPollClosedOnServer;
                this._ongoingPolls[targetRepresentative.GetFactionIndex()].OnCancelledOnServer = this.OnLordPollCancelledOnServer;
            }
            if (this.OnStartLordPoll != null)
            {
                this.OnStartLordPoll(component, component2);
            }
        }

        public void Vote(bool accepted)
        {
            if (GameNetwork.IsServer)
            {
                if (GameNetwork.MyPeer != null)
                {
                    this.ApplyVote(GameNetwork.MyPeer, accepted);
                    return;
                }
            }
            else
            {
                PersistentEmpireRepresentative persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
                if (!this._ongoingPolls.ContainsKey(persistentEmpireRepresentative.GetFactionIndex())) return;
                FactionPoll ongoingPoll = this._ongoingPolls[persistentEmpireRepresentative.GetFactionIndex()];
                if (ongoingPoll.IsOpen == false) return;
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new FactionPollResponse(accepted));
                GameNetwork.EndModuleEventAsClient();
            }
        }

        public void OnLordPollClosedOnServer(FactionPoll ongoingPoll)
        {
            bool flag = ongoingPoll.GotEnoughAcceptVotesToEnd();
            if (ongoingPoll.GotEnoughRejectVotesToEnd())
            {
                flag = false;
            }
            if (ongoingPoll.TargetPlayer.IsConnectionActive == false)
            {
                flag = false;
            }

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new FactionLordPollClosed(ongoingPoll.TargetPlayer, flag, ongoingPoll.FactionIndex));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None, null);
            ongoingPoll.Close();
            this.CloseLordPoll(flag, ongoingPoll.TargetPlayer, ongoingPoll.FactionIndex);
            if (!flag) return;

            Faction f = ongoingPoll.TargetPlayer.GetComponent<PersistentEmpireRepresentative>().GetFaction();
            f.pollUnlockedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 10 * 60;
            this._factionsBehavior.SetFactionLord(ongoingPoll.TargetPlayer, ongoingPoll.FactionIndex);
        }

        private void CloseLordPoll(bool accepted, NetworkCommunicator targetPeer, int factionIndex)
        {
            // PersistentEmpireRepresentative persistentEmpireRepresentative = targetPeer.GetComponent<PersistentEmpireRepresentative>();
            if (this._ongoingPolls.ContainsKey(factionIndex))
            {
                this._ongoingPolls[factionIndex].Close();
                this._ongoingPolls.Remove(factionIndex);
            }
            if (this.OnPollClosed != null)
            {
                this.OnPollClosed(accepted, targetPeer);
            }
        }

        public void OnLordPollCancelledOnServer(FactionPoll ongoingPoll)
        {
            List<NetworkCommunicator> pollProgressReceivers = ongoingPoll.GetPollProgressReceivers();
            int count = pollProgressReceivers.Count;
            for (int i = 0; i < count; i++)
            {
                GameNetwork.BeginModuleEventAsServer(pollProgressReceivers[i]);
                GameNetwork.WriteMessage(new FactionPollCancelled(ongoingPoll.FactionIndex));
                GameNetwork.EndModuleEventAsServer();
            }
            if (ongoingPoll != null && this._ongoingPolls.ContainsKey(ongoingPoll.FactionIndex))
            {
                this._ongoingPolls[ongoingPoll.FactionIndex].Cancel();
                this._ongoingPolls.Remove(ongoingPoll.FactionIndex);
            }
            this.CancelPoll(ongoingPoll.FactionIndex);
        }
        private void CancelPoll(int factionIndex)
        {

            if (this._ongoingPolls.ContainsKey(factionIndex))
            {
                this._ongoingPolls[factionIndex].Cancel();
                this._ongoingPolls.Remove(factionIndex);
            }

            if (this.OnPollCancelled == null)
            {
                return;
            }
            this.OnPollCancelled();
        }
    }
}

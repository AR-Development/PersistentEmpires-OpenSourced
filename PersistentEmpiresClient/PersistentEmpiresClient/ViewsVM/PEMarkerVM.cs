using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEMarkerVM : ViewModel
    {
        private Camera _missionCamera;
        private MBBindingList<PEPeerMarkerVM> _peerTargets;
        // private IComparer<PEPeerMarkerVM> _distanceComparer;
        private Dictionary<MissionPeer, PEPeerMarkerVM> _peerToMarker;

        public PEMarkerVM(Camera missionCamera)
        {
            this._missionCamera = missionCamera;
            this.PeerTargets = new MBBindingList<PEPeerMarkerVM>();
            this._peerToMarker = new Dictionary<MissionPeer, PEPeerMarkerVM>();
            // this._distanceComparer = new MultiplayerMissionMarkerVM.MarkerDistanceComparer();
        }

        public void OnLocalChatMessage(NetworkCommunicator Sender, string Message, bool shout)
        {
            this.AddChatBubble(Sender, "\"" + Message + "\"", shout ? "#AFAFAFFF" : "#DADADAFF");
        }

        public void AddChatBubble(NetworkCommunicator Sender, string Message, string color)
        {
            MissionPeer missionPeer = Sender.GetComponent<MissionPeer>();

            if (this._peerToMarker.ContainsKey(missionPeer))
            {
                this._peerToMarker[missionPeer].AddMessage(Message, color);
            }
        }

        public void NotifyTyping(NetworkCommunicator Sender)
        {
            MissionPeer missionPeer = Sender.GetComponent<MissionPeer>();

            if (this._peerToMarker.ContainsKey(missionPeer))
            {
                this._peerToMarker[missionPeer].NotifyTyping();
            }
        }

        public void Tick(float dt)
        {
            // this.OnRefreshPeerMarkers();
            this.UpdateTargetScreenPositions();
        }
        private void UpdateTargetScreenPositions()
        {
            this.PeerTargets.ApplyActionOnAllItems(delegate (PEPeerMarkerVM pt)
            {
                if (pt.ChatMessages.Count > 0)
                {
                    pt.UpdateScreenPosition(this._missionCamera);
                    pt.FadeOldMessages();
                }
                else if(pt.IsIconVisible)
                {
                    pt.FadeOldIcon();
                }
            });
            // this.PeerTargets.Sort(this._distanceComparer);
        }

        public void AddPeerMarker(MissionPeer addPeer)
        {
            List<PEPeerMarkerVM> list = this.PeerTargets.ToList();
            if (list.Where((peer) => peer.TargetPeer.Peer.Id.Equals(addPeer.Peer.Id)).Count() == 0)
            {
                PEPeerMarkerVM markerVm = new PEPeerMarkerVM(addPeer);
                this.PeerTargets.Add(markerVm);
                this._peerToMarker[addPeer] = markerVm;
            }
        }

        public void RefreshPeerMarkers()
        {
            // Agent controlledAgent = GameNetwork.MyPeer.ControlledAgent;
            List<PEPeerMarkerVM> list = this.PeerTargets.ToList();
            using (List<MissionPeer>.Enumerator enumerator = VirtualPlayer.Peers<MissionPeer>().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (list.Where((peer) => peer.TargetPeer.Peer.Id.Equals(enumerator.Current.Peer.Id)).Count() == 0)
                    {
                        PEPeerMarkerVM markerVm = new PEPeerMarkerVM(enumerator.Current);
                        this.PeerTargets.Add(markerVm);
                        this._peerToMarker[enumerator.Current] = markerVm;
                    }
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<PEPeerMarkerVM> PeerTargets
        {
            get
            {
                return this._peerTargets;
            }
            set
            {
                if (value != this._peerTargets)
                {
                    this._peerTargets = value;
                    base.OnPropertyChangedWithValue(value, "PeerTargets");
                }
            }
        }
    }
}

using PersistentEmpiresLib;
using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresLib.Helpers;

namespace PersistentEmpires.Views.ViewsVM.PETabMenu
{
    public class TabPlayerVM : ViewModel, IEquatable<TabPlayerVM>
    {
        private string _userName;
        private NetworkCommunicator _peer;
        private bool _isLord;
        private int _ping;
        private int _killCount;
        private int _deathCount;
        private bool _isMarshall;

        public TabPlayerVM(NetworkCommunicator peer, bool isLord)
        {
            this._userName = peer.UserName;
            this._peer = peer;
            this.IsLord = isLord;
            // InformationManager.DisplayMessage(new InformationMessage("virtualPlayerName: " + peer.VirtualPlayer.UserName));

            base.RefreshValues();
        }
        public void UpdateLord(string lordId)
        {
            this.IsLord = lordId == this._peer.VirtualPlayer.ToPlayerId();
        }

        public void UpdateMarshall(string marshallId)
        {
            this.IsMarshall = marshallId == this._peer.VirtualPlayer.ToPlayerId();
        }

        public NetworkCommunicator GetPeer()
        {
            return _peer;
        }
        public void UpdatePeer(NetworkCommunicator peer, bool isLord)
        {
            _peer = peer;
            UserName = peer.UserName;
            IsLord = isLord;
            // InformationManager.DisplayMessage(new InformationMessage("virtualPlayerName: " + peer.VirtualPlayer.UserName));

            base.RefreshValues();
        }

        [DataSourceProperty]
        public int KillCount
        {
            get => this._killCount;
            set
            {
                if (value != this._killCount)
                {
                    this._killCount = value;
                    base.OnPropertyChangedWithValue(value, "KillCount");
                }
            }
        }

        [DataSourceProperty]
        public int DeathCount
        {
            get => this._deathCount;
            set
            {
                if (value != this._deathCount)
                {
                    this._deathCount = value;
                    base.OnPropertyChangedWithValue(value, "DeathCount");
                }
            }
        }

        [DataSourceProperty]
        public bool IsVoiceMuted
        {
            get => this._peer.GetComponent<MissionPeer>() == null ? true : this._peer.GetComponent<MissionPeer>().IsMutedFromGameOrPlatform;
        }

        public void ExecuteUnMute()
        {
            if (this._peer.GetComponent<MissionPeer>() != null)
            {
                this._peer.GetComponent<MissionPeer>().SetMuted(false);
                base.OnPropertyChanged("IsVoiceMuted");
            }
        }

        public void ExecuteMute()
        {
            if (this._peer.GetComponent<MissionPeer>() != null)
            {
                this._peer.GetComponent<MissionPeer>().SetMuted(true);
                base.OnPropertyChanged("IsVoiceMuted");
            }
        }

        public bool Equals(TabPlayerVM other)
        {
            if (other is null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return (_peer == other._peer);
        }

        [DataSourceProperty]
        public bool CanSeeClass
        {
            get
            {
                PersistentEmpireRepresentative representative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
                if (representative == null) return false;
                PersistentEmpireRepresentative targetRepresentative = this._peer.GetComponent<PersistentEmpireRepresentative>();
                if (targetRepresentative == null) return false;
                return representative.GetFactionIndex() == targetRepresentative.GetFactionIndex();
            }
        }

        [DataSourceProperty]
        public int Ping
        {
            get => (int)this.GetPeer().AveragePingInMilliseconds;
        }

        [DataSourceProperty]
        public String UserClass
        {
            get => _peer.ControlledAgent != null ? _peer.ControlledAgent.Character.GetName().ToString() : "Spectator";
        }

        [DataSourceProperty]
        public bool IsMarshall
        {
            get => this._isMarshall;
            set
            {
                if (value != this._isMarshall)
                {
                    this._isMarshall = value;
                    base.OnPropertyChangedWithValue(value, "IsMarshall");
                }
            }
        }

        [DataSourceProperty]
        public bool IsLord
        {
            get => this._isLord;
            set
            {
                if (value != this._isLord)
                {
                    this._isLord = value;
                    base.OnPropertyChangedWithValue(value, "IsLord");
                }
            }
        }

        [DataSourceProperty]
        public String UserName
        {
            get => _userName;
            set
            {
                this._userName = value;
                base.OnPropertyChangedWithValue(value, "UserName");
            }
        }
    }
}

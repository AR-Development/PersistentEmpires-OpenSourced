using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.ViewsVM.FactionManagement
{
    public class PEFactionMemberItemVM : ViewModel
    {
        private string _userName;
        private NetworkCommunicator _peer;
        private Action<PEFactionMemberItemVM> ExecuteOnSelection;
        private bool _isSelected = false;
        private bool _isGranted;

        public PEFactionMemberItemVM(NetworkCommunicator peer, Action<PEFactionMemberItemVM> ExecuteOnSelection)
        {
            this._userName = peer.UserName;
            this._peer = peer;
            this.ExecuteOnSelection = ExecuteOnSelection;
            base.RefreshValues();
        }
        public PEFactionMemberItemVM(NetworkCommunicator peer, bool isGranted, Action<PEFactionMemberItemVM> ExecuteOnSelection)
        {
            this._userName = peer.UserName;
            this._peer = peer;
            this.ExecuteOnSelection = ExecuteOnSelection;
            this.IsGranted = isGranted;
            base.RefreshValues();
        }
        [DataSourceProperty]
        public bool IsGranted
        {
            get => this._isGranted;
            set
            {
                if (value != this._isGranted)
                {
                    this._isGranted = value;
                    base.OnPropertyChangedWithValue(value, "IsGranted");
                }
            }
        }
        public NetworkCommunicator Peer
        {
            get => this._peer;
        }
        [DataSourceProperty]
        public string UserName
        {
            get => this._userName;
            set
            {
                if (value != this._userName)
                {
                    this._userName = value;
                    base.OnPropertyChangedWithValue(value, "UserName");
                }
            }
        }

        public void OnSelection()
        {
            this.ExecuteOnSelection(this);
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                if (value != this._isSelected)
                {
                    this._isSelected = value;
                    base.OnPropertyChangedWithValue(value, "IsSelected");
                }
            }
        }
    }
}

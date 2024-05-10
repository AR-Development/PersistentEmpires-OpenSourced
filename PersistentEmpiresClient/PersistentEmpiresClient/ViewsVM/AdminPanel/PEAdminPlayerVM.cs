using PersistentEmpiresLib;
using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.Views.AdminPanel
{
    public class PEAdminPlayerVM : ViewModel
    {
        private NetworkCommunicator playerPeer;
        private string _playerName;
        private string _factionName;
        private bool _isSelected;
        private Action<PEAdminPlayerVM> _executeSelect;

        public PEAdminPlayerVM(NetworkCommunicator playerPeer, Action<PEAdminPlayerVM> executeSelect)
        {
            this.playerPeer = playerPeer;
            this.PlayerName = this.playerPeer.UserName;
            PersistentEmpireRepresentative persistentEmpireRepresentative = this.playerPeer.GetComponent<PersistentEmpireRepresentative>();
            this.FactionName = persistentEmpireRepresentative == null || persistentEmpireRepresentative.GetFaction() == null ? "Unknown" : persistentEmpireRepresentative.GetFaction().name;
            this._executeSelect = executeSelect;
        }

        public NetworkCommunicator GetPeer()
        {
            return this.playerPeer;
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

        [DataSourceProperty]
        public string FactionName
        {
            get => this._factionName;
            set
            {
                if (value != this._factionName)
                {
                    this._factionName = value;
                    base.OnPropertyChangedWithValue(value, "FactionName");
                }
            }
        }

        [DataSourceProperty]
        public string PlayerName
        {
            get => this._playerName;
            set
            {
                if (value != this._playerName)
                {
                    this._playerName = value;
                    base.OnPropertyChangedWithValue(value, "PlayerName");
                }
            }
        }

        public void ExecuteSelect()
        {
            this._executeSelect(this);
        }
    }
}

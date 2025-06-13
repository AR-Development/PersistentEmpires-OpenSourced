using PersistentEmpires.Views.Views.AdminPanel;
using PersistentEmpiresClient.ViewsVM.AdminPanel.Buttons;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.ViewsVM.AdminPanel
{
    public class PEAdminPlayerManagementVM : ViewModel
    {
        private MBBindingList<PEAdminPlayerVM> _players;
        private MBBindingList<PEAdminButtonVM> _adminButtons;
        private string _searchedPlayerName;
        private PEAdminPlayerVM _selectedPlayer;
        private static List<PEAdminButtonVM> _customButtons = new List<PEAdminButtonVM>();

        public PEAdminPlayerManagementVM()
        {
            _adminButtons = new MBBindingList<PEAdminButtonVM>()
            {
                new MakeLord(),
                new PermBan(),
                new TempBan(),
                new Kick(),
                new Kill(),
                new Fade(),
                new Freeze(),
                new TpToMe(),
                new TpTo(),
                new Heal(),
                new UnWound(),
            };

            // Config value is just red on server. This cant be used, and making call to check this value feels like overkill
            //if (WoundingBehavior.Instance.WoundingEnabled)
            //    _adminButtons.Add(new UnWound());

            if (_customButtons.Any())
            {
                _customButtons.ForEach(x => _adminButtons.Add(x));
            }

            OnPropertyChangedWithValue(_adminButtons, "AdminButtons");
        }

        public static void RegisterCustomButton(PEAdminButtonVM button)
        {
            if (!_customButtons.Contains(button))
            {
                _customButtons.Add(button);
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            this.Players = new MBBindingList<PEAdminPlayerVM>();
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                this.Players.Add(new PEAdminPlayerVM(peer, (PEAdminPlayerVM selected) =>
                {
                    this.SelectedPlayer = selected;
                }));
            }
            base.OnPropertyChanged("FilteredPlayers");
        }

        [DataSourceProperty]
        public MBBindingList<PEAdminPlayerVM> Players
        {
            get => _players;
            set
            {
                if (value != this._players)
                {
                    this._players = value;
                    base.OnPropertyChangedWithValue(value, "Players");
                }
            }
        }

        [DataSourceProperty]
        public string SearchedPlayerName
        {
            get => this._searchedPlayerName;
            set
            {
                if (value != this._searchedPlayerName)
                {
                    this._searchedPlayerName = value;
                    base.OnPropertyChangedWithValue(value, "SearchedPlayerName");
                    base.OnPropertyChanged("FilteredPlayers");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<PEAdminPlayerVM> FilteredPlayers
        {
            get
            {
                List<PEAdminPlayerVM> filtered = this.SearchedPlayerName == null || this.SearchedPlayerName == "" ? this.Players.ToList() : this.Players.Where(p => p.PlayerName.Contains(this.SearchedPlayerName) || p.FactionName.Contains(this.SearchedPlayerName)).ToList();
                MBBindingList<PEAdminPlayerVM> filteredBinding = new MBBindingList<PEAdminPlayerVM>();
                foreach (PEAdminPlayerVM f in filtered)
                {
                    filteredBinding.Add(f);
                }
                return filteredBinding;
            }
        }

        [DataSourceProperty]
        public PEAdminPlayerVM SelectedPlayer
        {
            get => this._selectedPlayer;
            set
            {
                if (value != this._selectedPlayer)
                {
                    if (this._selectedPlayer != null)
                    {
                        this._selectedPlayer.IsSelected = false;
                    }
                    this._selectedPlayer = value;
                    if (value != null)
                    {
                        this._selectedPlayer.IsSelected = true;
                    }

                    _adminButtons.ToList().ForEach(x => x.SetSelectedPlayer(value));
                    base.OnPropertyChangedWithValue(this._selectedPlayer, "SelectedPlayer");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<PEAdminButtonVM> AdminButtons
        {
            get => _adminButtons;
            set
            {
                if (value != _adminButtons)
                {
                    this._adminButtons = value;
                    base.OnPropertyChangedWithValue(value, "AdminButtons");
                }
            }
        }
    }
}
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.ViewModelCollection.Input;

namespace PersistentEmpires.Views.ViewsVM.FactionManagement
{
    public class PEFactionLordPollVM : ViewModel
    {
        private bool _hasOngoingPoll;
        private bool _areKeysEnabled;
        private int _votesAccepted;
        private int _votesRejected;
        private string _pollInitiatorName;
        private string _pollDescription;
        private MPPlayerVM _targetPlayer;
        private MBBindingList<InputKeyItemVM> _keys;

        public PEFactionLordPollVM()
        {
            this.Keys = new MBBindingList<InputKeyItemVM>();
        }
        public void OnLordPoll(MissionPeer initiatorPeer, MissionPeer targetPeer)
        {
            this.TargetPlayer = new MPPlayerVM(targetPeer);
            this.PollInitiatorName = initiatorPeer.DisplayedName;
            // GameTexts.SetVariable("ACTION", isBanRequested ? MultiplayerPollProgressVM._banText : MultiplayerPollProgressVM._kickText);
            this.PollDescription = new TextObject("Polls a lord {TARGET}", null).SetTextVariable("TARGET", targetPeer.DisplayedName).ToString();
            this.VotesAccepted = 0;
            this.VotesRejected = 0;
            this.AreKeysEnabled = true;
            this.HasOngoingPoll = true;
        }
        public void OnPollUpdated(int votesAccepted, int votesRejected)
        {
            this.VotesAccepted = votesAccepted;
            this.VotesRejected = votesRejected;
        }
        public void OnPollClosed()
        {
            this.HasOngoingPoll = false;
        }
        public void OnPollOptionPicked()
        {
            this.AreKeysEnabled = false;
        }
        public void AddKey(GameKey key)
        {
            this.Keys.Add(InputKeyItemVM.CreateFromGameKey(key, false));
        }
        [DataSourceProperty]
        public bool AreKeysEnabled
        {
            get
            {
                return this._areKeysEnabled;
            }
            set
            {
                if (value != this._areKeysEnabled)
                {
                    this._areKeysEnabled = value;
                    base.OnPropertyChangedWithValue(value, "AreKeysEnabled");
                }
            }
        }

        [DataSourceProperty]
        public int VotesAccepted
        {
            get
            {
                return this._votesAccepted;
            }
            set
            {
                if (this._votesAccepted != value)
                {
                    this._votesAccepted = value;
                    base.OnPropertyChangedWithValue(value, "VotesAccepted");
                }
            }
        }

        [DataSourceProperty]
        public int VotesRejected
        {
            get
            {
                return this._votesRejected;
            }
            set
            {
                if (this._votesRejected != value)
                {
                    this._votesRejected = value;
                    base.OnPropertyChangedWithValue(value, "VotesRejected");
                }
            }
        }

        [DataSourceProperty]
        public string PollInitiatorName
        {
            get
            {
                return this._pollInitiatorName;
            }
            set
            {
                if (this._pollInitiatorName != value)
                {
                    this._pollInitiatorName = value;
                    base.OnPropertyChangedWithValue(value, "PollInitiatorName");
                }
            }
        }

        [DataSourceProperty]
        public string PollDescription
        {
            get
            {
                return this._pollDescription;
            }
            set
            {
                if (this._pollDescription != value)
                {
                    this._pollDescription = value;
                    base.OnPropertyChangedWithValue(value, "PollDescription");
                }
            }
        }

        [DataSourceProperty]
        public MPPlayerVM TargetPlayer
        {
            get
            {
                return this._targetPlayer;
            }
            set
            {
                if (value != this._targetPlayer)
                {
                    this._targetPlayer = value;
                    base.OnPropertyChangedWithValue(value, "TargetPlayer");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InputKeyItemVM> Keys
        {
            get
            {
                return this._keys;
            }
            set
            {
                if (this._keys != value)
                {
                    this._keys = value;
                    base.OnPropertyChangedWithValue(value, "Keys");
                }
            }
        }

        [DataSourceProperty]
        public bool HasOngoingPoll
        {
            get => this._hasOngoingPoll;
            set
            {
                if (value != this._hasOngoingPoll)
                {
                    this._hasOngoingPoll = value;
                    base.OnPropertyChangedWithValue(value, "HasOngoingPoll");
                }
            }
        }
    }
}

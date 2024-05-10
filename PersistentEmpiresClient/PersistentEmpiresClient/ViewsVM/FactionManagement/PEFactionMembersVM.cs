using PersistentEmpiresLib.Factions;
using System;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.ViewsVM.FactionManagement
{
    public class PEFactionMembersVM : ViewModel
    {
        private string _title;
        private MBBindingList<PEFactionMemberItemVM> _members;
        private Action _onCancel;
        private Action<PEFactionMemberItemVM> _onApply;
        private Action _close;
        private PEFactionMemberItemVM _selectedMember;
        private string _buttonText;
        private string _searchPlayer;
        private MBBindingList<PEFactionMemberItemVM> _filteredMembers;

        public PEFactionMembersVM(string _title, string _buttonText, Action onCancel, Action<PEFactionMemberItemVM> onApply, Action close)
        {
            this.Members = new MBBindingList<PEFactionMemberItemVM>();
            this.Title = _title;
            this.ButtonText = _buttonText;
            this._onCancel = onCancel;
            this._onApply = onApply;
            this._close = close;
            this._filteredMembers = new MBBindingList<PEFactionMemberItemVM>();
        }

        public PEFactionMembersVM(string _title, string _buttonText, bool isGranted, Action onCancel, Action<PEFactionMemberItemVM> onApply, Action close)
        {
            this.Members = new MBBindingList<PEFactionMemberItemVM>();
            this.Title = _title;
            this.ButtonText = _buttonText;
            this._onCancel = onCancel;
            this._onApply = onApply;
            this._close = close;
            this._filteredMembers = new MBBindingList<PEFactionMemberItemVM>();
        }

        public void RefreshItems(Faction faction, bool excludeMySelf = false)
        {
            this.SelectedMember = null;
            this.Members.Clear();
            foreach (NetworkCommunicator member in faction.members)
            {
                if (excludeMySelf && member == GameNetwork.MyPeer) continue;
                PEFactionMemberItemVM memberItemVm = new PEFactionMemberItemVM(member, (PEFactionMemberItemVM selected) =>
                {
                    if (this._selectedMember != null)
                    {
                        this._selectedMember.IsSelected = false;
                    }
                    this.SelectedMember = selected;
                    this.SelectedMember.IsSelected = true;
                });
                this.Members.Add(memberItemVm);
            }
            this.RefreshValues();
        }

        public bool CanApplyValue()
        {
            return this.SelectedMember != null;
        }

        public void OnCancel()
        {
            this._onCancel();
        }

        public void OnApply()
        {
            if (this.CanApplyValue())
            {
                this._onApply(this.SelectedMember);
            }
        }



        [DataSourceProperty]
        public string ButtonText
        {
            get => this._buttonText;
            set
            {
                if (value != this._buttonText)
                {
                    this._buttonText = value;
                    base.OnPropertyChangedWithValue(value, "ButtonText");
                }
            }
        }

        [DataSourceProperty]
        public string SearchPlayer
        {
            get => this._searchPlayer;
            set
            {
                if (value != this._searchPlayer)
                {
                    this._searchPlayer = value;
                    base.OnPropertyChangedWithValue(value, "SearchPlayer");
                    this._filteredMembers = new MBBindingList<PEFactionMemberItemVM>();
                    foreach (PEFactionMemberItemVM member in this.Members.Where(m => m.UserName.StartsWith(value)))
                    {
                        this._filteredMembers.Add(member);
                    }
                    base.OnPropertyChanged("FilteredMembers");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<PEFactionMemberItemVM> FilteredMembers
        {
            get
            {
                if (this.SearchPlayer == null || this.SearchPlayer == "")
                {
                    return this.Members;
                }
                else
                {
                    return this._filteredMembers;
                }
            }
        }

        [DataSourceProperty]
        public bool CanApply
        {
            get => this.CanApplyValue();
        }

        [DataSourceProperty]
        public PEFactionMemberItemVM SelectedMember
        {
            get => this._selectedMember;
            set
            {
                if (value != this._selectedMember)
                {
                    this._selectedMember = value;
                    base.OnPropertyChangedWithValue(this._selectedMember, "SelectedMember");
                    base.OnPropertyChanged("CanApply");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<PEFactionMemberItemVM> Members
        {
            get => this._members;
            set
            {
                if (value != this._members)
                {
                    this._members = value;
                    base.OnPropertyChangedWithValue(this._members, "Members");
                }
            }
        }

        [DataSourceProperty]
        public string Title
        {
            get => this._title;
            set
            {
                if (value != this._title)
                {
                    this._title = value;
                    base.OnPropertyChangedWithValue(this._title, "Title");
                }
            }
        }
    }
}

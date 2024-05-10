using System;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.FactionManagement
{
    public class PEFactionChangeNameVM : ViewModel
    {
        private Action _onCancel;
        private Action<string> _onApply;
        private Action _close;
        private string _factionName;
        private string _factionNameNotApplicable;

        public PEFactionChangeNameVM(Action OnCancel, Action<string> OnApply, Action Close)
        {
            this.FactionName = "";
            this.FactionNameNotApplicable = "Cannot be empty";
            this._onCancel = OnCancel;
            this._onApply = OnApply;
            this._close = Close;
        }

        public bool CanApplyValue()
        {
            if (this.FactionName == "")
            {
                this.FactionNameNotApplicable = "Cannot be empty";
                return false;
            }
            if (this.FactionName.Length < 3)
            {
                this.FactionNameNotApplicable = "Faction Name Cannot Be Less Than 3 Chars";
                return false;
            }
            if (this.FactionName.Length > 100)
            {
                this.FactionNameNotApplicable = "Faction Name Cannot Be Bigger Than 100 Chars";
                return false;
            }
            this.FactionNameNotApplicable = "";
            return true;
        }

        public void OnCancel()
        {
            this._onCancel();
        }

        public void OnApply()
        {
            if (this.CanApplyValue())
            {
                this._close();
                this._onApply(this.FactionName);
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
                    base.OnPropertyChanged("CanApply");
                }
            }
        }
        [DataSourceProperty]
        public bool CanApply
        {
            get => this.CanApplyValue();
        }
        [DataSourceProperty]
        public string FactionNameNotApplicable
        {
            get => this._factionNameNotApplicable;
            set
            {
                if (value != this._factionNameNotApplicable)
                {
                    this._factionNameNotApplicable = value;
                    base.OnPropertyChangedWithValue(value, "FactionNameNotApplicable");
                }
            }
        }
    }
}

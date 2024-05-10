using PersistentEmpires.Views.ViewsVM.PETabMenu;
using PersistentEmpiresLib.Factions;
using System;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.AdminPanel
{
    public class PEAdminFactionPanelVM : ViewModel
    {
        private MBBindingList<TabFactionVM> _factions;
        private Action<TabFactionVM, string> _setName;
        private Action<TabFactionVM> _resetBanner;
        private Action<TabFactionVM> _joinFaction;
        private TabFactionVM _selectedFaction;
        private string _name;

        private void ExecuteSelectFaction(TabFactionVM selected)
        {
            this.SelectedFaction = selected;
        }

        public PEAdminFactionPanelVM(Dictionary<int, Faction> Factions, Action<TabFactionVM, string> _setName, Action<TabFactionVM> _resetBanner, Action<TabFactionVM> _joinFaction)
        {
            this.Factions = new MBBindingList<TabFactionVM>();
            foreach (int i in Factions.Keys)
            {
                TabFactionVM fVm = new TabFactionVM(Factions[i], i, this.ExecuteSelectFaction);
                this.Factions.Add(fVm);
            }
            this._setName = _setName;
            this._resetBanner = _resetBanner;
            this._joinFaction = _joinFaction;
        }

        public void ExecuteSetName()
        {
            if (this.CanApply && this.Name != null && this.Name != "")
            {
                this._setName(this.SelectedFaction, this.Name);
            }
        }

        public void ExecuteResetBanner()
        {
            if (this.CanApply)
            {
                this._resetBanner(this.SelectedFaction);
            }
        }

        public void ExecuteJoinFaction()
        {
            if (this.CanApply)
            {
                this._joinFaction(this.SelectedFaction);
            }
        }

        [DataSourceProperty]
        public string Name
        {
            get => this._name;
            set
            {
                if (value != this._name)
                {
                    this._name = value;
                    base.OnPropertyChangedWithValue(value, "Name");
                }
            }
        }

        [DataSourceProperty]
        public bool CanApply
        {
            get => this.SelectedFaction != null;
        }
        [DataSourceProperty]
        public TabFactionVM SelectedFaction
        {
            get => this._selectedFaction;
            set
            {
                if (this._selectedFaction != value)
                {
                    if (this._selectedFaction != null)
                    {
                        this._selectedFaction.IsSelected = false;
                    }
                    this._selectedFaction = value;
                    if (value != null)
                    {
                        this._selectedFaction.IsSelected = true;
                    }
                    this.Name = this._selectedFaction.FactionName;
                    base.OnPropertyChangedWithValue(value, "SelectedFaction");
                    base.OnPropertyChanged("CanApply");
                }
            }
        }
        [DataSourceProperty]
        public MBBindingList<TabFactionVM> Factions
        {
            get => this._factions;
            set
            {
                if (value != this._factions)
                {
                    this._factions = value;
                    base.OnPropertyChangedWithValue(value, "Factions");
                }
            }
        }

        public void RefreshValues(Dictionary<int, Faction> Factions)
        {
            this.Factions = new MBBindingList<TabFactionVM>();
            foreach (int i in Factions.Keys)
            {
                if (i <= 1) continue;
                TabFactionVM fVm = new TabFactionVM(Factions[i], i, this.ExecuteSelectFaction);
                this.Factions.Add(fVm);
            }
        }
    }
}

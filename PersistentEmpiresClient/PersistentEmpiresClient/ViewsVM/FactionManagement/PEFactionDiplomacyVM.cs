using PersistentEmpires.Views.ViewsVM.PETabMenu;
using PersistentEmpiresLib.Factions;
using System;
using System.Collections.Generic;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.FactionManagement
{
    public class PEFactionDiplomacyVM : ViewModel
    {
        private MBBindingList<TabFactionVM> _factions;
        private Action<TabFactionVM> _declareWar;
        private Action<TabFactionVM> _makePeace;
        private TabFactionVM _selectedFaction;

        private void ExecuteSelectFaction(TabFactionVM selected)
        {
            this.SelectedFaction = selected;
        }
        public PEFactionDiplomacyVM(Dictionary<int, Faction> Factions, Action<TabFactionVM> DeclareWar, Action<TabFactionVM> MakePeace)
        {
            this.Factions = new MBBindingList<TabFactionVM>();
            foreach (int i in Factions.Keys)
            {
                TabFactionVM fVm = new TabFactionVM(Factions[i], i, this.ExecuteSelectFaction);
                this.Factions.Add(fVm);
            }
            this._declareWar = DeclareWar;
            this._makePeace = MakePeace;
        }

        public void ExecuteDeclareWar()
        {
            if (this.SelectedFaction != null && this.CanDeclareWar)
            {
                this.SelectedFaction.IsSelected = false;
                this.SelectedFaction.ShowWarIcon = true;
                this._declareWar(this.SelectedFaction);
                // this.SelectedFaction = null;
            }
        }

        public void ExecuteMakePeace()
        {
            if (this.SelectedFaction != null && this.CanDeclarePeace)
            {
                this.SelectedFaction.IsSelected = false;
                this.SelectedFaction.ShowWarIcon = false;
                this._makePeace(this.SelectedFaction);
                //  this.SelectedFaction = null;

            }
        }

        public bool CanDeclareWarCalc()
        {
            if (this.SelectedFaction == null) return false;
            if (this.UserFaction == null) return false;
            if (this.UserFaction.warDeclaredTo.Contains(this.SelectedFaction.FactionIndex)) return false;
            return true;
        }
        public bool CanDeclarePeaceCalc()
        {
            if (this.SelectedFaction == null) return false;
            if (this.UserFaction == null) return false;
            if (!this.UserFaction.warDeclaredTo.Contains(this.SelectedFaction.FactionIndex)) return false;
            return true;
        }

        public void RefreshValues(Dictionary<int, Faction> Factions, Faction UserFaction, int userFactionIndex)
        {
            this.SelectedFaction = null;
            this.Factions = new MBBindingList<TabFactionVM>();
            this.UserFaction = UserFaction;
            foreach (int i in Factions.Keys)
            {
                if (i <= 1) continue;
                if (i == userFactionIndex) continue;
                TabFactionVM fVm = new TabFactionVM(Factions[i], i, this.ExecuteSelectFaction);
                if (this.UserFaction.warDeclaredTo.Contains(i)) fVm.ShowWarIcon = true;
                if (Factions[i].warDeclaredTo.Contains(userFactionIndex)) fVm.ShowWarIcon = true;
                this.Factions.Add(fVm);
            }
        }
        [DataSourceProperty]
        public bool CanDeclareWar
        {
            get => this.CanDeclareWarCalc();
        }
        [DataSourceProperty]
        public bool CanDeclarePeace
        {
            get => this.CanDeclarePeaceCalc();
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
                    base.OnPropertyChangedWithValue(value, "SelectedFaction");
                    base.OnPropertyChanged("CanDeclareWar");
                    base.OnPropertyChanged("CanDeclarePeace");
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

        public Faction UserFaction { get; private set; }
    }
}

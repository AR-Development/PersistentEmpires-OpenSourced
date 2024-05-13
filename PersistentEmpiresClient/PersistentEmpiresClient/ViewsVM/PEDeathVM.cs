using PersistentEmpires.Views.ViewsVM.PETabMenu;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEDeathVM : ViewModel
    {
        private int _spawnTimer = 0;
        private CastleVM _selectedCastle;
        private int _selectedSpawnPosition;

        public PEDeathVM() { }

        [DataSourceProperty]
        public bool IsActive
        {
            get => this._spawnTimer > 0;
        }

        [DataSourceProperty]
        public int SpawnTimer
        {
            get => this._spawnTimer;
            set
            {
                if (this._spawnTimer != value)
                {
                    this._spawnTimer = value;
                    base.OnPropertyChangedWithValue(value, "SpawnTimer");
                    base.OnPropertyChanged("IsActive");
                }
            }
        }
        [DataSourceProperty]
        public int SelectedSpawnPosition
        {
            get => this._selectedSpawnPosition;
            set
            {
                if (value != this._selectedSpawnPosition)
                {
                    this._selectedSpawnPosition = value;
                    base.OnPropertyChangedWithValue(value, "SelectedSpawnPosition");
                }
            }
        }
        [DataSourceProperty]
        public bool IsCastleSelected
        {
            get => this._selectedCastle != null;
        }
        [DataSourceProperty]
        public CastleVM SelectedCastle
        {
            get => this._selectedCastle;
            set
            {
                if (value != this._selectedCastle)
                {
                    this._selectedCastle = value;
                    base.OnPropertyChangedWithValue(value, "SelectedCastle");
                    base.OnPropertyChanged("IsCastleSelected");
                }
            }
        }
    }
}

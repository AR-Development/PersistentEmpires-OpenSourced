using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.PETabMenu
{
    public class CastleVM : ViewModel
    {
        private PE_CastleBanner _castleBanner;
        private string _castleName;

        public PE_CastleBanner GetCastleBanner()
        {
            return this._castleBanner;
        }
        public CastleVM(PE_CastleBanner castleBanner)
        {
            this._castleBanner = castleBanner;
            this.CastleName = castleBanner.CastleName;
        }

        [DataSourceProperty]
        public string CastleName
        {
            get => this._castleName;
            set
            {
                if (this._castleName != value)
                {
                    this._castleName = value;
                    this.OnPropertyChangedWithValue(value, "CastleName");
                }
            }
        }


    }
}

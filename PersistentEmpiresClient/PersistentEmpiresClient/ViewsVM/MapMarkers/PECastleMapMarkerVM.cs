using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.FlagMarker.Targets;

namespace PersistentEmpires.Views.ViewsVM.MapMarkers
{
    public class PECastleMapMarkerVM : MissionMarkerTargetVM
    {
        private PE_CastleBanner _castleBanner;
        private string _castleName;
        private ImageIdentifierVM _bannerImage;

        public PE_CastleBanner GetBanner()
        {
            return this._castleBanner;
        }
        public PECastleMapMarkerVM(PE_CastleBanner banner) : base(MissionMarkerType.Peer)
        {
            this._castleBanner = banner;
            this.CastleName = banner.CastleName;
            this.UpdateBanner();
        }

        public override Vec3 WorldPosition
        {
            get => this._castleBanner.GameEntity.GetGlobalFrame().origin;
        }

        protected override float HeightOffset => 0;

        [DataSourceProperty]
        public string CastleName
        {
            get => this._castleName;
            set
            {
                if (value != this._castleName)
                {
                    this._castleName = value;
                    base.OnPropertyChangedWithValue(value, "CastleName");
                }
            }
        }



        [DataSourceProperty]
        public ImageIdentifierVM BannerImage
        {
            get => _bannerImage;
            set
            {
                this._bannerImage = value;
                base.OnPropertyChangedWithValue(value, "BannerImage");
            }
        }
        public void UpdateBanner()
        {
            Banner banner = this._castleBanner.GetOwnerFaction().banner;
            BannerCode bannercode = BannerCode.CreateFrom(banner);
            this.BannerImage = new ImageIdentifierVM(bannercode, true);
        }
    }
}

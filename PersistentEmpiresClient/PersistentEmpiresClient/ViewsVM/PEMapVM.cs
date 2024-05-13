using PersistentEmpires.Views.ViewsVM.MapMarkers;
using PersistentEmpiresLib.SceneScripts;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEMapVM : ViewModel
    {
        private Camera _missionCamera;
        private MBBindingList<PECastleMapMarkerVM> _castleMarkers;
        private PEPlayerMapMarkerVM _playerMapMarker;
        private bool _isActive;
        public PEMapVM(Camera missionCamera)
        {
            this._missionCamera = missionCamera;
            this.CastleMarkers = new MBBindingList<PECastleMapMarkerVM>();
            this.PlayerMapMarker = new PEPlayerMapMarkerVM();
            this.IsActive = false;
        }

        public void RefreshCastles(List<PE_CastleBanner> castleBanners)
        {
            if (this.CastleMarkers.Count == 0)
            {
                foreach (PE_CastleBanner banner in castleBanners) this.CastleMarkers.Add(new PECastleMapMarkerVM(banner));
            }
            else
            {
                foreach (var marker in this.CastleMarkers)
                {
                    PE_CastleBanner banner = castleBanners.Find(c => c.CastleIndex == marker.GetBanner().CastleIndex);
                    marker.UpdateBanner();
                }
            }

            this.CastleMarkers.ApplyActionOnAllItems(delegate (PECastleMapMarkerVM pt)
            {
                pt.UpdateScreenPosition(this._missionCamera);
            });
        }

        public void Tick(float dt)
        {
            this.UpdateTargetScreenPositions();
        }
        private void UpdateTargetScreenPositions()
        {
            if (this.PlayerMapMarker != null && this._missionCamera != null)
            {
                this.PlayerMapMarker.UpdateScreenPosition(this._missionCamera);
            }
        }

        [DataSourceProperty]
        public bool IsActive
        {
            get => this._isActive;
            set
            {
                if (value != this._isActive)
                {
                    this._isActive = value;
                    base.OnPropertyChangedWithValue(value, "IsActive");
                }
            }
        }

        [DataSourceProperty]
        public PEPlayerMapMarkerVM PlayerMapMarker
        {
            get => this._playerMapMarker;
            set
            {
                if (value != this._playerMapMarker)
                {
                    this._playerMapMarker = value;
                    base.OnPropertyChangedWithValue(value, "PlayerMapMaker");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<PECastleMapMarkerVM> CastleMarkers
        {
            get => this._castleMarkers;
            set
            {
                if (value != this._castleMarkers)
                {
                    this._castleMarkers = value;
                    base.OnPropertyChangedWithValue(value, "CastleMarkers");
                }
            }
        }

    }
}

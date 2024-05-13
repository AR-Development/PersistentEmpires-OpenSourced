using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresLib.SceneScripts;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PEMapView : MissionView
    {
        public bool IsActive = false;
        public bool IsSceneSupportsMap = false;
        public Camera MapCamera;

        private GauntletLayer _gauntletLayer;
        private PEMapVM _dataSource;
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this.ViewOrderPriority = 1;
            MapCamera = Camera.CreateCamera();
            Vec3 vec = default(Vec3);
            GameEntity entity = base.Mission.Scene.FindEntityWithTag("camera_instance");
            if (entity == null) return;
            this.IsSceneSupportsMap = true;
            entity.GetCameraParamsFromCameraScript(this.MapCamera, ref vec);

            this._dataSource = new PEMapVM(MapCamera);
            this._gauntletLayer = new GauntletLayer(1, "GauntletLayer", false);
            this._gauntletLayer.LoadMovie("PEMapView", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);
        }
        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            if (_gauntletLayer != null)
            {
                base.MissionScreen.RemoveLayer(this._gauntletLayer);
            }
            this._gauntletLayer = null;
            if (_dataSource != null)
            {
                this._dataSource.OnFinalize();
            }
            this._dataSource = null;
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (this.IsSceneSupportsMap == false) return;
            this._dataSource.Tick(dt);
            if (base.MissionScreen.InputManager.IsKeyReleased(InputKey.M))
            {
                if (this.IsActive)
                {
                    this.MissionScreen.CustomCamera = null;
                    this.CloseUI();
                    this.IsActive = false;
                }
                else
                {
                    this.MissionScreen.CustomCamera = this.MapCamera;
                    this.OpenUI();
                    this.IsActive = true;
                }
            }
        }

        private void OpenUI()
        {
            List<GameEntity> entities = new List<GameEntity>();
            base.Mission.Scene.GetAllEntitiesWithScriptComponent<PE_CastleBanner>(ref entities);
            List<PE_CastleBanner> castleBanners = entities.Select(e => e.GetFirstScriptOfType<PE_CastleBanner>()).ToList();
            this._dataSource.RefreshCastles(castleBanners);
            this._dataSource.IsActive = true;
        }

        private void CloseUI()
        {
            this._dataSource.IsActive = false;
        }
    }
}

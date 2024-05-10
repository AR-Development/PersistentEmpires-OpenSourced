using PersistentEmpires.Views.ViewsVM;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PEUseProgressScreen : MissionView
    {
        public static PEUseProgressScreen Instance { get; private set; }

        private GauntletLayer _gauntletLayer;
        private PEUseProgressVM _dataSource;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._dataSource = new PEUseProgressVM();
            PEUseProgressScreen.Instance = this;
            this._gauntletLayer = new GauntletLayer(100000);

            this._gauntletLayer.LoadMovie("PEUseProgress", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);
        }

        public void StartCounter(string progressTitle, int seconds)
        {
            this._dataSource.StartCounter(progressTitle, seconds);
        }

        public void StopCounter()
        {
            this._dataSource.StopCounter();
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            this._dataSource.Tick(dt);
        }
    }
}

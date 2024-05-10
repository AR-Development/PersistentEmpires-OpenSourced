using PersistentEmpires.Views.ViewsVM;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PESpawnMissionView : MissionView
    {
        private GauntletLayer _gauntletLayer;

        private PESpawnHudVM _dataSource;

        private MissionLobbyComponent _missionLobbyComponent;
        private MultiplayerMissionAgentVisualSpawnComponent _agentVisualComponent;

        private bool isOpen;
        public PESpawnMissionView()
        {
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._agentVisualComponent = this.Mission.GetMissionBehavior<MultiplayerMissionAgentVisualSpawnComponent>();
            this._agentVisualComponent.OnMyAgentVisualSpawned += this.OnMyAgentVisualSpawned;
            this._agentVisualComponent.OnMyAgentSpawnedFromVisual += this.OnMyAgentSpawnedFromVisual;
            this._dataSource = new PESpawnHudVM();
        }

        public void OnMyAgentVisualSpawned()
        {
            this.OnOpen();
        }
        public void OnMyAgentSpawnedFromVisual()
        {
            this.OnClose();
        }

        private void OnOpen()
        {
            if (isOpen)
            {
                return;
            }
            GameKey hotkey = HotKeyManager.GetCategory("CombatHotKeyCategory").GetGameKey("Action");

            this._gauntletLayer = new GauntletLayer(1);
            this._dataSource.ActionMessage = "Press " + hotkey.ToString() + " To Respawn";
            this._gauntletLayer.LoadMovie("PESpawnUi", this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            TwoDimensionEngineResourceContext resourceContext = UIResourceManager.ResourceContext;
            ResourceDepot uiresourceDepot = UIResourceManager.UIResourceDepot;
            base.MissionScreen.AddLayer(this._gauntletLayer);
            isOpen = true;
        }

        private void OnClose()
        {
            if (!isOpen) return;
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            isOpen = false;
        }
    }
}

using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresLib;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace PersistentEmpires.Views.Views
{
    public class PEMoneyPouchScreen : MissionView
    {
        private GauntletLayer _gauntletLayer;
        private PEMoneyPouchVM _dataSource;
        private PersistentEmpireRepresentative _persistentEmpireRepresentative;
        public bool IsActive { get; private set; }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._dataSource = new PEMoneyPouchVM();
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (this._gauntletLayer == null && base.MissionScreen.InputManager.IsKeyReleased(InputKey.P))
            {
                if (this.IsActive)
                {
                    this.Close();
                }
                else
                {
                    this.Open();
                }
            }
            if (this._gauntletLayer != null && this._gauntletLayer.Input.IsKeyReleased(InputKey.P))
            {
                if (this.IsActive)
                {
                    this.Close();
                }
                else
                {
                    this.Open();
                }
            }
            if (this._gauntletLayer != null && this.IsActive && (this._gauntletLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") || this._gauntletLayer.Input.IsHotKeyReleased("Exit")))
            {
                this.Close();
            }
        }

        private void Open()
        {
            if (this.IsActive) return;

            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriority);
            this._gauntletLayer.IsFocusLayer = true;
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            this._gauntletLayer.LoadMovie("PEMoneyPouch", this._dataSource);
            // this._dataSource.GoldInput = this._persistentEmpireRepresentative.Gold;
            this._dataSource.RefreshValues(GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>());
            base.MissionScreen.AddLayer(this._gauntletLayer);
            ScreenManager.TrySetFocus(this._gauntletLayer);
            this.IsActive = true;
        }

        private void Close()
        {
            this.IsActive = false;
            this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;
        }
    }
}

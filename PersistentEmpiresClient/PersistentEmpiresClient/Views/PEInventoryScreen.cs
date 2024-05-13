using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace PersistentEmpires.Views.Views
{
    public class PEInventoryScreen : PEBaseInventoryScreen
    {
        private PEInventoryVM _dataSource;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._playerInventoryComponent.OnOpenInventory += this.OpenInventory;
            this._playerInventoryComponent.OnForceCloseInventory += this.ForceCloseInventory;
            this._dataSource = new PEInventoryVM(base.HandleClickItem);
            this.ViewOrderPriority = 50;
        }

        private void ForceCloseInventory()
        {
            if (this.IsActive)
            {
                this.CloseInventoryAux();
            }
        }

        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            this._playerInventoryComponent.OnOpenInventory -= this.OpenInventory;
        }

        public override bool OnEscape()
        {

            if (this.IsActive)
            {
                this.CloseInventory();
            }
            return true;
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);


            if (base.MissionScreen.InputManager.IsKeyReleased(InputKey.I))
            {

                if (this.IsActive)
                {
                    this.CloseInventory();
                }
                else
                {
                    this.RequestOpenInventory();
                }
            }
            if (this._gauntletLayer != null && this.IsActive && (this._gauntletLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") || this._gauntletLayer.Input.IsHotKeyReleased("Exit")))
            {

                this.CloseInventory();
            }
        }

        private void CloseInventoryAux()
        {
            this.IsActive = false;
            this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;
        }

        public void CloseInventory()
        {
            if (this.IsActive)
            {
                this.CloseInventoryAux();
                if (this._dataSource.TargetInventoryId != "")
                {
                    GameNetwork.BeginModuleEventAsClient();
                    GameNetwork.WriteMessage(new ClosedInventory(this._dataSource.TargetInventoryId));
                    GameNetwork.EndModuleEventAsClient();
                }
            }
        }
        public void RequestOpenInventory()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestOpenInventory("", true));
            GameNetwork.EndModuleEventAsClient();
        }

        public void OpenInventory(Inventory playerInventory, Inventory requestedInventory)
        {
            if (this.IsActive) return;

            if (GameNetwork.MyPeer.ControlledAgent != null)
            {
                this._dataSource.SetEquipmentSlots(AgentHelpers.GetCurrentAgentEquipment(GameNetwork.MyPeer.ControlledAgent));
                this._dataSource.SetItems(playerInventory);
                this._dataSource.SetRequestedItems(requestedInventory);
            }
            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriority);
            this._gauntletLayer.IsFocusLayer = false;
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            this._gauntletLayer.LoadMovie("PEPlayerInventory", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            ScreenManager.TrySetFocus(this._gauntletLayer);
            this.IsActive = true;
        }

        protected override PEInventoryVM GetInventoryVM()
        {
            return this._dataSource;
        }
    }
}

using PersistentEmpires.Views.Views;
using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresClient.ViewsVM;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using System.Collections.Generic;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace PersistentEmpiresClient.Views
{
    public class PEBaseItemList<T, U, V> : PEBaseInventoryScreen
        where T : PEBaseItemListVM<U, V>
    {
        public T _dataSource;

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (this._gauntletLayer != null && this.IsActive && (this._gauntletLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") || this._gauntletLayer.Input.IsHotKeyReleased("Exit")))
            {
                this.Close();
            }
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (!this.IsActive || this._dataSource == null) return;
            this._dataSource.OnTick();
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
        }

        public void OnOpen(List<V> items, Inventory playerInventory, string movieName)
        {
            if (this.IsActive) return;
            this._dataSource.RefreshValues(items, playerInventory);
            this._dataSource.PlayerInventory.SetEquipmentSlots(AgentHelpers.GetCurrentAgentEquipment(GameNetwork.MyPeer.ControlledAgent));
            this._gauntletLayer = new GauntletLayer(50);
            this._gauntletLayer.IsFocusLayer = true;
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            this._gauntletLayer.LoadMovie(movieName, this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            ScreenManager.TrySetFocus(this._gauntletLayer);
            this.IsActive = true;
        }

        public virtual void Close()
        {
            if (this.IsActive)
            {
                this.CloseAux();
            }
        }

        protected void CloseAux()
        {
            this.IsActive = false;
            this._dataSource.NameFilter = "";
            this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;
        }

        protected override PEInventoryVM GetInventoryVM()
        {
            return this._dataSource.PlayerInventory;
        }
    }
}

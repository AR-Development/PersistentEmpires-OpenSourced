using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpires.Views.ViewsVM;
using PersistentEmpires.Views.ViewsVM.CraftingStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace PersistentEmpires.Views.Views
{
    public class PECraftingStationScreen : PEBaseInventoryScreen
    {
        private CraftingComponent _craftingComponent;
        private PECraftingStationVM _dataSource;
        private PE_CraftingStation ActiveEntity;
        private long _craftingStartedAt;
        private long _craftingFinishAt;
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._craftingComponent = base.Mission.GetMissionBehavior<CraftingComponent>();
            this._craftingComponent.OnCraftingUse += this.OnOpen;
            this._craftingComponent.OnCraftingStarted += this.OnCraftingStarted;
            this._craftingComponent.OnCraftingCompleted += this.OnCraftingCompleted;
            this._dataSource = new PECraftingStationVM(base.HandleClickItem);
        }

        private void OnCraftingStarted(PE_CraftingStation craftingStation, int craftIndex, NetworkCommunicator player)
        {
            this._craftingStartedAt = DateTimeOffset.Now.ToUnixTimeSeconds();
            this._dataSource.CraftingDuration = craftingStation.Craftables[craftIndex].CraftTime;
            this._craftingFinishAt = this._craftingStartedAt + this._dataSource.CraftingDuration;
            this._dataSource.IsCrafting = true;
            this._dataSource.PastDuration = 0;
        }
        private void OnCraftingCompleted()
        {
            this._dataSource.IsCrafting = false;
            this._dataSource.CraftingDuration = 0;
            this._dataSource.PastDuration = 0;
        }
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (this._dataSource == null) return;
            if (this._dataSource.IsCrafting)
            {
                this._dataSource.PastDuration = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds() - this._craftingStartedAt);
            }
        }
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (this._gauntletLayer != null && this.IsActive && (this._gauntletLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") || this._gauntletLayer.Input.IsHotKeyReleased("Exit")))
            {
                this.Close();
            }
        }
        public void Close()
        {
            if (this.IsActive)
            {
                this.CloseAux();
            }
        }
        private void CloseAux()
        {
            this.IsActive = false;
            this._dataSource.Filter = "";
            this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;

        }
        private void OnOpen(PE_CraftingStation craftingStation, Inventory playerInventory)
        {
            if (this.IsActive) return;
            this.ActiveEntity = craftingStation;
            this._dataSource.RefreshValues(craftingStation, playerInventory, this.ExecuteCraft);
            this._dataSource.PlayerInventory.SetEquipmentSlots(AgentHelpers.GetCurrentAgentEquipment(GameNetwork.MyPeer.ControlledAgent));
            this._gauntletLayer = new GauntletLayer(50);
            this._gauntletLayer.IsFocusLayer = true;
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            this._gauntletLayer.LoadMovie("PECraftingStation", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            ScreenManager.TrySetFocus(this._gauntletLayer);
            this.IsActive = true;
        }

        public void ExecuteCraft(PECraftingStationItemVM selectedCraft) {
            if(this._dataSource.IsCrafting)
            {
                InformationManager.DisplayMessage(new InformationMessage("You're already crafting. You need to wait.", new Color(1f, 0, 0)));
                return;
            }
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestExecuteCraft(this.ActiveEntity, selectedCraft.CraftableIndex));
            GameNetwork.EndModuleEventAsClient();
        }

        protected override PEInventoryVM GetInventoryVM()
        {
            return this._dataSource.PlayerInventory;
        }
    }
}

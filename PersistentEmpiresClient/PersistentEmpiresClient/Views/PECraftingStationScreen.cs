using PersistentEmpires.Views.ViewsVM;
using PersistentEmpires.Views.ViewsVM.CraftingStation;
using PersistentEmpiresClient.Views;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.Views
{
    public class PECraftingStationScreen : PEBaseItemList<PECraftingStationVM, PECraftingStationItemVM, Craftable>
    {
        private CraftingComponent _craftingComponent;
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

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (!this.IsActive || this._dataSource == null) return;
            if (this._dataSource.IsCrafting) this._dataSource.PastDuration = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds() - this._craftingStartedAt);
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

        private void OnOpen(PE_CraftingStation craftingStation, Inventory playerInventory)
        {
            if (this.IsActive) return;
            this.ActiveEntity = craftingStation;
            this._dataSource.CraftingStation = craftingStation;
            this._dataSource.Craft = ExecuteCraft;
            base.OnOpen(craftingStation.Craftables, playerInventory, "PECraftingStation");
        }

        public void ExecuteCraft(PECraftingStationItemVM selectedCraft)
        {
            if (this._dataSource.IsCrafting)
            {
                InformationManager.DisplayMessage(new InformationMessage("You're already crafting. You need to wait.", new Color(1f, 0, 0)));
                return;
            }
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestExecuteCraft(this.ActiveEntity, selectedCraft.CraftableIndex));
            GameNetwork.EndModuleEventAsClient();
        }
    }
}

using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpires.Views.ViewsVM;
using PersistentEmpires.Views.ViewsVM.StockpileMarket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace PersistentEmpires.Views.Views
{
    public class PEStockpileMarketScreen : PEBaseInventoryScreen
    {
        private PEStockpileMarketVM _dataSource;
        private StockpileMarketComponent _stockpileMarketComponent;
        private PE_StockpileMarket ActiveEntity;
        public override void OnMissionScreenInitialize() {
            base.OnMissionScreenInitialize();
            this._stockpileMarketComponent = base.Mission.GetMissionBehavior<StockpileMarketComponent>();
            this._stockpileMarketComponent.OnStockpileMarketOpen += this.OnOpen;
            this._stockpileMarketComponent.OnStockpileMarketUpdate += this.OnUpdate;
            this._stockpileMarketComponent.OnStockpileMarketUpdateMultiHandler += this.OnUpdateMulti;
            this._dataSource = new PEStockpileMarketVM(this.HandleClickItem);
        }

        private void OnUpdateMulti(PE_StockpileMarket stockpileMarket, List<int> indexes, List<int> stocks)
        {
            if(this.IsActive)
            {
                for(int i = 0; i < indexes.Count; i++)
                {
                    int index = indexes[i];
                    int stock = stocks[i];
                    this._dataSource.ItemList[index].Stock = stock;
                }
                this._dataSource.OnPropertyChanged("FilteredItemList");
            }
            // throw new NotImplementedException();
        }

        private void OnUpdate(PE_StockpileMarket stockpileMarket, int itemIndex, int newStock)
        {
            if(this.IsActive)
            {
                this._dataSource.ItemList[itemIndex].Stock = newStock;
                this._dataSource.OnPropertyChanged("FilteredItemList");
            }
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow) { 
            if(affectedAgent.IsMine)
            {
                this.CloseImportExport();
            }
        }

        private void CloseImportExportAux()
        {
            this.IsActive = false;
            this._dataSource.Filter = "";
            this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;
        }
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (this._gauntletLayer != null && this.IsActive && (this._gauntletLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") || this._gauntletLayer.Input.IsHotKeyReleased("Exit")))
            {
                this.CloseImportExport();
            }
        }

        public override void HandleClickItem(PEItemVM clickedSlot)
        {
            if (this._gauntletLayer.Input.IsShiftDown())
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new InventoryHotkey(clickedSlot.DropTag));
                GameNetwork.EndModuleEventAsClient();
            }
            if(this._gauntletLayer.Input.IsControlDown()  && clickedSlot.Item != null && clickedSlot.Count > 0)
            {
                PEStockpileMarketItemVM item = this._dataSource.ItemList.ToList().FirstOrDefault(itemVm => itemVm.MarketItem.Item.StringId == clickedSlot.Item.StringId);
                if (item != null) this.Sell(item);
            }
        }
        public void CloseImportExport()
        {
            if (this.IsActive)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new RequestCloseStockpileMarket(this.ActiveEntity));
                GameNetwork.EndModuleEventAsClient();
                this.CloseImportExportAux();
            }
        }
        public void Buy(PEStockpileMarketItemVM stockpileMarketItemVM) {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestBuyItem(this.ActiveEntity, stockpileMarketItemVM.ItemIndex));
            GameNetwork.EndModuleEventAsClient();
        }
        public void Sell(PEStockpileMarketItemVM stockpileMarketItemVM) {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestSellItem(this.ActiveEntity, stockpileMarketItemVM.ItemIndex));
            GameNetwork.EndModuleEventAsClient();
        }



        public void UnpackBoxes() {
            List<PEItemVM> items = this._dataSource.PlayerInventory.InventoryItems.ToList();
            for(int i = 0; i < items.Count; i++)
            {
                PEItemVM item = items[i];
                if (item.Count == 0) continue;
                CraftingBox box = this.ActiveEntity.CraftingBoxes.Where(c => c.BoxItem.StringId == item.Item.StringId).FirstOrDefault();
                if (box == null) continue;
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StockpileUnpackBox(i, this.ActiveEntity));
                GameNetwork.EndModuleEventAsClient();
                break;
            }
        }

        private void OnOpen(PE_StockpileMarket stockpileMarket, Inventory playerInventory)
        {
            if (this.IsActive) return;
            this.ActiveEntity = stockpileMarket;
            
            this._dataSource.RefreshValues(this.ActiveEntity, playerInventory, this.Buy, this.Sell, this.UnpackBoxes);
            this._dataSource.PlayerInventory.SetEquipmentSlots(AgentHelpers.GetCurrentAgentEquipment(GameNetwork.MyPeer.ControlledAgent));
            this._gauntletLayer = new GauntletLayer(50);
            this._gauntletLayer.IsFocusLayer = true;
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            this._gauntletLayer.LoadMovie("PEStockpileMarket", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            ScreenManager.TrySetFocus(this._gauntletLayer);
            this.IsActive = true;
        }

        protected override PEInventoryVM GetInventoryVM()
        {
            return this._dataSource.PlayerInventory;
        }
    }
}

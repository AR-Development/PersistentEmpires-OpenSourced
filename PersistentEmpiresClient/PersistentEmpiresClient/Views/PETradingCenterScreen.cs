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
    public class PETradingCenterScreen : PEBaseInventoryScreen
    {
        private PETradeCenterVM _dataSource;
        private TradingCenterBehavior tradingCenterBehavior;
        private PE_TradeCenter ActiveEntity;
        public override void OnMissionScreenInitialize() {
            base.OnMissionScreenInitialize();
            this.tradingCenterBehavior = base.Mission.GetMissionBehavior<TradingCenterBehavior>();
            this.tradingCenterBehavior.OnTradingCenterOpen += this.OnOpen;
            this.tradingCenterBehavior.OnTradingCenterUpdate += this.OnUpdate;
            this.tradingCenterBehavior.OnTradingCenterUpdateMultiHandler += this.OnUpdateMulti;
            this._dataSource = new PETradeCenterVM(base.HandleClickItem);
        }

        private void OnUpdateMulti(PE_TradeCenter stockpileMarket, List<int> indexes, List<int> stocks)
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

        private void OnUpdate(PE_TradeCenter stockpileMarket, int itemIndex, int newStock)
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

        
        public void CloseImportExport()
        {
            if (this.IsActive)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new RequestCloseTradingCenter(this.ActiveEntity));
                GameNetwork.EndModuleEventAsClient();
                this.CloseImportExportAux();
            }
        }
        public void Buy(PEStockpileMarketItemVM stockpileMarketItemVM) {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestTradingBuyItem(this.ActiveEntity, stockpileMarketItemVM.ItemIndex));
            GameNetwork.EndModuleEventAsClient();
        }
        public void Sell(PEStockpileMarketItemVM stockpileMarketItemVM) {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestTradingSellItem(this.ActiveEntity, stockpileMarketItemVM.ItemIndex));
            GameNetwork.EndModuleEventAsClient();
        }
        public void GetPrices(PEStockpileMarketItemVM stockpileMarketItemVM)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestTradingPrices(this.ActiveEntity, stockpileMarketItemVM.ItemIndex));
            GameNetwork.EndModuleEventAsClient();
        }

        private void OnOpen(PE_TradeCenter stockpileMarket, Inventory playerInventory)
        {
            if (this.IsActive) return;
            this.ActiveEntity = stockpileMarket;
            
            this._dataSource.RefreshValues(this.ActiveEntity, playerInventory, this.Buy, this.Sell, this.GetPrices);
            this._dataSource.PlayerInventory.SetEquipmentSlots(AgentHelpers.GetCurrentAgentEquipment(GameNetwork.MyPeer.ControlledAgent));
            this._gauntletLayer = new GauntletLayer(50);
            this._gauntletLayer.IsFocusLayer = true;
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            this._gauntletLayer.LoadMovie("PETradingCenter", this._dataSource);
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

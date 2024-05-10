using PersistentEmpires.Views.ViewsVM;
using PersistentEmpires.Views.ViewsVM.ImportExport;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace PersistentEmpires.Views.Views
{
    public class PEImportExport : PEBaseInventoryScreen
    {
        private PEImportExportVM _dataSource;
        private ImportExportComponent _importExportComponent;
        private PE_ImportExport ActiveEntity;
        public PEImportExport()
        {

        }
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._importExportComponent = base.Mission.GetMissionBehavior<ImportExportComponent>();
            this._importExportComponent.OnOpenImportExport += this.OnOpen;
            this._dataSource = new PEImportExportVM(base.HandleClickItem);
        }
        private void CloseImportExportAux()
        {
            this.IsActive = false;
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
                this.CloseImportExportAux();

            }
        }
        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            if (affectedAgent.IsMine)
            {
                this.CloseImportExport();
            }
        }
        private void OnOpen(PE_ImportExport ImportExportEntity, Inventory PlayerInventory)
        {
            if (this.IsActive) return;
            this.ActiveEntity = ImportExportEntity;
            this._dataSource.RefreshValues(ImportExportEntity, PlayerInventory, this.ExportItem, this.ImportItem);
            this._dataSource.PlayerInventory.SetEquipmentSlots(AgentHelpers.GetCurrentAgentEquipment(GameNetwork.MyPeer.ControlledAgent));
            this._gauntletLayer = new GauntletLayer(50);
            this._gauntletLayer.IsFocusLayer = true;
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            this._gauntletLayer.LoadMovie("PEImportExport", this._dataSource);
            if (base.MissionScreen != null)
            {
                base.MissionScreen.AddLayer(this._gauntletLayer);
                ScreenManager.TrySetFocus(this._gauntletLayer);
                this.IsActive = true;

            }
        }
        protected void ExportItem(PEImportExportItemVM exportItemVM)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestExportItem(exportItemVM.Item, this.ActiveEntity));
            GameNetwork.EndModuleEventAsClient();
        }
        protected void ImportItem(PEImportExportItemVM importItemVM)
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestImportItem(importItemVM.Item, this.ActiveEntity));
            GameNetwork.EndModuleEventAsClient();
        }
        protected override PEInventoryVM GetInventoryVM()
        {
            return this._dataSource.PlayerInventory;
        }
    }
}

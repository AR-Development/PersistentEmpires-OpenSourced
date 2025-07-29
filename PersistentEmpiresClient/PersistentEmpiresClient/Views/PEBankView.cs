using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresLib;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace PersistentEmpires.Views.Views
{
    public class PEBankView : MissionView
    {
        private BankingComponent bankingComponent;
        private PEBankVM _dataSource;
        private bool IsActive;

        public PE_Bank ActiveEntity { get; private set; }

        private GauntletLayer _gauntletLayer;

        public PEBankView()
        {

        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this.bankingComponent = base.Mission.GetMissionBehavior<BankingComponent>();
            this.bankingComponent.OnOpenBank += this.OnOpen;
            this._dataSource = new PEBankVM(this.DepositAmount, this.WithdrawAmount);
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

        private void OnOpen(PE_Bank Bank, int amount, int taxes)
        {
            if (this.IsActive) return;
            this.ActiveEntity = Bank;
            this._dataSource.RefreshValues(amount, 100 - taxes);
            this._gauntletLayer = new GauntletLayer(50);
            this._gauntletLayer.IsFocusLayer = true;
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            this._gauntletLayer.LoadMovie("PEBank", this._dataSource);
            this.bankingComponent.BankTaxRate = taxes;
            if (base.MissionScreen != null)
            {
                base.MissionScreen.AddLayer(this._gauntletLayer);
                ScreenManager.TrySetFocus(this._gauntletLayer);
                this.IsActive = true;
            }
        }

        protected void DepositAmount(PEBankVM vm)
        {
            if (GameNetwork.MyPeer.ControlledAgent == null)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEBankViewError1", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            if (vm.Amount <= 0)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEBankViewError2", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            Vec3 myPos = GameNetwork.MyPeer.ControlledAgent.Position;
            Vec3 bankPos = this.ActiveEntity.GameEntity.GetGlobalFrame().origin;
            if (bankPos.Distance(myPos) > 5)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEBankViewError3", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            PersistentEmpireRepresentative representative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (representative.HaveEnoughGold(vm.Amount) == false)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PE_Not_Enough_Gold", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestBankAction(this.ActiveEntity, vm.Amount, true));
            GameNetwork.EndModuleEventAsClient();
            vm.Balance += (vm.Amount * bankingComponent.BankTaxRate) / 100;
            vm.Amount = 0;
        }
        protected void WithdrawAmount(PEBankVM vm)
        {
            if (GameNetwork.MyPeer.ControlledAgent == null)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEBankViewError5", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            if (vm.Amount <= 0)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEBankViewError6", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            Vec3 myPos = GameNetwork.MyPeer.ControlledAgent.Position;
            Vec3 bankPos = this.ActiveEntity.GameEntity.GetGlobalFrame().origin;
            if (bankPos.Distance(myPos) > 5)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEBankViewError7", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            PersistentEmpireRepresentative representative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (vm.Balance < vm.Amount)
            {
                InformationManager.DisplayMessage(new InformationMessage("You dont have enough money", Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestBankAction(this.ActiveEntity, vm.Amount, false));
            GameNetwork.EndModuleEventAsClient();

            vm.Balance -= vm.Amount;
            vm.Amount = 0;
        }

    }
}

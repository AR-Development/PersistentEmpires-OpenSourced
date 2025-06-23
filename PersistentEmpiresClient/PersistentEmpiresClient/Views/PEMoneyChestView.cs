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
    public class PEMoneyChestView : MissionView
    {
        private PEMoneyChestVM _dataSource;
        private bool IsActive;

        public PE_MoneyChest ActiveEntity { get; private set; }

        private GauntletLayer _gauntletLayer;

        public PEMoneyChestView()
        {

        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            PE_MoneyChest.OnMoneyChestAccessed += this.OnOpen;
            MoneyChestBehavior mcb = base.Mission.GetMissionBehavior<MoneyChestBehavior>();
            mcb.OnUpdateGoldMC += Mcb_OnUpdateGoldMC;
            this._dataSource = new PEMoneyChestVM(this.DepositAmount, this.WithdrawAmount);
        }

        private void Mcb_OnUpdateGoldMC(PE_MoneyChest mc, int newAmount)
        {
            if (this.ActiveEntity == mc)
            {
                this._dataSource.Balance = newAmount;
            }
        }

        private void WithdrawAmount(PEMoneyChestVM vm)
        {
            if (GameNetwork.MyPeer.ControlledAgent == null)
            {
                
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEMoneyChestViewError1", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            if (vm.Amount <= 0)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEMoneyChestViewError2", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            Vec3 myPos = GameNetwork.MyPeer.ControlledAgent.Position;
            Vec3 bankPos = this.ActiveEntity.GameEntity.GetGlobalFrame().origin;
            if (bankPos.Distance(myPos) > 5)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEMoneyChestViewError3", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            PersistentEmpireRepresentative representative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (this.ActiveEntity.Gold < vm.Amount)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PE_Not_Enough_Gold", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            if (this.ActiveEntity.CanUserUse(GameNetwork.MyPeer) == false)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEMoneyChestViewError5", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }

            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new WithdrawDepositMoneychest(this.ActiveEntity, vm.Amount, true));
            GameNetwork.EndModuleEventAsClient();
            vm.Balance -= vm.Amount;
            vm.Amount = 0;
        }

        private void DepositAmount(PEMoneyChestVM vm)
        {
            if (GameNetwork.MyPeer.ControlledAgent == null)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEMoneyChestViewError6", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            if (vm.Amount <= 0)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEMoneyChestViewError7", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            Vec3 myPos = GameNetwork.MyPeer.ControlledAgent.Position;
            Vec3 bankPos = this.ActiveEntity.GameEntity.GetGlobalFrame().origin;
            if (bankPos.Distance(myPos) > 5)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEMoneyChestViewError8", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            PersistentEmpireRepresentative representative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (representative.HaveEnoughGold(vm.Amount) == false)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PE_Not_Enough_Gold", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }
            if (this.ActiveEntity.CanUserUse(GameNetwork.MyPeer) == false)
            {
                InformationManager.DisplayMessage(new InformationMessage(GameTexts.FindText("PEMoneyChestViewError10", null).ToString(), Color.ConvertStringToColor("#FF0000FF")));
                return;
            }

            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new WithdrawDepositMoneychest(this.ActiveEntity, vm.Amount, false));
            GameNetwork.EndModuleEventAsClient();
            vm.Balance += vm.Amount;
            vm.Amount = 0;
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

        private void CloseImportExportAux()
        {
            this.IsActive = false;
            this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;
        }
        private void OnOpen(PE_MoneyChest chest)
        {
            if (this.IsActive) return;
            this.ActiveEntity = chest;
            this._dataSource.RefreshValues((int)chest.Gold);
            this._gauntletLayer = new GauntletLayer(50);
            this._gauntletLayer.IsFocusLayer = true;
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            this._gauntletLayer.LoadMovie("PEMoneyChest", this._dataSource);
            if (base.MissionScreen != null)
            {
                base.MissionScreen.AddLayer(this._gauntletLayer);
                ScreenManager.TrySetFocus(this._gauntletLayer);
                this.IsActive = true;
            }
        }
    }
}

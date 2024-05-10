using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresLib.NetworkMessages.Client;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace PersistentEmpires.Views.Views
{
    public class PEAdminChatScreen : MissionView
    {
        private GauntletLayer _gauntletLayer;
        private PELocalChatVM _dataSource;
        private bool IsActive;
        public PEAdminChatScreen() { }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._dataSource = new PELocalChatVM();
            this._dataSource.TextInput = "";
        }
        private void Close()
        {
            this.IsActive = false;
            this._gauntletLayer.IsFocusLayer = false;
            ScreenManager.TryLoseFocus(this._gauntletLayer);
            this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;
            Mission.GetMissionBehavior<MissionMainAgentController>().IsChatOpen = false;
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
            if (affectedAgent.IsMine && this.IsActive)
            {
                this.Close();
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (Agent.Main == null) return;
            if (this._gauntletLayer == null && base.MissionScreen.InputManager.IsKeyReleased(InputKey.BackSpace))
            {
                if (!this.IsActive)
                {
                    this.Open();
                }
            }
            if (this._gauntletLayer != null && this._gauntletLayer.Input.IsKeyReleased(InputKey.BackSpace))
            {
                if (!this.IsActive)
                {
                    this.Open();
                }
            }
            if (this._gauntletLayer != null && this.IsActive && (this._gauntletLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") || this._gauntletLayer.Input.IsHotKeyReleased("Exit")))
            {
                this.Close();
            }
            if (this._gauntletLayer != null && this.IsActive && this._gauntletLayer.Input.IsKeyReleased(InputKey.Enter))
            {
                this.SendAdminMessage();
            }
        }

        private void SendAdminMessage()
        {
            if (this._dataSource.TextInput != "")
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new AdminChat(this._dataSource.TextInput));
                GameNetwork.EndModuleEventAsClient();
            }
            this._dataSource.TextInput = "";
            this.Close();
        }

        private void Open()
        {
            if (this.IsActive) return;

            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriority);
            this._gauntletLayer.IsFocusLayer = true;

            Agent.EventControlFlag controls = 0;
            Agent.MovementControlFlag movementControlFlag = 0;
            if (Agent.Main != null)
            {
                controls = Agent.Main.EventControlFlags;
                movementControlFlag = Agent.Main.MovementFlags;
            }

            this._gauntletLayer.LoadMovie("PEAdminChat", this._dataSource);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            base.MissionScreen.AddLayer(this._gauntletLayer);
            this._gauntletLayer.IsFocusLayer = true;
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            Widget textWidget = this._gauntletLayer.UIContext.Root.FindChild("EditableLocalChat", true);
            this._gauntletLayer.UIContext.EventManager.SetWidgetFocused(textWidget);
            this._gauntletLayer.IsFocusLayer = true;
            ScreenManager.TrySetFocus(this._gauntletLayer);
            this.IsActive = true;
            Mission.GetMissionBehavior<MissionMainAgentController>().IsChatOpen = true;
        }
    }
}

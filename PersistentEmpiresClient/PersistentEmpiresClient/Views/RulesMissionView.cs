using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Text.RegularExpressions;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ScreenSystem;

namespace PersistentEmpires.Views.Views
{
    [DefaultView]
    public class RulesMissionView : MissionView
    {
        private GauntletLayer _gauntletLayer;
        private RulesViewModule _dataSource = new RulesViewModule();
        private static bool IsActive = false;

        public RulesMissionView()
        {

        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (MissionScreen.InputManager.IsKeyPressed(InputKey.F1) && PersistentEmpireClientBehavior.Rules != null)
            {
                if (IsActive)
                {
                    CloseView();
                }
                else
                {
                    ShowView();
                }
            }
        }

        private bool CloseView()
        {
            if (IsActive)
            {
                IsActive = false;
                _gauntletLayer.InputRestrictions.ResetInputRestrictions();
                MissionScreen.RemoveLayer(_gauntletLayer);
                _gauntletLayer = null;
                return true;
            }

            return false;
        }

        public override bool OnEscape()
        {
            return CloseView();
        }

        internal void ShowView()
        {
            if (_gauntletLayer != null)
            {
                CloseView();
            }
            else
            {
                IsActive = true;
                _gauntletLayer = new GauntletLayer(102);
                _gauntletLayer.IsFocusLayer = false;
                _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
                _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
                _gauntletLayer.LoadMovie("ShowRules", this._dataSource);
                MissionScreen.AddLayer(this._gauntletLayer);
                ScreenManager.TrySetFocus(this._gauntletLayer);

                _dataSource.Init(PersistentEmpireClientBehavior.Rules);
            }
        }

        public void CloseCraftingWindow()
        {
            _gauntletLayer.InputRestrictions.ResetInputRestrictions();
            MissionScreen.RemoveLayer(_gauntletLayer);
            _gauntletLayer = null;
        }
    }
}

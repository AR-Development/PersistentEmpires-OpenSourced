using PersistentEmpiresClient.ViewsVM.MessageLogPanel;
using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;

namespace PersistentEmpires.Views.Views
{
    [DefaultView]
    public class PEMessageLogScreen : TaleWorlds.MountAndBlade.View.MissionViews.MissionView
    {
        private List<KeyValuePair<string, string>> _log = new List<KeyValuePair<string, string>>();
        private int _counter = 0;
        private int _logSize = 100;
        private GauntletLayer _gauntletLayer;
        private MessageLog _dataSource = new MessageLog();
        private bool IsActive = false;

        public PEMessageLogScreen()
        {

        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            _log = new List<KeyValuePair<string, string>>();
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<LocalMessageServer>(this.HandleLocalMessageFromServerForLog);
                networkMessageHandlerRegisterer.Register<ShoutMessageServer>(this.HandleShoutMessageFromServerForLog);                
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (MissionScreen.InputManager.IsKeyPressed(InputKey.L))
            {
                if (IsActive)
                {
                    CloseLog();
                }
                else
                {
                    ShowLogView();
                }
            }
            else if (MissionScreen.InputManager.IsKeyPressed(InputKey.C) && MissionScreen.InputManager.IsControlDown())
            {
                _dataSource.Copy();
            }
        }

        private bool CloseLog()
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
            return CloseLog();
        }

        internal void ShowLogView()
        {
            if (_gauntletLayer != null)
            {
                CloseLog();
            }
            else
            {
                IsActive = true;
                _gauntletLayer = new GauntletLayer(100);
                _gauntletLayer.IsFocusLayer = false;
                _gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
                _gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
                _gauntletLayer.LoadMovie("MessageLogView", this._dataSource);
                MissionScreen.AddLayer(this._gauntletLayer);
                ScreenManager.TrySetFocus(this._gauntletLayer);
                _dataSource.Init(Log);
            }
        }

        private void HandleShoutMessageFromServerForLog(ShoutMessageServer message)
        {
            AddToLog($"{message.Message}");
        }

        private void HandleLocalMessageFromServerForLog(LocalMessageServer message)
        {
            AddToLog($"{message.Message}");
        }

        public void AddToLog(string message)
        {
            lock (_lock)
            {
                if (_counter == _logSize)
                {
                    _counter--;
                    _log.RemoveAt(_logSize - 1);
                }
                var tmp = new KeyValuePair<string, string>(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), message);
                _log.Insert(0, tmp);
                _dataSource.Add(tmp);
                _counter++;
            }
        }

        private object _lock = new object();
        internal List<KeyValuePair<string, string>> Log
        {
            get
            {
                lock (_lock)
                {
                    return _log.ToList();
                }
            }
            set
            {
                lock (this._lock)
                {
                    _log = value;
                }
            }
        }
    }
}

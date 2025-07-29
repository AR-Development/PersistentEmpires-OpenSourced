#if SERVER
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class AutorestartBehavior : MissionLogic
    {
        public bool IsActive = true;
        public int IntervalHour = 24;

        private long _restartAt = 0;
        private readonly Dictionary<int, (bool shown, string Message, string DebugMessage)> _checkpoints;

        public AutorestartBehavior()
        {
            _checkpoints = new Dictionary<int, (bool shown, string Message, string DebugMessage)>()
            {
                //{ 180, ("3 hours", GameTexts.FindText("AutorestartBehavior1", null).ToString()) },
                {3600, (false, "1 hour", GameTexts.FindText("AutorestartBehavior2", null).ToString()) },
                {1800, (false, "30 minutes", GameTexts.FindText("AutorestartBehavior3", null).ToString()) },
                {900, (false, "15 minutes", GameTexts.FindText("AutorestartBehavior4", null).ToString()) },
                {300, (false, "5 minutes", GameTexts.FindText("AutorestartBehavior5", null).ToString()) },
                {60, (false,"1 minute", GameTexts.FindText("AutorestartBehavior6", null).ToString()) },
                {30, (false, "30 seconds", GameTexts.FindText("AutorestartBehavior7", null).ToString()) },
                {20, (false, "20 seconds", GameTexts.FindText("AutorestartBehavior8", null).ToString()) },
                {10, (false, "10 seconds", GameTexts.FindText("AutorestartBehavior9", null).ToString()) }
            };
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            IsActive = ConfigManager.GetBoolConfig("AutorestartActive", true);
            IntervalHour = ConfigManager.GetIntConfig("AutorestartIntervalHours", 24);
        }

        private static bool needToBeTrigged = true;
        private static int _counter = 0;
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (++_counter < 5)
                return;
            // Reset counter
            _counter = 0;

            if (_restartAt == 0)
            {
                _restartAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (IntervalHour * 3600);
                return;
            }

            int remainingSeconds = (int)(_restartAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            if (needToBeTrigged && IsActive && remainingSeconds <= 2)
            {
                var saveBehavior = Mission.Current.GetMissionBehavior<SaveSystemBehavior>();

                needToBeTrigged = false;

                if (saveBehavior != null)
                {
                    saveBehavior.LastSaveAt -= saveBehavior.SaveDuration;
                }
            }
            else if (IsActive && remainingSeconds <= 0)
            {
                if (!SaveSystemBehavior.IsRunning)
                {
                    throw new Exception("Server auto restart.");
                }
            }

            if (_checkpoints.ContainsKey(remainingSeconds) && !_checkpoints[remainingSeconds].shown)
            {
                _checkpoints[remainingSeconds] = (true, _checkpoints[remainingSeconds].Message, _checkpoints[remainingSeconds].DebugMessage);
                //InformationComponent.Instance.BroadcastAnnouncement($"{_checkpoints[remainingSeconds].DebugMessage}");
                DiscordBehavior.NotifyServerStatus(_checkpoints[remainingSeconds].DebugMessage, DiscordBehavior.ColorPurple);
                InformationComponent.Instance.BroadcastQuickInformation(_checkpoints[remainingSeconds].DebugMessage, Colors.Red.ToUnsignedInteger());
                InformationComponent.Instance.BroadcastMessage(_checkpoints[remainingSeconds].DebugMessage, Colors.Red.ToUnsignedInteger());
                Debug.Print(_checkpoints[remainingSeconds].DebugMessage);
            }
        }
    }
}
#endif
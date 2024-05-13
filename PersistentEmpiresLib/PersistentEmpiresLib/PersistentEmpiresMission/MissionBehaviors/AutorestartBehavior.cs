using System;
using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class AutorestartBehavior : MissionLogic
    {
        public bool IsActive = true;
        public int IntervalHour = 24;

        private long _restartAt = 0;
        private readonly List<(int Minutes, string Message, string DebugMessage)> _checkpoints;

        public AutorestartBehavior()
        {
            _checkpoints = new List<(int, string, string)>
            {
                (180, "3 hours", "Server will be restarted in 3 hours."),
                (60, "1 hour", "Server will be restarted in 1 hour."),
                (30, "30 minutes", "Server will be restarted in 30 minutes."),
                (15, "15 minutes", "Server will be restarted in 15 minutes."),
                (5, "5 minutes", "Server will be restarted in 5 minutes."),
                (1, "1 minute", "Server will be restarted in 1 minute."),
                (0, "10 seconds", "Server will be restarted in 10 seconds.")
            };
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            if (GameNetwork.IsServer)
            {
                IsActive = ConfigManager.GetBoolConfig("AutorestartActive", true);
                IntervalHour = ConfigManager.GetIntConfig("AutorestartIntervalHours", 24);
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (_restartAt == 0)
            {
                _restartAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (IntervalHour * 3600);
                return;
            }

            long remainingSeconds = _restartAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (remainingSeconds <= 0)
            {
                throw new Exception("Server auto restart.");
            }

            int remainingMinutes = (int)(remainingSeconds / 60);
            foreach (var checkpoint in _checkpoints)
            {
                if (remainingMinutes <= checkpoint.Minutes && !_checkpoints.Contains(checkpoint))
                {
                    InformationComponent.Instance.BroadcastQuickInformation(checkpoint.Message);
                    Debug.Print(checkpoint.DebugMessage);
                    _checkpoints.Add(checkpoint);
                }
            }
        }
    }
}

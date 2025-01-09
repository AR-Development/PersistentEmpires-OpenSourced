
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class AutorestartBehavior : MissionLogic
    {
        public bool IsActive = true;
        public int IntervalHour = 24;

        private long _restartAt = 0;
        private readonly Dictionary<int, (string Message, string DebugMessage)> _checkpoints;

        public AutorestartBehavior()
        {
            _checkpoints = new Dictionary<int, (string Message, string DebugMessage)>()
            {
                //{ 180, ("3 hours", "Server will be restarted in 3 hours.") },
                //{3600, ("1 hour", "Server will be restarted in 1 hour.") },
                {1800, ("30 minutes", "Server will be restarted in 30 minutes.") },
                {900, ("15 minutes", "Server will be restarted in 15 minutes.") },
                {300, ("5 minutes", "Server will be restarted in 5 minutes.") },
                {60, ("1 minute", "Server will be restarted in 1 minute.") },
                {30, ("30 seconds", "Server will be restarted in 30 seconds.") },
                {20, ("20 seconds", "Server will be restarted in 20 seconds.") },
                {10, ("10 seconds", "Log the fuck off before you lose yo shit to the final messages.") }
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

            int remainingSeconds = (int)(_restartAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            if (IsActive && remainingSeconds <= 0)
            {
                throw new Exception("Server auto restart.");
            }

            if (_checkpoints.ContainsKey(remainingSeconds))
            {
                InformationComponent.Instance.BroadcastQuickInformation(_checkpoints[remainingSeconds].DebugMessage);
                Debug.Print(_checkpoints[remainingSeconds].DebugMessage);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class AutorestartBehavior : MissionLogic
    {
        public bool IsActive = true;
        public int IntervalHour = 24;

        private long _restartAt = 0;
        private Dictionary<string, bool> _checkpoints = new Dictionary<string, bool>();
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            if(GameNetwork.IsServer)
            {
                this.IsActive = ConfigManager.GetBoolConfig("AutorestartActive", true);
                this.IntervalHour = ConfigManager.GetIntConfig("AutorestartIntervalHours", 24);
            }

        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if(_restartAt == 0)
            {
                _restartAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (IntervalHour * 60 * 60);
                return;
            }

            long remainingSeconds = _restartAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int remainingMinutes = (int)(remainingSeconds / 60);
            int remainingHours = remainingMinutes / 60;

            if (remainingHours < 3 && _checkpoints.ContainsKey("3h") == false)
            {
                InformationComponent.Instance.BroadcastQuickInformation("Server will be restarted in 3 hours.");
                Debug.Print("Server will be restarted in 3 hours.");
                _checkpoints["3h"] = true;
            }
            else if (remainingHours < 1 && _checkpoints.ContainsKey("1h") == false)
            {
                InformationComponent.Instance.BroadcastQuickInformation("Server will be restarted in 1 hours.");
                Debug.Print("Server will be restarted in 1 hours.");
                _checkpoints["1h"] = true;
            }
            else if (remainingHours == 0 && remainingMinutes < 30 && _checkpoints.ContainsKey("30m") == false)
            {
                InformationComponent.Instance.BroadcastQuickInformation("Server will be restarted in 30 minutes.");
                Debug.Print("Server will be restarted in 30 minutes.");
                _checkpoints["30m"] = true;
            }
            else if (remainingHours == 0 && remainingMinutes < 15 && _checkpoints.ContainsKey("15m") == false)
            {
                InformationComponent.Instance.BroadcastQuickInformation("Server will be restarted in 15 minutes.");
                Debug.Print("Server will be restarted in 15 minutes.");
                _checkpoints["15m"] = true;
            }
            else if (remainingHours == 0 && remainingMinutes < 5 && _checkpoints.ContainsKey("5m") == false)
            {
                InformationComponent.Instance.BroadcastQuickInformation("Server will be restarted in 5 minutes.");
                Debug.Print("Server will be restarted in 5 minutes.");
                _checkpoints["5m"] = true;
            }
            else if (remainingHours == 0 && remainingMinutes < 1 && _checkpoints.ContainsKey("1m") == false)
            {
                InformationComponent.Instance.BroadcastQuickInformation("Server will be restarted in 1 minutes.");
                Debug.Print("Server will be restarted in 1 minutes.");
                _checkpoints["1m"] = true;
            }
            else if (remainingHours == 0 && remainingMinutes < 0 && remainingSeconds < 10 && _checkpoints.ContainsKey("10s") == false)
            {
                InformationComponent.Instance.BroadcastQuickInformation("Server will be restarted in 10 seconds.");
                Debug.Print("Server will be restarted in 10 seconds.");
                _checkpoints["10s"] = true;
            }

            if(remainingSeconds <= 0)
            {
                throw new Exception("Server auto restart.");
            }
        }

    }
}

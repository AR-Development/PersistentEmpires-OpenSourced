#if SERVER
using System;
using TaleWorlds.MountAndBlade;
using System.Timers;
using System.Linq;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib;
using TaleWorlds.Library;

namespace PersistentEmpiresServer.ServerMissions
{
    public class AutoPayBehavior : MissionNetwork
    {
        private static bool AutoPayEnabled = false;
        private static int AutoPayTime = 30;
        private static int AutoPayGold = 100;
        private static System.Timers.Timer AutoPayTimer = null;


        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            AutoPayEnabled = ConfigManager.GetBoolConfig("AutoPayEnabled", false);
            AutoPayTime = ConfigManager.GetIntConfig("AutoPayTimeMinutes", 30);
            AutoPayGold = ConfigManager.GetIntConfig("AutoPayGold", 100);

            if (AutoPayEnabled)
            {
                SetTimer();
            }

        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            RemoveAutoTimer();
        }

        private static void SetTimer()
        {
            AutoPayTimer = new System.Timers.Timer(AutoPayTime * 60 * 1000);
            AutoPayTimer.Elapsed += OnTimedEvent;
            AutoPayTimer.AutoReset = true;
            AutoPayTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            var activePlayers = GameNetwork.NetworkPeers.ToList().Where(x => x.ControlledAgent?.IsPlayerControlled == true);

            foreach (NetworkCommunicator peer in activePlayers)
            {
                SendSendGoldToPeer(peer);
            }
        }

        private static void SendSendGoldToPeer(NetworkCommunicator networkPeer)
        {
            var representative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            var message = $"Autopay message: Amount of {AutoPayGold} have been added to your purse. Next payment in {AutoPayTime} minutes.";
            
            representative.GoldGain(AutoPayGold);
            InformationComponent.Instance.SendMessage(message, Colors.Yellow.ToUnsignedInteger(), networkPeer);
        }

        internal static void RemoveAutoTimer()
        {
            AutoPayTimer?.Stop();
            AutoPayTimer?.Dispose();
        }
    }
}
#endif
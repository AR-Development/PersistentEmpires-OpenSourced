using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class DayNightCycleBehavior : MissionNetwork
    {
        public PE_DayNightCycle DayNightCycle;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);

            if (this.Mission.Scene.GetFirstEntityWithScriptComponent<PE_DayNightCycle>() != null)
            {
                this.DayNightCycle = this.Mission.Scene.GetFirstEntityWithScriptComponent<PE_DayNightCycle>().GetFirstScriptOfType<PE_DayNightCycle>();
            }

            Debug.Print(" ===> DAYNIGHT CYCLE CHECK " + (this.DayNightCycle == null).ToString() + " <====== ");
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator player)
        {
            base.OnPlayerConnectedToServer(player);

            if (this.DayNightCycle != null)
            {
                Debug.Print(" ===> DAYNIGHT CYCLE TIME SENT " + (this.DayNightCycle.TimeOfDay).ToString() + " <====== ");
                GameNetwork.BeginModuleEventAsServer(player);
                GameNetwork.WriteMessage(new SetDayTime(this.DayNightCycle.TimeOfDay));
                GameNetwork.EndModuleEventAsServer();
            }
        }
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<SetDayTime>(this.HandleSetDayTimeFromServer);
            }
            if (GameNetwork.IsServer)
            {

            }
        }

        private void HandleSetDayTimeFromServer(SetDayTime message)
        {
            if (this.DayNightCycle != null) this.DayNightCycle.SetTimeOfDay(message.TimeOfDay);
        }
    }
}

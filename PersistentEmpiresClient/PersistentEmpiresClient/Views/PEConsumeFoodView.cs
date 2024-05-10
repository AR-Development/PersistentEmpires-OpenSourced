using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PEConsumeFoodView : MissionView
    {
        public bool RequestedStartEat = false;
        private AgentHungerBehavior _agentHungerBehavior;
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._agentHungerBehavior = base.Mission.GetMissionBehavior<AgentHungerBehavior>();

        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            GameKey defendClick = HotKeyManager.GetCategory("CombatHotKeyCategory").GetGameKey("Defend");
            if (base.MissionScreen.SceneLayer.Input.IsGameKeyPressed(defendClick.Id))
            {
                this.RequestedStartEat = this._agentHungerBehavior.RequestStartEat();
            }
            else if (base.MissionScreen.SceneLayer.Input.IsGameKeyReleased(defendClick.Id) && this.RequestedStartEat)
            {
                this._agentHungerBehavior.RequestStopEat();
            }
        }
    }
}

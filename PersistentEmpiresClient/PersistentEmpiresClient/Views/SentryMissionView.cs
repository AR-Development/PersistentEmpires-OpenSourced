using System;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class SentryMissionView : MissionView
    {
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.N))
            {
                throw new Exception("Hello");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;

namespace PersistentEmpires.Views
{
    public static class PEViewCreator
    {
        public static MissionView CreateMissionScoreBoardUIHandler(Mission mission, bool isSingleTeam)
        {
            return ViewCreatorManager.CreateMissionView<MissionScoreboardUIHandler>(mission != null, mission, new object[]
            {
                isSingleTeam
            });
        }
    }
}

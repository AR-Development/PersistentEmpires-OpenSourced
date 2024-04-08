using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using RestSharp;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.Views
{
    public class PEWhitelistServer : MissionView
    {
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            string ip = NetworkMain.GameClient.LastBattleServerAddressForClient;
        }
    }
}

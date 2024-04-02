using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_TaxHandler : MissionObject
    {
        public int CastleId = -1;
        public int TaxPercentage = 10;

        public void AddTaxFeeToMoneyChest(int amount) {
            if (GameNetwork.IsServer == false) return;
            if (this.CastleId == -1) return;
            MoneyChestBehavior behavior = Mission.Current.GetMissionBehavior<MoneyChestBehavior>();
            behavior.AddTaxFromHandler(this, amount);
        }
    }
}

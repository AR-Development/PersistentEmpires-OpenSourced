using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;

namespace PersistentEmpiresHarmony.Patches
{
    class PatchSiegeWeapon
    {
        public static bool PrefixHasToBeDefendedByUser(ref bool __result, BattleSideEnum sideEnum)
        {
            __result = false;
            return false;
        }
    }
}

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

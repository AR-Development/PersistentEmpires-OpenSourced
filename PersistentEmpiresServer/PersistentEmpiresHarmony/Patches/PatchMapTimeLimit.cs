using System.Reflection;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresHarmony.Patches
{
    class PatchMapTimeLimit
    {
        public static MultiplayerOptionsProperty Postfix(MultiplayerOptionsProperty returnedValue)
        {
            if (returnedValue.Description.Equals("Maximum duration for the map. In minutes."))
            {
                var field = typeof(MultiplayerOptionsProperty).GetField("BoundsMax", BindingFlags.Public | BindingFlags.Instance);
                field.SetValue(returnedValue, 100000);
            }
            return returnedValue;
        }
    }
}

using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib
{
    public class Main : MBSubModuleBase
    {
        public static readonly string ModuleName = "PersistentEmpires";
        public delegate bool IsAdminDelegate(NetworkCommunicator player);
        public static IsAdminDelegate IsAdminFunc { get; set; }

        public static bool IsAdmin(NetworkCommunicator player)
        {
            if (IsAdminFunc != null) { return IsAdminFunc(player); }
            return false;
        }

        internal static bool IsPlayerAdmin(NetworkCommunicator player)
        {
            if (IsAdminFunc != null) { return IsAdminFunc(player); }
            return false;
        }
    }
}

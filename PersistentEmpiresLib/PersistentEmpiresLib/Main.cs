using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib
{
    public class Main : MBSubModuleBase
    {
        public delegate bool IsAdminDelegate(NetworkCommunicator player);
        public static IsAdminDelegate IsAdminFunc { get; set; }

        public static bool IsAdmin(NetworkCommunicator player)
        {
            if(IsAdminFunc != null) { return IsAdminFunc(player); }
            return false;
        }

        internal static bool IsPlayerAdmin(NetworkCommunicator player)
        {
            if (IsAdminFunc != null) { return IsAdminFunc(player); }
            return false;
        }
    }
}

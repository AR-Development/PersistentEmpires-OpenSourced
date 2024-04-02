using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views
{
    public class MentalrobDebugManager
    {
        
        public static void Initialize() {
            Debug.OnPrint += OnPrint;
            
        }

        private static void OnPrint(string arg1, ulong arg2)
        {
            var path = Path.Combine(ModuleHelper.GetModuleFullPath("PersistentEmpires"), "debugLog.txt");
            File.AppendAllText(path, arg1 + "\n");
        }
    }
}

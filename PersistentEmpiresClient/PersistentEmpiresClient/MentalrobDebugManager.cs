using PersistentEmpiresLib;
using System.IO;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;

namespace PersistentEmpires.Views
{
    public class MentalrobDebugManager
    {

        public static void Initialize()
        {
            Debug.OnPrint += OnPrint;

        }

        private static void OnPrint(string arg1, ulong arg2)
        {
            var path = Path.Combine(ModuleHelper.GetModuleFullPath(Main.ModuleName), "debugLog.txt");
            File.AppendAllText(path, arg1 + "\n");
        }
    }
}

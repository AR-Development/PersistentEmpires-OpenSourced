using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentEmpiresLib.Helpers
{
    public static class PEInformationManager
    {
        public static event Action<string, int> OnStartCounter;
        public static event Action OnStopCounter;
        public static void StartCounter(string progressTitle, int countTime)
        {
            if(PEInformationManager.OnStartCounter != null)
            {
                PEInformationManager.OnStartCounter(progressTitle, countTime);
            }
        }
        public static void StopCounter()
        {
            if (PEInformationManager.OnStopCounter != null)
            {
                PEInformationManager.OnStopCounter();
            }
        }
    }
}

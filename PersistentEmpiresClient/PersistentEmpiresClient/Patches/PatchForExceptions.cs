using PersistentEmpiresClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchForExceptions
    {
        public static Exception FinalizerException(Exception __exception)
        {
            if(__exception != null)
            {
                HarmonyLibClient.ExceptionThrown(__exception);
            }
            return __exception;
        }
    }
}

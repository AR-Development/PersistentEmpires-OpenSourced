using PersistentEmpiresClient;
using System;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchForExceptions
    {
        public static Exception FinalizerException(Exception __exception)
        {
            if (__exception != null)
            {
                HarmonyLibClient.ExceptionThrown(__exception);
            }
            return __exception;
        }
    }
}

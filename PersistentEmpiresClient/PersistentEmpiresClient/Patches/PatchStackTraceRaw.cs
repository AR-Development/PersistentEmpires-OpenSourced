using PersistentEmpiresClient;
using System;
using System.Diagnostics;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchStackTraceRaw
    {
        public static bool GetStackTraceRaw(int skipCount = 0)
        {
            StackTrace myTrace = new StackTrace(0, true);
            try
            {
                Exception rglException = new Exception("RGL Exception");
                throw rglException;
            }
            catch (Exception e)
            {
                HarmonyLibClient.RglExceptionThrown(myTrace, e);
            }

            return true;
        }

        public static void GetStackTraceRawPostfix(ref string __result)
        {
            StackTrace myTrace = new StackTrace(0, true);
            try
            {
                Exception rglException = new Exception("RGL Exception POSTFIX");
                throw rglException;
            }
            catch (Exception e)
            {
                HarmonyLibClient.RglExceptionThrown(myTrace, e);
            }
        }

        public static bool GetStackTraceRawDeep(StackTrace stack, int skipCount)
        {
            Exception rglException = new Exception("RGL Exception POSTFIX");
            HarmonyLibClient.RglExceptionThrown(stack, rglException);
            return true;
        }
    }
}

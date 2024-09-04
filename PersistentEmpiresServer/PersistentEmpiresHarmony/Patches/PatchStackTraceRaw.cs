/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
                PersistentEmpiresHarmonySubModule.RglExceptionThrown(myTrace, e);
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
                PersistentEmpiresHarmonySubModule.RglExceptionThrown(myTrace, e);
            }
        }

        public static bool GetStackTraceRawDeep(StackTrace stack, int skipCount)
        {
            Exception rglException = new Exception("RGL Exception POSTFIX");
            PersistentEmpiresHarmonySubModule.RglExceptionThrown(stack, rglException);
            return true;
        }
    }
}

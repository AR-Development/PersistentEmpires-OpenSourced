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

namespace PersistentEmpiresLib.Helpers
{
    public static class PEInformationManager
    {
        public static event Action<string, int> OnStartCounter;
        public static event Action OnStopCounter;
        public static void StartCounter(string progressTitle, int countTime)
        {
            if (PEInformationManager.OnStartCounter != null)
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

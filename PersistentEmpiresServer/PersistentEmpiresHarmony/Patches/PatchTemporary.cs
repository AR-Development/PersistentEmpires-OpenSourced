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

using NetworkMessages.FromClient;
using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchTemporary
    {
        public static bool PrefixHandleClientEventRequestUseObject(NetworkCommunicator networkPeer, RequestUseObject message)
        {
            return true;
        }
        public static Exception FinalizerHandleClientEventRequestUseObject(Exception __exception)
        {
            if (__exception != null)
            {
                Debug.Print("ERROR ON USE ITEM");
            }

            return __exception;
        }
    }
}

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

using System.Reflection;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresHarmony.Patches
{
    class PatchMapTimeLimit
    {
        public static MultiplayerOptionsProperty Postfix(MultiplayerOptionsProperty returnedValue)
        {
            if (returnedValue.Description.Equals("Maximum duration for the map. In minutes."))
            {
                var field = typeof(MultiplayerOptionsProperty).GetField("BoundsMax", BindingFlags.Public | BindingFlags.Instance);
                field.SetValue(returnedValue, 100000);
            }
            return returnedValue;
        }
    }
}

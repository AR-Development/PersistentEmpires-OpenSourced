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

using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using TaleWorlds.Library;

namespace PersistentEmpiresLib.SceneScripts.Extensions
{
    public static class IMissionObjectHash_Implementation
    {
        public static string GetMissionObjectHash(this IMissionObjectHash missionObjectHash)
        {
            MatrixFrame frame = missionObjectHash.GetMissionObject().GameEntity.GetGlobalFrame();
            float x = frame.origin.X;
            float y = frame.origin.Y;
            float z = frame.origin.Z;

            string toHashed = x + "," + y + "," + z;
            return CryptoHelper.GetHashString(toHashed);
        }
    }
}

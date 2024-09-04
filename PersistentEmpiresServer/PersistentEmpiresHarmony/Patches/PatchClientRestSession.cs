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

using TaleWorlds.Diamond;
using TaleWorlds.Library.Http;

namespace PersistentEmpiresHarmony.Patches
{
    public static class PatchClientRestSession
    {
        public static void PrefixClientRestSessionCtor(IClient client, ref string address, ref ushort port, ref bool isSecure, IHttpDriver platformNetworkClient)
        {
            address = "localhost";
            port = 3000;
            isSecure = false;
        }
    }
}

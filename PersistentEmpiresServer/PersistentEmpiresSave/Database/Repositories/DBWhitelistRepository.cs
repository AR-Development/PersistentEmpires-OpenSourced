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

using Dapper;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Linq;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBWhitelistRepository
    {
        public class DBWhitelist
        {
            public int Id { get; set; }
            public string PlayerId { get; set; }
            public bool Active { get; set; }

        }

        public static void Initialize()
        {
            SaveSystemBehavior.OnIsPlayerWhitelisted += IsPlayerWhitelisted;
        }
        public static bool IsPlayerWhitelisted(string playerId)
        {
            int count = DBConnection.Connection.Query("SELECT * FROM Whitelist WHERE PlayerId = @PlayerId AND Active = 1", new
            {
                PlayerId = playerId
            }).Count();
            return count > 0;
        }
    }
}

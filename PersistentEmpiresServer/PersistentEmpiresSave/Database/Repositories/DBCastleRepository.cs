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
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Collections.Generic;
using System.Linq;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBCastleRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnGetCastles += GetCastles;
            SaveSystemBehavior.OnCreateOrSaveCastle += CreateOrSaveCastle;
        }
        private static DBCastle CreateDBCastle(int castleIndex, int factionIndex)
        {
            return new DBCastle
            {
                FactionIndex = factionIndex,
                CastleIndex = castleIndex,
            };
        }
        public static DBCastle GetCastle(int castleIndex)
        {
            IEnumerable<DBCastle> factions = DBConnection.Connection.Query<DBCastle>("SELECT * FROM Castles WHERE CastleIndex = @CastleIndex", new { CastleIndex = castleIndex });
            if (factions.Count() == 0) return null;
            return factions.First();
        }

        private static DBCastle CreateOrSaveCastle(int castleIndex, int factionIndex)
        {
            if (GetCastle(castleIndex) == null)
            {
                return CreateCastle(castleIndex, factionIndex);
            }
            return SaveCastle(castleIndex, factionIndex);
            // DBCastle castle = CreateDBCastle(castleIndex, factionIndex);

        }

        private static DBCastle SaveCastle(int castleIndex, int factionIndex)
        {
            DBCastle dbFaction = CreateDBCastle(castleIndex, factionIndex);
            string updateSql = "UPDATE Castles SET FactionIndex = @FactionIndex WHERE CastleIndex = @CastleIndex";
            DBConnection.Connection.Execute(updateSql, dbFaction);
            return dbFaction;
        }

        private static DBCastle CreateCastle(int castleIndex, int factionIndex)
        {
            DBCastle dbFaction = CreateDBCastle(castleIndex, factionIndex);
            string insertSql = "INSERT INTO Castles (CastleIndex, FactionIndex) VALUES (@CastleIndex,@FactionIndex)";
            DBConnection.Connection.Execute(insertSql, dbFaction);
            return dbFaction;
        }

        private static IEnumerable<DBCastle> GetCastles()
        {
            return DBConnection.Connection.Query<DBCastle>("SELECT * FROM Castles");
        }
    }
}

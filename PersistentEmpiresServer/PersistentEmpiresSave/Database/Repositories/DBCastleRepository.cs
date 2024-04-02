using Dapper;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if(GetCastle(castleIndex) == null)
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

using Dapper;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            int count = DBConnection.Connection.Query("SELECT * FROM whitelist WHERE PlayerId = @PlayerId AND Active = 1", new
            {
                PlayerId = playerId
            }).Count();
            return count > 0;
        }
    }
}

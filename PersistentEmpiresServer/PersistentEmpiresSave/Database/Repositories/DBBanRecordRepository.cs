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
    public class DBBanRecordRepository
    {

        public static void Initialize()
        {
            
        }

        public static void AdminServerBehavior_OnBanPlayer(string PlayerId, string PlayerName, long BanEndsAt)
        {
            BanPlayer(PlayerId, PlayerName, BanEndsAt, "Banned in-game");
        }

        public static void BanPlayer(string playerId, string playerName, long banEndsAt, string banReason)
        {
            DBBanRecord banRecord = new DBBanRecord
            {
                PlayerId = playerId,
                PlayerName = playerName,
                CreatedAt = DateTime.UtcNow,
                BanEndsAt = DateTimeOffset.FromUnixTimeSeconds(banEndsAt).UtcDateTime,
                BanReason = banReason
            };

            string insertSql = "INSERT INTO BanRecords(PlayerId, PlayerName, BanReason, CreatedAt, BanEndsAt) VALUES(@PlayerId, @PlayerName, @BanReason, @CreatedAt, @BanEndsAt)";
            DBConnection.Connection.Execute(insertSql, banRecord);
        }

        public static void UnbanPlayer(string playerId, string unbanReason)
        {
            string updateSql = "UPDATE BanRecords SET BanEndsAt = 0, UnbanReason = @UnbanReason WHERE PlayerId = @PlayerId";
            DBConnection.Connection.Execute(updateSql, new
            {
                UnbanReason = unbanReason,
                PlayerId = playerId
            });
        }
        public static bool IsPlayerBanned(string playerId)
        {
            int count = DBConnection.Connection.Query("SELECT * FROM BanRecords WHERE BanEndsAt >= @CurrentTime AND PlayerId = @PlayerId", new {
                CurrentTime = DateTime.UtcNow,
                PlayerId = playerId
            }).Count();
            return count > 0;
        }
    }
}

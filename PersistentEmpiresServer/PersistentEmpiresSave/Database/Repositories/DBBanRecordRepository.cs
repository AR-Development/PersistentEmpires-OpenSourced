using Dapper;
using MySqlConnector;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Linq;
using System.Windows.Forms;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

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

        public static void AdminServerBehavior_OnUnBanPlayer(string adminId, string playerId)
        {
            var admin = GameNetwork.NetworkPeers.FirstOrDefault(x => x.VirtualPlayer?.ToPlayerId() == adminId);

            if (admin == null)
            {
                return;
            }

            if (UnBanPlayer(playerId, adminId))
            {
                LoggerHelper.LogAnAction(admin, LogAction.PlayerBansPlayer, null, new object[] { playerId });
                InformationComponent.Instance.SendMessage("Player was unbanned", new Color(0f, 0f, 1f).ToUnsignedInteger(), admin);
            }
            else
            {
                InformationComponent.Instance.SendMessage("Something went wrong", new Color(0f, 0f, 1f).ToUnsignedInteger(), admin);
            }
        }

        public static void BanPlayer(string playerId, string playerName, long banEndsAt, string banReason)
        {
            try
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
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
            }
        }

        public static bool UnBanPlayer(string playerId, string adminId)
        {
            var conn = new MySqlConnection(DBConnection.Connection.ConnectionString);
            int exists = 0;
            try
            {
                conn.Open();
                var sql = $"SELECT EXISTS(SELECT * " +
                $"FROM BanRecords " +
                $"WHERE BanEndsAt >= Now() AND PlayerId = '{playerId}'); ";
                var cmdSelect = new MySqlCommand(sql, conn);
                var rdr = cmdSelect.ExecuteReader();
                while (rdr.Read())
                {
                    exists = int.Parse(rdr[0].ToString());
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
                return false;
            }
            finally
            {
                conn.Close();
            }

            try
            {
                if (exists == 1)
                {
                    string upateQuerry = $"UPDATE BanRecords SET BanEndsAt = 0, UnbanReason = 'Unbanned in game by {adminId}' WHERE BanEndsAt >= Now() AND PlayerId = @PlayerId";
                    DBConnection.Connection.Execute(upateQuerry,
                    new
                    {
                        PlayerId = playerId,
                    });
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
                return false;
            }
        }

        public static void UnbanPlayer(string playerId, string unbanReason)
        {
            try
            {
                string updateSql = "UPDATE BanRecords SET BanEndsAt = 0, UnbanReason = @UnbanReason WHERE PlayerId = @PlayerId";
                DBConnection.Connection.Execute(updateSql, new
                {
                    UnbanReason = unbanReason,
                    PlayerId = playerId
                });
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
            }
        }

        public static bool IsPlayerBanned(string playerId)
        {
            try
            {
                int count = DBConnection.Connection.Query("SELECT * FROM BanRecords WHERE BanEndsAt >= @CurrentTime AND PlayerId = @PlayerId", new
                {
                    CurrentTime = DateTime.UtcNow,
                    PlayerId = playerId
                }).Count();
                return count > 0;
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
                
                return false;
            }
        }
    }
}
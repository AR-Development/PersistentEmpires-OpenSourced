using Dapper;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresServer.ServerMissions;
using System;
using TaleWorlds.Library;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBLogRepository
    {

        public static void Initialize()
        {
            LoggerHelper.OnLogAction += SaveLogToSql;
        }

        public static void SaveLogToSql(DBLog dblog)
        {
            Debug.Print(dblog.LogMessage);
            try
            {
                string insertSql = "INSERT INTO Logs (CreatedAt, IssuerPlayerId, IssuerPlayerName, IssuerCoordinates, ActionType, LogMessage, AffectedPlayers) VALUES (@CreatedAt, @IssuerPlayerId, @IssuerPlayerName, @IssuerCoordinates, @ActionType, @LogMessage, @AffectedPlayers)";
                DBConnection.Connection.Execute(insertSql, dblog);
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
            }
        }
    }
}

using Dapper;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBPlayerNameRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnCreatePlayerNameIfNotExists += CreatePlayerNameIfNotExists;
        }

        public static DBPlayerName CreateDBPlayerName(NetworkCommunicator player)
        {
            return new DBPlayerName
            {
                PlayerId = player.VirtualPlayer.Id.ToString(),
                PlayerName = player.UserName
            };
        }

        public static void CreatePlayerNameIfNotExists(NetworkCommunicator player)
        {
            try
            {
                DBPlayerName playerName = CreateDBPlayerName(player);
                IEnumerable<DBPlayerName> playerNames = DBConnection.Connection.Query<DBPlayerName>("SELECT * FROM PlayerNames WHERE PlayerName = @PlayerName", new
                {
                    PlayerName = player.UserName
                });
                if (playerNames.Count() == 0)
                {
                    string insertSql = "INSERT INTO PlayerNames (PlayerName, PlayerId) VALUES (@PlayerName, @PlayerId)";
                    DBConnection.Connection.Execute(insertSql, playerName);
                }
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
            }
        }
    }
}

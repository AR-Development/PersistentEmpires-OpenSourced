using Dapper;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpiresLib.SceneScripts.Extensions;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBHorseMarketRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnGetAllHorseMarkets += GetAllHorseMarkets;
            SaveSystemBehavior.OnGetHorseMarket += GetHorseMarket;
            SaveSystemBehavior.OnCreateOrSaveHorseMarket += CreateOrSaveHorseMarket;
        }

        private static DBHorseMarket CreateDBHorseMarket(PE_HorseMarket horseMarket)
        {

            return new DBHorseMarket
            {
                MissionObjectHash = horseMarket.GetMissionObjectHash(),
                Stock = horseMarket.Stock
            };
        }

        public static IEnumerable<DBHorseMarket> GetAllHorseMarkets()
        {
            try
            {
                Debug.Print("[Save Module] LOADING ALL HORSE MARKETS FROM DB");
                return DBConnection.Connection.Query<DBHorseMarket>("SELECT * FROM HorseMarkets");
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);

                return null;
            }
        }

        public static DBHorseMarket GetHorseMarket(PE_HorseMarket horseMarket)
        {
            try
            {
                Debug.Print("[Save Module] LOAD HORSEMARKET FROM DB (" + horseMarket.GetMissionObjectHash() + ")");
                IEnumerable<DBHorseMarket> result = DBConnection.Connection.Query<DBHorseMarket>("SELECT * FROM HorseMarkets WHERE MissionObjectHash = @MissionObjectHash", new { MissionObjectHash = horseMarket.GetMissionObjectHash() });
                Debug.Print("[Save Module] LOAD HORSEMARKET FROM DB (" + horseMarket.GetMissionObjectHash() + ") RESULT COUNT " + result.Count());
                if (result.Count() == 0) return null;
                return result.First();
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);

                return null;
            }
        }

        public static DBHorseMarket CreateOrSaveHorseMarket(PE_HorseMarket horseMarket)
        {

            if (GetHorseMarket(horseMarket) == null)
            {
                return CreateHorseMarket(horseMarket);
            }
            return SaveHorseMarket(horseMarket);
        }

        private static DBHorseMarket SaveHorseMarket(PE_HorseMarket horseMarket)
        {
            try
            {
                Debug.Print("[Save Module] UPDATE HORSE MARKET TO DB (" + horseMarket != null ? " " + horseMarket.GetMissionObjectHash() : "HORSE MARKET IS NULL !)");
                DBHorseMarket dbMarket = CreateDBHorseMarket(horseMarket);
                string insertQuery = "UPDATE HorseMarkets SET Stock = @Stock WHERE MissionObjectHash = @MissionObjectHash";
                DBConnection.Connection.Execute(insertQuery, dbMarket);
                Debug.Print("[Save Module] UPDATE HORSE MARKET TO DB (" + horseMarket != null ? " " + horseMarket.GetMissionObjectHash() : "HORSE MARKET IS NULL !)");
                return dbMarket;
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);

                return null;
            }
        }

        private static DBHorseMarket CreateHorseMarket(PE_HorseMarket horseMarket)
        {
            try
            {
                Debug.Print("[Save Module] CREATE HORSE MARKET TO DB (" + horseMarket != null ? " " + horseMarket.GetMissionObjectHash() : "HORSE MARKET IS NULL !)");
                DBHorseMarket dbMarket = CreateDBHorseMarket(horseMarket);
                string insertQuery = "INSERT INTO HorseMarkets (MissionObjectHash, Stock) VALUES (@MissionObjectHash, @Stock)";
                DBConnection.Connection.Execute(insertQuery, dbMarket);
                Debug.Print("[Save Module] CREATE HORSE MARKET TO DB (" + horseMarket != null ? " " + horseMarket.GetMissionObjectHash() : "HORSE MARKET IS NULL !)");
                return dbMarket;
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);

                return null;
            }
        }
    }
}

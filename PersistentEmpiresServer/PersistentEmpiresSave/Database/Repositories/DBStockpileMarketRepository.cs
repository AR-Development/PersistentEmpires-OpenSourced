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
    public class DBStockpileMarketRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnGetAllStockpileMarkets += GetAllStockpileMarkets;
            SaveSystemBehavior.OnGetStockpileMarket += GetStockpileMarket;
            SaveSystemBehavior.OnUpsertStockpileMarkets += UpsertStockpileMarkets;
        }

        private static DBStockpileMarket CreateDBStockpileMarket(PE_StockpileMarket stockpileMarket)
        {
            Debug.Print("[Save Module] CREATE DB STOCKPILE MARKET (" + stockpileMarket != null ? " " + stockpileMarket.GetMissionObjectHash() : "STOCKPILE MARKET IS NULL !)");
            return new DBStockpileMarket
            {
                MissionObjectHash = stockpileMarket.GetMissionObjectHash(),
                MarketItemsSerialized = stockpileMarket.SerializeStocks()
            };
        }
        public static IEnumerable<DBStockpileMarket> GetAllStockpileMarkets()
        {
            try
            {
                Debug.Print("[Save Module] LOADING ALL STOCKPILE MARKETS FROM DB");
                return DBConnection.Connection.Query<DBStockpileMarket>("SELECT * FROM StockpileMarkets");
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);

                return null;
            }
        }
        public static DBStockpileMarket GetStockpileMarket(PE_StockpileMarket stockpileMarket)
        {
            try
            {
                Debug.Print("[Save Module] LOAD STOCKPILE FROM DB (" + stockpileMarket.GetMissionObjectHash() + ")");
                IEnumerable<DBStockpileMarket> result = DBConnection.Connection.Query<DBStockpileMarket>("SELECT * FROM StockpileMarkets WHERE MissionObjectHash = @MissionObjectHash", new { MissionObjectHash = stockpileMarket.GetMissionObjectHash() });
                Debug.Print("[Save Module] LOAD STOCKPILE FROM DB (" + stockpileMarket.GetMissionObjectHash() + ") RESULT COUNT " + result.Count());
                if (result.Count() == 0) return null;
                return result.First();
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);

                return null;
            }
        }
        public static void UpsertStockpileMarkets(List<PE_StockpileMarket> markets)
        {
            try
            {
                Debug.Print($"[Save Module] INSERT/UPDATE FOR {markets.Count()} MARKETS TO DB");
                if (markets.Any())
                {
                    string query = @"
            INSERT INTO StockpileMarkets (MissionObjectHash, MarketItemsSerialized)
            VALUES ";

                    foreach (var market in markets)
                    {
                        var dbMarket = CreateDBStockpileMarket(market);

                        query += $"('{dbMarket.MissionObjectHash}', '{dbMarket.MarketItemsSerialized}'),";
                    }
                    // remove last ","
                    query = query.TrimEnd(',');
                    query += @" 
                    ON DUPLICATE KEY UPDATE
                    MarketItemsSerialized = VALUES(MarketItemsSerialized)";

                    DBConnection.Connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
            }
        }
    }
}
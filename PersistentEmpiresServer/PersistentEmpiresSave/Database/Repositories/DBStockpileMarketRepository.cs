using Dapper;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpiresLib.SceneScripts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBStockpileMarketRepository
    {
        public static void Initialize() {
            SaveSystemBehavior.OnGetAllStockpileMarkets += GetAllStockpileMarkets;
            SaveSystemBehavior.OnGetStockpileMarket += GetStockpileMarket;
            SaveSystemBehavior.OnCreateOrSaveStockpileMarket += CreateOrSaveStockpileMarket;
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
        public static IEnumerable<DBStockpileMarket> GetAllStockpileMarkets() {
            Debug.Print("[Save Module] LOADING ALL STOCKPILE MARKETS FROM DB");
            return DBConnection.Connection.Query<DBStockpileMarket>("SELECT * FROM StockpileMarkets");
        }
        public static DBStockpileMarket GetStockpileMarket(PE_StockpileMarket stockpileMarket)
        {
            Debug.Print("[Save Module] LOAD STOCKPILE FROM DB (" + stockpileMarket.GetMissionObjectHash() + ")");
            IEnumerable<DBStockpileMarket> result = DBConnection.Connection.Query<DBStockpileMarket>("SELECT * FROM StockpileMarkets WHERE MissionObjectHash = @MissionObjectHash", new { MissionObjectHash = stockpileMarket.GetMissionObjectHash() });
            Debug.Print("[Save Module] LOAD STOCKPILE FROM DB (" + stockpileMarket.GetMissionObjectHash() + ") RESULT COUNT "+result.Count());
            if (result.Count() == 0) return null;
            return result.First();
        }
        public static DBStockpileMarket CreateOrSaveStockpileMarket(PE_StockpileMarket stockpileMarket) {
            
            if (GetStockpileMarket(stockpileMarket) == null)
            {
                return CreateStockpileMarket(stockpileMarket);
            }
            return SaveStockpileMarket(stockpileMarket);
        }
        public static DBStockpileMarket CreateStockpileMarket(PE_StockpileMarket stockpileMarket)
        {
            Debug.Print("[Save Module] CREATE STOCKPILE MARKET TO DB (" + stockpileMarket != null ? " " + stockpileMarket.GetMissionObjectHash() : "STOCKPILE MARKET IS NULL !)");
            DBStockpileMarket dbMarket = CreateDBStockpileMarket(stockpileMarket);
            string insertQuery = "INSERT INTO StockpileMarkets (MissionObjectHash, MarketItemsSerialized) VALUES (@MissionObjectHash, @MarketItemsSerialized)";
            DBConnection.Connection.Execute(insertQuery, dbMarket);
            Debug.Print("[Save Module] CREATED STOCKPILE MARKET TO DB (" + stockpileMarket != null ? " " + stockpileMarket.GetMissionObjectHash() : "STOCKPILE MARKET IS NULL !)");
            return dbMarket;
        }

        public static DBStockpileMarket SaveStockpileMarket(PE_StockpileMarket stockpileMarket)
        {
            Debug.Print("[Save Module] UPDATING STOCKPILE MARKET TO DB (" + stockpileMarket != null ? " " + stockpileMarket.GetMissionObjectHash() : "STOCKPILE MARKET IS NULL !)");
            DBStockpileMarket dbMarket = CreateDBStockpileMarket(stockpileMarket);
            string insertQuery = "UPDATE StockpileMarkets SET MarketItemsSerialized = @MarketItemsSerialized WHERE MissionObjectHash = @MissionObjectHash";
            DBConnection.Connection.Execute(insertQuery, dbMarket);
            Debug.Print("[Save Module] UPDATED STOCKPILE MARKET TO DB (" + stockpileMarket != null ? " " + stockpileMarket.GetMissionObjectHash() : "STOCKPILE MARKET IS NULL !)");
            return dbMarket;
        }
        
    }
}

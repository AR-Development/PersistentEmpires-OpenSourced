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
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpiresLib.SceneScripts.Extensions;
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
            Debug.Print("[Save Module] LOADING ALL HORSE MARKETS FROM DB");
            return DBConnection.Connection.Query<DBHorseMarket>("SELECT * FROM HorseMarkets");
        }

        public static DBHorseMarket GetHorseMarket(PE_HorseMarket horseMarket)
        {
            Debug.Print("[Save Module] LOAD HORSEMARKET FROM DB (" + horseMarket.GetMissionObjectHash() + ")");
            IEnumerable<DBHorseMarket> result = DBConnection.Connection.Query<DBHorseMarket>("SELECT * FROM HorseMarkets WHERE MissionObjectHash = @MissionObjectHash", new { MissionObjectHash = horseMarket.GetMissionObjectHash() });
            Debug.Print("[Save Module] LOAD HORSEMARKET FROM DB (" + horseMarket.GetMissionObjectHash() + ") RESULT COUNT " + result.Count());
            if (result.Count() == 0) return null;
            return result.First();
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
            Debug.Print("[Save Module] UPDATE HORSE MARKET TO DB (" + horseMarket != null ? " " + horseMarket.GetMissionObjectHash() : "HORSE MARKET IS NULL !)");
            DBHorseMarket dbMarket = CreateDBHorseMarket(horseMarket);
            string insertQuery = "UPDATE HorseMarkets SET Stock = @Stock WHERE MissionObjectHash = @MissionObjectHash";
            DBConnection.Connection.Execute(insertQuery, dbMarket);
            Debug.Print("[Save Module] UPDATE HORSE MARKET TO DB (" + horseMarket != null ? " " + horseMarket.GetMissionObjectHash() : "HORSE MARKET IS NULL !)");
            return dbMarket;
        }

        private static DBHorseMarket CreateHorseMarket(PE_HorseMarket horseMarket)
        {
            Debug.Print("[Save Module] CREATE HORSE MARKET TO DB (" + horseMarket != null ? " " + horseMarket.GetMissionObjectHash() : "HORSE MARKET IS NULL !)");
            DBHorseMarket dbMarket = CreateDBHorseMarket(horseMarket);
            string insertQuery = "INSERT INTO HorseMarkets (MissionObjectHash, Stock) VALUES (@MissionObjectHash, @Stock)";
            DBConnection.Connection.Execute(insertQuery, dbMarket);
            Debug.Print("[Save Module] CREATE HORSE MARKET TO DB (" + horseMarket != null ? " " + horseMarket.GetMissionObjectHash() : "HORSE MARKET IS NULL !)");
            return dbMarket;
        }
    }
}

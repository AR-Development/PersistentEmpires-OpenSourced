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
    public class DBUpgradeableBuildingRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnGetAllUpgradeableBuildings += GetAllUpgradeableBuildings;
            SaveSystemBehavior.OnGetUpgradeableBuilding += GetUpgradeableBuilding;
            SaveSystemBehavior.OnCreateOrSaveUpgradebleBuilding += CreateOrSaveUpgradebleBuilding;
        }

        private static DBUpgradeableBuilding CreateDBUpgradeableBuilding(PE_UpgradeableBuildings upgradeableBuilding)
        {
            Debug.Print("[Save Module] CREATE DB UPGRADEABLE BUILDING (" + upgradeableBuilding != null ? " " + upgradeableBuilding.GetMissionObjectHash() : " UPGRADEABLE BUILDING MARKET IS NULL !)");
            return new DBUpgradeableBuilding
            {
                CurrentTier = upgradeableBuilding.CurrentTier,
                IsUpgrading = upgradeableBuilding.IsUpgrading,
                MissionObjectHash = upgradeableBuilding.GetMissionObjectHash()
            };
        }

        public static DBUpgradeableBuilding CreateOrSaveUpgradebleBuilding(PE_UpgradeableBuildings upgradeableBuildings)
        {
            if (GetUpgradeableBuilding(upgradeableBuildings) == null)
            {
                return CreateUpgradeableBuilding(upgradeableBuildings);
            }
            return SaveUpgradeableBuilding(upgradeableBuildings);
        }
        public static DBUpgradeableBuilding CreateUpgradeableBuilding(PE_UpgradeableBuildings upgradeableBuilding)
        {
            Debug.Print("[Save Module] CREATE UPGRADEABLE BUILDING TO DB(" + upgradeableBuilding != null ? " " + upgradeableBuilding.GetMissionObjectHash() : " UPGRADEABLE BUILDING MARKET IS NULL !)");
            DBUpgradeableBuilding db = CreateDBUpgradeableBuilding(upgradeableBuilding);
            string insertSql = "INSERT INTO UpgradeableBuildings (MissionObjectHash, CurrentTier, IsUpgrading) VALUES (@MissionObjectHash, @CurrentTier, @IsUpgrading)";
            DBConnection.Connection.Execute(insertSql, db);
            Debug.Print("[Save Module] CREATED UPGRADEABLE BUILDING TO DB(" + upgradeableBuilding != null ? " " + upgradeableBuilding.GetMissionObjectHash() : " UPGRADEABLE BUILDING MARKET IS NULL !)");
            return db;
        }

        public static DBUpgradeableBuilding SaveUpgradeableBuilding(PE_UpgradeableBuildings upgradeableBuilding)
        {
            Debug.Print("[Save Module] UPDATE UPGRADEABLE BUILDING TO DB(" + upgradeableBuilding != null ? " " + upgradeableBuilding.GetMissionObjectHash() : " UPGRADEABLE BUILDING MARKET IS NULL !)");
            DBUpgradeableBuilding db = CreateDBUpgradeableBuilding(upgradeableBuilding);
            string insertSql = "UPDATE UpgradeableBuildings SET CurrentTier = @CurrentTier, IsUpgrading = @IsUpgrading WHERE MissionObjectHash = @MissionObjectHash";
            DBConnection.Connection.Execute(insertSql, db);
            Debug.Print("[Save Module] UPDATED UPGRADEABLE BUILDING TO DB(" + upgradeableBuilding != null ? " " + upgradeableBuilding.GetMissionObjectHash() : " UPGRADEABLE BUILDING MARKET IS NULL !)");
            return db;
        }

        public static IEnumerable<DBUpgradeableBuilding> GetAllUpgradeableBuildings()
        {
            Debug.Print("[Save Module] GET ALL UPGRADEABLE BUILDINGS");
            return DBConnection.Connection.Query<DBUpgradeableBuilding>("SELECT * FROM UpgradeableBuildings");
        }

        public static DBUpgradeableBuilding GetUpgradeableBuilding(PE_UpgradeableBuildings upgradeableBuildings)
        {
            Debug.Print("[Save Module] LOADING UPGRADEABLE BUILDING FROM DB (" + upgradeableBuildings.GetMissionObjectHash() + ")");
            IEnumerable<DBUpgradeableBuilding> result = DBConnection.Connection.Query<DBUpgradeableBuilding>("SELECT * FROM UpgradeableBuildings WHERE MissionObjectHash = @MissionObjectHash", new
            {
                MissionObjectHash = upgradeableBuildings.GetMissionObjectHash()
            });
            Debug.Print("[Save Module] LOADING UPGRADEABLE BUILDING FROM DB (" + upgradeableBuildings.GetMissionObjectHash() + ") RESULT COUNT " + result.Count());
            if (result.Count() == 0) return null;
            return result.First();
        }
    }
}

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
    public class DBUpgradeableBuildingRepository
    {
        public static void Initialize() {
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
            if(GetUpgradeableBuilding(upgradeableBuildings) == null)
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

        public static IEnumerable<DBUpgradeableBuilding> GetAllUpgradeableBuildings() {
            Debug.Print("[Save Module] GET ALL UPGRADEABLE BUILDINGS");
            return DBConnection.Connection.Query<DBUpgradeableBuilding>("SELECT * FROM UpgradeableBuildings");
        }

        public static DBUpgradeableBuilding GetUpgradeableBuilding(PE_UpgradeableBuildings upgradeableBuildings)
        {
            Debug.Print("[Save Module] LOADING UPGRADEABLE BUILDING FROM DB (" +upgradeableBuildings.GetMissionObjectHash()+")");
             IEnumerable<DBUpgradeableBuilding> result = DBConnection.Connection.Query<DBUpgradeableBuilding>("SELECT * FROM UpgradeableBuildings WHERE MissionObjectHash = @MissionObjectHash", new { 
                MissionObjectHash = upgradeableBuildings.GetMissionObjectHash()
            });
            Debug.Print("[Save Module] LOADING UPGRADEABLE BUILDING FROM DB (" + upgradeableBuildings .GetMissionObjectHash()+ ") RESULT COUNT "+result.Count());
            if (result.Count() == 0) return null;
            return result.First();
        }
    }
}

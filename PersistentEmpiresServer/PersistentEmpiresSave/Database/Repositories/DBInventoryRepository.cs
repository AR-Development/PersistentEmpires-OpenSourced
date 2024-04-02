using Dapper;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBInventoryRepository
    {
        public static void Initialize() {
            SaveSystemBehavior.OnGetAllInventories += GetAllInventories;
            SaveSystemBehavior.OnGetOrCreateInventory += GetOrCreateInventory;
            SaveSystemBehavior.OnGetOrCreatePlayerInventory += GetOrCreatePlayerInventory;
            SaveSystemBehavior.OnCreateOrSaveInventory += CreateOrSaveInventory;
            SaveSystemBehavior.OnCreateOrSavePlayerInventory += CreateOrSavePlayerInventory;
        }
        private static DBInventory CreateDBInventoryFromPlayer(NetworkCommunicator networkCommunicator)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkCommunicator.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return null; // Shouldn't be the case
            Debug.Print("[Save Module] CREATING DBInventory FOR PLAYER "+ (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
            return new DBInventory
            {
                InventoryId = "PlayerInventory",
                InventorySerialized = persistentEmpireRepresentative == null ? "||||" : persistentEmpireRepresentative.GetInventory().Serialize(),
                IsPlayerInventory = true,
                PlayerId = networkCommunicator.VirtualPlayer.Id.ToString()
            };
        }

        private static DBInventory CreateDBInventoryFromId(string inventoryId)
        {
            PlayerInventoryComponent playerInventoryComponent = Mission.Current.GetMissionBehavior<PlayerInventoryComponent>();
            Debug.Print("[Save Module] CREATING DBInventory FOR INVENTORY " + inventoryId + " IS REGISTERED ? " + playerInventoryComponent.CustomInventories.ContainsKey(inventoryId));
            return new DBInventory
            {
                InventoryId = inventoryId,
                InventorySerialized = playerInventoryComponent.CustomInventories[inventoryId].Serialize(),
                IsPlayerInventory = false,
                PlayerId = null
            };
        }

        public static IEnumerable<DBInventory> GetAllInventories()
        {
            Debug.Print("[Save Module] LOADING ALL INVENTORIES FROM DB");
            return DBConnection.Connection.Query<DBInventory>("SELECT * FROM Inventories WHERE IsPlayerInventory = 0");
        }

        public static DBInventory GetInventory(string inventoryId)
        {
            Debug.Print("[Save Module] LOADING INVENTORY "+ inventoryId+" FROM DB");
            IEnumerable<DBInventory> results = DBConnection.Connection.Query<DBInventory>("SELECT * FROM Inventories WHERE InventoryId = @InventoryId", new { InventoryId = inventoryId });
            Debug.Print("[Save Module] LOADING INVENTORY " + inventoryId + " RESULT COUNT IS " + results.Count());
            if (results.Count() == 0) return null;
            return results.First();
        }

        public static DBInventory GetPlayerInventory(NetworkCommunicator networkCommunicator)
        {
            Debug.Print("[Save Module] LOADING INVENTORY FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!") + " FROM DB");
            IEnumerable<DBInventory> results = DBConnection.Connection.Query<DBInventory>("SELECT * FROM Inventories WHERE IsPlayerInventory = 1 and PlayerId = @PlayerId", new { PlayerId = networkCommunicator.VirtualPlayer.Id.ToString() });
            Debug.Print("[Save Module] LOADING INVENTORY FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!") + " RESULT COUNT IS "+ results.Count());
            if (results.Count() == 0) return null;
            return results.First();
        }
        public static DBInventory GetOrCreatePlayerInventory(NetworkCommunicator networkCommunicator, out bool created)
        {
            
            created = false;
            DBInventory dbInventory = GetPlayerInventory(networkCommunicator);
            if (dbInventory == null)
            {
                created = true;
                dbInventory = CreatePlayerInventory(networkCommunicator);
            }
            return dbInventory;
        }
        public static DBInventory CreatePlayerInventory(NetworkCommunicator networkCommunicator)
        {
            DBInventory dbInventory = CreateDBInventoryFromPlayer(networkCommunicator);
            if (dbInventory == null) return dbInventory;
            Debug.Print("[Save Module] CREATING INVENTORY FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
            string insertQuery = "INSERT INTO Inventories (PlayerId, InventoryId, IsPlayerInventory, InventorySerialized) VALUES (@PlayerId, @InventoryId, @IsPlayerInventory, @InventorySerialized)";
            DBConnection.Connection.Execute(insertQuery, dbInventory);
            Debug.Print("[Save Module] CREATED INVENTORY FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
            return dbInventory;
        }
        public static DBInventory SavePlayerInventory(NetworkCommunicator networkCommunicator)
        {
            DBInventory dbInventory = CreateDBInventoryFromPlayer(networkCommunicator);
            if (dbInventory == null) return dbInventory;
            Debug.Print("[Save Module] UPDATING INVENTORY FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
            string updateQuery = "UPDATE Inventories SET InventorySerialized = @InventorySerialized WHERE PlayerId = @PlayerId";
            DBConnection.Connection.Execute(updateQuery, dbInventory);
            Debug.Print("[Save Module] UPDATED INVENTORY FOR PLAYER " + (networkCommunicator != null ? networkCommunicator.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
            return dbInventory;
        }
        public static DBInventory CreateOrSavePlayerInventory(NetworkCommunicator networkCommunicator)
        {
            if (GetPlayerInventory(networkCommunicator) == null)
            {
                return CreatePlayerInventory(networkCommunicator);
            }
            return SavePlayerInventory(networkCommunicator);
        }

        public static DBInventory GetOrCreateInventory(string inventoryId)
        {
            DBInventory dBInventory = GetInventory(inventoryId);
            if (dBInventory == null)
            {
                dBInventory = CreateInventory(inventoryId);
            }
            return dBInventory;
        }

        public static DBInventory CreateOrSaveInventory(string inventoryId)
        {
            DBInventory dbInventory = GetInventory(inventoryId);
            if (dbInventory == null)
            {
                return CreateInventory(inventoryId);
            }
            return SaveInventory(inventoryId);
        }

        public static DBInventory CreateInventory(string inventoryId)
        {
            DBInventory dbInventory = CreateDBInventoryFromId(inventoryId);
            PlayerInventoryComponent playerInventoryComponent = Mission.Current.GetMissionBehavior<PlayerInventoryComponent>();
            Debug.Print("[Save Module] CREATING RECORD FOR INVENTORY " + inventoryId + " IS REGISTERED ? " + playerInventoryComponent.CustomInventories.ContainsKey(inventoryId));
            string insertQuery = "INSERT INTO Inventories (InventoryId, IsPlayerInventory, InventorySerialized) VALUES (@InventoryId, 0, @InventorySerialized)";
            DBConnection.Connection.Execute(insertQuery, dbInventory);
            Debug.Print("[Save Module] CREATED RECORD FOR INVENTORY " + inventoryId + " IS REGISTERED ? " + playerInventoryComponent.CustomInventories.ContainsKey(inventoryId));
            return dbInventory;
        }
        public static DBInventory SaveInventory(string inventoryId)
        {
            DBInventory dbInventory = CreateDBInventoryFromId(inventoryId);
            PlayerInventoryComponent playerInventoryComponent = Mission.Current.GetMissionBehavior<PlayerInventoryComponent>();
            Debug.Print("[Save Module] UPDATING RECORD FOR INVENTORY " + inventoryId + " IS REGISTERED ? " + playerInventoryComponent.CustomInventories.ContainsKey(inventoryId));
            string updateQuery = "UPDATE Inventories SET InventorySerialized = @InventorySerialized WHERE InventoryId = @InventoryId";
            Debug.Print("[Save Module] UPDATED RECORD FOR INVENTORY " + inventoryId + " IS REGISTERED ? " + playerInventoryComponent.CustomInventories.ContainsKey(inventoryId));
            DBConnection.Connection.Execute(updateQuery, dbInventory);
            return dbInventory;
        }
    }
}

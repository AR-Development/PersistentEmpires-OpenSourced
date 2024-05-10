using Dapper;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBPlayerRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnCreateOrSavePlayer += CreateOrSavePlayer;
            SaveSystemBehavior.OnGetOrCreatePlayer += GetOrCreatePlayer;
            SaveSystemBehavior.OnDiscordRegister += OnDiscordRegister;
            SaveSystemBehavior.OnGetPlayer += OnGetPlayer;
            SaveSystemBehavior.OnPlayerUpdateCustomName += OnPlayerUpdateCustomName;
        }

        private static bool OnPlayerUpdateCustomName(NetworkCommunicator peer, string customName)
        {
            string fetchFirst = "SELECT CustomName FROM Players WHERE LOWER(CustomName) = @customName OR LOWER(Name) = @customName";
            IEnumerable<DBPlayer> players = DBConnection.Connection.Query<DBPlayer>(fetchFirst, new
            {
                CustomName = customName.ToLower()
            });
            if (players.Count() > 0) return false;

            string updateQuery = "UPDATE Players SET CustomName = @customName WHERE PlayerId = @PlayerId";
            DBConnection.Connection.Execute(updateQuery, new
            {
                CustomName = customName,
                PlayerId = peer.VirtualPlayer.Id.ToString()
            });
            IEnumerable<DBPlayerName> playerNames = DBConnection.Connection.Query<DBPlayerName>("SELECT PlayerName FROM PlayerNames WHERE PlayerName = @PlayerName", new
            {
                PlayerName = customName
            });
            if (playerNames.Count() == 0)
            {
                string insertSql = "INSERT INTO PlayerNames (PlayerName, PlayerId) VALUES (@PlayerName, @PlayerId)";
                DBConnection.Connection.Execute(insertSql, new DBPlayerName()
                {
                    PlayerId = peer.VirtualPlayer.Id.ToString(),
                    PlayerName = customName
                });
            }
            return true;
        }

        private static DBPlayer OnGetPlayer(string playerId)
        {
            IEnumerable<DBPlayer> getQuery = DBConnection.Connection.Query<DBPlayer>("SELECT * FROM Players WHERE PlayerId = @PlayerId", new { PlayerId = playerId });
            if (getQuery.Count() == 0) return null;
            return getQuery.First();
        }

        private static void OnDiscordRegister(NetworkCommunicator player, string id)
        {
            bool created = false;
            DBPlayer dbplayer = GetOrCreatePlayer(player, out created);

            string updateQuery = "UPDATE Players SET DiscordId = @DiscordId WHERE PlayerId = @PlayerId";
            DBConnection.Connection.Execute(updateQuery, new
            {
                DiscordId = id,
                PlayerId = dbplayer.PlayerId
            });
        }

        private static DBPlayer CreateDBPlayer(NetworkCommunicator peer)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            Debug.Print("[Save Module] CREATING DBPlayer FOR PLAYER " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!") + " IS CONTROLLEDAGENT NULL ? " + (peer.ControlledAgent == null) + " IS REPRESENTATIVE NULL ? " + (persistentEmpireRepresentative == null));
            DBPlayer dbPlayer = new DBPlayer
            {
                PlayerId = peer.VirtualPlayer.Id.ToString(),
                Name = peer.VirtualPlayer.UserName,
                Hunger = persistentEmpireRepresentative == null ? 10 : persistentEmpireRepresentative.GetHunger(),
                FactionIndex = persistentEmpireRepresentative == null ? 0 : persistentEmpireRepresentative.GetFactionIndex(),
                Health = peer.ControlledAgent == null ? 100 : (int)peer.ControlledAgent.Health,
                Money = persistentEmpireRepresentative == null ? 100 : persistentEmpireRepresentative.Gold,
                Class = persistentEmpireRepresentative == null ? "pe_peasant" : persistentEmpireRepresentative.GetClassId(),
                PosX = peer.ControlledAgent == null || !peer.ControlledAgent.IsActive() ? 0 : peer.ControlledAgent.Position.X,
                PosY = peer.ControlledAgent == null || !peer.ControlledAgent.IsActive() ? 0 : peer.ControlledAgent.Position.Y,
                PosZ = peer.ControlledAgent == null || !peer.ControlledAgent.IsActive() ? 0 : peer.ControlledAgent.Position.Z,
            };

            if (peer.ControlledAgent != null && peer.ControlledAgent.IsActive() && persistentEmpireRepresentative != null)
            {
                MissionEquipment wieldedEquipment = peer.ControlledAgent.Equipment;
                Equipment armors = peer.ControlledAgent.SpawnEquipment;

                if (!wieldedEquipment[EquipmentIndex.Weapon0].IsEmpty)
                {
                    dbPlayer.Equipment_0 = wieldedEquipment[EquipmentIndex.Weapon0].Item.StringId;
                    if (wieldedEquipment[EquipmentIndex.Weapon0].IsAnyConsumable() || wieldedEquipment[EquipmentIndex.Weapon0].Item.Type == ItemObject.ItemTypeEnum.Crossbow || wieldedEquipment[EquipmentIndex.Weapon0].Item.Type == ItemObject.ItemTypeEnum.Musket)
                    {
                        dbPlayer.Ammo_0 = (int)wieldedEquipment[EquipmentIndex.Weapon0].Amount;
                    }
                    else
                    {
                        dbPlayer.Ammo_0 = ItemHelper.GetMaximumAmmo(wieldedEquipment[EquipmentIndex.Weapon0].Item);
                    }
                }
                if (!wieldedEquipment[EquipmentIndex.Weapon1].IsEmpty)
                {
                    dbPlayer.Equipment_1 = wieldedEquipment[EquipmentIndex.Weapon1].Item.StringId;

                    if (wieldedEquipment[EquipmentIndex.Weapon1].IsAnyConsumable() || wieldedEquipment[EquipmentIndex.Weapon1].Item.Type == ItemObject.ItemTypeEnum.Crossbow || wieldedEquipment[EquipmentIndex.Weapon1].Item.Type == ItemObject.ItemTypeEnum.Musket)
                    {
                        dbPlayer.Ammo_1 = (int)wieldedEquipment[EquipmentIndex.Weapon1].Amount;
                    }
                    else
                    {
                        dbPlayer.Ammo_1 = ItemHelper.GetMaximumAmmo(wieldedEquipment[EquipmentIndex.Weapon1].Item);
                    }
                }
                if (!wieldedEquipment[EquipmentIndex.Weapon2].IsEmpty)
                {
                    dbPlayer.Equipment_2 = wieldedEquipment[EquipmentIndex.Weapon2].Item.StringId;
                    if (wieldedEquipment[EquipmentIndex.Weapon2].IsAnyConsumable() || wieldedEquipment[EquipmentIndex.Weapon2].Item.Type == ItemObject.ItemTypeEnum.Crossbow || wieldedEquipment[EquipmentIndex.Weapon2].Item.Type == ItemObject.ItemTypeEnum.Musket)
                    {
                        dbPlayer.Ammo_2 = (int)wieldedEquipment[EquipmentIndex.Weapon2].Amount;
                    }
                    else
                    {
                        dbPlayer.Ammo_2 = ItemHelper.GetMaximumAmmo(wieldedEquipment[EquipmentIndex.Weapon2].Item);
                    }
                }
                if (!wieldedEquipment[EquipmentIndex.Weapon3].IsEmpty)
                {
                    dbPlayer.Equipment_3 = wieldedEquipment[EquipmentIndex.Weapon3].Item.StringId;
                    if (wieldedEquipment[EquipmentIndex.Weapon3].IsAnyConsumable() || wieldedEquipment[EquipmentIndex.Weapon3].Item.Type == ItemObject.ItemTypeEnum.Crossbow || wieldedEquipment[EquipmentIndex.Weapon3].Item.Type == ItemObject.ItemTypeEnum.Musket)
                    {
                        dbPlayer.Ammo_3 = (int)wieldedEquipment[EquipmentIndex.Weapon3].Amount;
                    }
                    else
                    {
                        dbPlayer.Ammo_3 = ItemHelper.GetMaximumAmmo(wieldedEquipment[EquipmentIndex.Weapon3].Item);
                    }
                }

                if (!armors[EquipmentIndex.Head].IsEmpty)
                {
                    dbPlayer.Armor_Head = armors[EquipmentIndex.Head].Item.StringId;
                }
                if (!armors[EquipmentIndex.Body].IsEmpty)
                {
                    dbPlayer.Armor_Body = armors[EquipmentIndex.Body].Item.StringId;
                }
                if (!armors[EquipmentIndex.Cape].IsEmpty)
                {
                    dbPlayer.Armor_Cape = armors[EquipmentIndex.Cape].Item.StringId;
                }
                if (!armors[EquipmentIndex.Leg].IsEmpty)
                {
                    dbPlayer.Armor_Leg = armors[EquipmentIndex.Leg].Item.StringId;
                }
                if (!armors[EquipmentIndex.Gloves].IsEmpty)
                {
                    dbPlayer.Armor_Gloves = armors[EquipmentIndex.Gloves].Item.StringId;
                }

                if (peer.ControlledAgent.MountAgent != null)
                {
                    EquipmentElement horse = peer.ControlledAgent.MountAgent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot];
                    EquipmentElement horseHarness = peer.ControlledAgent.MountAgent.SpawnEquipment[EquipmentIndex.HorseHarness];
                    dbPlayer.Horse = horse.Item == null ? null : horse.Item.StringId;
                    dbPlayer.HorseHarness = horseHarness.Item == null ? null : horseHarness.Item.StringId;
                }
            }
            return dbPlayer;
        }

        public static IEnumerable<DBPlayer> GetPlayer(NetworkCommunicator peer)
        {
            Debug.Print("[Save Module] LOAD PLAYER FROM DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
            IEnumerable<DBPlayer> result = DBConnection.Connection.Query<DBPlayer>("SELECT * FROM Players WHERE PlayerId = @PlayerId", new { PlayerId = peer.VirtualPlayer.Id.ToString() });
            Debug.Print("[Save Module] LOAD PLAYER FROM DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!") + " RESULT COUNT : " + result.Count());
            return result;
        }

        public static IEnumerable<DBPlayer> GetPlayerFromId(string playerId)
        {
            return DBConnection.Connection.Query<DBPlayer>("SELECT * FROM Players WHERE PlayerId = @PlayerId", new { PlayerId = playerId });
        }
        public static DBPlayer GetOrCreatePlayer(NetworkCommunicator peer, out bool created)
        {
            // DBConnection.Connection.Query<DBPlayer>();
            IEnumerable<DBPlayer> getQuery = DBPlayerRepository.GetPlayer(peer);
            created = false;
            if (getQuery.Count() == 0)
            {
                created = true;
                DBPlayerRepository.CreatePlayer(peer);
                getQuery = DBPlayerRepository.GetPlayer(peer);
            }
            return getQuery.First();
        }

        public static DBPlayer CreateOrSavePlayer(NetworkCommunicator peer)
        {
            if (GetPlayer(peer).Count() > 0)
            {
                return SavePlayer(peer);
            }
            else
            {
                return CreatePlayer(peer);
            }
        }
        public static DBPlayer SavePlayer(NetworkCommunicator peer)
        {
            Debug.Print("[Save Module] SAVING PLAYER TO DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));


            string updateQuery = "UPDATE Players SET Name = @Name, Hunger = @Hunger, Health = @Health, Money = @Money, Horse = @Horse, HorseHarness = @HorseHarness, Equipment_0 = @Equipment_0, Equipment_1 = @Equipment_1, Equipment_2 = @Equipment_2, Equipment_3 = @Equipment_3, Armor_Head = @Armor_Head, Armor_Body = @Armor_Body, Armor_Leg = @Armor_Leg, Armor_Gloves = @Armor_Gloves, Armor_Cape = @Armor_Cape, PosX = @PosX, PosY = @PosY, PosZ = @PosZ, FactionIndex = @FactionIndex, Class = @Class, Ammo_0 = @Ammo_0, Ammo_1 = @Ammo_1, Ammo_2 = @Ammo_2, Ammo_3 = @Ammo_3 WHERE PlayerId = @PlayerId";
            DBPlayer player = CreateDBPlayer(peer);
            if (player.FactionIndex == -1) player.FactionIndex = 0;
            DBConnection.Connection.Execute(updateQuery, player);
            Debug.Print("[Save Module] SAVED PLAYER TO DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
            return player;
        }
        public static DBPlayer CreatePlayer(NetworkCommunicator peer)
        {
            Debug.Print("[Save Module] CREATING PLAYER TO DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
            Debug.Print("Creating DBPlayer for " + peer.UserName);
            string insertQuery = "INSERT INTO Players (PlayerId, Name, Hunger, Health, Money, Horse, HorseHarness, Equipment_0, Equipment_1, Equipment_2, Equipment_3, Armor_Head, Armor_Body, Armor_Leg, Armor_Gloves, Armor_Cape, PosX, PosY, PosZ, FactionIndex, Class, Ammo_0, Ammo_1, Ammo_2, Ammo_3) VALUES (@PlayerId, @Name, @Hunger, @Health, @Money, @Horse, @HorseHarness, @Equipment_0, @Equipment_1, @Equipment_2, @Equipment_3, @Armor_Head, @Armor_Body, @Armor_Leg, @Armor_Gloves, @Armor_Cape, @PosX, @PosY, @PosZ, @FactionIndex, @Class, @Ammo_0, @Ammo_1, @Ammo_2, @Ammo_3)";
            DBPlayer player = CreateDBPlayer(peer);
            if (player.FactionIndex == -1) player.FactionIndex = 0;
            DBConnection.Connection.Execute(insertQuery, player);
            Debug.Print("[Save Module] CREATED PLAYER TO DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));

            return player;
        }

        public static void OnUpdateCustomName(NetworkCommunicator player, string customName)
        {
            string updateQuery = "UPDATE Players SET CustomName = @CustomName WHERE PlayerId = @PlayerId";
            bool created;
            DBPlayer dbplayer = GetOrCreatePlayer(player, out created);

            DBConnection.Connection.Execute(updateQuery, new
            {
                CustomName = customName,
                PlayerId = dbplayer.PlayerId
            });
        }
    }
}

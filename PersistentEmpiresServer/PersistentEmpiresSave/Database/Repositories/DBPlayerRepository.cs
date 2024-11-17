﻿using Dapper;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresSave.Database.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;
using static PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors.SaveSystemBehavior;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBPlayerRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnCreateOrSavePlayer += UpsertPlayer;
            SaveSystemBehavior.OnCreateOrSavePlayers += UpsertPlayers;
            SaveSystemBehavior.OnGetOrCreatePlayer += GetOrCreatePlayer;
            SaveSystemBehavior.OnDiscordRegister += OnDiscordRegister;
            SaveSystemBehavior.OnGetPlayer += OnGetPlayer;
            SaveSystemBehavior.OnPlayerUpdateCustomName += OnPlayerUpdateCustomName;
            SaveSystemBehavior.OnPlayerUpdateWoundedUntil += OnPlayerUpdateWoundedUntil;
            SaveSystemBehavior.OnSaveDefaultsForNewPlayer += OnSaveDefaultsForNewPlayer;
            SaveSystemBehavior.OnGetWoundedUntil += OnGetWoundedUntil;
        }

        private static bool OnPlayerUpdateCustomName(NetworkCommunicator peer, string customName)
        {
            string fetchFirst = "SELECT CustomName FROM Players WHERE LOWER(CustomName) = @customName OR LOWER(Name) = @customName";
            IEnumerable<DBPlayer> players = DBConnection.Connection.Query<DBPlayer>(fetchFirst, new
            {
                CustomName = customName.EncodeSpecialMariaDbChars().ToLower()
            });
            if (players.Count() > 0) return false;

            string oldName = "SELECT CustomName, Name FROM Players WHERE PlayerId = @PlayerId";
            IEnumerable<DBPlayer> playerOldNameResult = DBConnection.Connection.Query<DBPlayer>(oldName, new
            {
                PlayerId = peer.VirtualPlayer.ToPlayerId()
            });
            var playerOldName = playerOldNameResult.FirstOrDefault().CustomName;
            if(string.IsNullOrEmpty(playerOldName))
            {
                playerOldName = playerOldNameResult.FirstOrDefault().Name;
            }

            DBInventoryRepository.UpdateInventoryId($"{peer.VirtualPlayer.Id.ToString()}_{playerOldName.EncodeSpecialMariaDbChars()}", $"{peer.VirtualPlayer.Id.ToString()}_{customName.EncodeSpecialMariaDbChars()}");

            string updateQuery = "UPDATE Players SET CustomName = @customName, PlayerId = @PlayerId WHERE PlayerId = @OldPlayerId";
            DBConnection.Connection.Execute(updateQuery, new
            {
                CustomName = customName.EncodeSpecialMariaDbChars(),
                PlayerId = $"{peer.VirtualPlayer.Id.ToString()}_{customName.EncodeSpecialMariaDbChars()}",
                OldPlayerId = $"{peer.VirtualPlayer.Id.ToString()}_{playerOldName.EncodeSpecialMariaDbChars()}",
            });
            
            IEnumerable<DBPlayerName> playerNames = DBConnection.Connection.Query<DBPlayerName>("SELECT PlayerName FROM PlayerNames WHERE PlayerName = @PlayerName", new
            {
                PlayerName = customName.EncodeSpecialMariaDbChars()
            });
            if (playerNames.Count() == 0)
            {
                string insertSql = "INSERT INTO PlayerNames (PlayerName, PlayerId) VALUES (@PlayerName, @PlayerId)";
                DBConnection.Connection.Execute(insertSql, new DBPlayerName()
                {
                    PlayerId = peer.VirtualPlayer.Id.ToString(),
                    PlayerName = customName.EncodeSpecialMariaDbChars()
                });
            }
            return true;
        }

        private static void OnPlayerUpdateWoundedUntil(NetworkCommunicator peer, long woundedUntil)
        {
            bool created = false;
            DBPlayer dbplayer = GetOrCreatePlayer(peer, out created);

            string updateQuery = "UPDATE Players SET WoundedUntil = @WoundedUntil WHERE PlayerId = @PlayerId";
            DBConnection.Connection.Execute(updateQuery, new
            {
                WoundedUntil = woundedUntil,
                PlayerId = dbplayer.PlayerId
            });
        }

        private static void OnSaveDefaultsForNewPlayer(NetworkCommunicator player, Equipment equipment)
        {
            Debug.Print($"[Save Module] OnSaveDefaultsForNewPlayer {player.VirtualPlayer?.Id} ITEMS TO DB");
            string query = @"
        INSERT INTO Players (PlayerId, Name, Hunger, Health, Money, Horse, HorseHarness, Equipment_0, Equipment_1, Equipment_2, Equipment_3, Armor_Head, Armor_Body, Armor_Leg, Armor_Gloves, Armor_Cape, PosX, PosY, PosZ, FactionIndex, Class, Ammo_0, Ammo_1, Ammo_2, Ammo_3, WoundedUntil)
        VALUES ";
            var dbPlayer = CreateDBPlayer(player, equipment);
            if (dbPlayer.FactionIndex == -1) dbPlayer.FactionIndex = 0;
            query += $"('{dbPlayer.PlayerId}', '{dbPlayer.Name.EncodeSpecialMariaDbChars()}', {dbPlayer.Hunger}, {dbPlayer.Health}, {dbPlayer.Money}, '{(string.IsNullOrEmpty(dbPlayer.Horse) ? "null" : dbPlayer.Horse)}', '{(string.IsNullOrEmpty(dbPlayer.HorseHarness) ? "null" : dbPlayer.HorseHarness)}', '{dbPlayer.Equipment_0}', '{dbPlayer.Equipment_1}', '{dbPlayer.Equipment_2}', '{dbPlayer.Equipment_3}', '{dbPlayer.Armor_Head}', '{dbPlayer.Armor_Body}', '{dbPlayer.Armor_Leg}', '{dbPlayer.Armor_Gloves}', '{dbPlayer.Armor_Cape}', {dbPlayer.PosX}, {dbPlayer.PosY}, {dbPlayer.PosZ}, {dbPlayer.FactionIndex}, '{dbPlayer.Class}', {dbPlayer.Ammo_0}, {dbPlayer.Ammo_1}, {dbPlayer.Ammo_2}, {dbPlayer.Ammo_3}, {(dbPlayer.WoundedUntil.HasValue ? dbPlayer.WoundedUntil.Value.ToString() : "NULL")}),";
            // remove last ","
            query = query.TrimEnd(',');
            query += @" 
        ON DUPLICATE KEY UPDATE
        Name = VALUES(Name), Hunger = VALUES(Hunger), Health = VALUES(Health), Money = VALUES(Money), Horse = VALUES(Horse), HorseHarness = VALUES(HorseHarness), Equipment_0 = VALUES(Equipment_0), Equipment_1 = VALUES(Equipment_1), Equipment_2 = VALUES(Equipment_2), Equipment_3 = VALUES(Equipment_3), Armor_Head = VALUES(Armor_Head), Armor_Body = VALUES(Armor_Body), Armor_Leg = VALUES(Armor_Leg), Armor_Gloves = VALUES(Armor_Gloves), Armor_Cape = VALUES(Armor_Cape), PosX = VALUES(PosX), PosY = VALUES(PosY), PosZ = VALUES(PosZ), FactionIndex = VALUES(FactionIndex), Class = VALUES(Class), Ammo_0 = VALUES(Ammo_0), Ammo_1 = VALUES(Ammo_1), Ammo_2 = VALUES(Ammo_2), Ammo_3 = VALUES(Ammo_3), WoundedUntil = VALUES(WoundedUntil)";
            DBConnection.Connection.Execute(query);
        }

        private static long? OnGetWoundedUntil(NetworkCommunicator player)
        {
            IEnumerable<long?> getQuery = DBConnection.Connection.Query<long?>("SELECT WoundedUntil FROM Players WHERE PlayerId = @PlayerId",
                new { PlayerId = player.VirtualPlayer?.Id.ToString() });
            if (getQuery.Count() == 0) return null;
            return getQuery.First();
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
                PlayerId = peer.VirtualPlayer.ToPlayerId(),
                Name = peer.VirtualPlayer.UserName.EncodeSpecialMariaDbChars(),
                Hunger = persistentEmpireRepresentative?.GetHunger() ?? 10,
                FactionIndex = persistentEmpireRepresentative?.GetFactionIndex() ?? 0,
                Health = (int)(peer.ControlledAgent?.Health ?? 100),
                Money = persistentEmpireRepresentative?.Gold ?? 100,
                Class = persistentEmpireRepresentative?.GetClassId() ?? PersistentEmpireBehavior.DefaultClass,
                PosX = peer.ControlledAgent?.IsActive() == true ? peer.ControlledAgent.Position.X : 0,
                PosY = peer.ControlledAgent?.IsActive() == true ? peer.ControlledAgent.Position.Y : 0,
                PosZ = peer.ControlledAgent?.IsActive() == true ? peer.ControlledAgent.Position.Z : 0,
                WoundedUntil = persistentEmpireRepresentative.GetWoundedUntil(),
            };

            if (peer.ControlledAgent?.IsActive() == true && persistentEmpireRepresentative != null)
            {
                MissionEquipment wieldedEquipment = peer.ControlledAgent.Equipment;
                Equipment armors = peer.ControlledAgent.SpawnEquipment;

                for (int i = 0; i < 4; i++)
                {
                    if (!wieldedEquipment[i].IsEmpty)
                    {
                        switch (i)
                        {
                            case 0:
                                dbPlayer.Equipment_0 = wieldedEquipment[i].Item.StringId;
                                dbPlayer.Ammo_0 = wieldedEquipment[i].IsAnyConsumable() || wieldedEquipment[i].Item.Type == ItemObject.ItemTypeEnum.Crossbow || wieldedEquipment[i].Item.Type == ItemObject.ItemTypeEnum.Musket
                                    ? (int)wieldedEquipment[i].Amount
                                    : ItemHelper.GetMaximumAmmo(wieldedEquipment[i].Item);
                                break;
                            case 1:
                                dbPlayer.Equipment_1 = wieldedEquipment[i].Item.StringId;
                                dbPlayer.Ammo_1 = wieldedEquipment[i].IsAnyConsumable() || wieldedEquipment[i].Item.Type == ItemObject.ItemTypeEnum.Crossbow || wieldedEquipment[i].Item.Type == ItemObject.ItemTypeEnum.Musket
                                    ? (int)wieldedEquipment[i].Amount
                                    : ItemHelper.GetMaximumAmmo(wieldedEquipment[i].Item);
                                break;
                            case 2:
                                dbPlayer.Equipment_2 = wieldedEquipment[i].Item.StringId;
                                dbPlayer.Ammo_2 = wieldedEquipment[i].IsAnyConsumable() || wieldedEquipment[i].Item.Type == ItemObject.ItemTypeEnum.Crossbow || wieldedEquipment[i].Item.Type == ItemObject.ItemTypeEnum.Musket
                                    ? (int)wieldedEquipment[i].Amount
                                    : ItemHelper.GetMaximumAmmo(wieldedEquipment[i].Item);
                                break;
                            case 3:
                                dbPlayer.Equipment_3 = wieldedEquipment[i].Item.StringId;
                                dbPlayer.Ammo_3 = wieldedEquipment[i].IsAnyConsumable() || wieldedEquipment[i].Item.Type == ItemObject.ItemTypeEnum.Crossbow || wieldedEquipment[i].Item.Type == ItemObject.ItemTypeEnum.Musket
                                    ? (int)wieldedEquipment[i].Amount
                                    : ItemHelper.GetMaximumAmmo(wieldedEquipment[i].Item);
                                break;
                        }
                    }
                }

                dbPlayer.Armor_Head = !armors[EquipmentIndex.Head].IsEmpty ? armors[EquipmentIndex.Head].Item.StringId : null;
                dbPlayer.Armor_Body = !armors[EquipmentIndex.Body].IsEmpty ? armors[EquipmentIndex.Body].Item.StringId : null;
                dbPlayer.Armor_Cape = !armors[EquipmentIndex.Cape].IsEmpty ? armors[EquipmentIndex.Cape].Item.StringId : null;
                dbPlayer.Armor_Leg = !armors[EquipmentIndex.Leg].IsEmpty ? armors[EquipmentIndex.Leg].Item.StringId : null;
                dbPlayer.Armor_Gloves = !armors[EquipmentIndex.Gloves].IsEmpty ? armors[EquipmentIndex.Gloves].Item.StringId : null;

                if (peer.ControlledAgent.MountAgent != null)
                {
                    dbPlayer.Horse = peer.ControlledAgent.MountAgent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot].Item?.StringId;
                    dbPlayer.HorseHarness = peer.ControlledAgent.MountAgent.SpawnEquipment[EquipmentIndex.HorseHarness].Item?.StringId;
                }
            }

            return dbPlayer;
        }

        private static DBPlayer CreateDBPlayer(NetworkCommunicator peer, Equipment equipment)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            Debug.Print("[Save Module] CREATING DBPlayer FOR PLAYER " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!") + " IS CONTROLLEDAGENT NULL ? " + (peer.ControlledAgent == null) + " IS REPRESENTATIVE NULL ? " + (persistentEmpireRepresentative == null));

            DBPlayer dbPlayer = new DBPlayer
            {
                PlayerId = peer.VirtualPlayer.ToPlayerId(),
                Name = peer.VirtualPlayer.UserName.EncodeSpecialMariaDbChars(),
                Hunger = persistentEmpireRepresentative?.GetHunger() ?? 10,
                FactionIndex = persistentEmpireRepresentative?.GetFactionIndex() ?? 0,
                Health = (int)(peer.ControlledAgent?.Health ?? 100),
                Money = persistentEmpireRepresentative?.Gold ?? 100,
                Class = persistentEmpireRepresentative?.GetClassId() ?? PersistentEmpireBehavior.DefaultClass,
                PosX = peer.ControlledAgent?.IsActive() == true ? peer.ControlledAgent.Position.X : 0,
                PosY = peer.ControlledAgent?.IsActive() == true ? peer.ControlledAgent.Position.Y : 0,
                PosZ = peer.ControlledAgent?.IsActive() == true ? peer.ControlledAgent.Position.Z : 0,
                WoundedUntil = persistentEmpireRepresentative.GetWoundedUntil(),
            };

            if (!equipment[EquipmentIndex.Weapon0].IsEmpty)
            {
                dbPlayer.Equipment_0 = equipment[EquipmentIndex.Weapon0].Item.StringId;
                dbPlayer.Ammo_0 = ItemHelper.GetMaximumAmmo(equipment[EquipmentIndex.Weapon0].Item);
            }

            if (!equipment[EquipmentIndex.Weapon1].IsEmpty)
            {
                dbPlayer.Equipment_1 = equipment[EquipmentIndex.Weapon1].Item.StringId;
                dbPlayer.Ammo_1 = ItemHelper.GetMaximumAmmo(equipment[EquipmentIndex.Weapon1].Item);
            }

            if (!equipment[EquipmentIndex.Weapon2].IsEmpty)
            {
                dbPlayer.Equipment_2 = equipment[EquipmentIndex.Weapon2].Item.StringId;
                dbPlayer.Ammo_2 = ItemHelper.GetMaximumAmmo(equipment[EquipmentIndex.Weapon2].Item);
            }

            if (!equipment[EquipmentIndex.Weapon3].IsEmpty)
            {
                dbPlayer.Equipment_3 = equipment[EquipmentIndex.Weapon3].Item.StringId;
                dbPlayer.Ammo_3 = ItemHelper.GetMaximumAmmo(equipment[EquipmentIndex.Weapon3].Item);
            }

            if (!equipment[EquipmentIndex.Head].IsEmpty)
            {
                dbPlayer.Armor_Head = equipment[EquipmentIndex.Head].Item.StringId;
            }
            else
            {
                dbPlayer.Armor_Head = null;
            }

            if (!equipment[EquipmentIndex.Cape].IsEmpty)
            {
                dbPlayer.Armor_Cape = equipment[EquipmentIndex.Cape].Item.StringId;
            }
            else
            {
                dbPlayer.Armor_Cape = null;
            }

            if (!equipment[EquipmentIndex.Gloves].IsEmpty)
            {
                dbPlayer.Armor_Gloves = equipment[EquipmentIndex.Gloves].Item.StringId;
            }
            else
            {
                dbPlayer.Armor_Gloves = null;
            }

            if (!equipment[EquipmentIndex.Body].IsEmpty)
            {
                dbPlayer.Armor_Body = equipment[EquipmentIndex.Body].Item.StringId;
            }
            else
            {
                dbPlayer.Armor_Body = null;
            }

            if (!equipment[EquipmentIndex.Leg].IsEmpty)
            {
                dbPlayer.Armor_Leg = equipment[EquipmentIndex.Leg].Item.StringId;
            }
            else
            {
                dbPlayer.Armor_Leg = null;
            }

            if (!equipment[EquipmentIndex.Horse].IsEmpty)
            {
                dbPlayer.Horse = equipment[EquipmentIndex.Horse].Item.StringId;
            }

            if (!equipment[EquipmentIndex.HorseHarness].IsEmpty)
            {
                dbPlayer.HorseHarness = equipment[EquipmentIndex.HorseHarness].Item.StringId;
            }

            return dbPlayer;
        }

        public static IEnumerable<DBPlayer> GetPlayer(NetworkCommunicator peer)
        {
            Debug.Print("[Save Module] LOAD PLAYER FROM DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
            IEnumerable<DBPlayer> result = DBConnection.Connection.Query<DBPlayer>("SELECT * FROM Players WHERE PlayerId = @PlayerId", new { PlayerId = peer.VirtualPlayer.ToPlayerId() });
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

        public static DBPlayer UpsertPlayer(NetworkCommunicator player)
        {
            Debug.Print($"[Save Module] INSERT/UPDATE PLAYER {player.VirtualPlayer?.Id} ITEMS TO DB");
            string query = @"
        INSERT INTO Players (PlayerId, Name, Hunger, Health, Money, Horse, HorseHarness, Equipment_0, Equipment_1, Equipment_2, Equipment_3, Armor_Head, Armor_Body, Armor_Leg, Armor_Gloves, Armor_Cape, PosX, PosY, PosZ, FactionIndex, Class, Ammo_0, Ammo_1, Ammo_2, Ammo_3, WoundedUntil)
        VALUES ";
            var dbPlayer = CreateDBPlayer(player);
            if (dbPlayer.FactionIndex == -1) dbPlayer.FactionIndex = 0;
            query += $"('{dbPlayer.PlayerId}', '{dbPlayer.Name.EncodeSpecialMariaDbChars()}', {dbPlayer.Hunger}, {dbPlayer.Health}, {dbPlayer.Money}, '{(string.IsNullOrEmpty(dbPlayer.Horse) ? "null" : dbPlayer.Horse)}', '{(string.IsNullOrEmpty(dbPlayer.HorseHarness) ? "null" : dbPlayer.HorseHarness)}', '{dbPlayer.Equipment_0}', '{dbPlayer.Equipment_1}', '{dbPlayer.Equipment_2}', '{dbPlayer.Equipment_3}', '{dbPlayer.Armor_Head}', '{dbPlayer.Armor_Body}', '{dbPlayer.Armor_Leg}', '{dbPlayer.Armor_Gloves}', '{dbPlayer.Armor_Cape}', {dbPlayer.PosX}, {dbPlayer.PosY}, {dbPlayer.PosZ}, {dbPlayer.FactionIndex}, '{dbPlayer.Class}', {dbPlayer.Ammo_0}, {dbPlayer.Ammo_1}, {dbPlayer.Ammo_2}, {dbPlayer.Ammo_3}, {(dbPlayer.WoundedUntil.HasValue ? dbPlayer.WoundedUntil.Value.ToString() : "NULL")}),";
            // remove last ","
            query = query.TrimEnd(',');
            query += @" 
        ON DUPLICATE KEY UPDATE
        Name = VALUES(Name), Hunger = VALUES(Hunger), Health = VALUES(Health), Money = VALUES(Money), Horse = VALUES(Horse), HorseHarness = VALUES(HorseHarness), Equipment_0 = VALUES(Equipment_0), Equipment_1 = VALUES(Equipment_1), Equipment_2 = VALUES(Equipment_2), Equipment_3 = VALUES(Equipment_3), Armor_Head = VALUES(Armor_Head), Armor_Body = VALUES(Armor_Body), Armor_Leg = VALUES(Armor_Leg), Armor_Gloves = VALUES(Armor_Gloves), Armor_Cape = VALUES(Armor_Cape), PosX = VALUES(PosX), PosY = VALUES(PosY), PosZ = VALUES(PosZ), FactionIndex = VALUES(FactionIndex), Class = VALUES(Class), Ammo_0 = VALUES(Ammo_0), Ammo_1 = VALUES(Ammo_1), Ammo_2 = VALUES(Ammo_2), Ammo_3 = VALUES(Ammo_3), WoundedUntil = VALUES(WoundedUntil)";
            DBConnection.Connection.Execute(query);

            return dbPlayer;
        }

        public static void UpsertPlayers(List<NetworkCommunicator> players)
        {
            Debug.Print($"[Save Module] INSERT/UPDATE FOR {players.Count()} PLAYER ITEMS TO DB");
            if (players.Any())
            {
                string query = @"
        INSERT INTO Players (PlayerId, Name, Hunger, Health, Money, Horse, HorseHarness, Equipment_0, Equipment_1, Equipment_2, Equipment_3, Armor_Head, Armor_Body, Armor_Leg, Armor_Gloves, Armor_Cape, PosX, PosY, PosZ, FactionIndex, Class, Ammo_0, Ammo_1, Ammo_2, Ammo_3, WoundedUntil)
        VALUES ";
                foreach (var player in players)
                {
                    var dbPlayer = CreateDBPlayer(player);
                    if (dbPlayer.FactionIndex == -1) dbPlayer.FactionIndex = 0;
                    query += $"('{dbPlayer.PlayerId}', '{dbPlayer.Name.EncodeSpecialMariaDbChars()}', {dbPlayer.Hunger}, {dbPlayer.Health}, {dbPlayer.Money}, '{(string.IsNullOrEmpty(dbPlayer.Horse) ? "null" : dbPlayer.Horse)}', '{(string.IsNullOrEmpty(dbPlayer.HorseHarness) ? "null" : dbPlayer.HorseHarness)}', '{dbPlayer.Equipment_0}', '{dbPlayer.Equipment_1}', '{dbPlayer.Equipment_2}', '{dbPlayer.Equipment_3}', '{dbPlayer.Armor_Head}', '{dbPlayer.Armor_Body}', '{dbPlayer.Armor_Leg}', '{dbPlayer.Armor_Gloves}', '{dbPlayer.Armor_Cape}', {dbPlayer.PosX}, {dbPlayer.PosY}, {dbPlayer.PosZ}, {dbPlayer.FactionIndex}, '{dbPlayer.Class}', {dbPlayer.Ammo_0}, {dbPlayer.Ammo_1}, {dbPlayer.Ammo_2}, {dbPlayer.Ammo_3}, {(dbPlayer.WoundedUntil.HasValue ? dbPlayer.WoundedUntil.Value.ToString() : "NULL")}),";
                }
                // remove last ","
                query = query.TrimEnd(',');
                query += @" 
        ON DUPLICATE KEY UPDATE
        Name = VALUES(Name), Hunger = VALUES(Hunger), Health = VALUES(Health), Money = VALUES(Money), Horse = VALUES(Horse), HorseHarness = VALUES(HorseHarness), Equipment_0 = VALUES(Equipment_0), Equipment_1 = VALUES(Equipment_1), Equipment_2 = VALUES(Equipment_2), Equipment_3 = VALUES(Equipment_3), Armor_Head = VALUES(Armor_Head), Armor_Body = VALUES(Armor_Body), Armor_Leg = VALUES(Armor_Leg), Armor_Gloves = VALUES(Armor_Gloves), Armor_Cape = VALUES(Armor_Cape), PosX = VALUES(PosX), PosY = VALUES(PosY), PosZ = VALUES(PosZ), FactionIndex = VALUES(FactionIndex), Class = VALUES(Class), Ammo_0 = VALUES(Ammo_0), Ammo_1 = VALUES(Ammo_1), Ammo_2 = VALUES(Ammo_2), Ammo_3 = VALUES(Ammo_3), WoundedUntil = VALUES(WoundedUntil)";
                DBConnection.Connection.Execute(query);
            }
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

        public static void DeletePlayer(string playerId)
        {
            Debug.Print("DeletePlayer for " + playerId);
            DBConnection.Connection.Query<DBPlayer>("DELETE FROM Players WHERE PlayerId = @PlayerId", new { PlayerId = playerId });
        }
    }
}
﻿using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    class AutoSaveJob
    {
        public Queue<List<NetworkCommunicator>> toBeSaved = new Queue<List<NetworkCommunicator>>();
        public List<List<T>> ChunkBy<T>(List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
        public AutoSaveJob(List<NetworkCommunicator> peers)
        {
            List<List<NetworkCommunicator>> chunk = ChunkBy<NetworkCommunicator>(peers, 10);
            foreach (var c in chunk) toBeSaved.Enqueue(c);
        }

        public bool isCompleted()
        {
            return toBeSaved.IsEmpty();
        }

        public void doWork()
        {
            Debug.Print("** Persistent Empires Auto Save ** Saving 10 players", 0, Debug.DebugColor.Blue);
            List<NetworkCommunicator> currentBatch = toBeSaved.Dequeue();
            foreach (NetworkCommunicator player in currentBatch)
            {
                if (player.ControlledAgent != null && player.ControlledAgent.IsActive())
                {
                    SaveSystemBehavior.HandleCreateOrSavePlayer(player);
                    SaveSystemBehavior.HandleCreateOrSavePlayerInventory(player);
                }
            }
        }
    }
    public class SaveSystemBehavior : MissionNetwork
    {
        public long LastSaveAt = DateTimeOffset.Now.ToUnixTimeSeconds();
        public int SaveDuration = 600;
        private AutoSaveJob _currentAutoSaveJob;

        /* Events for handles */
        public delegate void StartMigration();
        /* Players */
        public delegate DBPlayer CreateOrSavePlayer(NetworkCommunicator peer);
        public delegate DBPlayer GetOrCreatePlayer(NetworkCommunicator peer, out bool created);
        public delegate DBPlayer GetPlayer(string playerId);
        public delegate bool UpdateCustomName(NetworkCommunicator peer, string customName);
        /* Inventories */
        public delegate IEnumerable<DBInventory> GetAllInventories();
        public delegate DBInventory GetOrCreatePlayerInventory(NetworkCommunicator networkCommunicator, out bool created);
        public delegate DBInventory CreateOrSavePlayerInventory(NetworkCommunicator networkCommunicator);
        public delegate DBInventory GetOrCreateInventory(string inventoryId);
        public delegate DBInventory CreateOrSaveInventory(string inventoryId);
        /* Factions */
        public delegate IEnumerable<DBFactions> GetFactions();
        public delegate DBFactions GetFaction(int factionIndex);
        public delegate DBFactions CreateOrSaveFaction(Faction faction, int factionIndex);
        /* Upgradeable Buildings */
        public delegate IEnumerable<DBUpgradeableBuilding> GetAllUpgradeableBuildings();
        public delegate DBUpgradeableBuilding GetUpgradeableBuilding(PE_UpgradeableBuildings upgradeableBuildings);
        public delegate DBUpgradeableBuilding CreateOrSaveUpgradebleBuilding(PE_UpgradeableBuildings upgradeableBuildings);
        /* Stockpile Markets */
        public delegate IEnumerable<DBStockpileMarket> GetAllStockpileMarkets();
        public delegate DBStockpileMarket GetStockpileMarket(PE_StockpileMarket stockpileMarket);
        public delegate DBStockpileMarket CreateOrSaveStockpileMarket(PE_StockpileMarket stockpileMarket);
        /* Horse Markets */
        public delegate IEnumerable<DBHorseMarket> GetAllHorseMarkets();
        public delegate DBHorseMarket GetHorseMarket(PE_HorseMarket horseMarket);
        public delegate DBHorseMarket CreateOrSaveHorseMarket(PE_HorseMarket horseMarket);

        public delegate void CreatePlayerNameIfNotExists(NetworkCommunicator player);
        public delegate void DiscordRegister(NetworkCommunicator player, string id);

        public delegate bool IsPlayerWhitelisted(string player);


        /*Castles*/
        public delegate IEnumerable<DBCastle> GetCastles();
        public delegate DBCastle GetCastle(int factionIndex);
        public delegate DBCastle CreateOrSaveCastle(int castleIndex, int factionIndex);

        public static event IsPlayerWhitelisted OnIsPlayerWhitelisted;

        public static event CreatePlayerNameIfNotExists OnCreatePlayerNameIfNotExists;

        public static event DiscordRegister OnDiscordRegister;
        public static event StartMigration OnStartMigration;
        /* Players */
        public static event CreateOrSavePlayer OnCreateOrSavePlayer;
        public static event GetOrCreatePlayer OnGetOrCreatePlayer;
        public static event GetPlayer OnGetPlayer;
        public static event UpdateCustomName OnPlayerUpdateCustomName;
        /* Inventories */
        public static event GetOrCreatePlayerInventory OnGetOrCreatePlayerInventory;
        public static event CreateOrSavePlayerInventory OnCreateOrSavePlayerInventory;
        public static event GetOrCreateInventory OnGetOrCreateInventory;
        public static event CreateOrSaveInventory OnCreateOrSaveInventory;
        public static event GetAllInventories OnGetAllInventories;
        /* Factions */
        public static event GetFactions OnGetFactions;
        public static event GetFaction OnGetFaction;
        public static event CreateOrSaveFaction OnCreateOrSaveFaction;
        /* Castles */
        public static event GetCastles OnGetCastles;
        public static event CreateOrSaveCastle OnCreateOrSaveCastle;


        /* Upgradeable Buildings */
        public static event GetAllUpgradeableBuildings OnGetAllUpgradeableBuildings;
        public static event GetUpgradeableBuilding OnGetUpgradeableBuilding;
        public static event CreateOrSaveUpgradebleBuilding OnCreateOrSaveUpgradebleBuilding;
        /* Stockpile Markets */
        public static event GetAllStockpileMarkets OnGetAllStockpileMarkets;
        public static event GetStockpileMarket OnGetStockpileMarket;
        public static event CreateOrSaveStockpileMarket OnCreateOrSaveStockpileMarket;
        /* Horse Markets */
        public static event GetAllHorseMarkets OnGetAllHorseMarkets;
        public static event GetHorseMarket OnGetHorseMarket;
        public static event CreateOrSaveHorseMarket OnCreateOrSaveHorseMarket;

        public static void LogQuery(string query)
        {
            // File.AppendAllText("save-logs.txt", query + "\n");
        }

        public static bool HandleIsPlayerWhitelisted(NetworkCommunicator player)
        {
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnIsPlayerWhitelisted != null)
            {
                var result = OnIsPlayerWhitelisted(player.VirtualPlayer.Id.ToString());
                LogQuery(String.Format("OnIsPlayerWhitelisted Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return false;
        }

        public static void HandleDiscordRegister(NetworkCommunicator player, string id)
        {
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnDiscordRegister != null) OnDiscordRegister(player, id);
            LogQuery(String.Format("OnDiscordRegister Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
        }
        public static void HandleStartMigration()
        {
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            TaleWorlds.Library.Debug.Print("[Save System] Is OnStartMigration null ? " + (OnStartMigration == null).ToString());
            if (OnStartMigration != null) OnStartMigration();
            LogQuery(String.Format("OnStartMigration Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
        }

        public static void HandleCreatePlayerNameIfNotExists(NetworkCommunicator player)
        {
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnCreatePlayerNameIfNotExists != null) OnCreatePlayerNameIfNotExists(player);
            LogQuery(String.Format("OnCreatePlayerNameIfNotExists Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));

        }

        public static IEnumerable<DBHorseMarket> HandleGetAllHorseMarkets()
        {
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnGetAllStockpileMarkets != null)
            {
                var result = OnGetAllHorseMarkets();
                LogQuery(String.Format("OnGetAllHorseMarkets Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }

        public static DBHorseMarket HandleGetHorseMarket(PE_HorseMarket market)
        {
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnGetHorseMarket != null)
            {
                var result = OnGetHorseMarket(market);
                LogQuery(String.Format("OnGetHorseMarket Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }

            return null;
        }

        public static DBHorseMarket HandleCreateOrSaveHorseMarket(PE_HorseMarket market)
        {
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnCreateOrSaveHorseMarket != null)
            {
                var result = OnCreateOrSaveHorseMarket(market);
                LogQuery(String.Format("OnCreateOrSaveHorseMarket Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }

            return null;
        }

        public static IEnumerable<DBStockpileMarket> HandleGetAllStockpileMarkets()
        {
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            TaleWorlds.Library.Debug.Print("[Save System] Is OnGetAllStockpileMarkets null ? " + (OnGetAllStockpileMarkets == null).ToString());
            if (OnGetAllStockpileMarkets != null)
            {
                var result = OnGetAllStockpileMarkets();
                LogQuery(String.Format("OnGetAllStockpileMarkets Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }

            return null;
        }
        public static DBStockpileMarket HandleGetStockpileMarket(PE_StockpileMarket market)
        {
            Debug.Print("[Save System] Is OnGetStockpileMarket null ? " + (OnGetStockpileMarket == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (OnGetStockpileMarket != null)
            {
                var result = OnGetStockpileMarket(market);
                LogQuery(String.Format("OnGetStockpileMarket Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }
        public static DBStockpileMarket HandleCreateOrSaveStockpileMarket(PE_StockpileMarket stockPileMarket)
        {
            Debug.Print("[Save System] Is OnCreateOrSaveStockpileMarket null ? " + (OnCreateOrSaveStockpileMarket == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (OnCreateOrSaveStockpileMarket != null)
            {
                var result = OnCreateOrSaveStockpileMarket(stockPileMarket);
                LogQuery(String.Format("OnCreateOrSaveStockpileMarket Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }

            return null;
        }
        public static DBUpgradeableBuilding HandleCreateOrSaveUpgradebleBuilding(PE_UpgradeableBuildings building)
        {
            Debug.Print("[Save System] Is OnCreateOrSaveUpgradebleBuilding null ? " + (OnCreateOrSaveUpgradebleBuilding == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnCreateOrSaveUpgradebleBuilding != null)
            {
                var result = OnCreateOrSaveUpgradebleBuilding(building);
                LogQuery(String.Format("OnCreateOrSaveUpgradebleBuilding Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }

            return null;
        }

        public static IEnumerable<DBUpgradeableBuilding> HandleGetAllUpgradeableBuildings()
        {
            Debug.Print("[Save System] Is OnGetAllUpgradeableBuildings null ? " + (OnGetAllUpgradeableBuildings == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnGetAllUpgradeableBuildings != null)
            {
                var result = OnGetAllUpgradeableBuildings();
                LogQuery(String.Format("OnGetAllUpgradeableBuildings Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }

        public static DBUpgradeableBuilding HandleGetUpgradeableBuilding(PE_UpgradeableBuildings building)
        {
            Debug.Print("[Save System] Is OnGetUpgradeableBuilding null ? " + (OnGetUpgradeableBuilding == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnGetUpgradeableBuilding != null)
            {
                var result = OnGetUpgradeableBuilding(building);
                LogQuery(String.Format("OnGetUpgradeableBuilding Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }

        public static DBPlayer HandleCreateOrSavePlayer(NetworkCommunicator peer)
        {
            Debug.Print("[Save System] Is OnCreateOrSavePlayer null ? " + (OnCreateOrSavePlayer == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnCreateOrSavePlayer != null)
            {
                var result = OnCreateOrSavePlayer(peer);
                LogQuery(String.Format("OnCreateOrSavePlayer Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }

        public static DBPlayer HandleGetOrCreatePlayer(NetworkCommunicator peer, out bool created)
        {
            Debug.Print("[Save System] Is OnGetOrCreatePlayer null ? " + (OnGetOrCreatePlayer == null).ToString());
            created = false;
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnGetOrCreatePlayer != null)
            {
                var result = OnGetOrCreatePlayer(peer, out created);
                LogQuery(String.Format("OnGetOrCreatePlayer Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }
        public static IEnumerable<DBInventory> HandleGetAllInventories()
        {
            Debug.Print("[Save System] Is OnGetAllInventories null ? " + (OnGetAllInventories == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnGetAllInventories != null)
            {
                var result = OnGetAllInventories();
                LogQuery(String.Format("OnGetAllInventories Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }
        public static DBInventory HandleGetOrCreatePlayerInventory(NetworkCommunicator networkCommunicator, out bool created)
        {
            Debug.Print("[Save System] Is OnGetOrCreatePlayerInventory null ? " + (OnGetOrCreatePlayerInventory == null).ToString());
            created = false;
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnGetOrCreatePlayerInventory != null)
            {
                var result = OnGetOrCreatePlayerInventory(networkCommunicator, out created);
                LogQuery(String.Format("OnGetOrCreatePlayerInventory Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }
        public static DBInventory HandleCreateOrSavePlayerInventory(NetworkCommunicator networkCommunicator)
        {
            Debug.Print("[Save System] Is OnCreateOrSavePlayerInventory null ? " + (OnCreateOrSavePlayerInventory == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnCreateOrSavePlayerInventory != null)
            {
                var result = OnCreateOrSavePlayerInventory(networkCommunicator);
                LogQuery(String.Format("OnCreateOrSavePlayerInventory Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }
        public static DBInventory HandleGetOrCreateInventory(string inventoryId)
        {
            Debug.Print("[Save System] Is OnGetOrCreateInventory null ? " + (OnGetOrCreateInventory == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnGetOrCreateInventory != null)
            {
                var result = OnGetOrCreateInventory(inventoryId);
                LogQuery(String.Format("OnGetOrCreateInventory Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }
        public static DBInventory HandleCreateOrSaveInventory(string inventoryId)
        {
            Debug.Print("[Save System] Is OnCreateOrSaveInventory null ? " + (OnCreateOrSaveInventory == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnCreateOrSaveInventory != null)
            {
                var result = OnCreateOrSaveInventory(inventoryId);
                LogQuery(String.Format("OnCreateOrSaveInventory Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }

        public static IEnumerable<DBCastle> HandleGetCastles()
        {
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnGetCastles != null)
            {
                var result = OnGetCastles();
                LogQuery(String.Format("OnGetCastles Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }
        public static DBCastle HandleCreateOrSaveCastle(int castleIndex, int factionIndex)
        {
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnCreateOrSaveCastle != null)
            {
                var result = OnCreateOrSaveCastle(castleIndex, factionIndex);
                LogQuery(String.Format("OnCreateOrSaveCastle Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }

        public static IEnumerable<DBFactions> HandleGetFactions()
        {
            Debug.Print("[Save System] Is OnGetFactions null ? " + (OnGetFactions == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnGetFactions != null)
            {
                var result = OnGetFactions();
                LogQuery(String.Format("OnGetFactions Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }
        public static DBFactions HandleGetFaction(int factionIndex)
        {
            Debug.Print("[Save System] Is OnGetFaction null ? " + (OnGetFaction == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnGetFaction != null)
            {
                var result = OnGetFaction(factionIndex);
                LogQuery(String.Format("OnGetFaction Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }
        public static DBFactions HandleCreateOrSaveFaction(Faction faction, int factionIndex)
        {
            Debug.Print("[Save System] Is OnCreateOrSaveFaction null ? " + (OnCreateOrSaveFaction == null).ToString());
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnCreateOrSaveFaction != null)
            {
                var result = OnCreateOrSaveFaction(faction, factionIndex);
                LogQuery(String.Format("OnCreateOrSaveFaction Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return null;
        }
        public static bool HandlePlayerUpdateCustomName(NetworkCommunicator peer, string customName)
        {
            long rightNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (OnPlayerUpdateCustomName != null)
            {
                var result = OnPlayerUpdateCustomName(peer, customName);
                LogQuery(String.Format("OnPlayerUpdateCustomName Took {0} ms", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - rightNow));
                return result;
            }
            return false;
        }
        public override void OnMissionResultReady(MissionResult missionResult)
        {
            base.OnMissionResultReady(missionResult);
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                if (peer.ControlledAgent != null)
                {
                    HandleCreateOrSavePlayer(peer);
                    HandleCreateOrSavePlayerInventory(peer);
                }
            }
        }
        private void HandleExceptionalExit(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Debug.Print("UNHANDLED EXCEPTION THROWN : ");
            Debug.Print(e.Message);
            Debug.Print(e.StackTrace);
            Debug.Print(e.ToString());
            if (e.InnerException != null)
            {
                Debug.Print("  INNER EXCEPTION: ");
                Debug.Print(e.InnerException.Message);
                Debug.Print(e.InnerException.StackTrace);
                Debug.Print(e.InnerException.ToString());
            }
            HandleApplicationExit(sender, args);
        }
        private void HandleApplicationExit(object sender, EventArgs e)
        {
            Debug.Print("! SERVER CRASH DETECTED. SAVING PLAYER DATA ONLY !!!");
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                if (peer.ControlledAgent != null)
                {
                    HandleCreateOrSavePlayer(peer);
                    HandleCreateOrSavePlayerInventory(peer);
                }
            }
        }
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            AppDomain.CurrentDomain.ProcessExit += HandleApplicationExit;
            AppDomain.CurrentDomain.UnhandledException += HandleExceptionalExit;

            SaveDuration = ConfigManager.GetIntConfig("AutosaveDuration", 600);

            HandleStartMigration();

        }

        public static void OnAddNewPlayerOnServer(ref PlayerConnectionInfo playerConnectionInfo, bool serverPeer, bool isAdmin)
        {

            if (OnGetPlayer != null && playerConnectionInfo != null)
            {
                DBPlayer player = OnGetPlayer(playerConnectionInfo.PlayerID.ToString());
                if (player != null && player.CustomName != null && player.CustomName != "" && player.CustomName.IsEmpty() == false)
                {
                    playerConnectionInfo.Name = player.CustomName;
                    if (GameNetwork.VirtualPlayers != null)
                    {
                        foreach (NetworkCommunicator communicator in GameNetwork.DisconnectedNetworkPeers)
                        {
                            if (communicator == null) continue;
                            if (communicator.VirtualPlayer.Id == playerConnectionInfo.PlayerID)
                            {
                                communicator.VirtualPlayer.GetType().GetProperty("UserName").SetValue(communicator.VirtualPlayer, player.CustomName);
                            }
                        }
                    }
                }
            }
        }

        public static void RglExceptionThrown(System.Diagnostics.StackTrace e, Exception rglException)
        {
            // Define your error logging logic
        }

        public override void OnMissionTick(float dt)
        {
            // Auto save
            base.OnMissionTick(dt);
            if (DateTimeOffset.Now.ToUnixTimeSeconds() > this.LastSaveAt + this.SaveDuration)
            {
                // Create a Job
                InformationComponent.Instance.BroadcastMessage("* Autosave triggered. It might lag a bit", Colors.Blue.ToUnsignedInteger());
                this._currentAutoSaveJob = new AutoSaveJob(GameNetwork.NetworkPeers.ToList());
                this.LastSaveAt = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
            if (this._currentAutoSaveJob != null && this._currentAutoSaveJob.isCompleted() == false)
            {
                this._currentAutoSaveJob.doWork();
            }
        }
    }
}

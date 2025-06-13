using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpiresLib.SceneScripts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class PersistentEmpireBehavior : MissionMultiplayerGameModeBase
    {

        public class ServerTokenValidate
        {
            public string Token { get; set; }
            public int MaxPlayer { get; set; }
            public string ServerName { get; set; }
        }

        public class MasterServerResponse
        {
            public string key { get; set; }
            public string error { get; set; }
        }

        private FactionsBehavior _factionsBehavior;
        private CastlesBehavior _castlesBehavior;
        private long lastPolledAt = 0;
        private bool registered = false;
        public bool agentLabelEnabled = true;
        public override bool IsGameModeHidingAllAgentVisuals => true;

        public override bool IsGameModeUsingOpposingTeams => false;
        public string ServerSignature;

        // public static string ServerSignature = "";
        private static string _defaultClass = "pe_peasant";
        public static string DefaultClass { get { return _defaultClass; } }
        public static PersistentEmpireBehavior Instanse = null;

        public static void SetDefaultClass(string defaultClass)
        {
            if (string.IsNullOrEmpty(defaultClass)) return;

            _defaultClass = defaultClass;
        }

        public override void OnAgentMount(Agent agent)
        {
            base.OnAgentMount(agent);
            if (agent.IsPlayerControlled && agent.MissionPeer.GetNetworkPeer() != null)
            {
                LoggerHelper.LogAnAction(agent.MissionPeer.GetNetworkPeer(), LogAction.PlayerMountedHorse, null, new object[] { agent });
            }
        }

        public override void OnAgentDismount(Agent agent)
        {
            base.OnAgentDismount(agent);
            if (agent.IsPlayerControlled && agent.MissionPeer.GetNetworkPeer() != null)
            {
                LoggerHelper.LogAnAction(agent.MissionPeer.GetNetworkPeer(), LogAction.PlayerDismountedHorse, null, new object[] { agent });
            }
        }

        public override MultiplayerGameType GetMissionType()
        {
            return MultiplayerGameType.FreeForAll;
        }
        protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        {
            networkPeer.AddComponent<PersistentEmpireRepresentative>();

#if SERVER
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new EnableVoiceChat(ConfigManager.VoiceChatEnabled));
            GameNetwork.EndModuleEventAsServer();
#endif
        }
        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);
            if (affectedAgent.IsHuman && affectedAgent.IsPlayerControlled && affectedAgent.IsUsingGameObject)
            {
                // affectedAgent.StopUsingGameObjectMT(false, true, false);
                affectedAgent.StopUsingGameObjectMT(false);
            }

            if (GameNetwork.IsServer && affectorAgent != null && affectedAgent != affectorAgent && affectorAgent.IsHuman && affectorAgent.IsPlayerControlled && affectorAgent.IsActive())
            {

                NetworkCommunicator issuer = affectorAgent.MissionPeer.GetNetworkPeer();
                NetworkCommunicator affected = null;
                if (affectedAgent.IsHuman && affectedAgent.IsPlayerControlled)
                {
                    affected = affectedAgent.MissionPeer.GetNetworkPeer();
                }
                else if (affectedAgent.IsMount && affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.IsHuman && affectedAgent.RiderAgent.IsPlayerControlled)
                {
                    affected = affectedAgent.RiderAgent.MissionPeer.GetNetworkPeer();
                }
                LoggerHelper.LogAnAction(issuer, LogAction.PlayerHitToAgent, affected == null ? new AffectedPlayer[] { } : new AffectedPlayer[] { new AffectedPlayer(affected) }, new object[] { affectorWeapon, affectedAgent });

            }
            else if (GameNetwork.IsServer && affectorAgent != null && affectedAgent != affectorAgent && affectorAgent.IsMount && affectorAgent.RiderAgent != null && affectorAgent.IsActive())
            {
                NetworkCommunicator issuer = affectorAgent.RiderAgent.MissionPeer.GetNetworkPeer();
                NetworkCommunicator affected = null;
                if (affectedAgent.IsHuman && affectedAgent.IsPlayerControlled)
                {
                    affected = affectedAgent.MissionPeer.GetNetworkPeer();
                }
                else if (affectedAgent.IsMount && affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.IsHuman && affectedAgent.RiderAgent.IsPlayerControlled)
                {
                    affected = affectedAgent.RiderAgent.MissionPeer.GetNetworkPeer();
                }
                LoggerHelper.LogAnAction(issuer, LogAction.PlayerBumpedWithHorse, affected == null ? new AffectedPlayer[] { } : new AffectedPlayer[] { new AffectedPlayer(affected) }, new object[] { affectorWeapon, affectedAgent });
            }
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
            if (GameNetwork.IsServer)
            {
                if ((agentState == AgentState.Killed || agentState == AgentState.Unconscious || agentState == AgentState.Routed) && affectedAgent != null && affectedAgent.IsHuman)
                {
                    if (affectedAgent.MissionPeer != null && !affectedAgent.MissionPeer.GetNetworkPeer().QuitFromMission)
                    {
                        this.OnPlayerDies(affectedAgent.MissionPeer.GetNetworkPeer());
                    }

                    if (affectorAgent != null && affectedAgent != affectorAgent && affectorAgent.IsHuman && affectorAgent.IsPlayerControlled)
                    {
                        NetworkCommunicator affected = null;
                        if (affectedAgent.IsHuman && affectedAgent.IsPlayerControlled)
                        {
                            affected = affectedAgent.MissionPeer.GetNetworkPeer();
                        }
                        else if (affectedAgent.IsMount && affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.IsHuman && affectedAgent.RiderAgent.IsPlayerControlled)
                        {
                            affected = affectedAgent.RiderAgent.MissionPeer.GetNetworkPeer();
                        }
                        MissionWeapon weapon = new MissionWeapon();
                        if (affectorAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand) != EquipmentIndex.None)
                        {
                            weapon = affectorAgent.Equipment[affectorAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand)];
                        }
                        LoggerHelper.LogAnAction(affectorAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerKilledAnAgent, affected == null ? new AffectedPlayer[] { } : new AffectedPlayer[] { new AffectedPlayer(affected) }, new object[] { weapon, affectedAgent });
                    }
                }
            }
        }
        public override void OnPlayerConnectedToServer(NetworkCommunicator networkPeer)
        {
            base.OnPlayerConnectedToServer(networkPeer);
            networkPeer.QuitFromMission = false;
        }

#if SERVER
        private void SendRulesToNewClient(NetworkCommunicator networkPeer)
        {
            if(ConfigManager.Rules == null)
            {
                return;
            }

            var count = 0;
            var doWork = true;
            List<string> list = new List<string>();
            var rest = ConfigManager.Rules.OuterXml;
            while (doWork)
            {
                count++;
                if (rest.Length > 500)
                {
                    list.Add(rest.Substring(0, 500));
                    rest = rest.Substring(500);
                }
                else
                {
                    list.Add(rest.Substring(0));
                    doWork = false;
                }
            }
            var count2 = 1;
            foreach (var item in list)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new SendRulesToNewClientMessage(count2, count, item));
                GameNetwork.EndModuleEventAsServer();
                count2++;
            }
        }
        #endif

        protected override void HandleEarlyPlayerDisconnect(NetworkCommunicator peer)
        {
            base.HandleEarlyPlayerDisconnect(peer);
            peer.QuitFromMission = true;
        }
        public override void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
        {
            base.OnPlayerDisconnectedFromServer(networkPeer);
            networkPeer.QuitFromMission = true;

            SaveSystemBehavior saveSystemBehavior = base.Mission.GetMissionBehavior<SaveSystemBehavior>();

            PersistentEmpireRepresentative persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative != null)
            {
                persistentEmpireRepresentative.DisconnectedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            if (GameNetwork.IsServer)
            {
                LoggerHelper.LogAnAction(networkPeer, LogAction.PlayerDisconnected);
            }

            if (saveSystemBehavior != null && persistentEmpireRepresentative != null && networkPeer.ControlledAgent != null && persistentEmpireRepresentative.IsFirstAgentSpawned)
            {
                persistentEmpireRepresentative.IsFirstAgentSpawned = false;
                SaveSystemBehavior.HandleCreateOrSavePlayer(networkPeer);
                SaveSystemBehavior.HandleCreateOrSavePlayerInventory(networkPeer);

            }
            if (networkPeer.ControlledAgent != null && networkPeer.ControlledAgent.MountAgent != null)
            {
                networkPeer.ControlledAgent.FadeOut(true, true);
            }
        }

        private void OnPlayerDies(NetworkCommunicator peer)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            LoggerHelper.LogAnAction(peer, LogAction.PlayerDied);
            persistentEmpireRepresentative.SpawnTimer.Reset(Mission.Current.CurrentTime, (float)MissionLobbyComponent.GetSpawnPeriodDurationForPeer(peer.GetComponent<MissionPeer>()));
            if (persistentEmpireRepresentative.KickedFromFaction)
            {
                persistentEmpireRepresentative.SetClass("pe_serf");
                persistentEmpireRepresentative.KickedFromFaction = false;
            }
        }

        private bool checkAlphaNumeric(String name)
        {
            // ^[a-zA-Z0-9\s,\[,\],\(,\)]*$
            Regex rg = new Regex(@"^[a-zA-Z0-9ğüşöçıİĞÜŞÖÇ.\s,\[,\],\(,\),_,-,\p{IsCJKUnifiedIdeographs}]*$");
            return rg.IsMatch(name);
        }
#if SERVER
        public static void SyncPlayer(NetworkCommunicator networkPeer)
        {
            Instanse.HandleLateNewClientAfterSynchronized(networkPeer);
        }

        protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
            base.HandleLateNewClientAfterSynchronized(networkPeer);
            if (networkPeer.IsConnectionActive == false || networkPeer.IsNetworkActive == false) return;
            this._factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            this._castlesBehavior = base.Mission.GetMissionBehavior<CastlesBehavior>();
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative != null)
            {
                if (persistentEmpireRepresentative.Gold <= 0)
                {
                    persistentEmpireRepresentative.SetGold(ConfigManager.StartingGold);
                }
                else
                {
                    persistentEmpireRepresentative.SetGold(persistentEmpireRepresentative.Gold);
                }
            }
            MissionPeer component = networkPeer.GetComponent<MissionPeer>();
            if (GameNetwork.IsServer)
            {
                InformationComponent.Instance.SendMessage("Your player id is " + networkPeer.VirtualPlayer.Id.ToString(), Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), networkPeer);

                List<PE_CastleBanner> castleBanners = this._castlesBehavior.castles.Values.ToList();
                for (int i = 0; i < this._factionsBehavior.Factions.Keys.ToList().Count; i++)
                {
                    Faction f = this._factionsBehavior.Factions[i];
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new SyncFaction(i, f));
                    GameNetwork.EndModuleEventAsServer();
                    foreach (string s in this._factionsBehavior.Factions[i].marshalls)
                    {
                        GameNetwork.BeginModuleEventAsServer(networkPeer);
                        GameNetwork.WriteMessage(new AddMarshallIdToFaction(s, i));
                        GameNetwork.EndModuleEventAsServer();
                    }
                }
                foreach (PE_CastleBanner castleBanner in castleBanners)
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new SyncCastleBanner(castleBanner, castleBanner.FactionIndex));
                    GameNetwork.EndModuleEventAsServer();
                }
                foreach (NetworkCommunicator player in GameNetwork.NetworkPeers)
                {
                    PersistentEmpireRepresentative persistentEmpireRepresentative1 = player.GetComponent<PersistentEmpireRepresentative>();
                    if (persistentEmpireRepresentative1 != null)
                    {
                        Faction f = persistentEmpireRepresentative1.GetFaction();

                        GameNetwork.BeginModuleEventAsServer(networkPeer);
                        GameNetwork.WriteMessage(new SyncMember(player, persistentEmpireRepresentative1.GetFactionIndex(), f == null ? false : f.marshalls.Contains(player.VirtualPlayer.ToPlayerId()), FactionPollComponent.LordPollEnabled));
                        GameNetwork.EndModuleEventAsServer();
                    }
                }

                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new AgentLabelConfig(this.agentLabelEnabled));
                GameNetwork.EndModuleEventAsServer();

            }
            SaveSystemBehavior saveSystemBehavior = base.Mission.GetMissionBehavior<SaveSystemBehavior>();
            if (saveSystemBehavior != null)
            {
                bool created;

                SaveSystemBehavior.HandleCreatePlayerNameIfNotExists(networkPeer);
                DBPlayer dbPlayer = SaveSystemBehavior.HandleGetOrCreatePlayer(networkPeer, out created);
                if (!created)
                {
                    persistentEmpireRepresentative.SetClass(dbPlayer.Class);
                    persistentEmpireRepresentative.SetGold(dbPlayer.Money);
                    persistentEmpireRepresentative.SetHunger(dbPlayer.Hunger);
                    persistentEmpireRepresentative.SetWounded(dbPlayer.WoundedUntil);
                    this._factionsBehavior.SetPlayerFaction(networkPeer, dbPlayer.FactionIndex, -1);

                    persistentEmpireRepresentative.LoadedDbPosition = new Vec3(dbPlayer.PosX, dbPlayer.PosY, dbPlayer.PosZ);
                    Equipment loadedEquipment = new Equipment();
                    int[] loadedAmmo = new int[4];

                    if (dbPlayer.Equipment_0 != null)
                    {
                        loadedEquipment[EquipmentIndex.Weapon0] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Equipment_0));
                        loadedAmmo[0] = dbPlayer.Ammo_0;
                    }
                    if (dbPlayer.Equipment_1 != null)
                    {
                        loadedEquipment[EquipmentIndex.Weapon1] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Equipment_1));
                        loadedAmmo[1] = dbPlayer.Ammo_1;
                    }
                    if (dbPlayer.Equipment_2 != null)
                    {
                        loadedEquipment[EquipmentIndex.Weapon2] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Equipment_2));
                        loadedAmmo[2] = dbPlayer.Ammo_2;
                    }
                    if (dbPlayer.Equipment_3 != null)
                    {
                        loadedEquipment[EquipmentIndex.Weapon3] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Equipment_3));
                        loadedAmmo[3] = dbPlayer.Ammo_3;
                    }
                    if (dbPlayer.Armor_Head != null)
                    {
                        loadedEquipment[EquipmentIndex.Head] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Armor_Head));
                    }
                    if (dbPlayer.Armor_Cape != null)
                    {
                        loadedEquipment[EquipmentIndex.Cape] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Armor_Cape));
                    }
                    if (dbPlayer.Armor_Gloves != null)
                    {
                        loadedEquipment[EquipmentIndex.Gloves] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Armor_Gloves));
                    }
                    if (dbPlayer.Armor_Body != null)
                    {
                        loadedEquipment[EquipmentIndex.Body] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Armor_Body));
                    }
                    if (dbPlayer.Armor_Leg != null)
                    {
                        loadedEquipment[EquipmentIndex.Leg] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Armor_Leg));
                    }
                    if (dbPlayer.Horse != null)
                    {
                        loadedEquipment[EquipmentIndex.Horse] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Horse));
                    }
                    if (dbPlayer.HorseHarness != null)
                    {
                        loadedEquipment[EquipmentIndex.HorseHarness] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.HorseHarness));
                    }
                    persistentEmpireRepresentative.LoadedSpawnEquipment = loadedEquipment;
                    persistentEmpireRepresentative.LoadedHealth = dbPlayer.Health <= 0 ? 10 : dbPlayer.Health;
                    persistentEmpireRepresentative.LoadFromDb = true;
                    persistentEmpireRepresentative.LoadedAmmo = loadedAmmo;
                }
                else
                {
                    this._factionsBehavior.SetPlayerFaction(networkPeer, 0, -1);
                }

                // Load Inventory of the player.
                DBInventory dbInventory = SaveSystemBehavior.HandleGetOrCreatePlayerInventory(networkPeer, out created);
                if (!created)
                {
                    persistentEmpireRepresentative.SetInventory(Inventory.Deserialize(dbInventory.InventorySerialized, "PlayerInventory", null));
                }
                if (GameNetwork.IsServer)
                {
                    LoggerHelper.LogAnAction(networkPeer, LogAction.PlayerJoined);
                }
            }
#if SERVER
            SendRulesToNewClient(networkPeer);
#endif
        }
#endif
        public override void AfterStart()
        {
            Mission.Current.SetMissionCorpseFadeOutTimeInSeconds(60);
#if SERVER
            Instanse = this;
            ConfigManager.Initialize();
            this.agentLabelEnabled = ConfigManager.GetBoolConfig("AgentLabelEnabled", true);
#endif

            int maxPlayer = MultiplayerOptions.OptionType.MaxNumberOfPlayers.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
            string serverName = MultiplayerOptions.OptionType.ServerName.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);


            IEnumerable<DBUpgradeableBuilding> dbUpgradeables = SaveSystemBehavior.HandleGetAllUpgradeableBuildings();
            List<PE_UpgradeableBuildings> upgradeables = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_UpgradeableBuildings>().Select(g => g.GetFirstScriptOfType<PE_UpgradeableBuildings>()).ToList();

            Dictionary<string, DBUpgradeableBuilding> savedUpgrades = new Dictionary<string, DBUpgradeableBuilding>();

            foreach (DBUpgradeableBuilding building in dbUpgradeables)
            {
                savedUpgrades[building.MissionObjectHash] = building;
            }
            foreach (PE_UpgradeableBuildings upgradeable in upgradeables)
            {
                Debug.Print("Upgradeable id is : " + upgradeable.GetMissionObjectHash());
                if (savedUpgrades.ContainsKey(upgradeable.GetMissionObjectHash()))
                {
                    upgradeable.SetTier(savedUpgrades[upgradeable.GetMissionObjectHash()].CurrentTier);
                    upgradeable.SetIsUpgrading(savedUpgrades[upgradeable.GetMissionObjectHash()].IsUpgrading);
                }
            }
            // LOAD StockpileMarkets from DB
            IEnumerable<DBStockpileMarket> dbStockpileMarkets = SaveSystemBehavior.HandleGetAllStockpileMarkets();
            List<PE_StockpileMarket> stockpileMarkets = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_StockpileMarket>().Select(g => g.GetFirstScriptOfType<PE_StockpileMarket>()).ToList();
            Dictionary<string, DBStockpileMarket> savedStockpile = new Dictionary<string, DBStockpileMarket>();
            foreach (DBStockpileMarket dbStockpileMarket in dbStockpileMarkets)
            {
                savedStockpile[dbStockpileMarket.MissionObjectHash] = dbStockpileMarket;
            }
            foreach (PE_StockpileMarket stockpileMarket in stockpileMarkets)
            {
                if (savedStockpile.ContainsKey(stockpileMarket.GetMissionObjectHash()))
                {
                    stockpileMarket.DeserializeStocks(savedStockpile[stockpileMarket.GetMissionObjectHash()].MarketItemsSerialized);
                }
            }
            // LOAD HorseMarkets from DB
            IEnumerable<DBHorseMarket> dBHorseMarkets = SaveSystemBehavior.HandleGetAllHorseMarkets();
            List<PE_HorseMarket> horseMarkets = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_HorseMarket>().Select(g => g.GetFirstScriptOfType<PE_HorseMarket>()).ToList();
            Dictionary<string, DBHorseMarket> savedHorseMarket = new Dictionary<string, DBHorseMarket>();
            foreach (DBHorseMarket dBHorseMarket in dBHorseMarkets)
            {
                savedHorseMarket[dBHorseMarket.MissionObjectHash] = dBHorseMarket;
            }
            foreach (PE_HorseMarket horseMarket in horseMarkets)
            {
                if (savedHorseMarket.ContainsKey(horseMarket.GetMissionObjectHash()))
                {
                    horseMarket.UpdateReserve(savedHorseMarket[horseMarket.GetMissionObjectHash()].Stock);
                }
            }
        }
    }
}

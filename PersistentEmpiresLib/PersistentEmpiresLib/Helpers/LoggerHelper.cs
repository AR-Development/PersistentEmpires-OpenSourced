using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.Helpers
{
    public static class LogAction
    {
        public static readonly string LocalChat = "LocalChat"; // Done
        public static readonly string TeamChat = "TeamChat"; // Done
        public static readonly string PlayerHitToAgent = "PlayerHitAgent"; // Done
        public static readonly string PlayerKilledAnAgent = "PlayerKilledAgent"; // Done
        public static readonly string PlayerJoined = "PlayerJoined"; // Done
        public static readonly string PlayerDisconnected = "PlayerDisconnected"; // Done
        public static readonly string PlayerDroppedLoot = "PlayerDroppedLoot"; // Done
        public static readonly string PlayerFactionChange = "PlayerFactionChange"; // Done
        public static readonly string PlayerClassChange = "PlayerClassChange"; // Done
        public static readonly string FactionLordChanged = "FactionLordChanged";// Done
        public static readonly string FactionDeclaredWar = "FactionDeclaredWar"; // Done
        public static readonly string FactionMadePeace = "FactionMadePeace";// Done
        public static readonly string PlayerOpensChest = "PlayerOpensChest";// Done
        public static readonly string PlayerClosesChest = "PlayerClosesChest"; // Done
        public static readonly string PlayerTransferredItemToChest = "PlayerTransferredItemToChest"; // Done
        public static readonly string PlayerTransferredItemFromChest = "PlayerTransferredItemFromChest"; // Done
        public static readonly string PlayerOpensStockpile = "PlayerOpensStockpile";// Done
        public static readonly string PlayerClosesStockpile = "PlayerClosesStockpile";// Done
        public static readonly string PlayerBuysStockpile = "PlayerBuysStockpile";// Done
        public static readonly string PlayerSellsStockpile = "PlayerSellsStockpile";// Done
        public static readonly string PlayerSpawnedPrefab = "PlayerSpawnedPrefab"; // Done
        public static readonly string PlayerHitToDestructable = "PlayerHitToDestructable"; // Done
        public static readonly string PlayerRepairesTheDestructable = "PlayerRepairesTheDestructable"; // Done
        public static readonly string PlayerSpawn = "PlayerSpawn"; // Done
        public static readonly string PlayerDied = "PlayerDied"; // Done
        public static readonly string PlayerDroppedItem = "PlayerDroppedItem"; // Done
        public static readonly string PlayerDroppedGold = "PlayerDroppedGold"; // Done
        public static readonly string PlayerPickedUpGold = "PlayerPickedUpGold"; // Done
        public static readonly string PlayerBecomesGodlike = "PlayerBecomesGodlike"; // Done
        public static readonly string PlayerSpawnsItem = "PlayerSpawnsItem"; // Done
        public static readonly string PlayerItemGathers = "PlayerItemGathers"; // Done
        public static readonly string PlayerBansPlayer = "PlayerBansPlayer"; // Done
        public static readonly string PlayerKicksPlayer = "PlayerKicksPlayer"; // Done
        public static readonly string PlayerTempBanPlayer = "PlayerTempBanPlayer"; // Done
        public static readonly string PlayerFadesPlayer = "PlayerFadesPlayer"; // Done
        public static readonly string PlayerSpawnedMoney = "PlayerSpawnedMoney"; // Done
        public static readonly string PlayerDespawnedPrefab = "PlayerDespawnedPrefab"; // Done
        public static readonly string PlayerSlayPlayer = "PlayerSlayPlayer"; // Done
        public static readonly string PlayerFreezePlayer = "PlayerFreezePlayer";
        public static readonly string PlayerUnfreezePlayer = "PlayerUnfreezePlayer";
        public static readonly string PlayerTpToPlayer = "PlayerTpToPlayer";
        public static readonly string PlayerTpToMePlayer = "PlayerTpToMePlayer";
        public static readonly string PlayerHealedPlayer = "PlayerHealedPlayer";
        public static readonly string PlayerBumpedWithHorse = "PlayerBumpedWithHorse";
        public static readonly string PlayerRevealedMoneyPouch = "PlayerRevealedMoneyPouch";
        public static readonly string PlayerRevealedItemBag = "PlayerRevealedItemBag";
        public static readonly string PlayerCommitedSuicide = "PlayerCommitedSuicide";
        public static readonly string PlayerHitToAttachable = "PlayerHitToAttachable"; // Done
        public static readonly string PlayerDepositedToBank = "PlayerDepositedToBank";
        public static readonly string PlayerWithdrawToBank = "PlayerWithdrawToBank";
        public static readonly string PlayerMountedHorse = "PlayerMountedHorse";
        public static readonly string PlayerDismountedHorse = "PlayerDismountedHorse";
        public static readonly string PlayerChangedName = "PlayerChangedName";
    }

    public class LoggerHelper
    {
        public delegate void LogActionHandler(DBLog log);
        public static event LogActionHandler OnLogAction;

        private static string AffectedPlayersToString(AffectedPlayer[] players) {
            string[] str = new string[players.Length];
            for(int i = 0; i < players.Length; i++)
            {
                str[i] = players[i].ToString();
            }
            return String.Join(",", str);
        }

        private static string FormatLogForAgent(Agent agent, DateTime dateTime)
        {
            if(agent.IsHuman && agent.IsPlayerControlled)
            {
                return FormatLogForPlayer(agent.MissionPeer.GetNetworkPeer(), dateTime, false);
            }else if(agent.IsMount && agent.RiderAgent != null && agent.RiderAgent.IsHuman && agent.RiderAgent.IsPlayerControlled)
            {
                return String.Format("A Horse Rided By {0}", FormatLogForPlayer(agent.RiderAgent.MissionPeer.GetNetworkPeer(), dateTime, false));
            }else if(agent.IsMount)
            {
                return "A Horse";
            }
            return "an Animal";
        }

        public static string FormatLogForPlayer(NetworkCommunicator player, DateTime dateTime, bool logDateTime = true)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            string factionName = persistentEmpireRepresentative != null && persistentEmpireRepresentative.GetFaction() != null ? persistentEmpireRepresentative.GetFaction().name : "Unknown";
            if(logDateTime)
            {
                return String.Format("[{0}][{1}]", factionName, player.UserName);
            }
            return String.Format("[{0}][{1}]", factionName, player.UserName);

        }

        public static NetworkCommunicator GetAffectedPeerFromAgent(Agent affectedAgent)
        {
            if (affectedAgent.MissionPeer != null && affectedAgent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>() != null)
            {
                return affectedAgent.MissionPeer.GetNetworkPeer();
            }
            else if(affectedAgent.IsMount && affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.MissionPeer != null && affectedAgent.RiderAgent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>() != null)
            {
                return affectedAgent.RiderAgent.MissionPeer.GetNetworkPeer();
            }
            return null;
        }

        private static string GenerateActionMessage(NetworkCommunicator issuer, string actionType, DateTime dateTime, AffectedPlayer[] affectedPlayers = null, object[] oParams = null)
        {
            if(actionType == LogAction.PlayerJoined)
            {
                return String.Format("{0} joined to the server.", FormatLogForPlayer(issuer, dateTime));
            }
            else if (actionType == LogAction.PlayerDisconnected)
            {
                return String.Format("{0} disconnected from the server.", FormatLogForPlayer(issuer, dateTime));
            }
            else if (actionType == LogAction.LocalChat)
            {
                return String.Format("{0} said \"[{1}]\". Receivers: {2}", FormatLogForPlayer(issuer, dateTime), (string)oParams[0], AffectedPlayersToString(affectedPlayers));
            }else if(actionType == LogAction.TeamChat)
            {
                return String.Format("{0} said \"[{1}]\". Receivers: {2}", FormatLogForPlayer(issuer, dateTime), (string)oParams[0], AffectedPlayersToString(affectedPlayers));
            }
            else if (actionType == LogAction.PlayerHitToAgent)
            {
                MissionWeapon missionWeapon = (MissionWeapon)oParams[0];
                string attackedItem = missionWeapon.Item == null ? "fist" : missionWeapon.Item.Name.ToString();
                Agent affectedAgent = (Agent)oParams[1];

                string warStatus = "Neutral";
                NetworkCommunicator affectedPeer = GetAffectedPeerFromAgent(affectedAgent);
                if (affectedPeer != null)
                {
                    PersistentEmpireRepresentative persistentEmpireRepresentative = affectedPeer.GetComponent<PersistentEmpireRepresentative>();
                    PersistentEmpireRepresentative issuerRepr = issuer.GetComponent<PersistentEmpireRepresentative>();
                    if(issuerRepr.GetFaction() != null && persistentEmpireRepresentative.GetFaction() != null)
                    {
                        warStatus = issuerRepr.GetFaction().warDeclaredTo.Contains(persistentEmpireRepresentative.GetFactionIndex()) || persistentEmpireRepresentative.GetFaction().warDeclaredTo.Contains(issuerRepr.GetFactionIndex()) ? "Enemies" : "Neutral";
                    }
                }

                return String.Format("{0} attacked with {1} to {2} Their faction's was {3}", FormatLogForPlayer(issuer, dateTime), attackedItem, FormatLogForAgent(affectedAgent, dateTime),warStatus);
            }
            else if (actionType == LogAction.PlayerBumpedWithHorse)
            {
                MissionWeapon missionWeapon = (MissionWeapon)oParams[0];
                string attackedItem = missionWeapon.Item == null ? "fist" : missionWeapon.Item.Name.ToString();
                Agent affectedAgent = (Agent)oParams[1];
                string warStatus = "Neutral";
                NetworkCommunicator affectedPeer = GetAffectedPeerFromAgent(affectedAgent);
                if (affectedPeer != null)
                {
                    PersistentEmpireRepresentative persistentEmpireRepresentative = affectedPeer.GetComponent<PersistentEmpireRepresentative>();
                    PersistentEmpireRepresentative issuerRepr = issuer.GetComponent<PersistentEmpireRepresentative>();
                    if (issuerRepr.GetFaction() != null && persistentEmpireRepresentative.GetFaction() != null)
                    {
                        warStatus = issuerRepr.GetFaction().warDeclaredTo.Contains(persistentEmpireRepresentative.GetFactionIndex()) || persistentEmpireRepresentative.GetFaction().warDeclaredTo.Contains(issuerRepr.GetFactionIndex()) ? "Enemies" : "Neutral";
                    }
                }
                return String.Format("{0} bumped with a horse to {1} Their faction's was {2}", FormatLogForPlayer(issuer, dateTime), FormatLogForAgent(affectedAgent, dateTime), warStatus);
            }
            else if (actionType == LogAction.PlayerKilledAnAgent)
            {
                MissionWeapon missionWeapon = (MissionWeapon)oParams[0];
                string attackedItem = missionWeapon.Item == null ? "fist" : missionWeapon.Item.Name.ToString();
                Agent affectedAgent = (Agent)oParams[1];
                // NetworkCommunicator missionNetwork = affectedAgent.MissionPeer.GetNetworkPeer();
                string warStatus = "Neutral";
                NetworkCommunicator affectedPeer = GetAffectedPeerFromAgent(affectedAgent);
                if (affectedPeer != null)
                {
                    PersistentEmpireRepresentative persistentEmpireRepresentative = affectedPeer.GetComponent<PersistentEmpireRepresentative>();
                    PersistentEmpireRepresentative issuerRepr = issuer.GetComponent<PersistentEmpireRepresentative>();
                    if (issuerRepr.GetFaction() != null && persistentEmpireRepresentative.GetFaction() != null)
                    {
                        warStatus = issuerRepr.GetFaction().warDeclaredTo.Contains(persistentEmpireRepresentative.GetFactionIndex()) || persistentEmpireRepresentative.GetFaction().warDeclaredTo.Contains(issuerRepr.GetFactionIndex()) ? "Enemies" : "Neutral";
                    }
                }
                return String.Format("{0} killed {1} with {2} Their faction's was {3}", FormatLogForPlayer(issuer, dateTime), FormatLogForAgent(affectedAgent, dateTime), attackedItem, warStatus);
            }else if(actionType == LogAction.PlayerDroppedLoot)
            {
                Inventory inventory = (Inventory)oParams[0];
                
                string[] droppedLootStr = inventory.Slots.Where(s => s.IsEmpty() == false).Select(s => {
                    return s.Item + "*" + s.Count;
                }).ToArray();

                string droppedLoot = String.Join(",", droppedLootStr);
                return String.Format("{0} is died and drop({1}) the following loot: {2}", FormatLogForPlayer(issuer, dateTime), inventory.InventoryId,droppedLoot);
            }else if(actionType == LogAction.PlayerFactionChange)
            {
                Faction joinedFrom = (Faction)oParams[0];
                Faction joinedTo = (Faction)oParams[1];
                return String.Format("{0} is joined to faction {1} from {2}", FormatLogForPlayer(issuer, dateTime), joinedFrom != null ? joinedFrom.name : "Unknown", joinedTo != null ? joinedTo.name : "Unknown");
            }else if(actionType == LogAction.PlayerClassChange)
            {
                BasicCharacterObject bco = (BasicCharacterObject)oParams[0];
                return String.Format("{0} became a {1}", FormatLogForPlayer(issuer, dateTime), bco.Name);
            }else if(actionType == LogAction.FactionLordChanged)
            {
                return String.Format("{0} became lord", FormatLogForPlayer(issuer,dateTime));
            }else if(actionType == LogAction.FactionDeclaredWar)
            {
                Faction declaredTo = (Faction)oParams[0];
                return String.Format("{0} declared war to {1}", FormatLogForPlayer(issuer, dateTime), declaredTo != null ? declaredTo.name : "Unknown");
            }
            else if (actionType == LogAction.FactionMadePeace)
            {
                Faction declaredTo = (Faction)oParams[0];
                return String.Format("{0} made peace with {1}", FormatLogForPlayer(issuer, dateTime), declaredTo != null ? declaredTo.name : "Unknown");
            }else if(actionType == LogAction.PlayerOpensChest)
            {
                Inventory inv = (Inventory)oParams[0];
                return String.Format("{0} opened a chest/loot of {1}", FormatLogForPlayer(issuer, dateTime), inv.InventoryId);
            }
            else if (actionType == LogAction.PlayerClosesChest)
            {
                Inventory inv = (Inventory)oParams[0];
                return String.Format("{0} closed a chest/loot of {1}", FormatLogForPlayer(issuer, dateTime), inv.InventoryId);
            }
            else if (actionType == LogAction.PlayerTransferredItemToChest)
            {
                // Inventory inv = (Inventory)oParams[0];
                string droppedToInventory = (string)oParams[0];
                ItemObject item = (ItemObject)oParams[1];
                string itemName = item == null ? "null" : item.Name.ToString();
                int count = (int)oParams[2];
                return String.Format("{0} transferred {1}*{2} to the chest({3})", FormatLogForPlayer(issuer, dateTime), itemName, count, droppedToInventory);
            }
            else if (actionType == LogAction.PlayerTransferredItemFromChest)
            {
                // Inventory inv = (Inventory)oParams[0];
                string draggedFromInventory = (string)oParams[0];
                ItemObject item = (ItemObject)oParams[1];
                string itemName = item == null ? "null" : item.Name.ToString();
                int count = (int)oParams[2];
                return String.Format("{0} transferred {1}*{2} from the chest({3})", FormatLogForPlayer(issuer, dateTime), itemName, count, draggedFromInventory);
            }
            else if(actionType == LogAction.PlayerOpensStockpile)
            {
                PE_StockpileMarket stockpileMarket = (PE_StockpileMarket)oParams[0];
                return String.Format("{0} accessed to a stockpile market. Xml file of market is {1}", FormatLogForPlayer(issuer, dateTime), stockpileMarket.XmlFile);
            }
            else if (actionType == LogAction.PlayerClosesStockpile)
            {
                PE_StockpileMarket stockpileMarket = (PE_StockpileMarket)oParams[0];
                return String.Format("{0} closed the stockpile market. Xml file of market is {1}", FormatLogForPlayer(issuer, dateTime), stockpileMarket.XmlFile);
            }
            else if (actionType == LogAction.PlayerBuysStockpile)
            {
                MarketItem marketItem = (MarketItem)oParams[0];
                return String.Format("{0} bought {1} from the stockpile market with a price of {2}.", FormatLogForPlayer(issuer, dateTime), marketItem.Item.Name, marketItem.BuyPrice());
            }
            else if (actionType == LogAction.PlayerSellsStockpile)
            {
                MarketItem marketItem = (MarketItem)oParams[0];
                return String.Format("{0} sold {1} to the stockpile market with a price of {2}.", FormatLogForPlayer(issuer, dateTime), marketItem.Item.Name, marketItem.SellPrice());
            }else if(actionType == LogAction.PlayerSpawnedPrefab)
            {
                SpawnableItem spawnableItem = (SpawnableItem)oParams[0];
                return String.Format("{0} spawned a prefab with item {1}", FormatLogForPlayer(issuer, dateTime), spawnableItem.SpawnerItem.Name);
            }else if(actionType == LogAction.PlayerHitToDestructable)
            {
                string scriptName = (string)oParams[0];
                return String.Format("{0} hit to a destructable with a script of {1}", FormatLogForPlayer(issuer, dateTime), scriptName);
            }
            else if (actionType == LogAction.PlayerHitToAttachable)
            {
                return String.Format("{0} hit to a attachable(cart)", FormatLogForPlayer(issuer, dateTime));
            }
            else if (actionType == LogAction.PlayerRepairesTheDestructable)
            {
                string scriptName = (string)oParams[0];
                return String.Format("{0} repairs a destructable with a script of {1}", FormatLogForPlayer(issuer, dateTime), scriptName);
            }
            else if (actionType == LogAction.PlayerSpawn)
            {
                // Agent agent = (Agent)oParams[0];
                return String.Format("{0} spawned in the world", FormatLogForPlayer(issuer, dateTime));
            }
            else if (actionType == LogAction.PlayerDied)
            {
                // Agent agent = (Agent)oParams[0];
                return String.Format("{0} just died", FormatLogForPlayer(issuer, dateTime));
            }
            else if(actionType == LogAction.PlayerDroppedGold)
            {
                int amount = (int)oParams[0];
                return String.Format("{0} dropped a gold bag with amount of {1}", FormatLogForPlayer(issuer, dateTime), amount);

            }
            else if (actionType == LogAction.PlayerPickedUpGold)
            {
                int amount = (int)oParams[0];
                return String.Format("{0} picked up a gold bag with amount of {1}", FormatLogForPlayer(issuer, dateTime), amount);
            }else if(actionType == LogAction.PlayerDroppedItem)
            {
                string droppedToInventory = (string)oParams[0];
                string inventory = (string)oParams[1];
                ItemObject item = (ItemObject)oParams[2];
                int droppedCount = (int)oParams[3];
                string itemName = item == null ? "null" : item.Name.ToString();
                return String.Format("{0} dropped an item({1}*{2}) from inventory {3} to loot inventory {4}", FormatLogForPlayer(issuer, dateTime), itemName, droppedCount, inventory, droppedToInventory);
            }
            else if (actionType == LogAction.PlayerBecomesGodlike)
            {
                return String.Format("{0} became godlike", FormatLogForPlayer(issuer, dateTime));
            }else if(actionType == LogAction.PlayerSpawnsItem)
            {
                ItemObject item = (ItemObject)oParams[0];
                int count = (int)oParams[1];
                return String.Format("{0} spawned an item {1}*{2}", FormatLogForPlayer(issuer, dateTime), item.Name, count);
            }
            else if (actionType == LogAction.PlayerSpawnsItem)
            {
                ItemObject item = (ItemObject)oParams[0];
                int count = (int)oParams[1];
                return String.Format("{0} spawned an item {1}*{2}", FormatLogForPlayer(issuer, dateTime), item.Name, count);
            }
            else if (actionType == LogAction.PlayerItemGathers)
            {
                ItemObject item = (ItemObject)oParams[0];
                return String.Format("{0} gathered an item from ItemGathering script {1}", FormatLogForPlayer(issuer, dateTime), item.Name);
            }else if(actionType == LogAction.PlayerBansPlayer)
            {
                return String.Format("{0} banned a player: {1}", FormatLogForPlayer(issuer, dateTime), AffectedPlayersToString(affectedPlayers));
            }
            else if (actionType == LogAction.PlayerTempBanPlayer)
            {
                return String.Format("{0} temp banned a player: {1}", FormatLogForPlayer(issuer, dateTime), AffectedPlayersToString(affectedPlayers));
            }
            else if (actionType == LogAction.PlayerKicksPlayer)
            {
                return String.Format("{0} kicked a player: {1}", FormatLogForPlayer(issuer, dateTime), AffectedPlayersToString(affectedPlayers));
            }
            else if (actionType == LogAction.PlayerFadesPlayer)
            {
                return String.Format("{0} fade a player: {1}", FormatLogForPlayer(issuer, dateTime), AffectedPlayersToString(affectedPlayers));
            }else if(actionType == LogAction.PlayerSpawnedMoney)
            {
                int amount = (int)oParams[0];
                return String.Format("{0} spawned money with amount {1}", FormatLogForPlayer(issuer, dateTime), null, amount);
            }else if(actionType == LogAction.PlayerDespawnedPrefab)
            {
                SpawnableItem spawnableItem = (SpawnableItem)oParams[0];
                return String.Format("{0} de-spawned a prefab with item {1}", FormatLogForPlayer(issuer, dateTime), spawnableItem.SpawnerItem.Name);
            }
            else if (actionType == LogAction.PlayerSlayPlayer)
            {
                return String.Format("{0} slayed a player: {1}", FormatLogForPlayer(issuer, dateTime), AffectedPlayersToString(affectedPlayers));
            }
            else if (actionType == LogAction.PlayerFreezePlayer)
            {
                return String.Format("{0} Freezed a player: {1}", FormatLogForPlayer(issuer, dateTime), AffectedPlayersToString(affectedPlayers));
            }
            else if (actionType == LogAction.PlayerUnfreezePlayer)
            {
                return String.Format("{0} Unfreezed a player: {1}", FormatLogForPlayer(issuer, dateTime), AffectedPlayersToString(affectedPlayers));
            }
            else if (actionType == LogAction.PlayerTpToPlayer)
            {
                return String.Format("{0} teleported to a player: {1}", FormatLogForPlayer(issuer, dateTime), AffectedPlayersToString(affectedPlayers));
            }
            else if (actionType == LogAction.PlayerTpToMePlayer)
            {
                return String.Format("{0} teleported to himself a player: {1}", FormatLogForPlayer(issuer, dateTime), AffectedPlayersToString(affectedPlayers));
            }else if(actionType == LogAction.PlayerHealedPlayer)
            {
                return String.Format("{0} teleported to healed a player: {1}", FormatLogForPlayer(issuer, dateTime), AffectedPlayersToString(affectedPlayers));
            }else if(actionType == LogAction.PlayerRevealedMoneyPouch)
            {
                int amount = (int)oParams[0];
                return String.Format("{0} revealed his money pouch({1}) Receivers: {2}", FormatLogForPlayer(issuer, dateTime), amount, AffectedPlayersToString(affectedPlayers));
            }
            else if (actionType == LogAction.PlayerRevealedItemBag)
            {
                return String.Format("{0} revealed his item bag Receivers: {1}", FormatLogForPlayer(issuer, dateTime), AffectedPlayersToString(affectedPlayers));
            }else if(actionType == LogAction.PlayerCommitedSuicide)
            {
                return String.Format("{0} commited suicide", FormatLogForPlayer(issuer, dateTime));
            } else if(actionType == LogAction.PlayerDepositedToBank)
            {
                int amount = (int)oParams[0];
                return String.Format("{0} deposited {1} amount of gold to bank", FormatLogForPlayer(issuer, dateTime), amount);
            }
            else if (actionType == LogAction.PlayerWithdrawToBank)
            {
                int amount = (int)oParams[0];
                return String.Format("{0} withdrawn {1} amount of gold from bank", FormatLogForPlayer(issuer, dateTime), amount);
            } else if(actionType == LogAction.PlayerMountedHorse)
            {
                Agent agent = (Agent)oParams[0];
                return String.Format("{0} mounted a horse", FormatLogForAgent(agent, dateTime));
            }else if(actionType == LogAction.PlayerDismountedHorse)
            {
                Agent agent = (Agent)oParams[0];
                return String.Format("{0} dismounted a horse", FormatLogForAgent(agent, dateTime));
            }else if(actionType == LogAction.PlayerChangedName)
            {
                string name = (string)oParams[0];
                return String.Format("{0} changed his name to {1}", FormatLogForPlayer(issuer, dateTime), name);
            }
            return actionType;
        }

        public static string GetCoordinatesOfPlayer(NetworkCommunicator player)
        {
            if(player.ControlledAgent != null && player.ControlledAgent.IsActive())
            {
                return String.Format("({0},{1},{2})", player.ControlledAgent.Position.X, player.ControlledAgent.Position.Y, player.ControlledAgent.Position.Z);
            }
            return "(?,?,?)";
        }

        public static void LogAnAction(NetworkCommunicator issuer, string actionType, AffectedPlayer[] affectedPlayers = null, object[] oParams = null)
        {
            affectedPlayers = affectedPlayers ?? new AffectedPlayer[] { };
            oParams = oParams ?? new object[] { };

            string logMessage = GenerateActionMessage(issuer, actionType, DateTime.UtcNow, affectedPlayers, oParams);

            DBLog dbLog = new DBLog()
            {
                ActionType = actionType,
                AffectedPlayers = new Json<AffectedPlayer[]>(affectedPlayers),
                CreatedAt = DateTime.UtcNow,
                IssuerCoordinates = GetCoordinatesOfPlayer(issuer),
                IssuerPlayerId = issuer.VirtualPlayer.Id.ToString(),
                IssuerPlayerName = issuer.UserName,
                LogMessage = logMessage
            };
            if(OnLogAction != null)
            {
                OnLogAction(dbLog);
            }
        }
    }
}

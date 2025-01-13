using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Linq;
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
        public static readonly string PlayerTransferredItemToInventory = "PlayerTransferredItemToInventory"; // Done
        public static readonly string PlayerTransferredItemFromInventory = "PlayerTransferredItemFromInventory"; // Done
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

        private static string AffectedPlayersToString(AffectedPlayer[] players)
        {
            string[] str = new string[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                str[i] = players[i].ToString();
            }
            return String.Join(",", str);
        }

        private static string FormatLogForAgent(Agent agent, DateTime dateTime)
        {
            if (agent.IsHuman && agent.IsPlayerControlled)
            {
                return FormatLogForPlayer(agent.MissionPeer.GetNetworkPeer(), dateTime, false);
            }
            else if (agent.IsMount && agent.RiderAgent != null && agent.RiderAgent.IsHuman && agent.RiderAgent.IsPlayerControlled)
            {
                return String.Format("A Horse Rided By {0}", FormatLogForPlayer(agent.RiderAgent.MissionPeer.GetNetworkPeer(), dateTime, false));
            }
            else if (agent.IsMount)
            {
                return "A Horse";
            }
            return "an Animal";
        }

        public static string FormatLogForPlayer(NetworkCommunicator player, DateTime dateTime, bool logDateTime = true)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            string factionName = persistentEmpireRepresentative != null && persistentEmpireRepresentative.GetFaction() != null ? persistentEmpireRepresentative.GetFaction().name : "Unknown";
            if (logDateTime)
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
            else if (affectedAgent.IsMount && affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.MissionPeer != null && affectedAgent.RiderAgent.MissionPeer.GetNetworkPeer().GetComponent<PersistentEmpireRepresentative>() != null)
            {
                return affectedAgent.RiderAgent.MissionPeer.GetNetworkPeer();
            }
            return null;
        }

        private static string GenerateActionMessage(NetworkCommunicator issuer, string actionType, DateTime dateTime, AffectedPlayer[] affectedPlayers = null, object[] oParams = null)
        {
            switch (actionType)
            {
                case nameof(LogAction.PlayerJoined):
                    return $"{FormatLogForPlayer(issuer, dateTime)} joined to the server.";
                case nameof(LogAction.PlayerDisconnected):
                    return $"{FormatLogForPlayer(issuer, dateTime)} disconnected from the server.";
                case nameof(LogAction.LocalChat):
                case nameof(LogAction.TeamChat):
                    return $"{FormatLogForPlayer(issuer, dateTime)} said \"[{(string)oParams[0]}]\". Receivers: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerHitToAgent):
                case nameof(LogAction.PlayerBumpedWithHorse):
                case nameof(LogAction.PlayerKilledAnAgent):
                    return HandlePlayerAttackAgent(issuer, dateTime, (MissionWeapon)oParams[0], (Agent)oParams[1], actionType);
                case nameof(LogAction.PlayerDroppedLoot):
                    return HandlePlayerDroppedLoot(issuer, dateTime, (Inventory)oParams[0]);
                case nameof(LogAction.PlayerFactionChange):
                    return $"{FormatLogForPlayer(issuer, dateTime)} is joined to faction {((Faction)oParams[0])?.name ?? "Unknown"} from {((Faction)oParams[1])?.name ?? "Unknown"}";
                case nameof(LogAction.PlayerClassChange):
                    return $"{FormatLogForPlayer(issuer, dateTime)} became a {((BasicCharacterObject)oParams[0]).Name}";
                case nameof(LogAction.FactionLordChanged):
                    return $"{FormatLogForPlayer(issuer, dateTime)} became lord";
                case nameof(LogAction.FactionDeclaredWar):
                    return $"{FormatLogForPlayer(issuer, dateTime)} declared war to {((Faction)oParams[0])?.name ?? "Unknown"}";
                case nameof(LogAction.FactionMadePeace):
                    return $"{FormatLogForPlayer(issuer, dateTime)} made peace with {((Faction)oParams[0])?.name ?? "Unknown"}";
                case nameof(LogAction.PlayerOpensChest):
                case nameof(LogAction.PlayerClosesChest):
                    return $"{FormatLogForPlayer(issuer, dateTime)} {(actionType == LogAction.PlayerOpensChest ? "opened" : "closed")} a chest/loot of {((Inventory)oParams[0]).InventoryId}";
                case nameof(LogAction.PlayerTransferredItemToChest):
                    return $"{FormatLogForPlayer(issuer, dateTime)} transferred {((ItemObject)oParams[1])?.Name.ToString() ?? "null"}*{(int)oParams[2]} to the chest({(string)oParams[0]})";
                case nameof(LogAction.PlayerTransferredItemFromChest):
                    return $"{FormatLogForPlayer(issuer, dateTime)} transferred {((ItemObject)oParams[1])?.Name.ToString() ?? "null"}*{(int)oParams[2]} from the chest({(string)oParams[0]})";
                case nameof(LogAction.PlayerOpensStockpile):
                case nameof(LogAction.PlayerClosesStockpile):
                    return $"{FormatLogForPlayer(issuer, dateTime)} {(actionType == LogAction.PlayerOpensStockpile ? "accessed to" : "closed")} a stockpile market. Xml file of market is {((PE_StockpileMarket)oParams[0]).XmlFile}";
                case nameof(LogAction.PlayerBuysStockpile):
                    return $"{FormatLogForPlayer(issuer, dateTime)} bought {((MarketItem)oParams[0]).Item.Name} from the stockpile market with a price of {((MarketItem)oParams[0]).BuyPrice()}.";
                case nameof(LogAction.PlayerSellsStockpile):
                    return $"{FormatLogForPlayer(issuer, dateTime)} sold {((MarketItem)oParams[0]).Item.Name} to the stockpile market with a price of {((MarketItem)oParams[0]).SellPrice()}.";
                case nameof(LogAction.PlayerSpawnedPrefab):
                    return $"{FormatLogForPlayer(issuer, dateTime)} spawned a prefab with item {((SpawnableItem)oParams[0]).SpawnerItem.Name}";
                case nameof(LogAction.PlayerHitToDestructable):
                    return $"{FormatLogForPlayer(issuer, dateTime)} hit to a destructable with a script of {(string)oParams[0]}";
                case nameof(LogAction.PlayerHitToAttachable):
                    return $"{FormatLogForPlayer(issuer, dateTime)} hit to a attachable(cart)";
                case nameof(LogAction.PlayerRepairesTheDestructable):
                    return $"{FormatLogForPlayer(issuer, dateTime)} repairs a destructable with a script of {(string)oParams[0]}";
                case nameof(LogAction.PlayerSpawn):
                    return $"{FormatLogForPlayer(issuer, dateTime)} spawned in the world";
                case nameof(LogAction.PlayerDied):
                    return $"{FormatLogForPlayer(issuer, dateTime)} just died";
                case nameof(LogAction.PlayerDroppedGold):
                    return $"{FormatLogForPlayer(issuer, dateTime)} dropped a gold bag with amount of {(int)oParams[0]}";
                case nameof(LogAction.PlayerPickedUpGold):
                    return $"{FormatLogForPlayer(issuer, dateTime)} picked up a gold bag with amount of {(int)oParams[0]}";
                case nameof(LogAction.PlayerDroppedItem):
                    return $"{FormatLogForPlayer(issuer, dateTime)} dropped an item({((ItemObject)oParams[2])?.Name.ToString() ?? "null"}*{(int)oParams[3]}) from inventory {(string)oParams[1]} to loot inventory {(string)oParams[0]}";
                case nameof(LogAction.PlayerBecomesGodlike):
                    return $"{FormatLogForPlayer(issuer, dateTime)} became godlike";
                case nameof(LogAction.PlayerSpawnsItem):
                    return $"{FormatLogForPlayer(issuer, dateTime)} spawned an item {((ItemObject)oParams[0]).Name}*{(int)oParams[1]}";
                case nameof(LogAction.PlayerItemGathers):
                    return $"{FormatLogForPlayer(issuer, dateTime)} gathered an item from ItemGathering script {((ItemObject)oParams[0]).Name}";
                case nameof(LogAction.PlayerBansPlayer):
                    return $"{FormatLogForPlayer(issuer, dateTime)} banned a player: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerTempBanPlayer):
                    return $"{FormatLogForPlayer(issuer, dateTime)} temp banned a player: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerKicksPlayer):
                    return $"{FormatLogForPlayer(issuer, dateTime)} kicked a player: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerFadesPlayer):
                    return $"{FormatLogForPlayer(issuer, dateTime)} fade a player: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerSpawnedMoney):
                    return $"{FormatLogForPlayer(issuer, dateTime)} spawned money with amount {(int)oParams[0]}";
                case nameof(LogAction.PlayerDespawnedPrefab):
                    return $"{FormatLogForPlayer(issuer, dateTime)} de-spawned a prefab with item {((SpawnableItem)oParams[0]).SpawnerItem.Name}";
                case nameof(LogAction.PlayerSlayPlayer):
                    return $"{FormatLogForPlayer(issuer, dateTime)} slayed a player: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerFreezePlayer):
                    return $"{FormatLogForPlayer(issuer, dateTime)} Freezed a player: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerUnfreezePlayer):
                    return $"{FormatLogForPlayer(issuer, dateTime)} Unfreezed a player: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerTpToPlayer):
                    return $"{FormatLogForPlayer(issuer, dateTime)} teleported to a player: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerTpToMePlayer):
                    return $"{FormatLogForPlayer(issuer, dateTime)} teleported to himself a player: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerHealedPlayer):
                    return $"{FormatLogForPlayer(issuer, dateTime)} teleported to healed a player: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerRevealedMoneyPouch):
                    return $"{FormatLogForPlayer(issuer, dateTime)} revealed his money pouch({(int)oParams[0]}) Receivers: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerRevealedItemBag):
                    return $"{FormatLogForPlayer(issuer, dateTime)} revealed his item bag Receivers: {AffectedPlayersToString(affectedPlayers)}";
                case nameof(LogAction.PlayerCommitedSuicide):
                    return $"{FormatLogForPlayer(issuer, dateTime)} commited suicide";
                case nameof(LogAction.PlayerDepositedToBank):
                    return $"{FormatLogForPlayer(issuer, dateTime)} deposited {(int)oParams[0]} amount of gold to bank";
                case nameof(LogAction.PlayerWithdrawToBank):
                    return $"{FormatLogForPlayer(issuer, dateTime)} withdrawn {(int)oParams[0]} amount of gold from bank";
                case nameof(LogAction.PlayerMountedHorse):
                    return $"{FormatLogForAgent((Agent)oParams[0], dateTime)} mounted a horse";
                case nameof(LogAction.PlayerDismountedHorse):
                    return $"{FormatLogForAgent((Agent)oParams[0], dateTime)} dismounted a horse";
                case nameof(LogAction.PlayerChangedName):
                    return $"{FormatLogForPlayer(issuer, dateTime)} changed his name to {(string)oParams[0]}";
                default:
                    return actionType;
            }
        }

        private static string HandlePlayerAttackAgent(NetworkCommunicator issuer, DateTime dateTime, MissionWeapon missionWeapon, Agent affectedAgent, string actionType)
        {
            string attackedItem = missionWeapon.Item?.Name?.ToString() ?? "fist";
            string warStatus = GetWarStatus(issuer, affectedAgent);

            switch (actionType)
            {
                case nameof(LogAction.PlayerHitToAgent):
                    return $"{FormatLogForPlayer(issuer, dateTime)} attacked with {attackedItem} to {FormatLogForAgent(affectedAgent, dateTime)} Their faction's was {warStatus}";
                case nameof(LogAction.PlayerBumpedWithHorse):
                    return $"{FormatLogForPlayer(issuer, dateTime)} bumped with a horse to {FormatLogForAgent(affectedAgent, dateTime)} Their faction's was {warStatus}";
                case nameof(LogAction.PlayerKilledAnAgent):
                    return $"{FormatLogForPlayer(issuer, dateTime)} killed {FormatLogForAgent(affectedAgent, dateTime)} with {attackedItem} Their faction's was {warStatus}";
                default:
                    return string.Empty;
            }
        }

        private static string GetWarStatus(NetworkCommunicator issuer, Agent affectedAgent)
        {
            NetworkCommunicator affectedPeer = GetAffectedPeerFromAgent(affectedAgent);
            if (affectedPeer == null)
            {
                return "Neutral";
            }

            PersistentEmpireRepresentative persistentEmpireRepresentative = affectedPeer.GetComponent<PersistentEmpireRepresentative>();
            PersistentEmpireRepresentative issuerRepr = issuer.GetComponent<PersistentEmpireRepresentative>();

            if (issuerRepr.GetFaction() == null || persistentEmpireRepresentative.GetFaction() == null)
            {
                return "Neutral";
            }

            return issuerRepr.GetFaction().warDeclaredTo.Contains(persistentEmpireRepresentative.GetFactionIndex()) ||
                   persistentEmpireRepresentative.GetFaction().warDeclaredTo.Contains(issuerRepr.GetFactionIndex())
                ? "Enemies"
                : "Neutral";
        }

        private static string HandlePlayerDroppedLoot(NetworkCommunicator issuer, DateTime dateTime, Inventory inventory)
        {
            string[] droppedLootStr = inventory.Slots
                .Where(s => !s.IsEmpty())
                .Select(s => $"{s.Item}*{s.Count}")
                .ToArray();

            string droppedLoot = string.Join(",", droppedLootStr);
            return $"{FormatLogForPlayer(issuer, dateTime)} is died and drop({inventory.InventoryId}) the following loot: {droppedLoot}";
        }

        public static string GetCoordinatesOfPlayer(NetworkCommunicator player)
        {
            if (player.ControlledAgent != null && player.ControlledAgent.IsActive())
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
                IssuerPlayerId = issuer.VirtualPlayer.ToPlayerId(),
                IssuerPlayerName = issuer.UserName.EncodeSpecialMariaDbChars(),
                LogMessage = logMessage.EncodeSpecialMariaDbChars()
            };
            if (OnLogAction != null)
            {
                OnLogAction(dbLog);
            }
        }
    }
}

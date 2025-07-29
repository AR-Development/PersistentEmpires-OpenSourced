using FluentMigrator;
using Org.BouncyCastle.Utilities;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using static System.Net.Mime.MediaTypeNames;
using static TaleWorlds.MountAndBlade.MultiplayerOptions;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System;
using TaleWorlds.Core;
using TaleWorlds.PlayerServices;
using PersistentEmpiresLib.NetworkMessages.Client;
using System.Security.AccessControl;
using System.Xml.Linq;
using PersistentEmpiresLib.Factions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Runtime.InteropServices;
using FluentMigrator.Runner.Versioning;
using static PersistentEmpiresSave.Database.Repositories.DBWhitelistRepository;

namespace PersistentEmpiresSave.Database.Migrations
{
    [Migration(20221912001)]
    public class InitialTables_20221912001 : Migration
    {
        public override void Down()
        {
            Delete.Table("BanRecords");
            Delete.Table("Castles");
            Delete.Table("Factions");
            Delete.Table("HorseMarkets");
            Delete.Table("Identifiers");
            Delete.Table("Inventories");
            Delete.Table("Logs");
            Delete.Table("PlayerNames");
            Delete.Table("Players");
            Delete.Table("StockpileMarkets");
            Delete.Table("UpgradeableBuildings");
            Delete.Table("VersionInfo");
            Delete.Table("Whitelist");
            Delete.Table("Players");
            Delete.Table("Inventories");
            Delete.Table("Factions");
            Delete.Table("Castles");
            Delete.Table("UpgradeableBuildings");
            Delete.Table("StockpileMarkets");

        }

        public override void Up()
        {
            Create.Table("Players")
                .WithColumn("PlayerId").AsString().PrimaryKey().NotNullable()
                .WithColumn("DiscordId").AsString().Nullable()
                .WithColumn("Name").AsString(512).NotNullable()
                .WithColumn("Hunger").AsInt32().WithDefaultValue(0)
                .WithColumn("Health").AsInt32().WithDefaultValue(0)
                .WithColumn("Money").AsInt32().WithDefaultValue(0)
                .WithColumn("Horse").AsString().Nullable()
                .WithColumn("BankAmount").AsInt32().WithDefaultValue(0)
                .WithColumn("HorseHarness").AsString().Nullable()
                .WithColumn("Equipment_0").AsString().Nullable()
                .WithColumn("Equipment_1").AsString().Nullable()
                .WithColumn("Equipment_2").AsString().Nullable()
                .WithColumn("Equipment_3").AsString().Nullable()
                .WithColumn("Armor_Head").AsString().Nullable()
                .WithColumn("Armor_Body").AsString().Nullable()
                .WithColumn("Armor_Leg").AsString().Nullable()
                .WithColumn("Armor_Gloves").AsString().Nullable()
                .WithColumn("Armor_Cape").AsString().Nullable()
                .WithColumn("FactionIndex").AsInt32().WithDefaultValue(0)
                .WithColumn("Class").AsString().WithDefaultValue(PersistentEmpireBehavior.DefaultClass)
                .WithColumn("PosX").AsFloat().WithDefaultValue(0)
                .WithColumn("PosY").AsFloat().WithDefaultValue(0)
                .WithColumn("PosZ").AsFloat().WithDefaultValue(0)
                .WithColumn("Ammo_0").AsInt32().WithDefaultValue(0)
                .WithColumn("Ammo_1").AsInt32().WithDefaultValue(0)
                .WithColumn("Ammo_2").AsInt32().WithDefaultValue(0)
                .WithColumn("Ammo_3").AsInt32().WithDefaultValue(0)
                .WithColumn("CustomName").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("UpdatedAt").AsDateTime().NotNullable()
                .WithColumn("WoundedUntil").AsInt64().Nullable();

            Execute.Sql("ALTER TABLE Players MODIFY COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            Execute.Sql("ALTER TABLE `players` MODIFY `Armor_Body` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `Armor_Cape` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `Armor_Gloves` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `Armor_Head` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `Armor_Leg` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `Class` varchar(255) DEFAULT '\'perp_peasant\'' CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `CustomName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `DiscordId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `Equipment_0` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `Equipment_1` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `Equipment_2` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `Equipment_3` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `Horse` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `HorseHarness` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `Name` varchar(512) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `players` MODIFY `PlayerId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");            

            Create.Table("Inventories")
                .WithColumn("InventoryId").AsString().PrimaryKey().NotNullable()
                .WithColumn("IsPlayerInventory").AsBoolean()
                .WithColumn("InventorySerialized").AsCustom("TEXT").NotNullable()
                .WithColumn("UpdatedAt").AsDateTime().NotNullable();

            Execute.Sql("ALTER TABLE Inventories MODIFY COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            Execute.Sql("ALTER TABLE `inventories` MODIFY `InventoryId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");

            Create.Table("Factions")
                .WithColumn("FactionIndex").AsInt32().PrimaryKey().NotNullable()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("BannerKey").AsString().NotNullable()
                .WithColumn("LordId").AsString().Nullable()
                .WithColumn("PollUnlockedAt").AsInt64().WithDefaultValue(0)
                .WithColumn("Marshalls").AsCustom("TEXT").Nullable();

            Execute.Sql("ALTER TABLE `factions` MODIFY `BannerKey` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `factions` MODIFY `LordId` varchar(255)  CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `factions` MODIFY `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            
            Create.Table("Castles")
                .WithColumn("CastleIndex").AsInt32().PrimaryKey().NotNullable()
                .WithColumn("FactionIndex").AsInt32().NotNullable().WithDefaultValue(0);

            Create.Table("UpgradeableBuildings")
                .WithColumn("MissionObjectHash").AsString().PrimaryKey().NotNullable()
                .WithColumn("IsUpgrading").AsBoolean().WithDefaultValue(false)
                .WithColumn("CurrentTier").AsInt32().NotNullable().WithDefaultValue(0)
                .WithColumn("UpdatedAt").AsDateTime().NotNullable();

            Execute.Sql("ALTER TABLE UpgradeableBuildings MODIFY COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            Execute.Sql("ALTER TABLE `upgradeablebuildings` MODIFY `MissionObjectHash` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");

            Create.Table("StockpileMarkets")
                .WithColumn("MissionObjectHash").AsString().PrimaryKey().NotNullable()
                .WithColumn("MarketItemsSerialized").AsCustom("TEXT")
                .WithColumn("UpdatedAt").AsDateTime().NotNullable();

            Execute.Sql("ALTER TABLE StockpileMarkets MODIFY COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            Execute.Sql("ALTER TABLE `stockpilemarkets` MODIFY `MissionObjectHash` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");

            Create.Table("HorseMarkets")
                .WithColumn("MissionObjectHash").AsString().PrimaryKey().NotNullable()
                .WithColumn("Stock").AsInt32()
                .WithColumn("UpdatedAt").AsDateTime().NotNullable();

            Execute.Sql("ALTER TABLE HorseMarkets MODIFY COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            Execute.Sql("ALTER TABLE `horsemarkets` MODIFY `MissionObjectHash` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");

            Create.Table("Logs")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("IssuerPlayerId").AsString()
                .WithColumn("IssuerPlayerName").AsString()
                .WithColumn("ActionType").AsString()
                .WithColumn("IssuerCoordinates").AsString()
                .WithColumn("LogMessage").AsCustom("TEXT")
                .WithColumn("AffectedPlayers").AsCustom("JSON");

            Execute.Sql("ALTER TABLE `logs` MODIFY `ActionType` varchar(255)  CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `logs` MODIFY `IssuerCoordinates` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `logs` MODIFY `IssuerPlayerId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `logs` MODIFY `IssuerPlayerName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
                        
            Create.Table("BanRecords")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("PlayerId").AsString()
                .WithColumn("PlayerName").AsString()
                .WithColumn("BanReason").AsCustom("TEXT").Nullable()
                .WithColumn("UnbanReason").AsCustom("TEXT").Nullable()
                .WithColumn("BannedBy").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("BanEndsAt").AsDateTime().Nullable();
            
            Execute.Sql("ALTER TABLE `banrecords` MODIFY `BannedBy` varchar(255)  CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `banrecords` MODIFY `PlayerId` varchar(255)  CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `banrecords` MODIFY `PlayerName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");

            Create.Table("PlayerNames")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("PlayerName").AsString()
                .WithColumn("PlayerId").AsString();

            Execute.Sql("ALTER TABLE `playernames` MODIFY `PlayerId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `playernames` MODIFY `PlayerName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");

            Create.Table("Whitelist")
                .WithColumn("PlayerId").AsString().PrimaryKey()
                .WithColumn("Active").AsBoolean().WithDefaultValue(true);

            Execute.Sql("ALTER TABLE `whitelist` MODIFY `PlayerId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");

            Create.Table("Identifiers")
                .WithColumn("Identifier").AsString().NotNullable().PrimaryKey()
                .WithColumn("IdentifierType").AsString().Nullable()
                .WithColumn("UserId").AsInt64().NotNullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("UpdatedAt").AsDateTime().NotNullable();

            Execute.Sql("ALTER TABLE Identifiers MODIFY COLUMN UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP");
            Execute.Sql("ALTER TABLE `identifiers` MODIFY `Identifier` varchar(255)  CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci;");
            Execute.Sql("ALTER TABLE `identifiers` MODIFY `IdentifierType` varchar(255)  CHARACTER SET utf8mb4 COLLATE utf8mb4_uca1400_ai_ci");
        }
    }
}
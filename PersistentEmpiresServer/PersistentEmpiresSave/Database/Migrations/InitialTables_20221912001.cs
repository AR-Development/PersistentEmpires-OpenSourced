using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentEmpiresSave.Database.Migrations
{
    [Migration(20221912001)]
    public class InitialTables_20221912001 : Migration
    {
        public override void Down()
        {
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
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("PlayerId").AsString().NotNullable().Unique()
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
                .WithColumn("Class").AsString().WithDefaultValue("pe_peasant")
                .WithColumn("PosX").AsFloat().WithDefaultValue(0)
                .WithColumn("PosY").AsFloat().WithDefaultValue(0)
                .WithColumn("PosZ").AsFloat().WithDefaultValue(0)
                .WithColumn("Ammo_0").AsInt32().WithDefaultValue(0)
                .WithColumn("Ammo_1").AsInt32().WithDefaultValue(0)
                .WithColumn("Ammo_2").AsInt32().WithDefaultValue(0)
                .WithColumn("Ammo_3").AsInt32().WithDefaultValue(0)
                .WithColumn("CustomName").AsString().Nullable();

            Create.Index("PlayerId__Players")
                .OnTable("Players")
                .OnColumn("PlayerId");

            Create.Table("Inventories")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("PlayerId").AsString().Nullable()
                .WithColumn("InventoryId").AsString().NotNullable()
                .WithColumn("IsPlayerInventory").AsBoolean()
                .WithColumn("InventorySerialized").AsCustom("TEXT").NotNullable();

            Create.Index("PlayerId__Inventories")
                .OnTable("Inventories")
                .OnColumn("PlayerId");

            Create.Table("Factions")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("FactionIndex").AsInt32().NotNullable().Unique()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("BannerKey").AsString().NotNullable()
                .WithColumn("LordId").AsString().Nullable()
                .WithColumn("PollUnlockedAt").AsInt64().WithDefaultValue(0)
                .WithColumn("Marshalls").AsCustom("TEXT").Nullable();

            Create.Table("Castles")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("CastleIndex").AsInt32().NotNullable().Unique()
                .WithColumn("FactionIndex").AsInt32().NotNullable().WithDefaultValue(0);

            Create.Table("UpgradeableBuildings")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("MissionObjectHash").AsString().NotNullable()
                .WithColumn("IsUpgrading").AsBoolean().WithDefaultValue(false)
                .WithColumn("CurrentTier").AsInt32().NotNullable().WithDefaultValue(0);

            Create.Table("StockpileMarkets")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("MissionObjectHash").AsString().NotNullable()
                .WithColumn("MarketItemsSerialized").AsCustom("TEXT");

            Create.Table("HorseMarkets")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("MissionObjectHash").AsString().NotNullable()
                .WithColumn("Stock").AsInt32();

            Create.Table("Logs")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("CreatedAt").AsDateTime()
                .WithColumn("IssuerPlayerId").AsString()
                .WithColumn("IssuerPlayerName").AsString()
                .WithColumn("ActionType").AsString() // Hit,Capture,Kill,Gathering,Chat...
                .WithColumn("IssuerCoordinates").AsString()
                .WithColumn("LogMessage").AsCustom("TEXT") // Compiled for chatgpt
                .WithColumn("AffectedPlayers").AsCustom("JSON");

            Create.Table("BanRecords")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("PlayerId").AsString()
                .WithColumn("PlayerName").AsString()
                .WithColumn("BanReason").AsCustom("TEXT").Nullable()
                .WithColumn("UnbanReason").AsCustom("TEXT").Nullable()
                .WithColumn("BannedBy").AsString().Nullable()
                .WithColumn("CreatedAt").AsDateTime().Nullable()
                .WithColumn("BanEndsAt").AsDateTime().Nullable();

            Create.Table("PlayerNames")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("PlayerName").AsString()
                .WithColumn("PlayerId").AsString();
        }
    }
}

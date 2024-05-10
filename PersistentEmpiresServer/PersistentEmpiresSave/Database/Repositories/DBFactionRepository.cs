﻿using Dapper;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Collections.Generic;
using System.Linq;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBFactionRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnGetFactions += GetFactions;
            SaveSystemBehavior.OnCreateOrSaveFaction += CreateOrSaveFaction;
            SaveSystemBehavior.OnGetFaction += GetFaction;
        }
        public static IEnumerable<DBFactions> GetFactions()
        {
            return DBConnection.Connection.Query<DBFactions>("SELECT * FROM Factions");
        }
        private static DBFactions CreateDBFaction(Faction faction, int factionIndex)
        {
            return new DBFactions
            {
                FactionIndex = factionIndex,
                Name = faction.name,
                BannerKey = faction.banner.Serialize(),
                LordId = faction.lordId,
                PollUnlockedAt = faction.pollUnlockedAt,
                Marshalls = faction.SerializeMarshalls()
            };
        }
        public static DBFactions GetFaction(int factionIndex)
        {
            IEnumerable<DBFactions> factions = DBConnection.Connection.Query<DBFactions>("SELECT * FROM Factions WHERE FactionIndex = @FactionIndex", new { FactionIndex = factionIndex });
            if (factions.Count() == 0) return null;
            return factions.First();
        }
        public static DBFactions CreateOrSaveFaction(Faction faction, int factionIndex)
        {
            if (GetFaction(factionIndex) == null)
            {
                return CreateFaction(faction, factionIndex);
            }
            return SaveFaction(faction, factionIndex);
        }
        public static DBFactions CreateFaction(Faction faction, int factionIndex)
        {
            DBFactions dbFaction = CreateDBFaction(faction, factionIndex);
            string insertSql = "INSERT INTO Factions (FactionIndex, Name, BannerKey, LordId) VALUES (@FactionIndex, @Name, @BannerKey, @LordId)";
            DBConnection.Connection.Execute(insertSql, dbFaction);
            return dbFaction;
        }
        public static DBFactions SaveFaction(Faction faction, int factionIndex)
        {
            DBFactions dbFaction = CreateDBFaction(faction, factionIndex);
            string updateSql = "UPDATE Factions SET Name = @Name, BannerKey = @BannerKey, LordId = @LordId, PollUnlockedAt = @PollUnlockedAt, Marshalls = @Marshalls WHERE FactionIndex = @FactionIndex";
            DBConnection.Connection.Execute(updateSql, dbFaction);
            return dbFaction;
        }
    }
}

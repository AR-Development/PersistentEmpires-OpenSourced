/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Dapper;
using PersistentEmpiresLib.Database.DBEntities;
using System;
using System.Linq;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBBanRecordRepository
    {

        public static void Initialize()
        {

        }

        public static void AdminServerBehavior_OnBanPlayer(string PlayerId, string PlayerName, long BanEndsAt)
        {
            BanPlayer(PlayerId, PlayerName, BanEndsAt, "Banned in-game");
        }

        public static void BanPlayer(string playerId, string playerName, long banEndsAt, string banReason)
        {
            DBBanRecord banRecord = new DBBanRecord
            {
                PlayerId = playerId,
                PlayerName = playerName,
                CreatedAt = DateTime.UtcNow,
                BanEndsAt = DateTimeOffset.FromUnixTimeSeconds(banEndsAt).UtcDateTime,
                BanReason = banReason
            };

            string insertSql = "INSERT INTO BanRecords(PlayerId, PlayerName, BanReason, CreatedAt, BanEndsAt) VALUES(@PlayerId, @PlayerName, @BanReason, @CreatedAt, @BanEndsAt)";
            DBConnection.Connection.Execute(insertSql, banRecord);
        }

        public static void UnbanPlayer(string playerId, string unbanReason)
        {
            string updateSql = "UPDATE BanRecords SET BanEndsAt = 0, UnbanReason = @UnbanReason WHERE PlayerId = @PlayerId";
            DBConnection.Connection.Execute(updateSql, new
            {
                UnbanReason = unbanReason,
                PlayerId = playerId
            });
        }
        public static bool IsPlayerBanned(string playerId)
        {
            int count = DBConnection.Connection.Query("SELECT * FROM BanRecords WHERE BanEndsAt >= @CurrentTime AND PlayerId = @PlayerId", new
            {
                CurrentTime = DateTime.UtcNow,
                PlayerId = playerId
            }).Count();
            return count > 0;
        }
    }
}

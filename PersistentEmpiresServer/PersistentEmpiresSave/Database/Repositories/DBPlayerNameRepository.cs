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
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBPlayerNameRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnCreatePlayerNameIfNotExists += CreatePlayerNameIfNotExists;
        }

        public static DBPlayerName CreateDBPlayerName(NetworkCommunicator player)
        {
            return new DBPlayerName
            {
                PlayerId = player.VirtualPlayer.Id.ToString(),
                PlayerName = player.UserName
            };
        }

        public static void CreatePlayerNameIfNotExists(NetworkCommunicator player)
        {
            DBPlayerName playerName = CreateDBPlayerName(player);
            IEnumerable<DBPlayerName> playerNames = DBConnection.Connection.Query<DBPlayerName>("SELECT * FROM PlayerNames WHERE PlayerName = @PlayerName", new
            {
                PlayerName = player.UserName
            });
            if (playerNames.Count() == 0)
            {
                string insertSql = "INSERT INTO PlayerNames (PlayerName, PlayerId) VALUES (@PlayerName, @PlayerId)";
                DBConnection.Connection.Execute(insertSql, playerName);
            }
        }
    }
}

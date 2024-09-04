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
using MySqlConnector;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresSave.Database.Helpers;
using System.Data.Common;

namespace PersistentEmpiresSave.Database
{
    public class DBConnection
    {
        public static DbConnection Connection = null;


        public static void InitializeSqlConnection(string DbConnectionString)
        {
            SqlMapper.AddTypeHandler(new JsonTypeHandler<AffectedPlayer[]>());
            DBConnection.Connection = new MySqlConnection(DbConnectionString);
        }

        public static void ExecuteDapper(string query, object param)
        {
            Connection.Execute(query, param);
        }
    }
}

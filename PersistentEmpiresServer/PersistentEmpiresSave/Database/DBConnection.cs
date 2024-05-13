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

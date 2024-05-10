using Dapper;
using Newtonsoft.Json;
using PersistentEmpiresLib.Database.DBEntities;
using System.Data;

namespace PersistentEmpiresSave.Database.Helpers
{



    public class JsonTypeHandler<T> : SqlMapper.TypeHandler<Json<T>>
    {
        public override void SetValue(IDbDataParameter parameter, Json<T> value)
        {
            parameter.Value = JsonConvert.SerializeObject(value.Value);
        }

        public override Json<T> Parse(object value)
        {
            if (value is string json)
            {
                return new Json<T>(JsonConvert.DeserializeObject<T>(json));
            }

            return new Json<T>(default);
        }
    }
}

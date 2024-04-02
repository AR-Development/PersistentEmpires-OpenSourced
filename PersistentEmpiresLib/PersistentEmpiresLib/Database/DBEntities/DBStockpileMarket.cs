using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentEmpiresLib.Database.DBEntities
{
    public class DBStockpileMarket
    {
        public int Id { get; set; }
        public string MissionObjectHash {get;set;}
        public string MarketItemsSerialized { get; set; }
    }
}

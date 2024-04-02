using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentEmpiresLib.Database.DBEntities
{
    public class DBHorseMarket
    {
        public int Id { get; set; }
        public string MissionObjectHash { get; set; }
        public int Stock { get; set; }
    }
}

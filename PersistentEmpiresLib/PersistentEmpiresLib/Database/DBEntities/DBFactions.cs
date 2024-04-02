using PersistentEmpiresLib.Factions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentEmpiresLib.Database.DBEntities
{
    public class DBFactions
    {
        public int Id { get; set; }
        public int FactionIndex { get; set; }
        public string Name { get; set; }
        public string BannerKey { get; set; }
        public string LordId { get; set; }
        public long PollUnlockedAt { get; set; }
        public string Marshalls { get; set; }
        
    }
}

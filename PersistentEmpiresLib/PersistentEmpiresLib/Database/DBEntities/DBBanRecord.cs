using System;

namespace PersistentEmpiresLib.Database.DBEntities
{
    public class DBBanRecord
    {
        public int Id { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string BanReason { get; set; }
        public string UnbanReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime BanEndsAt { get; set; }
    }
}

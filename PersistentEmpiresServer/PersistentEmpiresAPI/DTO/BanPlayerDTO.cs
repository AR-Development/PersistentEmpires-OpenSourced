using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersistentEmpiresAPI.DTO
{
    public class BanPlayerDTO
    {
        public string PlayerId { get; set; }
        public long BanEndsAt { get; set; }
        public string BanReason { get; set; }
    }
}

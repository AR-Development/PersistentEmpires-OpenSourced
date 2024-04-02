using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.Database.DBEntities
{
    public class DBInventory
    {
        public int Id { get; set; }
        public string PlayerId { get; set; }
        public string InventoryId { get; set; }
        public bool IsPlayerInventory { get; set; }
        public string InventorySerialized { get; set; }


      
    }
}

namespace PersistentEmpiresLib.Database.DBEntities
{
    public class DBInventory
    {
        public string InventoryId { get; set; }
        public bool IsPlayerInventory { get; set; }
        public string InventorySerialized { get; set; }
    }
}
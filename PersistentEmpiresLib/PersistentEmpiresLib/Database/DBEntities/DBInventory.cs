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

namespace PersistentEmpiresLib.Database.DBEntities
{
    public class DBUpgradeableBuilding
    {
        public int Id { get; set; }
        public string MissionObjectHash { get; set; }
        public bool IsUpgrading { get; set; }
        public int CurrentTier { get; set; }
    }
}

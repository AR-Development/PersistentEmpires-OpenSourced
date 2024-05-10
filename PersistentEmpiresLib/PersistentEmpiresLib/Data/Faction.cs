using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.Factions
{
    public class Faction
    {
        public BasicCultureObject basicCultureObject { get; private set; }
        // public Team team { get; private set; }
        public String name { get; set; }

        public List<NetworkCommunicator> members = new List<NetworkCommunicator>();
        // virtual player id
        public string lordId { get; set; }

        public Team team { get; set; }
        public Banner banner { get; set; }
        // virtual player id's
        public List<string> doorManagers { get; set; }
        public List<string> chestManagers { get; set; }
        public List<string> marshalls { get; set; }
        public List<int> warDeclaredTo { get; set; }
        public long pollUnlockedAt { get; set; }


        public string SerializeMarshalls()
        {
            return string.Join("|", this.marshalls);
        }

        public void LoadMarshallsFromSerialized(string serialized)
        {
            if (serialized == null) this.marshalls = new List<string>();
            else this.marshalls = serialized.Split('|').ToList<string>();
        }


        public Faction(BasicCultureObject basicCultureObject, Banner banner, String name)
        {
            this.basicCultureObject = basicCultureObject;
            this.name = name;
            this.banner = banner;
            this.lordId = "0";
            this.doorManagers = new List<string>();
            this.chestManagers = new List<string>();
            this.warDeclaredTo = new List<int>();
            this.marshalls = new List<string>();
        }
        public Faction(Banner banner, String name)
        {
            this.basicCultureObject = MBObjectManager.Instance.GetObject<BasicCultureObject>("empire");
            this.name = name;
            this.banner = banner;
            this.lordId = "0";
            this.doorManagers = new List<string>();
            this.chestManagers = new List<string>();
            this.warDeclaredTo = new List<int>();
            this.marshalls = new List<string>();
        }
    }
}

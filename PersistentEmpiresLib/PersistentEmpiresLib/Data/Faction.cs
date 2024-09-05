/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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

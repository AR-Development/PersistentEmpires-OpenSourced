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

namespace PersistentEmpiresLib.Database.DBEntities
{
    public class DBPlayer
    {
        public int Id { get; set; }
        public string PlayerId { get; set; }
        public string DiscordId { get; set; }
        public string Name { get; set; }
        public int BankAmount { get; set; }
        public int Hunger { get; set; }
        public int Health { get; set; }
        public int Money { get; set; }
        public int FactionIndex { get; set; }
        public string Class { get; set; }
        public string Horse { get; set; }
        public string HorseHarness { get; set; }
        public string Equipment_0 { get; set; }
        public string Equipment_1 { get; set; }
        public string Equipment_2 { get; set; }
        public string Equipment_3 { get; set; }

        public int Ammo_0 { get; set; }
        public int Ammo_1 { get; set; }
        public int Ammo_2 { get; set; }
        public int Ammo_3 { get; set; }
        public string Armor_Head { get; set; }
        public string Armor_Body { get; set; }
        public string Armor_Leg { get; set; }
        public string Armor_Gloves { get; set; }
        public string Armor_Cape { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public string CustomName { get; set; }
    }
}

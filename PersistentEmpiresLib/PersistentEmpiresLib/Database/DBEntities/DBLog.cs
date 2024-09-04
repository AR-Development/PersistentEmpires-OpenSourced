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

using PersistentEmpiresLib.Helpers;
using System;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.Database.DBEntities
{

    public class AffectedPlayer
    {
        public AffectedPlayer(NetworkCommunicator player)
        {
            this.Coordinates = LoggerHelper.GetCoordinatesOfPlayer(player);
            this.SteamId = player.VirtualPlayer.Id.ToString();
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            this.FactionName = persistentEmpireRepresentative != null && persistentEmpireRepresentative.GetFaction() != null ? persistentEmpireRepresentative.GetFaction().name : "Unknown";
            this.PlayerName = player.UserName;
        }
        public string SteamId { get; set; }
        public string FactionName { get; set; }
        public string PlayerName { get; set; }
        public string Coordinates { get; set; }

        public override string ToString()
        {
            return String.Format("[{0}][{1}]", this.FactionName, this.PlayerName);
        }
    }
    public class DBLog
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string IssuerPlayerId { get; set; }
        public string IssuerPlayerName { get; set; }
        public string IssuerCoordinates { get; set; }
        public string ActionType { get; set; }
        public string LogMessage { get; set; }
        public Json<AffectedPlayer[]> AffectedPlayers { get; set; }
    }
}

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

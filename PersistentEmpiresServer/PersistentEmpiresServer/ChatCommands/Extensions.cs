using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresServer.ChatCommands.Commands;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands
{
    internal static class Extensions
    {
        internal static void SendMessageToPlayers(this Command command, NetworkCommunicator player, int distance, string message)
        {
            var position = player.ControlledAgent.Position;
            var  affectedPlayers = new List<AffectedPlayer>();

            foreach (NetworkCommunicator otherPlayer in GameNetwork.NetworkPeers)
            {
                if (otherPlayer.ControlledAgent == null) continue;

                var otherPlayerPosition = otherPlayer.ControlledAgent.Position;
                var d = position.Distance(otherPlayerPosition);
                
                if (d < distance)
                {
                    GameNetwork.BeginModuleEventAsServer(otherPlayer);
                    GameNetwork.WriteMessage(new CustomBubbleMessage(player, message));
                    GameNetwork.EndModuleEventAsServer();
                    if (otherPlayer != player)
                    {
                        affectedPlayers.Add(new AffectedPlayer(otherPlayer));
                    }
                }
            }

            LoggerHelper.LogAnAction(player, LogAction.LocalChat, affectedPlayers.ToArray(), new object[] { message });
        }
    }
}

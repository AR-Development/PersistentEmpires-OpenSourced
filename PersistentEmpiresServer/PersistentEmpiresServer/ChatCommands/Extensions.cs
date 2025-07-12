using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ChatCommands.Commands;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands
{
    internal static class Extensions
    {
        internal static void SendMessageToPlayers(this Command command, NetworkCommunicator player, int distance, string message, uint color, bool bubble, string logAction)
        {
            var position = player.ControlledAgent.Position;
            var affectedPlayers = new List<AffectedPlayer>();

            foreach (NetworkCommunicator otherPlayer in GameNetwork.NetworkPeers)
            {
                if (otherPlayer.ControlledAgent == null) continue;

                var otherPlayerPosition = otherPlayer.ControlledAgent.Position;
                var d = position.Distance(otherPlayerPosition);

                if (d < distance)
                {
                    InformationComponent.Instance.SendMessage(message, color, player);

                    if (bubble)
                    {
                        GameNetwork.BeginModuleEventAsServer(otherPlayer);
                        GameNetwork.WriteMessage(new CustomBubbleMessage(player, message, color));
                        GameNetwork.EndModuleEventAsServer();
                    }

                    if (otherPlayer != player)
                    {
                        affectedPlayers.Add(new AffectedPlayer(otherPlayer));
                    }
                }
            }
            if (string.IsNullOrEmpty(logAction))
            {
                logAction = LogAction.LocalChat;
            }

            LoggerHelper.LogAnAction(player, logAction, affectedPlayers.ToArray(), new object[] { message });
        }

        internal static void SendMessageToPlayer(this Command command, NetworkCommunicator player, string message, uint color, bool bubble, string logAction)
        {
            var position = player.ControlledAgent.Position;
            var affectedPlayers = new List<AffectedPlayer>();

            if (player.ControlledAgent == null) return;

            InformationComponent.Instance.SendMessage(message, color, player);

            if (bubble)
            {
                GameNetwork.BeginModuleEventAsServer(player);
                GameNetwork.WriteMessage(new CustomBubbleMessage(player, message, color));
                GameNetwork.EndModuleEventAsServer();
            }

            if (string.IsNullOrEmpty(logAction))
            {
                logAction = LogAction.LocalChat;
            }

            LoggerHelper.LogAnAction(player, logAction, affectedPlayers.ToArray(), new object[] { message });
        }
    }
}

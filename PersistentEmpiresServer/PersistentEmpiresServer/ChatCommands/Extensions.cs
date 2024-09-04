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
            var affectedPlayers = new List<AffectedPlayer>();

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

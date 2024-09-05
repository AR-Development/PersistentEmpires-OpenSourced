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

using NetworkMessages.FromServer;
using PersistentEmpiresServer.ServerMissions;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Help : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return "!help";
        }

        public string Description()
        {
            return "Help message";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            string[] commands = ChatCommandSystem.Instance.commands.Keys.ToArray();
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ServerMessage("-==== Command List ===-"));
            GameNetwork.EndModuleEventAsServer();

            foreach (string command in commands)
            {
                Command commandExecutable = ChatCommandSystem.Instance.commands[command];
                if (commandExecutable.CanUse(networkPeer))
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new ServerMessage(command + ": " + commandExecutable.Description()));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
            return true;
        }
    }
}

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

using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchGlobalChat
    {
        public delegate bool PlayerGlobalChatHandler(NetworkCommunicator peer, string message, bool teamOnly);
        public static event PlayerGlobalChatHandler OnGlobalChatReceived;

        public delegate bool ClientEventPlayerMessageAllHandler(NetworkCommunicator networkPeer, NetworkMessages.FromClient.PlayerMessageAll message);
        public static event ClientEventPlayerMessageAllHandler OnClientEventPlayerMessageAll;

        public delegate bool ClientEventPlayerMessageTeamHandler(NetworkCommunicator networkPeer, NetworkMessages.FromClient.PlayerMessageTeam message);
        public static event ClientEventPlayerMessageTeamHandler OnClientEventPlayerMessageTeam;
        public static bool PrefixOnPlayerMessageReceived(NetworkCommunicator networkPeer, string message, bool toTeamOnly)
        {
            if (OnGlobalChatReceived != null)
            {
                return OnGlobalChatReceived(networkPeer, message, toTeamOnly);
            }
            return true;
        }

        public static bool PrefixClientEventPlayerMessageAll(NetworkCommunicator networkPeer, NetworkMessages.FromClient.PlayerMessageAll message)
        {
            if (OnClientEventPlayerMessageAll != null)
            {
                return OnClientEventPlayerMessageAll(networkPeer, message);
            }
            return true;
        }

        public static bool PrefixClientEventPlayerMessageTeam(NetworkCommunicator networkPeer, NetworkMessages.FromClient.PlayerMessageTeam message)
        {
            if (OnClientEventPlayerMessageTeam != null)
            {
                return OnClientEventPlayerMessageTeam(networkPeer, message);
            }
            return true;
        }
    }
}

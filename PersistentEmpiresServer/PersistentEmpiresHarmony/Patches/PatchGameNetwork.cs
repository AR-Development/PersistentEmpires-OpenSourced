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
using System.IO;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchGameNetwork
    {
        public delegate void AddNewPlayerOnServerHandler(ref PlayerConnectionInfo playerConnectionInfo, bool serverPeer, bool isAdmin);
        public static event AddNewPlayerOnServerHandler OnAddNewPlayerOnServer;
        public delegate void HandleServerEventCreatePlayerHandler(CreatePlayer message);
        public static event HandleServerEventCreatePlayerHandler OnHandleServerEventCreatePlayer;
        public static bool PrefixAddNewPlayerOnServer(ref PlayerConnectionInfo playerConnectionInfo, bool serverPeer, bool isAdmin)
        {
            if (OnAddNewPlayerOnServer != null) OnAddNewPlayerOnServer(ref playerConnectionInfo, serverPeer, isAdmin);
            File.AppendAllText("network.txt", "== Add New Player " + playerConnectionInfo.Name + " ==\n");

            return true;
        }

        public static bool PrefixHandleServerEventCreatePlayer(CreatePlayer message)
        {
            if (OnHandleServerEventCreatePlayer != null) OnHandleServerEventCreatePlayer(message);
            return true;
        }

        public static bool PrefixWriteMessage(GameNetworkMessage message)
        {
            Debug.Print("** PE Network ** " + message.GetType().FullName, 0, Debug.DebugColor.Cyan);

            File.AppendAllText("network.txt", message.GetType().FullName + "\n");

            if (message is SynchronizeMissionObject)
            {
                Debug.Print("** PE SYNCRONIZEMISSION ** " + message.GetType().FullName, 0, Debug.DebugColor.Cyan);
                SynchronizeMissionObject sMessage = (SynchronizeMissionObject)message;
                SynchedMissionObject missionObject = (SynchedMissionObject)Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(sMessage.MissionObjectId);

                File.AppendAllText("network.txt", "  => Mission Object Is " + missionObject.GetType().Name + "\n");


            }
            return true;
        }
    }
}

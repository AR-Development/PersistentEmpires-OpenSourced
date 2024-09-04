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

using PersistentEmpiresLib;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;

namespace PersistentEmpiresServer.ServerMissions
{
    public class WhitelistBehavior : MissionNetwork
    {
        public bool IsEnabled { get; private set; }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.IsEnabled = ConfigManager.GetBoolConfig("WhitelistEnabled", false);

            if (this.IsEnabled)
            {
                Debug.Print("** PERSISTENT EMPIRES ** Whitelist Enabled.", 0, Debug.DebugColor.DarkYellow);
            }
        }
        protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator player)
        {
            if (!IsEnabled) return;
            base.HandleLateNewClientAfterSynchronized(player);
            bool isWhitelisted = SaveSystemBehavior.HandleIsPlayerWhitelisted(player);
            if (!isWhitelisted)
            {
                InformationComponent.Instance.SendMessage("You are not whitelisted. Your player id is: " + player.VirtualPlayer.Id.ToString(), Colors.Red.ToUnsignedInteger(), player);
                DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer.KickPlayer(player.VirtualPlayer.Id, false);
            }
        }

    }
}

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

using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_SpawnFrame : SynchedMissionObject
    {
        public int CastleIndex = -1;
        public bool SpawnFromCastle = false;
        public int FactionIndex = 0;

        public PE_CastleBanner GetCastleBanner()
        {
            if (!Mission.Current.GetMissionBehavior<CastlesBehavior>().castles.ContainsKey(this.CastleIndex)) return null;
            return Mission.Current.GetMissionBehavior<CastlesBehavior>().castles[this.CastleIndex];
        }

        public bool CanPeerSpawnHere(NetworkCommunicator peer)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            if (persistentEmpireRepresentative.GetFaction() == null)
            {
                return !this.SpawnFromCastle && (this.FactionIndex == 0 || this.FactionIndex == -1);
            }
            if (this.SpawnFromCastle && this.GetCastleBanner() != null)
            {
                return this.GetCastleBanner().FactionIndex == persistentEmpireRepresentative.GetFactionIndex();
            }

            return this.FactionIndex == persistentEmpireRepresentative.GetFactionIndex();
        }

        public PE_SpawnFrame()
        {

        }
    }
}

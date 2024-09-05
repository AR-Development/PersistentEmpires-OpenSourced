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

using PersistentEmpiresLib.NetworkMessages.Server;
using System.Reflection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    public abstract class PE_UsableSynchedObject : UsableMissionObject
    {
        public void AddBodyFlagsSynchedPE(BodyFlags flags, bool applyToChildren = true)
        {
            if ((base.GameEntity.BodyFlag & flags) != flags)
            {
                if (GameNetwork.IsServerOrRecorder)
                {
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new AddMissionObjectBodyFlagPE(this, flags, applyToChildren));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
                }
                base.GameEntity.AddBodyFlags(flags, applyToChildren);
                // this._initialSynchFlags |= SynchedMissionObject.SynchFlags.SynchBodyFlags;
                FieldInfo synchField = typeof(PE_InventoryEntity).BaseType.BaseType.GetField("_initialSynchFlags", BindingFlags.Instance | BindingFlags.NonPublic);
                SynchedMissionObject.SynchFlags synchFlags = (SynchedMissionObject.SynchFlags)synchField.GetValue(this);
                synchFlags |= SynchedMissionObject.SynchFlags.SynchBodyFlags;
                synchField.SetValue(this, synchFlags);
            }
        }
        public void AddPhysicsSynchedPE(Vec3 initialVelocity, Vec3 angularVelocity, string physicsMaterial)
        {
            GameEntity gameEntity = base.GameEntity;
            gameEntity.AddPhysics(gameEntity.Mass, gameEntity.CenterOfMass, gameEntity.GetBodyShape(), initialVelocity, angularVelocity, PhysicsMaterial.GetFromName(physicsMaterial), false, 0);
            if (GameNetwork.IsServerOrRecorder)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new AddPhysicsToMissionObject(this, initialVelocity, angularVelocity, physicsMaterial));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.AddToMissionRecord, null);
            }
        }
    }
}

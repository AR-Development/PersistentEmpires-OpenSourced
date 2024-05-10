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

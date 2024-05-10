using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System.Reflection;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.MountAndBlade.SynchedMissionObject;

namespace PersistentEmpiresLib.SceneScripts.Extensions
{

    public static class IMoveable_Implementation
    {
        public static void InitiateMoveSynch(this IMoveable moveable)
        {
            SynchedMissionObject synchObject = moveable.GetAttachedObject().GameEntity.GetFirstScriptOfType<SynchedMissionObject>();
            var prop = typeof(SynchedMissionObject).GetField("_initialSynchFlags", BindingFlags.NonPublic | BindingFlags.Instance);
            SynchedMissionObject.SynchFlags syncFlags = (SynchedMissionObject.SynchFlags)prop.GetValue(synchObject);
            syncFlags |= SynchFlags.SynchTransform;
            prop.SetValue(synchObject, syncFlags);
        }
        public static void SyncCurrentFrame(this IMoveable moveable)
        {
            if (GameNetwork.IsServer)
            {
                MatrixFrame currentFrame = moveable.GetAttachedObject().GameEntity.GetFrame();
                moveable.GetAttachedObject().SetFrameSynched(ref currentFrame, GameNetwork.IsClient);
            }
        }

        public static void RequestMovingUp(this IMoveable moveable)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StartMovingUpMoveableMachine(moveable.GetAttachedObject()));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public static void RequestMovingDown(this IMoveable moveable)
        {
            if (GameNetwork.IsClient)
            {
                // Send Start Moving Packet Bro.
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StartMovingDownMoveableMachine(moveable.GetAttachedObject()));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public static void RequestStopMovingUp(this IMoveable moveable)
        {
            if (GameNetwork.IsClient)
            {
                // Send Start Moving Packet Bro.
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StopMovingUpMoveableMachine(moveable.GetAttachedObject()));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public static void RequestStopMovingDown(this IMoveable moveable)
        {
            if (GameNetwork.IsClient)
            {
                // Send Start Moving Packet Bro.
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StopMovingDownMoveableMachine(moveable.GetAttachedObject()));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public static void RequestMovingForward(this IMoveable moveable)
        {
            if (GameNetwork.IsClient)
            {
                // Send Start Moving Packet Bro.
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StartMovingForwardMoveableMachine(moveable.GetAttachedObject()));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public static void RequestStopMovingForward(this IMoveable moveable)
        {
            if (GameNetwork.IsClient)
            {
                //
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StopMovingForwardMoveableMachine(moveable.GetAttachedObject()));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public static void RequestMovingBackward(this IMoveable moveable)
        {
            if (GameNetwork.IsClient)
            {
                //
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StartMovingBackwardMoveableMachine(moveable.GetAttachedObject()));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public static void RequestStopMovingBackward(this IMoveable moveable)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StopMovingBackwardMoveableMachine(moveable.GetAttachedObject()));
                GameNetwork.EndModuleEventAsClient();
            }
        }

        public static void RequestTurningLeft(this IMoveable moveable)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StartTurningLeftMoveableMachine(moveable.GetAttachedObject()));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public static void RequestStopTurningLeft(this IMoveable moveable)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StopTurningLeftMoveableMachine(moveable.GetAttachedObject()));
                GameNetwork.EndModuleEventAsClient();
            }
        }
        public static void RequestTurningRight(this IMoveable moveable)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StartTurningRightMoveableMachine(moveable.GetAttachedObject()));
                GameNetwork.EndModuleEventAsClient();
            }
        }

        public static void RequestStopTurningRight(this IMoveable moveable)
        {
            if (GameNetwork.IsClient)
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new StopTurningRightMoveableMachine(moveable.GetAttachedObject()));
                GameNetwork.EndModuleEventAsClient();
            }
        }

        public static void StartMovingUp(this IMoveable moveable)
        {
            moveable.IsMovingUp = true;

            /*if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new StartMovingUpMoveableMachineServer(moveable.GetAttachedObject(), moveable.GetAttachedObject().GameEntity.GetFrame()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }*/
            // moveable.SyncCurrentFrame();
        }
        public static void StartMovingDown(this IMoveable moveable)
        {
            moveable.IsMovingDown = true;

            /*if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new StartMovingDownMoveableMachineServer(moveable.GetAttachedObject(), moveable.GetAttachedObject().GameEntity.GetFrame()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }*/
            // moveable.SyncCurrentFrame();
        }
        public static void StopMovingUp(this IMoveable moveable)
        {
            moveable.IsMovingUp = false;

            /*if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new StopMovingUpMoveableMachineServer(moveable.GetAttachedObject(), moveable.GetAttachedObject().GameEntity.GetFrame()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }*/
            // moveable.SyncCurrentFrame();
        }
        public static void StopMovingDown(this IMoveable moveable)
        {
            moveable.IsMovingDown = false;

            /*if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new StopMovingDownMoveableMachineServer(moveable.GetAttachedObject(), moveable.GetAttachedObject().GameEntity.GetFrame()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }*/
            // moveable.SyncCurrentFrame();
        }
        public static void StartMovingForward(this IMoveable moveable)
        {
            moveable.IsMovingForward = true;

            /*if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new StartMovingForwardMoveableMachineServer(moveable.GetAttachedObject(), moveable.GetAttachedObject().GameEntity.GetFrame()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }*/
            // moveable.SyncCurrentFrame();
        }
        public static void StopMovingForward(this IMoveable moveable)
        {
            moveable.IsMovingForward = false;

            /*if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new StopMovingForwardMoveableMachineServer(moveable.GetAttachedObject(), moveable.GetAttachedObject().GameEntity.GetFrame()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }*/
            // moveable.SyncCurrentFrame();
        }
        public static void StartMovingBackward(this IMoveable moveable)
        {
            moveable.IsMovingBackward = true;

            /*if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new StartMovingBackwardMoveableMachineServer(moveable.GetAttachedObject(), moveable.GetAttachedObject().GameEntity.GetFrame()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }*/
        }
        public static void StopMovingBackward(this IMoveable moveable)
        {
            moveable.IsMovingBackward = false;

            /*if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new StopMovingBackwardMoveableMachineServer(moveable.GetAttachedObject(), moveable.GetAttachedObject().GameEntity.GetFrame()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }*/
            // moveable.SyncCurrentFrame();
        }

        public static void StartTurningLeft(this IMoveable moveable)
        {
            moveable.IsTurningLeft = true;

            /*if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new StartTurningLeftMoveableMachineServer(moveable.GetAttachedObject(), moveable.GetAttachedObject().GameEntity.GetFrame()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }*/
            // moveable.SyncCurrentFrame();
        }
        public static void StopTurningLeft(this IMoveable moveable)
        {
            moveable.IsTurningLeft = false;

            /*if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new StopTurningLeftMoveableMachineServer(moveable.GetAttachedObject(), moveable.GetAttachedObject().GameEntity.GetFrame()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }*/
            // moveable.SyncCurrentFrame();
        }
        public static void StartTurningRight(this IMoveable moveable)
        {
            moveable.IsTurningRight = true;

            /*if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new StartTurningRightMoveableMachineServer(moveable.GetAttachedObject(), moveable.GetAttachedObject().GameEntity.GetFrame()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }*/
            // moveable.SyncCurrentFrame();
        }

        public static void StopTurningRight(this IMoveable moveable)
        {
            moveable.IsTurningRight = false;

            /*if (GameNetwork.IsServer)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new StopTurningRightMoveableMachineServer(moveable.GetAttachedObject(), moveable.GetAttachedObject().GameEntity.GetFrame()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }*/
            // moveable.SyncCurrentFrame();
        }
        public static bool CanGoThere(this IMoveable moveable, MatrixFrame frame)
        {
            return !moveable.GetAttachedObject().GameEntity.CheckPointWithOrientedBoundingBox(frame.origin);
        }
        public static MatrixFrame MoveObjectTick(this IMoveable moveable, float dt)
        {
            MatrixFrame currentFrame = moveable.GetAttachedObject().GameEntity.GetFrame();

            if (moveable.IsMovingForward && moveable.GetCanAdvance())
            {
                currentFrame.Advance(moveable.GetAdvanceSpeed() * dt * 1);
            }
            if (moveable.IsMovingBackward && moveable.GetCanAdvance())
            {
                currentFrame.Advance(moveable.GetAdvanceSpeed() * dt * -1);
            }
            if (moveable.IsTurningLeft && moveable.GetCanRotate())
            {
                currentFrame.Rotate(moveable.GetRotationSpeed() * dt * 1, Vec3.Up);
            }
            if (moveable.IsTurningRight && moveable.GetCanRotate())
            {
                currentFrame.Rotate(moveable.GetRotationSpeed() * dt * -1, Vec3.Up);
            }
            if (moveable.IsMovingUp && moveable.GetCanElevate())
            {
                currentFrame.Elevate(moveable.GetElevationSpeed() * dt * 1);
            }
            if (moveable.IsMovingDown && moveable.GetCanElevate())
            {
                currentFrame.Elevate(moveable.GetElevationSpeed() * dt * -1);
            }



            Vec3 normalVector = new Vec3();
            float terrainZ = 0;
            moveable.GetAttachedObject().Scene.GetTerrainHeightAndNormal(currentFrame.origin.AsVec2, out terrainZ, out normalVector);

            if (!moveable.GetAlwaysAlignToTerritory() && currentFrame.origin.z <= terrainZ)
            {
                currentFrame.origin.z = terrainZ;
                currentFrame.rotation.u = normalVector;
                currentFrame.rotation.Orthonormalize();
            }
            else if (moveable.GetAlwaysAlignToTerritory() && moveable.GetPilotAgent() != null && currentFrame.origin.z <= terrainZ)
            {
                currentFrame.origin.z = moveable.GetPilotAgent().Position.Z > terrainZ ? moveable.GetPilotAgent().Position.Z : terrainZ;
                currentFrame.rotation.u = normalVector;
                currentFrame.rotation.Orthonormalize();
            }
            else
            {
                if (moveable.GetPilotAgent() != null && moveable.GetAlwaysAlignToTerritory())
                {
                    currentFrame.origin.z = moveable.GetPilotAgent().Position.Z;
                }
                currentFrame.rotation.u = new Vec3(0, 0, 1);
                currentFrame.rotation.Orthonormalize();
            }
            return currentFrame;
            // moveable.GetAttachedObject().GameEntity.SetFrame(ref currentFrame);
        }
    }
}

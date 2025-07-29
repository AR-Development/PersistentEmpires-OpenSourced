using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchMissionNetworkComponent
    {
        public static List<List<MissionObject>> chunkedMissionObjects = new List<List<MissionObject>>();
        public static Queue<SyncingTrack> peerSyncingQueue = new Queue<SyncingTrack>();
        public class SyncingTrack
        {
            public NetworkCommunicator peer;
            public int chunkIndex;
            public bool synced;

            public SyncingTrack(NetworkCommunicator peer, int chunkIndex, bool synced)
            {
                this.peer = peer;
                this.chunkIndex = chunkIndex;
                this.synced = synced;
            }
        }

        public static List<List<T>> ChunkList<T>(int chunkSize, List<T> list)
        {
            if (chunkSize <= 0)
            {
                throw new ArgumentException("Chunk size must be greater than zero.");
            }

            List<List<T>> result = new List<List<T>>();

            for (int i = 0; i < list.Count; i += chunkSize)
            {
                List<T> chunk = list.Skip(i).Take(chunkSize).ToList();
                result.Add(chunk);
            }

            return result;
        }

        private static void SendTeamsToPeer(NetworkCommunicator peer)
        {
            foreach (Team team in Mission.Current.Teams)
            {
                MBDebug.Print("Syncing a team to peer: " + peer.UserName + " Team Index: " + team.TeamIndex.ToString(), 0, Debug.DebugColor.Cyan);
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new AddTeam(team.TeamIndex, team.Side, team.Color, team.Color2, (team.Banner != null) ? BannerCode.CreateFrom(team.Banner).Code : string.Empty, team.IsPlayerGeneral, team.IsPlayerSergeant));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        private static void SendAttachedWeaponsToPeer(NetworkCommunicator networkPeer, Agent agent)
        {
            int attachedWeaponsCount = agent.GetAttachedWeaponsCount();

            for (int i = 0; i < attachedWeaponsCount; i++)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new AttachWeaponToAgent(agent.GetAttachedWeapon(i), agent.Index, agent.GetAttachedWeaponBoneIndex(i), agent.GetAttachedWeaponFrame(i)));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        private static void SendMountAgent(NetworkCommunicator networkPeer, Agent agent)
        {
            Debug.Print("** PE Better Syncing ** Mount Sending [" + agent.Monster.BaseMonster + "] To " + networkPeer.UserName, 0, Debug.DebugColor.Cyan, 17179869184UL);

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new CreateFreeMountAgent(agent, agent.Position, agent.GetMovementDirection()));
            GameNetwork.EndModuleEventAsServer();

            SendAttachedWeaponsToPeer(networkPeer, agent);

            if (!agent.IsActive())
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new MakeAgentDead(agent.Index, agent.State == AgentState.Killed, agent.GetCurrentActionValue(0)));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        private static void SendHumanAgent(NetworkCommunicator networkPeer, Agent agent)
        {
            Debug.Print("** PE Better Syncing ** Sending Human Agent [" + agent.MissionPeer != null ? agent.MissionPeer.DisplayedName : "null" + "] To " + networkPeer.UserName, 0, Debug.DebugColor.Cyan, 17179869184UL);
            Agent agent2 = agent.MountAgent;
            if (agent2 != null && agent2.RiderAgent == null)
            {
                agent2 = null;
            }
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            int index = agent.Index;
            BasicCharacterObject character = agent.Character;
            Monster monster = agent.Monster;
            Equipment spawnEquipment = agent.SpawnEquipment;
            MissionEquipment equipment = agent.Equipment;
            BodyProperties bodyPropertiesValue = agent.BodyPropertiesValue;
            int bodyPropertiesSeed = agent.BodyPropertiesSeed;
            bool isFemale = agent.IsFemale;
            Team team = agent.Team;
            int num = ((team != null) ? team.TeamIndex : (-1));
            Formation formation = agent.Formation;
            int num2 = ((formation != null) ? formation.Index : (-1));
            uint clothingColor = agent.ClothingColor1;
            uint clothingColor2 = agent.ClothingColor2;
            int num3 = ((agent2 != null) ? agent2.Index : (-1));
            Agent mountAgent = agent.MountAgent;
            Equipment equipment2 = ((mountAgent != null) ? mountAgent.SpawnEquipment : null);
            bool flag = agent.MissionPeer != null && agent.OwningAgentMissionPeer == null;
            Vec3 position = agent.Position;
            Vec2 movementDirection = agent.GetMovementDirection();
            MissionPeer missionPeer = agent.MissionPeer;
            NetworkCommunicator networkCommunicator;
            if ((networkCommunicator = ((missionPeer != null) ? missionPeer.GetNetworkPeer() : null)) == null)
            {
                MissionPeer owningAgentMissionPeer = agent.OwningAgentMissionPeer;
                networkCommunicator = ((owningAgentMissionPeer != null) ? owningAgentMissionPeer.GetNetworkPeer() : null);
            }
            GameNetwork.WriteMessage(new CreateAgent(index, character, monster, spawnEquipment, equipment, bodyPropertiesValue, bodyPropertiesSeed, isFemale, num, num2, clothingColor, clothingColor2, num3, equipment2, flag, position, movementDirection, networkCommunicator));
            GameNetwork.EndModuleEventAsServer();
            agent.LockAgentReplicationTableDataWithCurrentReliableSequenceNo(networkPeer);
            if (agent2 != null)
            {
                agent2.LockAgentReplicationTableDataWithCurrentReliableSequenceNo(networkPeer);
            }
            for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
            {
                for (int j = 0; j < agent.Equipment[equipmentIndex].GetAttachedWeaponsCount(); j++)
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new AttachWeaponToWeaponInAgentEquipmentSlot(agent.Equipment[equipmentIndex].GetAttachedWeapon(j), agent.Index, equipmentIndex, agent.Equipment[equipmentIndex].GetAttachedWeaponFrame(j)));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
            int num4 = agent.GetAttachedWeaponsCount();
            for (int k = 0; k < num4; k++)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new AttachWeaponToAgent(agent.GetAttachedWeapon(k), agent.Index, agent.GetAttachedWeaponBoneIndex(k), agent.GetAttachedWeaponFrame(k)));
                GameNetwork.EndModuleEventAsServer();
            }
            if (agent2 != null)
            {
                num4 = agent2.GetAttachedWeaponsCount();
                for (int l = 0; l < num4; l++)
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new AttachWeaponToAgent(agent2.GetAttachedWeapon(l), agent2.Index, agent2.GetAttachedWeaponBoneIndex(l), agent2.GetAttachedWeaponFrame(l)));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
            EquipmentIndex wieldedItemIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            int num5 = ((wieldedItemIndex != EquipmentIndex.None) ? agent.Equipment[wieldedItemIndex].CurrentUsageIndex : 0);
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new SetWieldedItemIndex(agent.Index, false, true, true, wieldedItemIndex, num5));
            GameNetwork.EndModuleEventAsServer();
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new SetWieldedItemIndex(agent.Index, true, true, true, agent.GetWieldedItemIndex(Agent.HandIndex.OffHand), num5));
            GameNetwork.EndModuleEventAsServer();
            MBActionSet actionSet = agent.ActionSet;
            if (actionSet.IsValid)
            {
                AnimationSystemData animationSystemData = agent.Monster.FillAnimationSystemData(actionSet, agent.Character.GetStepSize(), false);
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new SetAgentActionSet(agent.Index, animationSystemData));
                GameNetwork.EndModuleEventAsServer();
            }
            else
            {
                Debug.FailedAssert("Checking to see if we enter this condition.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Multiplayer\\MissionNetworkLogics\\MissionNetworkComponent.cs", "SendAgentsToPeer", 1975);
            }
        }

        private static void SendAgentsToPeer(NetworkCommunicator networkPeer)
        {
            Debug.Print("** PE Better Syncing ** Agents sending started for " + networkPeer.UserName, 0, Debug.DebugColor.Cyan, 17179869184UL);
            Mission currentMission = Mission.Current;
            foreach (Agent agent in currentMission.AllAgents)
            {
                AgentState state = agent.State;

                bool isActive = state == AgentState.Active;
                bool isKilledOrUnconscious = (state == AgentState.Killed || state == AgentState.Unconscious);
                bool hasAttachedWeapons = agent.GetAttachedWeaponsCount() > 0;
                bool hasWieldedItems = (!agent.IsMount && (agent.GetWieldedItemIndex(Agent.HandIndex.MainHand) >= EquipmentIndex.WeaponItemBeginSlot || agent.GetWieldedItemIndex(Agent.HandIndex.OffHand) >= EquipmentIndex.WeaponItemBeginSlot));
                bool isAgentInProximity = currentMission.IsAgentInProximityMap(agent);
                bool isMissileShooter = currentMission.Missiles.Any((Mission.Missile m) => m.ShooterAgent == agent);

                bool shouldSendAgent = (isActive || (!isActive && isMissileShooter));

                if (shouldSendAgent)
                {
                    if (agent.IsMount && agent.RiderAgent == null)
                    {
                        SendMountAgent(networkPeer, agent);
                    }
                    else if (!agent.IsMount)
                    {
                        SendHumanAgent(networkPeer, agent);
                    }
                    else
                    {
                        Debug.Print("** PE Better Syncing ** Not Sending Agent " + agent.Index, 0, Debug.DebugColor.Cyan, 17179869184UL);
                    }
                }
            }

            Debug.Print("** PE Better Syncing ** Agents sending ended for " + networkPeer.UserName, 0, Debug.DebugColor.Cyan, 17179869184UL);
        }


        private static void CallPrivateFunction(string function, MissionNetwork __instance, object[] parameters)
        {
            MethodInfo dynMethod = __instance.GetType().GetMethod(function, BindingFlags.NonPublic | BindingFlags.Instance);
            dynMethod.Invoke(__instance, parameters);
        }

        private static void SynchronizeMissionObjectsToPeer(NetworkCommunicator networkPeer, List<MissionObject> cachedMissionObjects)
        {
            foreach (MissionObject missionObject in cachedMissionObjects)
            {
                SynchedMissionObject synchedMissionObject = missionObject as SynchedMissionObject;
                if (synchedMissionObject != null && synchedMissionObject.GameEntity != null)
                {
                    // Debug.Print("** PE Better Syncing ** Object Name: " + missionObject.GetType().FullName, 0, Debug.DebugColor.Cyan, 17179869184UL);
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new SynchronizeMissionObject(synchedMissionObject));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
        }

        private static void SendMissilesToPeer(NetworkCommunicator networkPeer)
        {
            foreach (Mission.Missile missile in Mission.Current.Missiles)
            {
                Vec3 velocity = missile.GetVelocity();
                float num = velocity.Normalize();
                Mat3 identity = Mat3.Identity;
                identity.f = velocity;
                identity.Orthonormalize();
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                int index = missile.Index;
                int index2 = missile.ShooterAgent.Index;
                EquipmentIndex equipmentIndex = EquipmentIndex.None;
                MissionWeapon weapon = missile.Weapon;
                Vec3 position = missile.GetPosition();
                Vec3 vec = velocity;
                float num2 = num;
                Mat3 mat = identity;
                bool hasRigidBody = missile.GetHasRigidBody();
                MissionObject missionObjectToIgnore = missile.MissionObjectToIgnore;
                GameNetwork.WriteMessage(new CreateMissile(index, index2, equipmentIndex, weapon, position, vec, num2, mat, hasRigidBody, (missionObjectToIgnore != null) ? missionObjectToIgnore.Id : MissionObjectId.Invalid, false));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        private static void SyncupRuntimeObjects(NetworkCommunicator networkPeer)
        {
            foreach (Mission.DynamicallyCreatedEntity createdEntity in Mission.Current.AddedEntitiesInfo)
            {
                MissionObject missionObject = Mission.Current.MissionObjects.FirstOrDefault((MissionObject mo) => mo.Id == createdEntity.ObjectId);
                SynchedMissionObject synchedMissionObject = missionObject as SynchedMissionObject;
                if (synchedMissionObject != null)
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new SynchronizeMissionObject(synchedMissionObject));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
        }

        public static void OnTick()
        {
            if (peerSyncingQueue.Count <= 0) return;

            SyncingTrack syncingTrack = peerSyncingQueue.Peek();
            
            if (syncingTrack.peer.IsConnectionActive == false)
            {
                peerSyncingQueue.Dequeue();
                return;
            }

            List<MissionObject> toBeSend = chunkedMissionObjects[syncingTrack.chunkIndex];
            
            SynchronizeMissionObjectsToPeer(syncingTrack.peer, toBeSend);
            syncingTrack.chunkIndex = syncingTrack.chunkIndex + 1;
            
            if (syncingTrack.chunkIndex >= chunkedMissionObjects.Count)
            {
                peerSyncingQueue.Dequeue();
                SyncupRuntimeObjects(syncingTrack.peer);
                syncingTrack.peer.SendExistingObjects(Mission.Current);
                GameNetwork.BeginModuleEventAsServer(syncingTrack.peer);
                GameNetwork.WriteMessage(new ExistingObjectsEnd());
                GameNetwork.EndModuleEventAsServer();
                syncingTrack.synced = true;
            }
        }

        public static bool BetterSendExistingObjectsToPeer(MissionNetwork __instance, NetworkCommunicator networkPeer)
        {
            Debug.Print("** PE Better Syncing ** to peer : " + networkPeer.UserName.ToString(), 0, Debug.DebugColor.Cyan);
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new ExistingObjectsBegin());
            GameNetwork.EndModuleEventAsServer();
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new SynchronizeMissionTimeTracker((float)MissionTime.Now.ToSeconds));
            GameNetwork.EndModuleEventAsServer();

            SendTeamsToPeer(networkPeer);
            CallPrivateFunction("SendAgentsToPeer", __instance, new object[] { networkPeer });
            CallPrivateFunction("SendSpawnedMissionObjectsToPeer", __instance, new object[] { networkPeer });

            peerSyncingQueue.Enqueue(new SyncingTrack(networkPeer, 0, false));
            Debug.Print("** PE Better Syncing ** " + networkPeer.UserName.ToString() + " add to sync queue. Sync queue len is " + peerSyncingQueue.Count, 0, Debug.DebugColor.Cyan);

            SendMissilesToPeer(networkPeer);
            return false;
        }
    }
}

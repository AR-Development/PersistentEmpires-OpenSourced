using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpiresLib.SceneScripts.Extensions;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class PersistentEmpireSceneSyncBehaviors : MissionNetwork
    {
        public List<List<PE_RepairableDestructableComponent>> syncDestructableHitPoints = new List<List<PE_RepairableDestructableComponent>>();
        public List<List<PE_ItemGathering>> syncItemGathering = new List<List<PE_ItemGathering>>();
        public List<List<PE_DestructibleWithItem>> syncDestructableWithItems = new List<List<PE_DestructibleWithItem>>();

        public Queue<SyncingTrack> peerSyncDestructableHitPointsQueue = new Queue<SyncingTrack>();
        public Queue<SyncingTrack> peerSyncItemGatheringQueue = new Queue<SyncingTrack>();
        public Queue<SyncingTrack> peerSyncDestructableWithItemsQueue = new Queue<SyncingTrack>();

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
        public int RepairTimeoutAfterHit = 5 * 60;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
#if SERVER
            this.RepairTimeoutAfterHit = ConfigManager.GetIntConfig("RepairTimeoutAfterHit", 5 * 60);
            this.InitializeSyncMessages();
#endif
        }

        public void InitializeSyncMessages()
        {
            List<GameEntity> gameEntity = new List<GameEntity>();
            this.syncDestructableHitPoints = ChunkList<PE_RepairableDestructableComponent>(100, Mission.Current.MissionObjects
               .Where(o => o is PE_RepairableDestructableComponent)
               .Select(r => (PE_RepairableDestructableComponent)r)
               .ToList());
            this.syncItemGathering = ChunkList<PE_ItemGathering>(100, Mission.Current.MissionObjects
               .Where(o => o is PE_ItemGathering)
               .Select(r => (PE_ItemGathering)r)
               .ToList());
            this.syncDestructableWithItems = ChunkList<PE_DestructibleWithItem>(100, Mission.Current.MissionObjects
               .Where(o => o is PE_DestructibleWithItem)
               .Select(r => (PE_DestructibleWithItem)r)
               .ToList());
        }
        public override void AfterStart()
        {
            base.AfterStart();
            if (GameNetwork.IsServer)
            {
                this.InitializeTeleportDoors();
            }
        }
        private void SyncDestructibleWithItems(NetworkCommunicator peer)
        {
            this.peerSyncDestructableWithItemsQueue.Enqueue(new SyncingTrack(peer, 0, false));
        }
        private void SyncDestructableHitPoints(NetworkCommunicator peer)
        {
            this.peerSyncDestructableHitPointsQueue.Enqueue(new SyncingTrack(peer, 0, false));
        }

        private void SyncItemGatherings(NetworkCommunicator networkPeer)
        {
            this.peerSyncItemGatheringQueue.Enqueue(new SyncingTrack(networkPeer, 0, false));
        }

        // private void SyncAttachableObjects()
        private void SendDestructibleWithItemsInQueue(SyncingTrack track, Queue<SyncingTrack> q)
        {
            if (track.chunkIndex >= this.syncDestructableWithItems.Count)
            {
                q.Dequeue();
                return;
            }
            List<PE_DestructibleWithItem> toBeSend = this.syncDestructableWithItems[track.chunkIndex];
            foreach (PE_DestructibleWithItem comp in toBeSend)
            {
                GameNetwork.BeginModuleEventAsServer(track.peer);
                GameNetwork.WriteMessage(new SyncObjectHitpointsForDestructibleWithItem(comp, Vec3.Zero, comp.HitPoint));
                GameNetwork.EndModuleEventAsServer();
            }
            track.chunkIndex = track.chunkIndex + 1;

        }
        private void SendItemGatheringsInQueue(SyncingTrack track, Queue<SyncingTrack> q)
        {
            if (track.chunkIndex >= this.syncItemGathering.Count)
            {
                q.Dequeue();
                return;
            }
            List<PE_ItemGathering> toBeSend = this.syncItemGathering[track.chunkIndex];
            foreach (PE_ItemGathering itemGathering in toBeSend)
            {
                GameNetwork.BeginModuleEventAsServer(track.peer);
                GameNetwork.WriteMessage(new UpdateItemGatheringDestroyed(itemGathering, itemGathering.IsDestroyed));
                GameNetwork.EndModuleEventAsServer();
            }

        }
        private void SendDestructableHitPointsInQueue(SyncingTrack track, Queue<SyncingTrack> q)
        {
            if (track.chunkIndex >= this.syncDestructableHitPoints.Count)
            {
                q.Dequeue();
                return;
            }
            List<PE_RepairableDestructableComponent> toBeSend = this.syncDestructableHitPoints[track.chunkIndex];
            foreach (PE_RepairableDestructableComponent comp in toBeSend)
            {
                GameNetwork.BeginModuleEventAsServer(track.peer);
                GameNetwork.WriteMessage(new SyncObjectHitpointsPE(comp, Vec3.Zero, comp.HitPoint));
                GameNetwork.EndModuleEventAsServer();
            }
            track.chunkIndex = track.chunkIndex + 1;

        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            this.ProcessSyncQueue(peerSyncDestructableHitPointsQueue, "DestructableHitPoints");
            this.ProcessSyncQueue(peerSyncItemGatheringQueue, "ItemGatherings");
            this.ProcessSyncQueue(peerSyncDestructableWithItemsQueue, "DestructibleWithItems");
        }

        private void ProcessSyncQueue(Queue<SyncingTrack> queue, string queueName)
        {
            if (queue.Count == 0) return;
            SyncingTrack syncingTrack = queue.Peek();
            if (syncingTrack.peer.IsConnectionActive == false)
            {
                queue.Dequeue();
                return;
            }
            if (queueName == "DestructableHitPoints")
            {
                this.SendDestructableHitPointsInQueue(syncingTrack, queue);
            }
            if (queueName == "ItemGatherings")
            {
                this.SendItemGatheringsInQueue(syncingTrack, queue);
            }
            if (queueName == "DestructibleWithItems")
            {
                this.SendDestructibleWithItemsInQueue(syncingTrack, queue);
            }
        }


        private void SyncCarts(NetworkCommunicator peer)
        {
            List<GameEntity> gameEntities = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_AttachToAgent>().ToList();
            foreach (GameEntity g in gameEntities)
            {
                PE_AttachToAgent comp = g.GetFirstScriptOfType<PE_AttachToAgent>();
                if (comp.AttachedTo != null)
                {
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new SyncAttachToAgent(comp, comp.AttachedTo));
                    GameNetwork.EndModuleEventAsServer();
                }
            }

        }

        private void SyncHorseMarkets(NetworkCommunicator peer)
        {
            List<PE_HorseMarket> markets = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_HorseMarket>().Select(s => s.GetFirstScriptOfType<PE_HorseMarket>()).ToList();
            foreach (PE_HorseMarket m in markets)
            {
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new HorseMarketSetReserve(m, m.Stock));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        private void SyncMoveableObject(NetworkCommunicator peer)
        {
            List<GameEntity> gameEntities = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_MoveableMachine>().ToList();
            foreach (GameEntity g in gameEntities)
            {
                PE_MoveableMachine moveableMachine = g.GetFirstScriptOfType<PE_MoveableMachine>();
                if (moveableMachine.IsMovingBackward)
                {
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new StartMovingBackwardMoveableMachineServer(moveableMachine, moveableMachine.GameEntity.GetFrame()));
                    GameNetwork.EndModuleEventAsServer();
                }
                if (moveableMachine.IsMovingForward)
                {
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new StartMovingForwardMoveableMachineServer(moveableMachine, moveableMachine.GameEntity.GetFrame()));
                    GameNetwork.EndModuleEventAsServer();
                }
                if (moveableMachine.IsMovingDown)
                {
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new StartMovingDownMoveableMachineServer(moveableMachine, moveableMachine.GameEntity.GetFrame()));
                    GameNetwork.EndModuleEventAsServer();
                }
                if (moveableMachine.IsMovingUp)
                {
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new StartMovingUpMoveableMachineServer(moveableMachine, moveableMachine.GameEntity.GetFrame()));
                    GameNetwork.EndModuleEventAsServer();
                }
                if (moveableMachine.IsTurningLeft)
                {
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new StartTurningLeftMoveableMachineServer(moveableMachine, moveableMachine.GameEntity.GetFrame()));
                    GameNetwork.EndModuleEventAsServer();
                }
                if (moveableMachine.IsTurningRight)
                {
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new StartTurningRightMoveableMachineServer(moveableMachine, moveableMachine.GameEntity.GetFrame()));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
        }

        private void SyncUpgradeableBuilding(NetworkCommunicator peer)
        {
            List<GameEntity> gameEntity = new List<GameEntity>();
            base.Mission.Scene.GetAllEntitiesWithScriptComponent<PE_UpgradeableBuildings>(ref gameEntity);
            foreach (GameEntity g in gameEntity)
            {
                PE_UpgradeableBuildings comp = g.GetFirstScriptOfType<PE_UpgradeableBuildings>();

                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new UpgradeableBuildingSetTier(comp.CurrentTier, comp));
                GameNetwork.EndModuleEventAsServer();

                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new UpgradeableBuildingUpgrading(comp.IsUpgrading, comp));
                GameNetwork.EndModuleEventAsServer();

                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new SyncObjectHitpointsPE(comp, Vec3.Zero, comp.HitPoint));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        private void SyncMoneyChests(NetworkCommunicator peer)
        {
            List<GameEntity> gameEntity = new List<GameEntity>();
            base.Mission.Scene.GetAllEntitiesWithScriptComponent<PE_MoneyChest>(ref gameEntity);
            foreach (GameEntity g in gameEntity)
            {
                PE_MoneyChest moneyChest = g.GetFirstScriptOfType<PE_MoneyChest>();
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new UpdateMoneychestGold(moneyChest, moneyChest.Gold));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
            base.HandleLateNewClientAfterSynchronized(networkPeer);
            if (networkPeer.IsConnectionActive == false || networkPeer.IsNetworkActive == false) return;
            this.SyncDestructableHitPoints(networkPeer);
            this.SyncDestructibleWithItems(networkPeer);
            this.SyncItemGatherings(networkPeer);


            this.SyncUpgradeableBuilding(networkPeer);
            this.SyncCarts(networkPeer);
            this.SyncHorseMarkets(networkPeer);
            this.SyncLadderBuilder(networkPeer);
            this.SyncMoneyChests(networkPeer);
        }

        private void SyncLadderBuilder(NetworkCommunicator networkPeer)
        {
            List<GameEntity> gameEntity = new List<GameEntity>();
            base.Mission.Scene.GetAllEntitiesWithScriptComponent<PE_LadderBuilder>(ref gameEntity);

            foreach (GameEntity g in gameEntity)
            {
                PE_LadderBuilder comp = g.GetFirstScriptOfType<PE_LadderBuilder>();
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new SetLadderBuilder(comp, comp.GetLadderBuilt()));
                GameNetwork.EndModuleEventAsServer();
            }
        }


        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<SyncObjectHitpointsPE>(this.HandleSyncObjectHitpointsPE);
                networkMessageHandlerRegisterer.Register<ResetDestructableItem>(this.HandleResetDestructableItem);
                networkMessageHandlerRegisterer.Register<AddMissionObjectBodyFlagPE>(this.HandleAddMissionObjectBodyFlagPE);
                networkMessageHandlerRegisterer.Register<AddPhysicsToMissionObject>(this.HandleAddPhysicsToMissionObject);
                networkMessageHandlerRegisterer.Register<UpgradeableBuildingUpgrading>(this.UpgradeableBuildingUpgradingFromServer);
                networkMessageHandlerRegisterer.Register<UpgradeableBuildingSetTier>(this.HandleUpgradeableBuildingSetTierFromServer);
                networkMessageHandlerRegisterer.Register<SyncAttachToAgent>(this.HandleSyncAttachToAgentFromServer);
                networkMessageHandlerRegisterer.Register<HorseMarketSetReserve>(this.HandleHorseMarketSetReserveFromServer);
                networkMessageHandlerRegisterer.Register<ResetSiegeTower>(this.HandleResetSiegeTowerFromServer);
                networkMessageHandlerRegisterer.Register<ResetBatteringRam>(this.HandleResetBatteringRamFromServer);
                networkMessageHandlerRegisterer.Register<SetPE_BatteringRamHasArrivedAtTarget>(this.HandleSetPE_BatteringRamHasArrivedAtTargetFromServer);
                networkMessageHandlerRegisterer.Register<SyncObjectHitpointsForDestructibleWithItem>(this.HandleSyncObjectHitpointsForDestructibleWithItemFromServer);
                /*networkMessageHandlerRegisterer.Register<StartMovingBackwardMoveableMachineServer>(this.HandleStartMovingBackwardMoveableMachineServer);
				networkMessageHandlerRegisterer.Register<StartMovingForwardMoveableMachineServer>(this.HandleStartMovingForwardMoveableMachineServer);
				networkMessageHandlerRegisterer.Register<StartMovingUpMoveableMachineServer>(this.HandleStartMovingUpMoveableMachineServer);
				networkMessageHandlerRegisterer.Register<StartMovingDownMoveableMachineServer>(this.HandleStartMovingDownMoveableMachineServer);
				networkMessageHandlerRegisterer.Register<StartTurningLeftMoveableMachineServer>(this.HandleStartTurningLeftMoveableMachineServer);
				networkMessageHandlerRegisterer.Register<StartTurningRightMoveableMachineServer>(this.HandleStartTurningRightMoveableMachineServer);

				networkMessageHandlerRegisterer.Register<StopMovingBackwardMoveableMachineServer>(this.HandleStopMovingBackwardMoveableMachineServer);
				networkMessageHandlerRegisterer.Register<StopMovingForwardMoveableMachineServer>(this.HandleStopMovingForwardMoveableMachineServer);
				networkMessageHandlerRegisterer.Register<StopMovingUpMoveableMachineServer>(this.HandleStopMovingUpMoveableMachineServer);
				networkMessageHandlerRegisterer.Register<StopMovingDownMoveableMachineServer>(this.HandleStopMovingDownMoveableMachineServer);
				networkMessageHandlerRegisterer.Register<StopTurningLeftMoveableMachineServer>(this.HandleStopTurningLeftMoveableMachineServer);
				networkMessageHandlerRegisterer.Register<StopTurningRightMoveableMachineServer>(this.HandleStopTurningRightMoveableMachineServer);*/


                networkMessageHandlerRegisterer.Register<UpdateItemGatheringDestroyed>(this.HandleUpdateItemGatheringDestroyedFromServer);
                networkMessageHandlerRegisterer.Register<SetLadderBuilder>(this.HandleSetLadderBuilderFromServer);
            }
            else
            {
                networkMessageHandlerRegisterer.Register<StartMovingUpMoveableMachine>(this.HandleFromClientStartMovingUpMoveableMachine);
                networkMessageHandlerRegisterer.Register<StartMovingDownMoveableMachine>(this.HandleFromClientStartMovingDownMoveableMachine);
                networkMessageHandlerRegisterer.Register<StartMovingForwardMoveableMachine>(this.HandleFromClientStartMovingForwardMoveableMachine);
                networkMessageHandlerRegisterer.Register<StartMovingBackwardMoveableMachine>(this.HandleFromClientStartMovingBackwardMoveableMachine);
                networkMessageHandlerRegisterer.Register<StartTurningLeftMoveableMachine>(this.HandleFromClientStartTurningLeftMoveableMachine);
                networkMessageHandlerRegisterer.Register<StartTurningRightMoveableMachine>(this.HandleFromClientStartTurningRightMoveableMachine);

                networkMessageHandlerRegisterer.Register<StopMovingUpMoveableMachine>(this.HandleFromClientStopMovingUpMoveableMachine);
                networkMessageHandlerRegisterer.Register<StopMovingDownMoveableMachine>(this.HandleFromClientStopMovingDownMoveableMachine);
                networkMessageHandlerRegisterer.Register<StopMovingForwardMoveableMachine>(this.HandleFromClientStopMovingForwardMoveableMachine);
                networkMessageHandlerRegisterer.Register<StopMovingBackwardMoveableMachine>(this.HandleFromClientStopMovingBackwardMoveableMachine);
                networkMessageHandlerRegisterer.Register<StopTurningLeftMoveableMachine>(this.HandleFromClientStopTurningLeftMoveableMachine);
                networkMessageHandlerRegisterer.Register<StopTurningRightMoveableMachine>(this.HandleFromClientStopTurningRightMoveableMachine);
            }
        }

        private void HandleSyncObjectHitpointsForDestructibleWithItemFromServer(SyncObjectHitpointsForDestructibleWithItem message)
        {
            PE_DestructibleWithItem pe_DestructibleWithItem = (PE_DestructibleWithItem)message.MissionObject;
            pe_DestructibleWithItem.SetHitPoint(message.Hitpoints, message.ImpactDirection, null);
        }

        private void HandleSetPE_BatteringRamHasArrivedAtTargetFromServer(SetPE_BatteringRamHasArrivedAtTarget message)
        {
            (Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(message.BatteringRamId) as PE_BatteringRam).HasArrivedAtTarget = true;
        }

        private void HandleResetBatteringRamFromServer(ResetBatteringRam message)
        {
            ((PE_BatteringRamBuilder)message.BatteringRam).Reset();
        }

        private void HandleResetSiegeTowerFromServer(ResetSiegeTower message)
        {
            ((PE_SiegeTowerBuilder)message.SiegeTower).Reset();
        }

        private void HandleSetLadderBuilderFromServer(SetLadderBuilder message)
        {
            message.LadderBuilder.SetLadderBuilt(message.LadderBuilt);
        }

        private void HandleUpdateItemGatheringDestroyedFromServer(UpdateItemGatheringDestroyed message)
        {
            message.MissionObject.UpdateIsDestroyed(message.IsDestroyed);
        }


        private void HandleHorseMarketSetReserveFromServer(HorseMarketSetReserve message)
        {
            message.Market.UpdateReserve(message.Stock);
        }

        /*private void HandleStopTurningLeftMoveableMachineServer(StopTurningLeftMoveableMachineServer message)
        {
			if(message.Machine != null)
            {
				((IMoveable)message.Machine).StopTurningLeft();
				((IMoveable)message.Machine).SetFrameAfterTick(message.Frame);
            }
        }

        private void HandleStopTurningRightMoveableMachineServer(StopTurningRightMoveableMachineServer message)
        {
			if (message.Machine != null)
			{
				((IMoveable)message.Machine).StopTurningRight();
				((IMoveable)message.Machine).SetFrameAfterTick(message.Frame);
			}
		}

        private void HandleStopMovingDownMoveableMachineServer(StopMovingDownMoveableMachineServer message)
        {
			if (message.Machine != null)
			{
				((IMoveable)message.Machine).StopMovingDown();
				((IMoveable)message.Machine).SetFrameAfterTick(message.Frame);
			}
		}

        private void HandleStopMovingUpMoveableMachineServer(StopMovingUpMoveableMachineServer message)
        {
			if (message.Machine != null)
			{
				((IMoveable)message.Machine).StopMovingUp();
				((IMoveable)message.Machine).SetFrameAfterTick(message.Frame);
			}
		}

        private void HandleStopMovingForwardMoveableMachineServer(StopMovingForwardMoveableMachineServer message)
        {
			if (message.Machine != null)
			{
				((IMoveable)message.Machine).StopMovingForward();
				((IMoveable)message.Machine).SetFrameAfterTick(message.Frame);
			}
		}

        private void HandleStopMovingBackwardMoveableMachineServer(StopMovingBackwardMoveableMachineServer message)
        {
			if (message.Machine != null)
			{
				((IMoveable)message.Machine).StopMovingBackward();
				((IMoveable)message.Machine).SetFrameAfterTick(message.Frame);
			}
		}

        private void HandleStartTurningRightMoveableMachineServer(StartTurningRightMoveableMachineServer message)
        {
			if (message.Machine != null)
			{
				((IMoveable)message.Machine).StartTurningRight();
			};
        }

        private void HandleStartTurningLeftMoveableMachineServer(StartTurningLeftMoveableMachineServer message)
        {
			if (message.Machine != null)
			{
				((IMoveable)message.Machine).StartTurningLeft();
			};
		}

        private void HandleStartMovingDownMoveableMachineServer(StartMovingDownMoveableMachineServer message)
        {
			if(message.Machine != null)
            {
				((IMoveable)message.Machine).StartMovingDown();
            }
        }

        private void HandleStartMovingUpMoveableMachineServer(StartMovingUpMoveableMachineServer message)
        {
			if (message.Machine != null)
			{
				((IMoveable)message.Machine).StartMovingUp();
			}
		}

        private void HandleStartMovingForwardMoveableMachineServer(StartMovingForwardMoveableMachineServer message)
        {
			if (message.Machine != null)
			{
				((IMoveable)message.Machine).StartMovingForward();
			}
		}

        private void HandleStartMovingBackwardMoveableMachineServer(StartMovingBackwardMoveableMachineServer message)
        {
			if (message.Machine != null)
			{
				((IMoveable)message.Machine).StartMovingBackward();
			}
		}*/

        private void HandleSyncAttachToAgentFromServer(SyncAttachToAgent message)
        {
            message.AttachToAgent.SetAttachedAgent(message.UserAgent);
        }

        private void HandleUpgradeableBuildingSetTierFromServer(UpgradeableBuildingSetTier message)
        {
            ((PE_UpgradeableBuildings)message.UpgradingObject).SetTier(message.Tier);
        }

        private void UpgradeableBuildingUpgradingFromServer(UpgradeableBuildingUpgrading message)
        {
            ((PE_UpgradeableBuildings)message.UpgradingObject).SetIsUpgrading(message.IsUpgrading);
        }

        private bool HandleFromClientStopTurningRightMoveableMachine(NetworkCommunicator player, StopTurningRightMoveableMachine message)
        {
            if (player.ControlledAgent == null) return false;
            ((IMoveable)message.Object).StopTurningRight();
            return true;
        }

        private bool HandleFromClientStopTurningLeftMoveableMachine(NetworkCommunicator player, StopTurningLeftMoveableMachine message)
        {
            if (player.ControlledAgent == null) return false;
            ((IMoveable)message.Object).StopTurningLeft();
            return true;
        }

        private bool HandleFromClientStopMovingBackwardMoveableMachine(NetworkCommunicator player, StopMovingBackwardMoveableMachine message)
        {
            if (player.ControlledAgent == null) return false;
            ((IMoveable)message.Object).StopMovingBackward();
            return true;
        }

        private bool HandleFromClientStopMovingForwardMoveableMachine(NetworkCommunicator player, StopMovingForwardMoveableMachine message)
        {
            if (player.ControlledAgent == null) return false;
            ((IMoveable)message.Object).StopMovingForward();
            return true;
        }

        private bool HandleFromClientStopMovingDownMoveableMachine(NetworkCommunicator player, StopMovingDownMoveableMachine message)
        {
            if (player.ControlledAgent == null) return false;
            ((IMoveable)message.Object).StopMovingDown();
            return true;
        }

        private bool HandleFromClientStopMovingUpMoveableMachine(NetworkCommunicator player, StopMovingUpMoveableMachine message)
        {
            if (player.ControlledAgent == null) return false;
            ((IMoveable)message.Object).StopMovingUp();
            return true;
        }

        private bool HandleFromClientStartTurningRightMoveableMachine(NetworkCommunicator player, StartTurningRightMoveableMachine message)
        {
            if (player.ControlledAgent == null) return false;
            ((IMoveable)message.Object).StartTurningRight();
            return true;
        }

        private bool HandleFromClientStartTurningLeftMoveableMachine(NetworkCommunicator player, StartTurningLeftMoveableMachine message)
        {
            if (player.ControlledAgent == null) return false;
            ((IMoveable)message.Object).StartTurningLeft();
            return true;
        }

        private bool HandleFromClientStartMovingBackwardMoveableMachine(NetworkCommunicator player, StartMovingBackwardMoveableMachine message)
        {
            if (player.ControlledAgent == null) return false;
            ((IMoveable)message.Object).StartMovingBackward();
            return true;
        }

        private bool HandleFromClientStartMovingForwardMoveableMachine(NetworkCommunicator player, StartMovingForwardMoveableMachine message)
        {
            if (player.ControlledAgent == null) return false;
            ((IMoveable)message.Object).StartMovingForward();
            return true;
        }

        private bool HandleFromClientStartMovingDownMoveableMachine(NetworkCommunicator player, StartMovingDownMoveableMachine message)
        {
            if (player.ControlledAgent == null) return false;
            ((IMoveable)message.Object).StartMovingDown();
            return true;
        }

        private bool HandleFromClientStartMovingUpMoveableMachine(NetworkCommunicator player, StartMovingUpMoveableMachine message)
        {
            if (player.ControlledAgent == null) return false;
            ((IMoveable)message.Object).StartMovingUp();
            return true;
        }

        private void HandleAddPhysicsToMissionObject(AddPhysicsToMissionObject message)
        {
            if (message.MissionObject != null)
            {
                GameEntity gameEntity = message.MissionObject.GameEntity;
                gameEntity.AddPhysics(gameEntity.Mass, gameEntity.CenterOfMass, gameEntity.GetBodyShape(), message.InitialVelocity, message.AngularVelocity, PhysicsMaterial.GetFromName(message.PhysicsMaterial), false, 0);
            }
        }

        private void HandleAddMissionObjectBodyFlagPE(AddMissionObjectBodyFlagPE message)
        {
            if (message.MissionObject != null)
            {
                message.MissionObject.GameEntity.AddBodyFlags(message.BodyFlags, message.ApplyToChildren);
            }
        }

        protected void HandleResetDestructableItem(ResetDestructableItem message)
        {
            if (message.MissionObject != null)
            {
                message.MissionObject.GameEntity.GetFirstScriptOfType<PE_DestructibleWithItem>().ResetObject();
            }
        }
        protected void HandleSyncObjectHitpointsPE(SyncObjectHitpointsPE message)
        {
            if (message.MissionObject != null)
            {
                message.MissionObject.GameEntity.GetFirstScriptOfType<PE_DestructableComponent>().SetHitPoint(message.Hitpoints, message.ImpactDirection, null);
            }
        }

        protected void InitializeTeleportDoors()
        {
            List<GameEntity> gameEntities = new List<GameEntity>();
            Mission.Current.Scene.GetAllEntitiesWithScriptComponent<PE_TeleportDoor>(ref gameEntities);
            Dictionary<string, List<PE_TeleportDoor>> listOfDoors = new Dictionary<string, List<PE_TeleportDoor>>();
            foreach (GameEntity gameEntity in gameEntities)
            {
                PE_TeleportDoor teleportDoor = gameEntity.GetFirstScriptOfType<PE_TeleportDoor>();
                if (listOfDoors.ContainsKey(teleportDoor.LinkText))
                {
                    listOfDoors[teleportDoor.LinkText].Add(teleportDoor);
                }
                else
                {
                    listOfDoors[teleportDoor.LinkText] = new List<PE_TeleportDoor>();
                    listOfDoors[teleportDoor.LinkText].Add(teleportDoor);
                }
            }
            foreach (List<PE_TeleportDoor> linkedDoors in listOfDoors.Values.Where(l => l.Count >= 2))
            {
                linkedDoors[0].SetLinkedDoor(linkedDoors[1]);
                linkedDoors[1].SetLinkedDoor(linkedDoors[0]);
            }
        }
    }
}

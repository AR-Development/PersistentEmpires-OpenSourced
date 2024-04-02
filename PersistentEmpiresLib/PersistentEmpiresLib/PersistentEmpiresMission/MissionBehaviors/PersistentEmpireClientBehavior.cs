using NetworkMessages.FromServer;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class PersistentEmpireClientBehavior : MissionMultiplayerGameModeBaseClient
    {
        public event Action<bool> OnAgentLabelConfig;
        public event Action<GoldGain> OnGoldGainEvent;
        public event Action OnSynchronized;
        public override bool IsGameModeUsingGold => true;
        public override bool IsGameModeTactical => false;
        public override bool IsGameModeUsingRoundCountdown => false;
        public override MultiplayerGameType GameType => MultiplayerGameType.FreeForAll;
        private FactionsBehavior _factionsBehavior;
        private CastlesBehavior _castlesBehavior;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            base.MissionNetworkComponent.OnMyClientSynchronized += this.OnMyClientSynchronized;
            base.Mission.OnItemPickUp += this.OnItemPickup;
        }

        private void OnMyClientSynchronized()
        {
            this._myRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            this._factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            this._castlesBehavior = base.Mission.GetMissionBehavior<CastlesBehavior>();
            if(this.OnSynchronized != null)
            {
                this.OnSynchronized();
            }
        }


        public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
        {
            if (representative != null && base.MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Ending)
            {
                representative.UpdateGold(goldAmount);
            }
        }
        public override int GetGoldAmount()
        {
            return this._myRepresentative.Gold;
        }
        
        public override void AfterStart()
        {
            base.Mission.SetMissionMode(MissionMode.Battle, true);
            
        }
        
        protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
        {
            if (GameNetwork.IsClient)
            {
                registerer.Register<SyncGold>(this.HandleServerEventUpdateGold);
                // registerer.Register<GoldGain>((this.HandleServerEventTDMGoldGain));
                registerer.Register<PEGoldGain>(this.HandlServerEventGoldGain);
                registerer.Register<PEGoldLost>(this.HandlServerEventGoldLost);
                registerer.Register<SyncFaction>(this.HandleSyncFaction);
                registerer.Register<SyncCastleBanner>(this.HandleSyncCastleBanner);
                registerer.Register<AdminOrErrorMessage>(this.HandleAdminOrErrorMessage);
                registerer.Register<SyncMember>(this.HandleSyncMember);
                registerer.Register<AddMarshallIdToFaction>(this.HandleAddMarshallIdToFactionServer);
                registerer.Register<ServerHandshake>(this.HandleServerHandshakeFromServer);
                registerer.Register<AgentLabelConfig>(this.HandleServerAgentLabelConfig);
                registerer.Register<ExistingObjectsEnd>(this.HandleFromServerExistingObjectsEnd);
            }
            else
            {
                registerer.Register<MyDiscordId>(this.HandleClientMyDiscordId);
                registerer.Register<RequestSuicide>(this.HandleClientRequestSuicide);
            }
        }

        private void HandleFromServerExistingObjectsEnd(ExistingObjectsEnd message)
        {
            for(int i = 0;i < Mission.Current.Teams.Count; i++)
            {
                for(int j = 0; j < Mission.Current.Teams.Count; j++)
                {
                    Mission.Current.Teams[i].SetIsEnemyOf(Mission.Current.Teams[j], true);
                }
            }

        }

        private void HandleServerAgentLabelConfig(AgentLabelConfig message)
        {
            if(this.OnAgentLabelConfig != null)
            {
                this.OnAgentLabelConfig(message.Enabled);
            }
        }

        private void HandleServerHandshakeFromServer(ServerHandshake message)
        {
            
        }

        private void HandleAddMarshallIdToFactionServer(AddMarshallIdToFaction message)
        {
            if(this._factionsBehavior.Factions.ContainsKey(message.FactionIndex))
            {
                this._factionsBehavior.Factions[message.FactionIndex].marshalls.Add(message.MarshallId);
            }
        }

        private bool HandleClientRequestSuicide(NetworkCommunicator player,RequestSuicide message)
        {
            if(player.IsConnectionActive && player.ControlledAgent != null)
            {
                Agent agent = player.ControlledAgent;
                Blow blow = new Blow(agent.Index);
                blow.DamageType = TaleWorlds.Core.DamageTypes.Pierce;
                blow.BoneIndex = agent.Monster.HeadLookDirectionBoneIndex;
                blow.GlobalPosition = agent.Position;
                blow.GlobalPosition.z = blow.GlobalPosition.z + agent.GetEyeGlobalHeight();
                blow.BaseMagnitude = 2000f;
                blow.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
                blow.InflictedDamage = 2000;
                blow.SwingDirection = agent.LookDirection;
                MatrixFrame frame = agent.Frame;
                blow.SwingDirection = frame.rotation.TransformToParent(new Vec3(-1f, 0f, 0f, -1f));
                blow.SwingDirection.Normalize();
                blow.Direction = blow.SwingDirection;
                blow.DamageCalculated = true;
                sbyte mainHandItemBoneIndex = agent.Monster.MainHandItemBoneIndex;
                AttackCollisionData attackCollisionDataForDebugPurpose = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(false, false, false, true, false, false, false, false, false, false, false, false, CombatCollisionResult.StrikeAgent, -1, 0, 2, blow.BoneIndex, BoneBodyPartType.Head, mainHandItemBoneIndex, Agent.UsageDirection.AttackLeft, -1, CombatHitResultFlags.NormalHit, 0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, Vec3.Up, blow.Direction, blow.GlobalPosition, Vec3.Zero, Vec3.Zero, agent.Velocity, Vec3.Up);
                agent.RegisterBlow(blow, attackCollisionDataForDebugPurpose);
                LoggerHelper.LogAnAction(player, LogAction.PlayerCommitedSuicide);
            }
            return true;
        }

        private bool HandleClientMyDiscordId(NetworkCommunicator player,MyDiscordId message)
        {
            SaveSystemBehavior.HandleDiscordRegister(player, message.Id);
            return true;
        }

        private void HandleSyncCastleBanner(SyncCastleBanner message)
        {
            this._castlesBehavior.UpdateCastle(message.CastleBanner.CastleIndex, message.FactionIndex);
        }

        private void HandleSyncFaction(SyncFaction message)
        {
            this._factionsBehavior.AddFaction(message.FactionIndex, message.Faction);
            /*foreach (NetworkCommunicator member in message.Faction.members.ToList())
            {
                member.GetComponent<PersistentEmpireRepresentative>().SetFaction(message.Faction, message.FactionIndex);
            }*/
        }

        private void HandleSyncMember(SyncMember message)
        {
            if(message.FactionIndex != -1)
            {
                Faction f = this._factionsBehavior.Factions[message.FactionIndex];
                if(!message.Peer.IsMine)
                {
                    this._factionsBehavior.SetPlayerFaction(message.Peer, message.FactionIndex, -1);
                    if (message.IsMarshall) f.marshalls.Add(message.Peer.VirtualPlayer.Id.ToString());
                }
                message.Peer.GetComponent<PersistentEmpireRepresentative>().SetFaction(f, message.FactionIndex);
            }
        }

        private void HandlServerEventGoldLost(PEGoldLost message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;
            persistentEmpireRepresentative.GoldLost(message.Lost);
        }

        private void HandlServerEventGoldGain(PEGoldGain message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;
            persistentEmpireRepresentative.GoldGain(message.Gain);
        }

        private void HandleServerEventUpdateGold(SyncGold message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;
            persistentEmpireRepresentative.SetGold(message.Gold);
        }

        private void HandleAdminOrErrorMessage(AdminOrErrorMessage adminOrErrorMessage) {
            InformationManager.AddSystemNotification(adminOrErrorMessage.Message);
        }

        // [HandleProcessCorruptedStateExceptions]
        // [SecurityCritical]
        public void OnItemPickup(Agent picker, SpawnedItemEntity item)
        {
            if (GameNetwork.IsServer) return;
            NetworkCommunicator peer = picker.MissionPeer.GetNetworkPeer();
            PersistentEmpireRepresentative representative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (representative == null) return;
            if (representative.GetFaction() == null) return;
            Banner banner = representative.GetFaction().banner;
            // BannerRenderer.RequestRenderBanner(banner, item.GameEntity);
        }



        private PersistentEmpireRepresentative _myRepresentative;
    }
}

using NetworkMessages.FromServer;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresServer.SpawnBehavior
{
    public class PersistentEmpiresSpawningBehavior : SpawningBehaviorBase
    {
        private FactionsBehavior factionBehavior;
        protected event Action<MissionPeer> OnPeerSpawnedFromVisuals;
        protected event Action<MissionPeer> OnAllAgentsFromPeerSpawnedFromVisuals;
        protected bool PersistPosition = false;
        public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
        {
            return true;
        }

        public override void Initialize(SpawnComponent spawnComponent)
        {

            base.Initialize(spawnComponent);
            this.PersistPosition = ConfigManager.GetBoolConfig("PersistDisconnectPosition", false);
        }

        public override int GetMaximumReSpawnPeriodForPeer(MissionPeer peer)
        {
            return WoundingBehavior.Instance.WoundingEnabled ? 1 : 30;
        }

        protected override bool IsRoundInProgress()
        {
            return Mission.Current.CurrentState == Mission.State.Continuing;
        }

        public MatrixFrame GetSpawnFrame(NetworkCommunicator peer)
        {
            if (WoundingBehavior.Instance.WoundingEnabled 
                && WoundingBehavior.Instance.IsPlayerWounded(peer)
                && WoundingBehavior.Instance.DeathPlace.ContainsKey(peer.VirtualPlayer?.ToPlayerId()))
            {
                return new MatrixFrame(Mat3.Identity, WoundingBehavior.Instance.DeathPlace[peer.VirtualPlayer?.ToPlayerId()]);
            }

            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            PE_SpawnFrame frame;
            if (persistentEmpireRepresentative == null)
            {
                frame = base.Mission.GetMissionBehavior<SpawnFrameSelectionBehavior>().DefaultSpawnFrames[0];
            }
            else if (persistentEmpireRepresentative.GetNextSpawnFrame() == null)
            {
                List<PE_SpawnFrame> spawnable = persistentEmpireRepresentative.GetSpawnableCastleFrames();
                if (spawnable.Count > 0)
                {
                    frame = spawnable[0];
                }
                else
                {
                    frame = base.Mission.GetMissionBehavior<SpawnFrameSelectionBehavior>().DefaultSpawnFrames[0];
                }
            }
            else
            {
                frame = persistentEmpireRepresentative.GetNextSpawnFrame();
                if (frame.GetCastleBanner() != null && frame.GetCastleBanner().FactionIndex != persistentEmpireRepresentative.GetFactionIndex()) frame = base.Mission.GetMissionBehavior<SpawnFrameSelectionBehavior>().DefaultSpawnFrames[0];
            }
            return frame.GameEntity.GetGlobalFrame();
        }

        public void OverridenOnTick(float dt)
        {
            foreach (NetworkCommunicator networkCommunicator in GameNetwork.NetworkPeers)
            {
                if (networkCommunicator.IsSynchronized)
                {
                    MissionPeer component = networkCommunicator.GetComponent<MissionPeer>();
                    PersistentEmpireRepresentative persistentEmpireRepresentative = networkCommunicator.GetComponent<PersistentEmpireRepresentative>();
                    if (component != null && component.ControlledAgent == null && component.HasSpawnedAgentVisuals && !this.CanUpdateSpawnEquipment(component) && persistentEmpireRepresentative != null)
                    {
                        if (WoundingBehavior.Instance.WoundingEnabled 
                            && WoundingBehavior.Instance.DeathEquipment.ContainsKey(networkCommunicator.VirtualPlayer?.ToPlayerId())
                            && !WoundingBehavior.Instance.DeathEquipment[networkCommunicator.VirtualPlayer?.ToPlayerId()].Item1)
                        {
                            continue;
                        }

                        MultiplayerClassDivisions.MPHeroClass mpheroClassForPeer = MBObjectManager.Instance.GetObjectTypeList<MultiplayerClassDivisions.MPHeroClass>().FirstOrDefault((mpHeroClass) => mpHeroClass.HeroCharacter.StringId == persistentEmpireRepresentative.GetClassId());

                        BasicCharacterObject basicCharacterObject = mpheroClassForPeer.HeroCharacter;
                        uint color = persistentEmpireRepresentative.GetFaction().banner.GetPrimaryColor();
                        uint color2 = persistentEmpireRepresentative.GetFaction().banner.GetFirstIconColor();
                        Banner banner = new Banner(persistentEmpireRepresentative.GetFaction().basicCultureObject.BannerKey);
                        AgentBuildData agentBuildData = new AgentBuildData(basicCharacterObject).VisualsIndex(0).Team(component.Team).TroopOrigin(new BasicBattleAgentOrigin(basicCharacterObject)).Formation(component.ControlledFormation).IsFemale(component.Peer.IsFemale).ClothingColor1(color).ClothingColor2(color2).Banner(banner);
                        agentBuildData.MissionPeer(component);
                        Equipment equipment = basicCharacterObject.Equipment.Clone(false);
                        agentBuildData.Equipment(persistentEmpireRepresentative.LoadFromDb ? persistentEmpireRepresentative.LoadedSpawnEquipment.Clone(false) : equipment);

                        if (WoundingBehavior.Instance.WoundingEnabled)
                        {
                            if (WoundingBehavior.Instance.IsWounded.ContainsKey(networkCommunicator.VirtualPlayer?.ToPlayerId())
                                && WoundingBehavior.Instance.IsWounded[networkCommunicator.VirtualPlayer?.ToPlayerId()])
                            {
                                agentBuildData.Equipment(WoundingBehavior.Instance.DeathEquipment[networkCommunicator.VirtualPlayer?.ToPlayerId()].Item2);
                                WoundingBehavior.Instance.DeathEquipment.Remove(networkCommunicator.VirtualPlayer?.ToPlayerId()); // Done.
                            }
                        }

                        agentBuildData.BodyProperties(GetBodyProperties(component, component.Culture));
                        agentBuildData.Age((int)agentBuildData.AgentBodyProperties.Age);

                        MatrixFrame spawnFrame = this.GetSpawnFrame(networkCommunicator);
                        if (spawnFrame.IsIdentity)
                        {
                            Debug.FailedAssert("Spawn frame could not be found.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Multiplayer\\SpawnBehaviors\\SpawningBehaviors\\SpawningBehaviorBase.cs", "OnTick", 194);
                        }
                        Vec2 v;
                        if (!(spawnFrame.origin != agentBuildData.AgentInitialPosition))
                        {
                            v = spawnFrame.rotation.f.AsVec2.Normalized();
                            Vec2? agentInitialDirection = agentBuildData.AgentInitialDirection;
                            if (!(v != agentInitialDirection))
                            {
                                Debug.FailedAssert("PE Spawn frame could not be found.", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Multiplayer\\SpawnBehaviors\\SpawningBehaviors\\SpawningBehaviorBase.cs", "OnTick", 194);
                            }
                        }
                        if (persistentEmpireRepresentative.LoadFromDb)
                        {
                            if (persistentEmpireRepresentative.LoadedDbPosition == new Vec3(0, 0, 0) || persistentEmpireRepresentative.DisconnectedAt + (30 * 60) < DateTimeOffset.UtcNow.ToUnixTimeSeconds() || this.PersistPosition)
                            {
                                Debug.Print(String.Format("SELECTED POSITION FOR {0} => ({1},{2},{3})", component.DisplayedName, spawnFrame.origin.x, spawnFrame.origin.y, spawnFrame.origin.z));
                                agentBuildData.InitialPosition(spawnFrame.origin);
                            }
                            else
                            {
                                agentBuildData.InitialPosition(persistentEmpireRepresentative.LoadedDbPosition);
                            }
                        }
                        else
                        {
                            agentBuildData.InitialPosition(spawnFrame.origin);
                        }
                        v = spawnFrame.rotation.f.AsVec2;
                        agentBuildData.InitialDirection(v);
                        Agent agent = this.Mission.SpawnAgent(agentBuildData, WoundingBehavior.Instance.WoundingEnabled ? false : true);
                        Agent mountAgent = agent.MountAgent;

                        LoggerHelper.LogAnAction(networkCommunicator, LogAction.PlayerSpawn, null, new object[] { agent });

                        Debug.Print("AGENT INDEX " + agent.Index + " SPAWNED FOR PLAYER " + agent.MissionPeer.GetNetworkPeer().UserName);

                        if (mountAgent != null)
                        {
                            mountAgent.UpdateAgentProperties();
                        }
                        agent.WieldInitialWeapons(Agent.WeaponWieldActionType.InstantAfterPickUp);
                        if (persistentEmpireRepresentative.LoadFromDb)
                        {
                            agent.Health = persistentEmpireRepresentative.LoadedHealth;

                            for (EquipmentIndex index = EquipmentIndex.Weapon0; index < EquipmentIndex.ExtraWeaponSlot; index++)
                            {
                                if (agent.Equipment[index].IsEmpty == false && (agent.Equipment[index].IsAnyConsumable() || agent.Equipment[index].Item.Type == ItemObject.ItemTypeEnum.Crossbow || agent.Equipment[index].Item.Type == ItemObject.ItemTypeEnum.Musket))
                                {
                                    MissionWeapon missionWeapon = new MissionWeapon(agent.Equipment[index].Item, null, null, (short)persistentEmpireRepresentative.LoadedAmmo[(int)index]);
                                    agent.EquipWeaponWithNewEntity(index, ref missionWeapon);
                                    // agent.Equipment[index]
                                }
                            }
                        }
                        else
                        {
                            agent.Health = mpheroClassForPeer.Health;
                        }
                        Action<MissionPeer> onPeerSpawnedFromVisuals = this.OnPeerSpawnedFromVisuals;
                        if (persistentEmpireRepresentative.LoadFromDb) persistentEmpireRepresentative.LoadFromDb = false;
                        persistentEmpireRepresentative.IsFirstAgentSpawned = true;
                        if (onPeerSpawnedFromVisuals != null)
                        {
                            onPeerSpawnedFromVisuals(component);
                        }
                        int spawnCountThisRound = component.SpawnCountThisRound;
                        component.SpawnCountThisRound = spawnCountThisRound + 1;
                        Action<MissionPeer> onAllAgentsFromPeerSpawnedFromVisuals = this.OnAllAgentsFromPeerSpawnedFromVisuals;
                        if (onAllAgentsFromPeerSpawnedFromVisuals != null)
                        {
                            onAllAgentsFromPeerSpawnedFromVisuals(component);
                        }
                        var missionPeer = agentBuildData.AgentMissionPeer;
                        if (GameNetwork.IsServerOrRecorder)
                        {
                            GameNetwork.BeginBroadcastModuleEvent();
                            GameNetwork.WriteMessage(new RemoveAgentVisualsForPeer(missionPeer.GetNetworkPeer()));
                            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None, null);
                        }
                        missionPeer.HasSpawnedAgentVisuals = false;
                        missionPeer.ControlledAgent.UpdateAgentStats();
                    }
                }
            }
        }

        public override void OnTick(float dt)
        {
            this.SpawnAgents();
            this.OverridenOnTick(dt);
        }

        protected override void SpawnAgents()
        {
            // throw new NotImplementedException();
            foreach (NetworkCommunicator networkCommunicator in GameNetwork.NetworkPeers)
            {
                if (networkCommunicator.IsSynchronized)
                {

                    MissionPeer component = networkCommunicator.GetComponent<MissionPeer>();
                    PersistentEmpireRepresentative representative = networkCommunicator.GetComponent<PersistentEmpireRepresentative>();

                    if (component != null && representative != null && component.ControlledAgent == null && !component.HasSpawnedAgentVisuals && component.Team != null
                        && component.Team != base.Mission.SpectatorTeam && representative.GetFaction() != null && representative.SpawnTimer.Check(base.Mission.CurrentTime))
                    {
                        Faction fact = representative.GetFaction();
                        BasicCultureObject basicCultureObject = fact.basicCultureObject;

                        // MultiplayerClassDivisions.MPHeroClass mpheroClassForPeer = MultiplayerClassDivisions.GetMPHeroClasses().Where((MultiplayerClassDivisions.MPHeroClass x) => x.HeroCharacter.StringId == PersistentEmpireRepresentative.DefaultClass).First();

                        /*foreach(MultiplayerClassDivisions.MPHeroClass mpHeroClass in MultiplayerClassDivisions.GetMPHeroClasses(component.Culture))
                        {
                            Debug.Print(mpHeroClass.HeroCharacter.StringId);
                        }*/
                        BasicCharacterObject selectedCharacterObject = MBObjectManager.Instance.GetObject<BasicCharacterObject>(representative.GetClassId());

                        if (selectedCharacterObject == null)
                        {
                            Debug.Print("*PERSISTENT EMPIRES* Player class is null", 0, Debug.DebugColor.Red);
                            selectedCharacterObject = MBObjectManager.Instance.GetObject<BasicCharacterObject>(PersistentEmpireBehavior.DefaultClass);
                        }

                        component.Culture = selectedCharacterObject.Culture;
                        component.SelectedTroopIndex = MultiplayerClassDivisions.GetMPHeroClasses(selectedCharacterObject.Culture).Select((value, index) => new { value, index }).First((a) => a.value.HeroCharacter.StringId == representative.GetClassId()).index;
                        component.NextSelectedTroopIndex = component.SelectedTroopIndex;
                        MultiplayerClassDivisions.MPHeroClass mpheroClassForPeer = MultiplayerClassDivisions.GetMPHeroClassForPeer(component, true);

                        BasicCharacterObject heroCharacter = mpheroClassForPeer.HeroCharacter;
                        Equipment equipment = heroCharacter.Equipment.Clone(false);

                        var agentEquipment = representative.LoadFromDb ? representative.LoadedSpawnEquipment.Clone(false) : equipment;

                        AgentBuildData agentBuildData = new AgentBuildData(heroCharacter)
                            .MissionPeer(component)
                            .Equipment(agentEquipment)
                            .Team(component.Team)
                            .TroopOrigin(new BasicBattleAgentOrigin(heroCharacter))
                            .IsFemale(component.Peer.IsFemale)
                            .BodyProperties(GetBodyProperties(component, component.Culture))
                            .VisualsIndex(0)
                            .ClothingColor1(fact.banner.GetPrimaryColor())
                            .ClothingColor2(fact.banner.GetFirstIconColor());


                        if (this.GameMode.ShouldSpawnVisualsForServer(networkCommunicator))
                        {
                            component.HasSpawnedAgentVisuals = true;
                            component.EquipmentUpdatingExpired = false;
                        }
                        this.GameMode.HandleAgentVisualSpawning(networkCommunicator, agentBuildData, 0, true);

                        // update data from default class
                        if (!representative.LoadFromDb)
                        {
                            SaveSystemBehavior.HandleSaveDefaultsForNewPlayer(networkCommunicator, agentEquipment);
                        }
                    }
                }
            }
        }
    }

}

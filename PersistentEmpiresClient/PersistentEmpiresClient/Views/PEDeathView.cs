using PersistentEmpires.Views.ViewsVM;
using PersistentEmpires.Views.ViewsVM.PETabMenu;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PEDeathView : MissionView
    {
        private GauntletLayer _gauntletLayer;
        private PEDeathVM _dataSource;
        private bool _isEnabled;
        private long _spawnerStartedAt;
        private Dictionary<PE_CastleBanner, List<PE_SpawnFrame>> _selectableCastles;
        private List<PE_SpawnFrame> _defaultSpawnPoints;
        private PE_CastleBanner _selectedCastle = null;
        private CastlesBehavior _castleBehavior;
        private FactionsBehavior _factionsBehavior;
        private int _selectedIndex = 0;
        private int _selectedCastleIndex = -1;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._dataSource = new PEDeathVM();
            this._gauntletLayer = new GauntletLayer(this.ViewOrderPriority, "GauntletLayer", false);
            this._gauntletLayer.LoadMovie("PEDeathView", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            base.Mission.GetMissionBehavior<MultiplayerMissionAgentVisualSpawnComponent>().OnMyAgentVisualSpawned += this.OnMainAgentVisualSpawned;
            _selectableCastles = new Dictionary<PE_CastleBanner, List<PE_SpawnFrame>>();
            this._castleBehavior = base.Mission.GetMissionBehavior<CastlesBehavior>();
            this._factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            this._factionsBehavior.OnPlayerJoinedFaction += OnPlayerJoinedFaction;
            this._castleBehavior.OnCastleUpdated += OnCastleUpdated;
            this._defaultSpawnPoints = new List<PE_SpawnFrame>();
            this.GetSelectableCastles();
            this.GetDefaultSpawnPoints();
            // base.Mission.GetMissionBehavior<MultiplayerMissionAgentVisualSpawnComponent>().OnMyAgentVisualSpawned += this.OnMainAgentVisualSpawned;
        }

        private void OnPlayerJoinedFaction(int factionIndex, Faction faction, int joinedFromIndex, NetworkCommunicator player)
        {
            if (player.IsMine)
            {
                this.GetSelectableCastles();
                this.GetDefaultSpawnPoints();
            }
        }

        private void GetDefaultSpawnPoints()
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;
            int factionIndex = persistentEmpireRepresentative.GetFactionIndex();
            this._defaultSpawnPoints = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_SpawnFrame>()
                .Select(g => g.GetFirstScriptOfType<PE_SpawnFrame>())
                .Where(p => !p.SpawnFromCastle && (p.FactionIndex == factionIndex || p.FactionIndex == 0)).ToList();
        }

        private void OnCastleUpdated(PE_CastleBanner castle)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (_selectableCastles == null) return;
            if (persistentEmpireRepresentative == null) return;
            int factionIndex = persistentEmpireRepresentative.GetFactionIndex();
            if (_selectableCastles.ContainsKey(castle) && castle.FactionIndex != factionIndex)
            {
                _selectableCastles.Remove(castle);
                if (this._isEnabled)
                {
                    if (this._selectableCastles.Keys.Count > 0)
                    {
                        this._selectedIndex = 0;
                        this._selectedCastle = this._selectableCastles.Keys.ToList()[0];
                        this.UpdateSelection(this._selectedCastle, this._selectedIndex);
                    }
                    else
                    {
                        this.UpdateSelection(null, 0);
                    }
                }
            }
            if (_selectableCastles.ContainsKey(castle) == false && castle.FactionIndex == factionIndex)
            {
                this._selectableCastles[castle] = this.GetCastleSpawnFrames(castle);

                if (this._isEnabled)
                {
                    if (this._selectableCastles.Keys.Count > 0)
                    {
                        this._selectedIndex = 0;
                        this._selectedCastle = this._selectableCastles.Keys.ToList()[0];
                        this.UpdateSelection(this._selectedCastle, this._selectedIndex);
                    }
                }
            }
        }

        private List<PE_SpawnFrame> GetCastleSpawnFrames(PE_CastleBanner castle)
        {
            List<PE_SpawnFrame> frames = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_SpawnFrame>()
                .Select(g => g.GetFirstScriptOfType<PE_SpawnFrame>()).ToList();
            return base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_SpawnFrame>()
                .Select(g => g.GetFirstScriptOfType<PE_SpawnFrame>())
                .Where(p => p.CastleIndex == castle.CastleIndex && p.SpawnFromCastle).ToList();
        }


        public void GetSelectableCastles()
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;
            int factionIndex = persistentEmpireRepresentative.GetFactionIndex();
            List<GameEntity> gameEntities = new List<GameEntity>();
            base.Mission.Scene.GetAllEntitiesWithScriptComponent<PE_CastleBanner>(ref gameEntities);
            foreach (GameEntity entity in gameEntities)
            {
                PE_CastleBanner castle = entity.GetFirstScriptOfType<PE_CastleBanner>();
                if (castle.FactionIndex == factionIndex || castle.FactionIndex == 0)
                {
                    this._selectableCastles[castle] = this.GetCastleSpawnFrames(castle);
                }
            }
        }

        public void Enable()
        {
            if (this._selectableCastles.Count == 0) this.GetSelectableCastles();
            if (this._defaultSpawnPoints.Count == 0) this.GetDefaultSpawnPoints();
            this._dataSource.SpawnTimer = 30;
            this._spawnerStartedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this._isEnabled = true;
            if (this._selectableCastles.Keys.Count > 0)
            {
                this._selectedIndex = 0;
                this._selectedCastle = this._selectableCastles.Keys.ToList()[0];
                this.UpdateSelection(this._selectedCastle, this._selectedIndex);
            }
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
            if (affectedAgent.IsMine && blow.DamageType != DamageTypes.Invalid)
            {
                // this._dataSource.OnMainAgentRemoved(affectorAgent, blow);
                this.Enable();
            }
        }


        private int CorrectSelectedIndex(PE_CastleBanner selectedCastle, int selectedIndex)
        {
            if (selectedCastle != null)
            {
                if (this._selectableCastles.ContainsKey(selectedCastle) && selectedIndex >= this._selectableCastles[selectedCastle].Count) return this._selectableCastles[selectedCastle].Count - 1;
                return selectedIndex;
            }
            else
            {
                if (selectedIndex >= this._defaultSpawnPoints.Count) return this._defaultSpawnPoints.Count - 1;
                return selectedIndex;
            }

        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (this._isEnabled)
            {
                if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.Tilde))
                {
                    List<PE_CastleBanner> castles = this._selectableCastles.Keys.ToList();
                    // this._selectedCastleIndex = this._selectedIndex % (castles.Count + 1);
                    bool flag = false;
                    while (!flag)
                    {
                        this._selectedCastleIndex = this._selectedCastleIndex + 1;
                        this._selectedCastleIndex = this._selectedCastleIndex >= castles.Count ? -1 : this._selectedCastleIndex;
                        if (this._selectedCastleIndex == -1)
                        {
                            this._selectedCastle = null;
                            flag = true;
                            break;
                        }
                        else if (this._selectableCastles[castles[this._selectedCastleIndex]].Count > 0)
                        {
                            this._selectedCastle = castles[this._selectedCastleIndex];
                            flag = false;
                            break;
                        }
                    }
                    this._selectedIndex = CorrectSelectedIndex(this._selectedCastle, this._selectedIndex);
                    this.UpdateSelection(this._selectedCastle, this._selectedIndex);
                }
                if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.D1))
                {
                    this._selectedIndex = 0;
                    this._selectedIndex = CorrectSelectedIndex(this._selectedCastle, this._selectedIndex);
                    this.UpdateSelection(this._selectedCastle, this._selectedIndex);
                }
                else if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.D2))
                {
                    this._selectedIndex = 1;
                    this._selectedIndex = CorrectSelectedIndex(this._selectedCastle, this._selectedIndex);
                    this.UpdateSelection(this._selectedCastle, this._selectedIndex);
                }
                else if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.D3))
                {
                    this._selectedIndex = 2;
                    this._selectedIndex = CorrectSelectedIndex(this._selectedCastle, this._selectedIndex);
                    this.UpdateSelection(this._selectedCastle, this._selectedIndex);
                }
                else if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.D4))
                {
                    this._selectedIndex = 3;
                    this._selectedIndex = CorrectSelectedIndex(this._selectedCastle, this._selectedIndex);
                    this.UpdateSelection(this._selectedCastle, this._selectedIndex);
                }
                else if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.D5))
                {
                    this._selectedIndex = 4;
                    this._selectedIndex = CorrectSelectedIndex(this._selectedCastle, this._selectedIndex);
                    this.UpdateSelection(this._selectedCastle, this._selectedIndex);
                }
            }
        }

        public void UpdateSelection(PE_CastleBanner selectedCastle, int selectedIndex)
        {
            PE_SpawnFrame selectedFrame;
            if (selectedCastle != null && this._selectableCastles.ContainsKey(selectedCastle) && this._selectableCastles[selectedCastle].Count > selectedIndex)
            {
                selectedFrame = this._selectableCastles[selectedCastle][selectedIndex];
            }
            else
            {
                selectedFrame = this._defaultSpawnPoints[selectedIndex];
            }
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new PreferredSpawnPoint(selectedFrame));
            GameNetwork.EndModuleEventAsClient();

            this._dataSource.SelectedCastle = selectedCastle == null ? null : new CastleVM(selectedCastle);
            this._dataSource.SelectedSpawnPosition = selectedIndex;
            Vec3 elevated = selectedFrame.GameEntity.GetGlobalFrame().Elevate(2).origin;
            base.MissionScreen.CombatCamera.Position = elevated;
            base.MissionScreen.CombatCamera.LookAt(elevated, elevated, selectedFrame.GameEntity.GetGlobalFrame().rotation.u);
        }

        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (this._isEnabled && this._dataSource.SpawnTimer > 0)
            {
                this._dataSource.SpawnTimer = 30 - (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - this._spawnerStartedAt);
                if (this._dataSource.SpawnTimer < 0) this._dataSource.SpawnTimer = 0;
            }
        }


        private void OnMainAgentVisualSpawned()
        {
            this._dataSource.SpawnTimer = 0;
            this._isEnabled = false;
            // this._selectableCastles.Clear();
        }
    }
}

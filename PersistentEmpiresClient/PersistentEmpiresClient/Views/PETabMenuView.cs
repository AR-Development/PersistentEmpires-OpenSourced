using PersistentEmpires.Views.ViewsVM;
using PersistentEmpires.Views.ViewsVM.PETabMenu;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using PersistentEmpiresLib.Helpers;

namespace PersistentEmpires.Views.Views
{
    public class PETabMenuView : MissionView
    {
        private PETabMenuVM _dataSource;
        private GauntletLayer _gauntletLayer;
        private bool _isActive;
        private FactionsBehavior _factionsBehavior;
        private CastlesBehavior _castlesBehavior;
        private bool _isMouseVisible;
        private bool _mouseRequstedWhileScoreboardActive;

        public Action<bool> OnScoreboardToggled;


        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            base.Mission.IsFriendlyMission = false;
            base.MissionScreen.SceneLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ScoreboardHotKeyCategory"));
            this.InitializeLayer();
        }

        public void OnFactionAdded(Faction faction, int factionIndex)
        {
            TabFactionVM tbv = new TabFactionVM(faction, factionIndex, (TabFactionVM tabfaction) =>
            {
                this._dataSource.SelectedFaction.IsSelected = false;
                tabfaction.IsSelected = true;
                this._dataSource.SelectedFaction = tabfaction;
                // this._dataSource.SelectedFaction.
                foreach (TabFactionVM tabFactionVM in this._dataSource.Factions)
                {
                    tabFactionVM.ShowWarIcon = tabfaction.factionObj.warDeclaredTo.Contains(tabFactionVM.FactionIndex) || tabFactionVM.factionObj.warDeclaredTo.Contains(tabfaction.FactionIndex);
                }
            });

            if (!this._dataSource.Factions.Any(x=> x.FactionIndex == factionIndex))
            {
                this._dataSource.Factions.Add(tbv);
            }

            if (this._dataSource.SelectedFaction == null)
            {
                tbv.IsSelected = true;
                this._dataSource.SelectedFaction = this._dataSource.Factions[0];
            }
        }

        public void OnPlayerJoined(int factionIndex, Faction faction, int joinedFromIndex, NetworkCommunicator player)
        {
            if (joinedFromIndex != -1)
            {
                int indexAt = this._dataSource.Factions[joinedFromIndex].Members
                     .Select((value, index) => new { value, index })
                     .Where(pair => pair.value.GetPeer().VirtualPlayer.ToPlayerId() == player.VirtualPlayer.ToPlayerId())
                     .Select(pair => pair.index + 1)
                     .FirstOrDefault() - 1;
                this._dataSource.RemoveMemberAtIndex(joinedFromIndex, indexAt);
                // this._dataSource.OnPropertyChanged("AllMemberCount");
            }


            // this._dataSource.Factions[factionIndex].Members.Add(new TabPlayerVM(player, faction.lordId == player.VirtualPlayer.Id.ToString()));
            if (faction != null)
            {
                TabPlayerVM newPlayer = new TabPlayerVM(player, faction.lordId == player.VirtualPlayer.ToPlayerId());
                this._dataSource.AddMember(factionIndex, newPlayer);
                newPlayer.OnPropertyChanged("UserClass");
            }
            // this._dataSource.OnPropertyChanged("AllMemberCount");
            if (player.IsMine)
            {
                if (joinedFromIndex != -1)
                {
                    foreach (TabPlayerVM t in this._dataSource.Factions[joinedFromIndex].Members) t.OnPropertyChanged("CanSeeClass");
                }
                if (factionIndex != -1)
                {
                    foreach (TabPlayerVM t in this._dataSource.Factions[factionIndex].Members) t.OnPropertyChanged("CanSeeClass");
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            bool flag2 = base.MissionScreen.SceneLayer.Input.IsHotKeyDown("HoldShow") || this._gauntletLayer.Input.IsHotKeyDown("HoldShow");
            bool isActive = (flag2 && !base.MissionScreen.IsRadialMenuActive && !base.Mission.IsOrderMenuOpen);
            if (this._isActive && (base.MissionScreen.SceneLayer.Input.IsGameKeyPressed(35) || this._gauntletLayer.Input.IsGameKeyPressed(35)))
            {
                this._mouseRequstedWhileScoreboardActive = true;
            }
            bool mouseState = (this._isActive && this._mouseRequstedWhileScoreboardActive);
            this.SetMouseState(mouseState);

            this.ToggleScoreboard(isActive);
        }

        private void SetMouseState(bool isMouseVisible)
        {
            if (this._isMouseVisible != isMouseVisible)
            {
                this._isMouseVisible = isMouseVisible;
                if (!this._isMouseVisible)
                {
                    this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
                }
                else
                {
                    this._gauntletLayer.InputRestrictions.SetInputRestrictions(this._isMouseVisible, InputUsageMask.Mouse);
                }
                if (this._dataSource == null)
                {
                    return;
                }
                this._dataSource.SetMouseState(isMouseVisible);
            }
        }

        private void ToggleScoreboard(bool isActive)
        {
            if (this._isActive != isActive)
            {
                this._isActive = isActive;
                this._dataSource.IsActive = this._isActive;
                base.MissionScreen.SetCameraLockState(this._isActive);
                if (!this._isActive)
                {
                    this._mouseRequstedWhileScoreboardActive = false;
                }
                Action<bool> onScoreboardToggled = this.OnScoreboardToggled;
                if (onScoreboardToggled == null)
                {
                    return;
                }
                onScoreboardToggled(this._isActive);
            }

        }
        private void OnCastleAdded(int factionIndex, PE_CastleBanner castle)
        {
            CastleVM castleVM = new CastleVM(castle);
            if (this._dataSource.Factions.Count > factionIndex)
            {
                if (!this._dataSource.Factions[factionIndex].Castles.Any(x => x.GetCastleBanner().CastleIndex ==  castle.CastleIndex))
                {
                    this._dataSource.Factions[factionIndex].Castles.Add(castleVM);
                }
            }
        }
        private void OnUpdateCastle(PE_CastleBanner castle)
        {
            this.OnCastleAdded(castle.FactionIndex, castle);
            foreach (TabFactionVM f in this._dataSource.Factions)
            {
                List<CastleVM> _castleVm = f.Castles.Where((CastleVM c) => c.GetCastleBanner().CastleIndex == castle.CastleIndex && castle.FactionIndex != f.FactionIndex).ToList();
                if (_castleVm.Count > 0)
                {
                    f.Castles.Remove(_castleVm[0]);
                }
            }
        }

        private void OnFactionUpdate(int factionIndex, Faction faction)
        {
            this._dataSource.Factions[factionIndex].FactionName = faction.name;
            BannerCode bannerCode = BannerCode.CreateFrom(faction.banner);
            this._dataSource.Factions[factionIndex].BannerImage = new ImageIdentifierVM(bannerCode, true);
            this._dataSource.Factions[factionIndex].RefreshValues();
        }
        private void InitializeLayer()
        {
            this._dataSource = new PETabMenuVM();
            this._factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            this._factionsBehavior.OnFactionAdded += this.OnFactionAdded;
            this._factionsBehavior.OnPlayerJoinedFaction += this.OnPlayerJoined;
            this._factionsBehavior.OnFactionUpdated += this.OnFactionUpdate;
            this._factionsBehavior.OnFactionLordChanged += this.OnFactionLordChanged;
            this._factionsBehavior.OnFactionMarshallChanged += this.OnFactionMarshallChanged;
            this._factionsBehavior.OnFactionDeclaredWar += this.OnFactionDeclaredSomething;
            this._factionsBehavior.OnFactionMakePeace += this.OnFactionDeclaredSomething;
            foreach (int key in this._factionsBehavior.Factions.Keys)
            {
                this.OnFactionAdded(this._factionsBehavior.Factions[key], key);
            }
            this._gauntletLayer = new GauntletLayer(2);
            this._gauntletLayer.LoadMovie("PETabMenu", this._dataSource);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("ScoreboardHotKeyCategory"));

            this._castlesBehavior = base.Mission.GetMissionBehavior<CastlesBehavior>();
            this._castlesBehavior.OnCastleAdded += this.OnCastleAdded;
            this._castlesBehavior.OnCastleUpdated += this.OnUpdateCastle;
            foreach (PE_CastleBanner castleBanner in this._castlesBehavior.castles.Values)
            {
                this.OnUpdateCastle(castleBanner);
            }
            base.MissionScreen.AddLayer(this._gauntletLayer);
        }

        private void OnFactionDeclaredSomething(int declarer, int declaredTo)
        {
            if (this._dataSource.SelectedFaction != null)
            {
                foreach (TabFactionVM tabFactionVM in this._dataSource.Factions)
                {
                    tabFactionVM.ShowWarIcon = this._dataSource.SelectedFaction.factionObj.warDeclaredTo.Contains(tabFactionVM.FactionIndex) || tabFactionVM.factionObj.warDeclaredTo.Contains(this._dataSource.SelectedFaction.FactionIndex); ;

                }
            }
        }
        private void OnFactionMarshallChanged(Faction faction, int factionIndex, NetworkCommunicator newMarshall)
        {
            foreach (TabPlayerVM playerVM in this._dataSource.Factions[factionIndex].Members) playerVM.UpdateMarshall(newMarshall.VirtualPlayer.ToPlayerId());

        }
        private void OnFactionLordChanged(Faction faction, int factionIndex, NetworkCommunicator newLord)
        {
            foreach (TabPlayerVM playerVM in this._dataSource.Factions[factionIndex].Members) playerVM.UpdateLord(newLord.VirtualPlayer.ToPlayerId());
        }
    }
}

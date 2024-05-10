using PersistentEmpires.Views.ViewsVM.FactionManagement;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views.FactionManagement
{
    public class PEFactionLordPoll : MissionView
    {
        private FactionPollComponent _factionPollComponent;
        private FactionsBehavior _factionsBehavior;
        private PEFactionLordPollVM _dataSource;
        private GauntletLayer _gauntletLayer;
        private bool _isActive;

        private InputContext _input
        {
            get
            {
                return base.MissionScreen.SceneLayer.Input;
            }
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._factionPollComponent = base.Mission.GetMissionBehavior<FactionPollComponent>();
            this._factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            this._factionPollComponent.OnStartLordPoll += this.OnLordPoll;
            this._factionPollComponent.OnPollUpdate += this.OnPollUpdated;
            this._factionPollComponent.OnPollClosed += this.OnPollClosed;
            this._dataSource = new PEFactionLordPollVM();
            this._gauntletLayer = new GauntletLayer(24, "GauntletLayer", false);
            this._gauntletLayer.LoadMovie("MultiplayerPollingProgress", this._dataSource);
            this._input.RegisterHotKeyCategory(HotKeyManager.GetCategory("PollHotkeyCategory"));
            this._dataSource.AddKey(HotKeyManager.GetCategory("PollHotkeyCategory").GetGameKey(106));
            this._dataSource.AddKey(HotKeyManager.GetCategory("PollHotkeyCategory").GetGameKey(107));
            base.MissionScreen.AddLayer(this._gauntletLayer);
        }

        private void OnPollClosed(bool result, NetworkCommunicator subject)
        {
            this._isActive = false;
            this._dataSource.OnPollClosed();
        }

        public override void OnMissionScreenFinalize()
        {
            this._factionPollComponent.OnStartLordPoll -= this.OnLordPoll;
            this._factionPollComponent.OnPollUpdate -= this.OnPollUpdated;
            this._factionPollComponent.OnPollClosed -= this.OnPollClosed;
            base.OnMissionScreenFinalize();
        }

        private void OnPollUpdated(int votesAccepted, int votesRejected)
        {
            this._dataSource.OnPollUpdated(votesAccepted, votesRejected);
        }
        private void OnLordPoll(MissionPeer pollStarter, MissionPeer lordCandidate)
        {
            this._isActive = true;
            this._dataSource.OnLordPoll(pollStarter, lordCandidate);
        }
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            if (this._isActive)
            {
                if (this._input.IsGameKeyPressed(106))
                {
                    this._isActive = false;
                    this._factionPollComponent.Vote(true);
                    this._dataSource.OnPollOptionPicked();
                    return;
                }
                if (this._input.IsGameKeyPressed(107))
                {
                    this._isActive = false;
                    this._factionPollComponent.Vote(false);
                    this._dataSource.OnPollOptionPicked();
                }
            }
        }

    }
}

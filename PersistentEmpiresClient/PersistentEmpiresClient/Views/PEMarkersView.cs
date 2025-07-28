using PersistentEmpires.Views.ViewsVM;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views.Views
{
    public class PEMarkersView : MissionView
    {
        private LocalChatComponent localChatComponent;
        private MoneyPouchBehavior moneyPouchBehavior;
        private PEMapView _peMapView;
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._dataSource = new PEMarkerVM(base.MissionScreen.CombatCamera);
            this._gauntletLayer = new GauntletLayer(1, "GauntletLayer", false);
            this._gauntletLayer.LoadMovie("PEMarkers", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            this.localChatComponent = base.Mission.GetMissionBehavior<LocalChatComponent>();
            this.moneyPouchBehavior = base.Mission.GetMissionBehavior<MoneyPouchBehavior>();
            localChatComponent.OnPlayerIsTypingMessage += OnPlayerIsTypingMessage;
            this.localChatComponent.OnLocalChatMessage += this.OnLocalChatMessage;
            this.localChatComponent.OnCustomBubbleMessage += this.OnCustomBubbleMessage;
            this.localChatComponent.OnCustomBubbleMessage2 += this.OnCustomBubbleMessage2;
            this.moneyPouchBehavior.OnRevealedMoneyPouch += this.OnRevealedMoneyPouch;
            this.factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            this.factionsBehavior.OnPlayerJoinedFaction += this.OnPlayerJoinedFaction;
            this._dataSource.RefreshPeerMarkers();
            this._peMapView = base.Mission.GetMissionBehavior<PEMapView>();
        }

        private void OnPlayerIsTypingMessage(NetworkCommunicator Sender)
        {
            if (Sender.ControlledAgent == null || Sender.Equals(GameNetwork.MyPeer) || _peMapView.IsActive)
            {
                return;
            }

            _dataSource.NotifyTyping(Sender);
        }

        private void OnCustomBubbleMessage(NetworkCommunicator Sender, string Message, bool shout)
        {
            if (Sender.ControlledAgent == null || Sender.Equals(GameNetwork.MyPeer) || _peMapView.IsActive)
            {
                return;
            }

            _dataSource.AddChatBubble(Sender, Message, "#ab47bcFF");
        }

        public void OnCustomBubbleMessage2(NetworkCommunicator Sender, string Message, string color)
        {
            if (Sender.ControlledAgent == null) return;
            if (Sender.Equals(GameNetwork.MyPeer)) return;
            if (this._peMapView.IsActive) return;


            this._dataSource.AddChatBubble(Sender, Message, color);
        }

        private void OnPlayerJoinedFaction(int factionIndex, Faction faction, int joinedFromIndex, NetworkCommunicator player)
        {
            if (player.GetComponent<MissionPeer>() != null)
            {
                this._dataSource.AddPeerMarker(player.GetComponent<MissionPeer>());
            }
        }

        private void OnRevealedMoneyPouch(NetworkCommunicator player, int Gold)
        {
            if (player.ControlledAgent == null) return;
            if (player.Equals(GameNetwork.MyPeer)) return;
            this._dataSource.AddChatBubble(player, player.UserName + " revealed his money pouch (" + Gold + "g)", "#FFEB3BFF");
        }

        private void OnLocalChatMessage(NetworkCommunicator Sender, string Message, bool shout)
        {
            if (Sender.ControlledAgent == null) return;
            if (Sender.Equals(GameNetwork.MyPeer)) return;
            if (this._peMapView.IsActive) return;


            this._dataSource.OnLocalChatMessage(Sender, Message, shout);
        }

        public override void OnMissionScreenFinalize()
        {
            base.OnMissionScreenFinalize();
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;
            this._dataSource.OnFinalize();
            this._dataSource = null;
        }
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            this._dataSource.Tick(dt);
        }

        private GauntletLayer _gauntletLayer;
        private PEMarkerVM _dataSource;
        private FactionsBehavior factionsBehavior;
    }
}

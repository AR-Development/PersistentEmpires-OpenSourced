using PersistentEmpiresLib;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.ViewModelCollection.EscapeMenu;

namespace PersistentEmpires.Views.Views
{
    public class PEEscapeMenu : MissionGauntletMultiplayerEscapeMenu
    {
        private MissionOptionsComponent _missionOptionsComponent;
        private FactionUIComponent _factionManagementComponent;
        private FactionPollComponent _factionPollComponent;
        private PersistentEmpireRepresentative _persistentEmpireRepresentative;
        private ProximityChatComponent _proximityChatComponent;
        private AdminClientBehavior _adminBehavior;
        public PEEscapeMenu(string gameType) : base(gameType)
        {

        }

        public override bool OnEscape()
        {
            PEInventoryScreen inventoryScreen = base.Mission.GetMissionBehavior<PEInventoryScreen>();
            if(inventoryScreen.IsActive) {
                inventoryScreen.CloseInventory();
                return false;
            }
            return base.OnEscape();
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._missionOptionsComponent = base.Mission.GetMissionBehavior<MissionOptionsComponent>();
            this._factionManagementComponent = base.Mission.GetMissionBehavior<FactionUIComponent>();
            this._factionPollComponent = base.Mission.GetMissionBehavior<FactionPollComponent>();
            this._adminBehavior = base.Mission.GetMissionBehavior<AdminClientBehavior>();
            this._proximityChatComponent = base.Mission.GetMissionBehavior<ProximityChatComponent>();
            TextObject title = new TextObject("Persistent Empires");
            this.DataSource = new MPEscapeMenuVM(null, title);
        }

        protected override List<EscapeMenuItemVM> GetEscapeMenuItems()
        {
            List<EscapeMenuItemVM> list = new List<EscapeMenuItemVM>();
            _persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            list.Add(new EscapeMenuItemVM(new TextObject("{=e139gKZc}Return to the Game", null), delegate (object o)
            {
                base.OnEscapeMenuToggled(false);
            }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));
            list.Add(new EscapeMenuItemVM(new TextObject("{=NqarFr4P}Options", null), delegate (object o)
            {
                base.OnEscapeMenuToggled(false);
                MissionOptionsComponent missionOptionsComponent = this._missionOptionsComponent;
                if (missionOptionsComponent == null)
                {
                    return;
                }
                missionOptionsComponent.OnAddOptionsUIHandler();
            }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));
            if(this._proximityChatComponent != null)
            {
                list.Add(new EscapeMenuItemVM(new TextObject("Voice Chat Options", null), delegate (object o)
                {
                    base.OnEscapeMenuToggled(false);
                    this._proximityChatComponent.HandleOption();
                }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));
            }
            if (_persistentEmpireRepresentative != null && _persistentEmpireRepresentative.IsAdmin)
            {
                list.Add(new EscapeMenuItemVM(new TextObject("Admin Panel", null), delegate(object o)
                {
                    base.OnEscapeMenuToggled(false);
                    this._adminBehavior.HandleAdminPanelClick();

                }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));
            }

            if (_persistentEmpireRepresentative != null && _persistentEmpireRepresentative.GetFaction() != null && (_persistentEmpireRepresentative.GetFaction().lordId == GameNetwork.MyPeer.VirtualPlayer.Id.ToString() || _persistentEmpireRepresentative.GetFaction().marshalls.Contains(GameNetwork.MyPeer.VirtualPlayer.Id.ToString())))
            {
                list.Add(new EscapeMenuItemVM(new TextObject("Manage Your Faction", null), delegate (object o)
                {
                    base.OnEscapeMenuToggled(false);
                    if (this._factionManagementComponent == null)
                    {
                        return;
                    }
                    this._factionManagementComponent.OnFactionManagementClickHandler();
                }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));
            }
            if(_persistentEmpireRepresentative != null && _persistentEmpireRepresentative.GetFactionIndex() > 1)
            {
                list.Add(new EscapeMenuItemVM(new TextObject("Poll A Lord", null), delegate (object o)
                {
                    base.OnEscapeMenuToggled(false);
                    if (this._factionManagementComponent == null)
                    {
                        return;
                    }
                    MissionPeer myPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
                    // this._factionPollComponent.RequestLordPlayerPoll(GameNetwork.MyPeer);
                    this._factionManagementComponent.OnFactionLordPollClickHandler();
                }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));
            }
            list.Add(new EscapeMenuItemVM(new TextObject("Commit Suicide", null), delegate (object o) {
                InquiryData inquiry = new InquiryData("Are you sure ?", "You will die and lose your items. Are you sure ?", true, true, "Yes", "No", () =>
                    {
                        base.OnEscapeMenuToggled(false);
                        if(Agent.Main != null)
                        {
                            GameNetwork.BeginModuleEventAsClient();
                            GameNetwork.WriteMessage(new RequestSuicide());
                            GameNetwork.EndModuleEventAsClient();
                        }
                    },
                () => { 
                    
                });
                InformationManager.ShowInquiry(inquiry);
            }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));

            list.Add(new EscapeMenuItemVM(new TextObject("{=InGwtrWt}Quit", null), delegate (object o)
            {
                
                InformationManager.ShowInquiry(
                    new InquiryData(
                        new TextObject("{=InGwtrWt}Quit", null).ToString(),
                        new TextObject("{=lxq6SaQn}Are you sure want to quit?", null).ToString(),
                        true,
                        true,
                        GameTexts.FindText("str_yes", null).ToString(),
                        GameTexts.FindText("str_no", null).ToString(),
                        delegate ()
                {
                    LobbyClient gameClient = NetworkMain.GameClient;
                    if (gameClient.CurrentState == LobbyClient.State.InCustomGame)
                    {
                        gameClient.QuitFromCustomGame();
                        return;
                    }
                    if (gameClient.CurrentState == LobbyClient.State.HostingCustomGame)
                    {
                        gameClient.EndCustomGame();
                        return;
                    }
                    gameClient.QuitFromMatchmakerGame();
                }, null, "", 0f, null), false, false);
            }, null, () => new Tuple<bool, TextObject>(false, TextObject.Empty), false));
            return list;
        }
    }
}

using PersistentEmpires.Views.Views;
using PersistentEmpires.Views.Views.AdminPanel;
using PersistentEmpires.Views.Views.FactionManagement;
using PersistentEmpiresLib;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace PersistentEmpires.Views
{
    [ViewCreatorModule]
    public class PersistentEmpireViews
    {
        [ViewMethod("PersistentEmpires")]
        public static MissionView[] OpenPersistentEmpires(Mission mission)
        {
            List<MissionView> list = new List<MissionView>();
            list.Add(MultiplayerViewCreator.CreateMissionServerStatusUIHandler());
            list.Add(new PEEscapeMenu(Main.ModuleName));
            list.Add(ViewCreator.CreateOptionsUIHandler());
            list.Add(ViewCreator.CreateMissionBoundaryCrossingView());
            list.Add(ViewCreator.CreateMissionMainAgentEquipDropView(mission));
            list.Add(ViewCreator.CreateMissionMainAgentEquipmentController(mission));
            list.Add(MultiplayerViewCreator.CreateMissionKillNotificationUIHandler());
            list.Add(new PERangedSiegeWeaponViewController());
            list.Add(new PEAgentLabelUIHandler());
            list.Add(new PEConsumeFoodView());
            list.Add(new PEAgentStatusView());
            list.Add(new PEFactionKickPlayer());
            list.Add(new MissionBoundaryWallView());
            list.Add(new PESpawnMissionView());
            list.Add(new PETabMenuView());
            list.Add(new PEFactionManagementMenu());
            list.Add(new PEFactionChangeBanner());
            list.Add(new PEFactionChangeName());
            list.Add(new PEFactionDoorKeys());
            list.Add(new PEFactionLordPoll());
            list.Add(new PELordPollPick());
            list.Add(new PEFactionChestKeys());
            list.Add(new PEInventoryScreen());
            list.Add(new PEFactionDiplomacy());
            list.Add(new PEImportExport());
            list.Add(new MissionItemContourControllerView());
            list.Add(new MissionAgentContourControllerView());
            list.Add(new PECraftingStationScreen());
            list.Add(new PEStockpileMarketScreen());
            list.Add(new PEMoneyPouchScreen());
            list.Add(new PELocalChatScreen());
            list.Add(new PEUseProgressScreen());
            list.Add(new PEProximityChatView());
            list.Add(new PEDeathView());
            list.Add(new PEAdminPanelView());
            list.Add(new PEAdminPlayerManagementView());
            list.Add(new PEAdminItemPanelView());
            list.Add(new PEAdminFactionView());
            list.Add(new PEAdminGoldView());
            list.Add(new PEBannerRenderConsumer());
            list.Add(new PEMarkersView());
            list.Add(new PEDiscordView());
            list.Add(new PEWhitelistServer());
            list.Add(new PEFactionAssignMarshall());
            list.Add(new PEFactionTransferLordship());
            list.Add(new PEBankView());
            list.Add(new PEPatreonView());
            list.Add(new PETradingCenterScreen());
            list.Add(new PEPlayInstrumentView());
            list.Add(new PEMapView());
            list.Add(new PEMoneyChestView());
            list.Add(new PEAnimationsView());
            list.Add(new PEAdminChatScreen());
            list.Add(new PEChatBox());
            list.Add(new PEAdminTeleportView());
            // list.Add(new SentryMissionView());
            list.Add(MultiplayerViewCreator.CreateMultiplayerMissionDeathCardUIHandler(null));

            return list.ToArray();
        }
    }
}

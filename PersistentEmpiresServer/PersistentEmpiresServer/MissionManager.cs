using PersistentEmpiresLib;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using PersistentEmpiresServer.SpawnBehavior;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Source.Missions;

namespace PersistentEmpiresServer
{
    public class MissionManager
    {
        [MissionMethod]
        public static void OpenPersistentEmpires(string scene)
        {
            MissionState.OpenNew(Main.ModuleName, new MissionInitializerRecord(scene), delegate (Mission missionController)
            {
                return new MissionBehavior[]
                    {
                        new PersistentEmpireBehavior(),
                        new SaveSystemBehavior(), // Most important behavior
						new DayNightCycleBehavior(),
						// 2.0.0.76561198064533271
						MissionLobbyComponent.CreateBehavior(),
                        new DrowningBehavior(),
                        new NotAllPlayersJoinFixBehavior(),
                        new AnimalButcheringBehavior(),
                        new InformationComponent(),
                        new FactionsBehavior(),
                        new CastlesBehavior(),
                        new DoctorBehavior(),
                        new FactionPollComponent(),
                        new WoundingBehavior(),
                        new PlayerInventoryComponent(),
                        new PersistentEmpireClientBehavior(),
                        new PersistentEmpireSceneSyncBehaviors(),
                        new ImportExportComponent(),
                        new MultiplayerTimerComponent(),
						// new MultiplayerMissionAgentVisualSpawnComponent(),
						new CraftingComponent(),
                        new MoneyPouchBehavior(),
                        new StockpileMarketComponent(),
                        new LocalChatComponent(),
                        new InstrumentsBehavior(),
                        new CombatlogBehavior(),
						// new WhitelistBehavior(),
						new AgentHungerBehavior(),
                        new SpawnFrameSelectionBehavior(),
						// new ConsoleMatchStartEndHandler(),
						new SpawnComponent(new PersistentEmpireSpawnFrameBehavior(), new PersistentEmpiresSpawningBehavior()),
                        new MissionLobbyEquipmentNetworkComponent(),
                        new ProximityChatComponent(),
						// new MultiplayerTeamSelectComponent(),
						// new MissionHardBorderPlacer(),
						new MissionBoundaryPlacer(),
                        new MissionBoundaryCrossingHandler(),
						// new MultiplayerPollComponent(),
						// new MultiplayerAdminComponent(),
						// new MultiplayerGameNotificationsComponent(),
						new MissionOptionsComponent(),
                        new MissionScoreboardComponent(new TDMScoreboardData()),
                        new MissionAgentPanicHandler(), // APTAL ERAYIN İŞLERİ
						new AdminServerBehavior(),
                        new DiscordBehavior(),
                        new BankingComponent(),
                        new PatreonRegistryBehavior(),
                        new TradingCenterBehavior(),
                        new MoneyChestBehavior(),
						// new DecapitationBehavior(),
						new PickpocketingBehavior(),
                        new LockpickingBehavior(),
                        new PoisoningBehavior(),
                        new AutorestartBehavior(),
                        new AnimationBehavior(),
                        new ChatCommandSystem(),
                        new WhitelistBehavior(),
                        new AutoPayBehavior(),
						// new AgentHumanAILogic(),
						// new EquipmentControllerLeaveLogic(),
						// new MultiplayerPreloadHelper()
					};
            }, true, true);
        }
    }
}

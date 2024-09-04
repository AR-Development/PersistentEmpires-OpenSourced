/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
            MissionState.OpenNew("PersistentEmpires", new MissionInitializerRecord(scene), delegate (Mission missionController)
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

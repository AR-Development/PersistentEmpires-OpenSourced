using PersistentEmpiresHarmony;
using PersistentEmpiresHarmony.Patches;
using PersistentEmpiresLib.GameModes;
using PersistentEmpiresLib.PersistentEmpiresGameModels;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresSave.Database.Repositories;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Debug = TaleWorlds.Library.Debug;

namespace PersistentEmpiresServer
{
    public class PersistentEmpires : MBSubModuleBase
    {

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            base.InitializeGameStarter(game, starterObject);
            CompressionBasic.MissionObjectIDCompressionInfo = new CompressionInfo.Integer(-1, 1000000, true);
            CompressionBasic.RoundGoldAmountCompressionInfo = new CompressionInfo.Integer(0, Int32.MaxValue, true);
            CompressionBasic.MapTimeLimitCompressionInfo = new CompressionInfo.Integer(1, int.MaxValue, true);
            CompressionBasic.BigRangeLowResLocalPositionCompressionInfo = new CompressionInfo.Float(-50000f, 50000f, 16);
            CompressionBasic.PositionCompressionInfo = new CompressionInfo.Float(-200f, 10385f, 22);
            CompressionBasic.PlayerCompressionInfo = new CompressionInfo.Integer(-1, 1048576, true);




            starterObject.AddModel(new PEAgentStatCalculateModel());
            starterObject.AddModel(new DefaultItemValueModel());
            starterObject.AddModel(new PEAgentApplyDamageModel());

            PersistentEmpireSkills.Initialize(game);
        }

        public override void OnMultiplayerGameStart(Game game, object starterObject)
        {
            InformationManager.DisplayMessage(new InformationMessage("** Persistent Empires, Multiplayer Game Start Loading..."));
            Debug.Print("** Persistent Empires, Multiplayer Game Start Loading...");

            PersistentEmpiresGameMode.OnStartMultiplayerGame += MissionManager.OpenPersistentEmpires;

            PatchGlobalChat.OnClientEventPlayerMessageTeam += FactionsBehavior.PatchGlobalChat_OnClientEventPlayerMessageTeam;
            PersistentEmpiresHarmonySubModule.OnRglExceptionThrown += SaveSystemBehavior.RglExceptionThrown;
            PatchGameNetwork.OnAddNewPlayerOnServer += SaveSystemBehavior.OnAddNewPlayerOnServer;

            AdminServerBehavior.OnIsPlayerBanned += DBBanRecordRepository.IsPlayerBanned;
            AdminServerBehavior.OnBanPlayer += DBBanRecordRepository.AdminServerBehavior_OnBanPlayer;

            TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new PersistentEmpiresGameMode("PersistentEmpires"));
        }
    }
}

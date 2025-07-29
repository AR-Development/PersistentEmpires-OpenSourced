using HarmonyLib;
using PersistentEmpiresHarmony.Patches;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using TaleWorlds.DotNet;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Network.Messages;
using Debug = TaleWorlds.Library.Debug;

namespace PersistentEmpiresHarmony
{
    public class PersistentEmpiresHarmonySubModule : MBSubModuleBase
    {
        public delegate void ExceptionThrownHandler(Exception e);
        public static event ExceptionThrownHandler OnExceptionThrown;

        public delegate void RglExceptionThrownHandler(StackTrace e, Exception rglException);
        public static event RglExceptionThrownHandler OnRglExceptionThrown;

        public static void ExceptionThrown(Exception exception)
        {
            if (OnExceptionThrown != null)
            {
                OnExceptionThrown(exception);
            }
        }
        public static void RglExceptionThrown(StackTrace trace, Exception rglException)
        {
            if (OnRglExceptionThrown != null)
            {
                OnRglExceptionThrown(trace, rglException);
            }
        }
        public static PersistentEmpiresHarmonySubModule Instance;
        public HarmonyLib.Harmony HarmonyHandle;

        public void PatchFinalizer(MethodInfo original)
        {
            var finalizer = typeof(PatchForExceptions).GetMethod("FinalizerException", BindingFlags.Public | BindingFlags.Static);
            HarmonyHandle.Patch(original, finalizer: new HarmonyMethod(finalizer));
        }
        public void PatchPrefix(MethodInfo original, MethodInfo prefix)
        {
            HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));
        }
        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);

            PatchMissionNetworkComponent.OnTick();
        }
        public override void OnBeforeMissionBehaviorInitialize(Mission mission)
        {
            List<MissionObject> cachedMissionObjects = mission.MissionObjects
               .Where(o => o is SynchedMissionObject)
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_DestructibleWithItem")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_Chair")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_ChairUsePoint")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_ItemGathering")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_ChairUsePoint")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_Chair")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_CraftingStation")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_ChangeClass")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_PrefabSpawner")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_MoneyChest")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_SpawnFrame")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_UpgradeableBuildings")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_HorseMarket")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_StockpileMarket")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_AnimalSpawner")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_TeleportDoor")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_CastleBanner")
               .Where(o => o.GetType().FullName != "PersistentEmpiresLib.SceneScripts.PE_ImportExport")
               .ToList();
            PatchMissionNetworkComponent.chunkedMissionObjects = PatchMissionNetworkComponent.ChunkList<MissionObject>(100, cachedMissionObjects);

            Debug.Print("** PE Better Syncing ** Cached " + cachedMissionObjects.Count + " amount of objects ", 0, Debug.DebugColor.Cyan, 17179869184UL);
        }
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Instance = this;
            HarmonyHandle = new HarmonyLib.Harmony("mentalrob.persistentempires.bannerlord");
            Debug.Print("** Persistent Harmony ** Harmony Handle Created.", 0, Debug.DebugColor.Yellow);
            var original = typeof(MultiplayerOptionsExtensions).GetMethod("GetOptionProperty", BindingFlags.Public | BindingFlags.Static);
            Debug.Print("** Persistent Harmony ** Patched [MultiplayerOptionsExtensions::GetOptionProperty] " + original.FullDescription(), 0, Debug.DebugColor.Yellow);
            var postfix = typeof(PatchMapTimeLimit).GetMethod("Postfix");
            HarmonyHandle.Patch(original, postfix: new HarmonyMethod(postfix));
            Debug.Print("** Persistent Harmony ** Patched [MultiplayerOptionsExtensions::GetOptionProperty]", 0, Debug.DebugColor.Yellow);

            original = typeof(ChatBox).GetMethod("OnPlayerMessageReceived", BindingFlags.NonPublic | BindingFlags.Instance);
            var prefix = typeof(PatchGlobalChat).GetMethod("PrefixOnPlayerMessageReceived", BindingFlags.Public | BindingFlags.Static);
            HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));
            Debug.Print("** Persistent Harmony ** Patched [ChatBox::OnPlayerMessageReceived]", 0, Debug.DebugColor.Yellow);
            original = typeof(ChatBox).GetMethod("HandleClientEventPlayerMessageAll", BindingFlags.NonPublic | BindingFlags.Instance);
            prefix = typeof(PatchGlobalChat).GetMethod("PrefixClientEventPlayerMessageAll", BindingFlags.Public | BindingFlags.Static);
            HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));
            Debug.Print("** Persistent Harmony ** Patched [ChatBox::HandleClientEventPlayerMessageAll]", 0, Debug.DebugColor.Yellow);

            original = typeof(ChatBox).GetMethod("HandleClientEventPlayerMessageTeam", BindingFlags.NonPublic | BindingFlags.Instance);
            prefix = typeof(PatchGlobalChat).GetMethod("PrefixClientEventPlayerMessageTeam", BindingFlags.Public | BindingFlags.Static);
            HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));
            Debug.Print("** Persistent Harmony ** Patched [ChatBox::HandleClientEventPlayerMessageTeam]", 0, Debug.DebugColor.Yellow);


            original = typeof(Managed).GetMethod("GetStackTraceRaw", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(StackTrace), typeof(int) }, null);
            prefix = typeof(PatchStackTraceRaw).GetMethod("GetStackTraceRawDeep", BindingFlags.Public | BindingFlags.Static);
            HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));
            Debug.Print("** Persistent Harmony ** Patched [Managed::GetStackTraceRaw]", 0, Debug.DebugColor.Yellow);

            original = typeof(LobbyClient).GetMethod("OnJoinCustomGameResultMessage", BindingFlags.NonPublic | BindingFlags.Instance);
            prefix = typeof(PatchRequestJoin).GetMethod("PrefixOnJoinCustomGameResultMessage", BindingFlags.Public | BindingFlags.Static);
            HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));
            Debug.Print("** Persistent Harmony ** Patched [LobbyClient::OnJoinCustomGameResultMessage]", 0, Debug.DebugColor.Yellow);

            original = typeof(GameNetwork).GetMethod("AddNewPlayerOnServer", BindingFlags.Public | BindingFlags.Static);
            prefix = typeof(PatchGameNetwork).GetMethod("PrefixAddNewPlayerOnServer", BindingFlags.Public | BindingFlags.Static);
            HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));
            Debug.Print("** Persistent Harmony ** Patched [GameNetwork::AddNewPlayerOnServer]", 0, Debug.DebugColor.Yellow);


            original = typeof(GameNetwork).GetMethod("HandleServerEventCreatePlayer", BindingFlags.NonPublic | BindingFlags.Static);
            prefix = typeof(PatchGameNetwork).GetMethod("PrefixHandleServerEventCreatePlayer", BindingFlags.Public | BindingFlags.Static);
            HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));
            Debug.Print("** Persistent Harmony ** Patched [GameNetwork::HandleServerEventCreatePlayer]", 0, Debug.DebugColor.Yellow);

            original = typeof(SiegeWeapon).GetMethod("HasToBeDefendedByUser", BindingFlags.Public | BindingFlags.Instance);
            prefix = typeof(PatchSiegeWeapon).GetMethod("PrefixHasToBeDefendedByUser", BindingFlags.Public | BindingFlags.Static);
            HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));
            Debug.Print("** Persistent Harmony ** Patched [SiegeWeapon::HasToBeDefendedByUser]", 0, Debug.DebugColor.Yellow);


            // original = typeof(GameNetwork).GetMethod("WriteMessage", BindingFlags.Public | BindingFlags.Static);
            // prefix = typeof(PatchGameNetwork).GetMethod("PrefixWriteMessage", BindingFlags.Public | BindingFlags.Static);
            // HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));
            // Debug.Print("** Persistent Harmony ** Patched [GameNetwork::WriteMessage]", 0, Debug.DebugColor.Yellow);


            // Better Sync

            original = typeof(MissionNetworkComponent).GetMethod("SendExistingObjectsToPeer", BindingFlags.NonPublic | BindingFlags.Instance);
            prefix = typeof(PatchMissionNetworkComponent).GetMethod("BetterSendExistingObjectsToPeer", BindingFlags.Public | BindingFlags.Static);
            HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));
            Debug.Print("** Persistent Harmony ** Patched [GameNetwork::SendExistingObjectsToPeer]", 0, Debug.DebugColor.Yellow);

            original = typeof(GameNetworkMessage).GetMethod("WriteBannerCodeToPacket", BindingFlags.Public | BindingFlags.Static);
            prefix = typeof(PatchReadBannerCodeFromPacket).GetMethod("PrefixWriteBannerCodeToPacket", BindingFlags.Public | BindingFlags.Static);
            HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));

            original = typeof(GameNetworkMessage).GetMethod("ReadBannerCodeFromPacket", BindingFlags.Public | BindingFlags.Static);
            prefix = typeof(PatchReadBannerCodeFromPacket).GetMethod("PrefixReadBannerCodeFromPacket", BindingFlags.Public | BindingFlags.Static);
            HarmonyHandle.Patch(original, prefix: new HarmonyMethod(prefix));
        }
    }
}

using PersistentEmpiresClient;
using PersistentEmpiresLib.ErrorLogging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.PlatformService;
using TaleWorlds.ScreenSystem;

namespace PersistentEmpires.Views
{
    public class SentryForView
    {
        // public static RavenClient Raven;
        public static void InitializeSentry()
        {
            AppDomain.CurrentDomain.UnhandledException += HandleExceptionalExit;
            HarmonyLibClient.OnRglExceptionThrown += RglExceptionThrown;
            HarmonyLibClient.Instance.PatchFinalizer(typeof(Managed).GetMethod("ApplicationTick", BindingFlags.NonPublic | BindingFlags.Static));
            HarmonyLibClient.Instance.PatchFinalizer(typeof(ScriptComponentBehavior).GetMethod("OnTick", BindingFlags.NonPublic | BindingFlags.Instance));
            HarmonyLibClient.Instance.PatchFinalizer(typeof(TaleWorlds.MountAndBlade.Module).GetMethod("OnApplicationTick", BindingFlags.NonPublic | BindingFlags.Instance));
            HarmonyLibClient.Instance.PatchFinalizer(typeof(Mission).GetMethod("Tick", BindingFlags.NonPublic | BindingFlags.Instance));
            HarmonyLibClient.Instance.PatchFinalizer(typeof(MissionBehavior).GetMethod("OnMissionTick", BindingFlags.Public | BindingFlags.Instance));
            HarmonyLibClient.Instance.PatchFinalizer(typeof(MBSubModuleBase).GetMethod("OnSubModuleLoad", BindingFlags.NonPublic | BindingFlags.Instance));
            HarmonyLibClient.Instance.PatchFinalizer(typeof(MissionView).GetMethod("OnMissionScreenTick", BindingFlags.Public | BindingFlags.Instance));
            HarmonyLibClient.Instance.PatchFinalizer(typeof(ScreenManager).GetMethod("Tick", BindingFlags.Public | BindingFlags.Static));
        }

        public static void RglExceptionThrown(StackTrace obj, Exception rglException)
        {
            
        }


        private static void HandleExceptionalExit(object sender, UnhandledExceptionEventArgs args)
        {            
            Exception e = (Exception)args.ExceptionObject;
        }

        public static void Dispose()
        {
        }
    }
}

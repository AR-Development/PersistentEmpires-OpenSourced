using NetworkMessages.FromClient;
using System;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchTemporary
    {
        public static bool PrefixHandleClientEventRequestUseObject(NetworkCommunicator networkPeer, RequestUseObject message)
        {
            return true;
        }
        public static Exception FinalizerHandleClientEventRequestUseObject(Exception __exception)
        {
            if (__exception != null)
            {
                Debug.Print("ERROR ON USE ITEM");
            }

            return __exception;
        }
    }
}

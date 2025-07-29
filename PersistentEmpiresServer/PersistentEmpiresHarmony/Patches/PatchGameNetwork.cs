using NetworkMessages.FromServer;
using System.IO;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchGameNetwork
    {
        public delegate void AddNewPlayerOnServerHandler(ref PlayerConnectionInfo playerConnectionInfo, bool serverPeer, bool isAdmin);
        public static event AddNewPlayerOnServerHandler OnAddNewPlayerOnServer;
        public delegate void HandleServerEventCreatePlayerHandler(CreatePlayer message);
        public static event HandleServerEventCreatePlayerHandler OnHandleServerEventCreatePlayer;
        public static bool PrefixAddNewPlayerOnServer(ref PlayerConnectionInfo playerConnectionInfo, bool serverPeer, bool isAdmin)
        {
            if (OnAddNewPlayerOnServer != null) OnAddNewPlayerOnServer(ref playerConnectionInfo, serverPeer, isAdmin);
            //File.AppendAllText("network.txt", "== Add New Player " + playerConnectionInfo.Name + " ==\n");

            return true;
        }

        public static bool PrefixHandleServerEventCreatePlayer(CreatePlayer message)
        {
            if (OnHandleServerEventCreatePlayer != null) OnHandleServerEventCreatePlayer(message);
            return true;
        }

        public static bool PrefixWriteMessage(GameNetworkMessage message)
        {
            //Debug.Print("** PE Network ** " + message.GetType().FullName, 0, Debug.DebugColor.Cyan);
            //File.AppendAllText("network.txt", message.GetType().FullName + "\n");
            if (message is SynchronizeMissionObject)
            {
                //Debug.Print("** PE SYNCRONIZEMISSION ** " + message.GetType().FullName, 0, Debug.DebugColor.Cyan);
                SynchronizeMissionObject sMessage = (SynchronizeMissionObject)message;
                SynchedMissionObject missionObject = (SynchedMissionObject)Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(sMessage.MissionObjectId);
                //File.AppendAllText("network.txt", "  => Mission Object Is " + missionObject.GetType().Name + "\n");
            }
            return true;
        }
    }
}

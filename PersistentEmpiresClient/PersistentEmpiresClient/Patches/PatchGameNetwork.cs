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
            return true;
        }

        public static bool PrefixHandleServerEventCreatePlayer(CreatePlayer message)
        {
            if (OnHandleServerEventCreatePlayer != null) OnHandleServerEventCreatePlayer(message);
            return true;
        }
    }
}

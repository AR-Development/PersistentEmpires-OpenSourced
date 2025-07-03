using PersistentEmpiresLib.NetworkMessages.Client;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchGlobalChat
    {
        public delegate bool PlayerGlobalChatHandler(NetworkCommunicator peer, string message, bool teamOnly);
        public static event PlayerGlobalChatHandler OnGlobalChatReceived;

        public delegate bool ClientEventPlayerMessageAllHandler(NetworkCommunicator networkPeer, NetworkMessages.FromClient.PlayerMessageAll message);
        public static event ClientEventPlayerMessageAllHandler OnClientEventPlayerMessageAll;

        public delegate bool ClientEventPlayerMessageTeamHandler(NetworkCommunicator networkPeer, NetworkMessages.FromClient.PlayerMessageTeam message);
        public static event ClientEventPlayerMessageTeamHandler OnClientEventPlayerMessageTeam;
        public static bool PrefixOnPlayerMessageReceived(NetworkCommunicator networkPeer, string message, bool toTeamOnly)
        {
            if (OnGlobalChatReceived != null)
            {
                return OnGlobalChatReceived(networkPeer, message, toTeamOnly);
            }
            return true;
        }

        public static bool PrefixClientEventPlayerMessageAll(NetworkCommunicator networkPeer, NetworkMessages.FromClient.PlayerMessageAll message)
        {
            if (OnClientEventPlayerMessageAll != null)
            {
                return OnClientEventPlayerMessageAll(networkPeer, message);
            }
            return true;
        }

        public static bool PrefixClientEventPlayerMessageTeam(NetworkCommunicator networkPeer, NetworkMessages.FromClient.PlayerMessageTeam message)
        {
            if (OnClientEventPlayerMessageTeam != null)
            {
                return OnClientEventPlayerMessageTeam(networkPeer, message);
            }
            return true;
        }

        public static void PrefixSendMessageToAll(string message, List<VirtualPlayer> receiverList)
        {
            if (message != "")
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new LocalMessage(message));
                GameNetwork.EndModuleEventAsClient();
            }
        }

        public static void PrefixSendMessageToTeam(string message, List<VirtualPlayer> receiverList)
        {
            if (message != "")
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new LocalMessage(message));
                GameNetwork.EndModuleEventAsClient();
            }
        }

        public static void PrefixSendMessageToWhisperTarget(string message, string platformName, string whisperTarget)
        {
            if (message != "")
            {
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new LocalMessage(message));
                GameNetwork.EndModuleEventAsClient();
            }
        }
    }
}
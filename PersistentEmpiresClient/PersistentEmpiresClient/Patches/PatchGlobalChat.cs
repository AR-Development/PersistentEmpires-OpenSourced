using HarmonyLib;
using PersistentEmpiresLib.NetworkMessages.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer;

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

        public static bool PrefixExecuteSendMessage(MPChatVM __instance)
        {
            string str = __instance.WrittenText;
            if (string.IsNullOrEmpty(str))
            {
                __instance.WrittenText = string.Empty;
            }
            else
            {
                if (str.Length > __instance.MaxMessageLength)
                    str = __instance.WrittenText.Substring(0, __instance.MaxMessageLength);

                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new LocalMessage(str));
                GameNetwork.EndModuleEventAsClient();
            }

            return true;
        }
    }
}
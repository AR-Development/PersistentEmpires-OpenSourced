using Messages.FromLobbyServer.ToClient;
using PersistentEmpiresClient;
using System;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.Lobby.CustomGame;

namespace PersistentEmpires.Views
{
    public class Whitelister
    {
        public static bool PatchJoinCustomGame(GameServerEntry selectedServer, string passwordInput)
        {
            try
            {
                // CheckWhitelist(selectedServer.Address);
            }
            catch (Exception e)
            {
            }
            return true;
        }

        public static void Initialize()
        {
            /*PatchRequestJoin.OnJoinCustomGameResultMessage += HandleJoinCustomGameResultMessage;*/
            var original = typeof(MPCustomGameVM).GetMethod("JoinCustomGame", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var prefix = typeof(Whitelister).GetMethod("PatchJoinCustomGame", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            HarmonyLibClient.Instance.PatchPrefix(original, prefix);
        }


        public static bool HandleJoinCustomGameResultMessage(JoinCustomGameResultMessage message)
        {
            return true;
        }
    }
}

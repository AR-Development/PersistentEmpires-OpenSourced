using Messages.FromLobbyServer.ToClient;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchRequestJoin
    {
        public delegate bool HandleJoinCustomGameResultMessage(JoinCustomGameResultMessage message);
        public static event HandleJoinCustomGameResultMessage OnJoinCustomGameResultMessage;

        public static bool PrefixOnJoinCustomGameResultMessage(JoinCustomGameResultMessage message)
        {
            if (OnJoinCustomGameResultMessage != null)
            {
                return OnJoinCustomGameResultMessage(message);
            }
            return true;
        }
    }
}

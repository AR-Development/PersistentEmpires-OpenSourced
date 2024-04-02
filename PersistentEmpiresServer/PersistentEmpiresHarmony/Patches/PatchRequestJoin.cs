using Messages.FromLobbyServer.ToClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade.Diamond;

namespace PersistentEmpiresHarmony.Patches
{
    public class PatchRequestJoin
    {
        public delegate bool HandleJoinCustomGameResultMessage(JoinCustomGameResultMessage message);
        public static event HandleJoinCustomGameResultMessage OnJoinCustomGameResultMessage;

        public static bool PrefixOnJoinCustomGameResultMessage(JoinCustomGameResultMessage message)
        {
            if(OnJoinCustomGameResultMessage != null)
            {
               return OnJoinCustomGameResultMessage(message);
            }
            return true;
        }
    }
}

using TaleWorlds.Diamond;
using TaleWorlds.Library.Http;

namespace PersistentEmpiresHarmony.Patches
{
    public static class PatchClientRestSession
    {
        public static void PrefixClientRestSessionCtor(IClient client, ref string address, ref ushort port, ref bool isSecure, IHttpDriver platformNetworkClient)
        {
            address = "localhost";
            port = 3000;
            isSecure = false;
        }
    }
}

using PersistentEmpiresLib.Helpers;

namespace PersistentEmpiresClient.Patches
{
    public static class PatchReadBannerCodeFromPacket
    {
        public static bool PrefixReadBannerCodeFromPacket(ref bool bufferReadValid, ref string __result)
        {
            __result = PENetworkModule.ReadBannerCodeFromPacket(ref bufferReadValid);
            return false;
        }

        public static bool PrefixWriteBannerCodeToPacket(string bannerCode)
        {
            PENetworkModule.WriteBannerCodeToPacket(bannerCode);
            return false;
        }
    }
}

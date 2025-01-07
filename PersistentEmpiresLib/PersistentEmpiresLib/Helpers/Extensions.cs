using TaleWorlds.Core;

namespace PersistentEmpiresLib.Helpers
{
    public static class Extensions
    {
        public static string ToPlayerId(this VirtualPlayer virtualPlayer)
        {
            return $"{virtualPlayer.Id.ToString()}_{virtualPlayer.UserName}";
        }

        public static string EncodeSpecialMariaDbChars(this string tmp)
        {
            return tmp.Replace(@"""", "'").Replace(@"''", @"'").Replace(@"'", @"\'").Replace(@"\\", @"\");
        }
    }
}
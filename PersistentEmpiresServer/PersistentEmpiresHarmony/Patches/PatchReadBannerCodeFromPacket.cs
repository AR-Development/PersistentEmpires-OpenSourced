using System;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresHarmony.Patches
{
    public static class PatchReadBannerCodeFromPacket
    {
        public static String ReadBannerCodeFromPacket(ref bool bufferReadValid)
        {
            int size = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 100, true), ref bufferReadValid);
            int[] result = new int[size * 10];
            for (int i = 0; i < (size * 10); i = i + 10)
            {
                int meshId = GameNetworkMessage.ReadIntFromPacket(CompressionBasic.BannerDataMeshIdCompressionInfo, ref bufferReadValid);
                int colorId = GameNetworkMessage.ReadIntFromPacket(CompressionBasic.BannerDataColorIndexCompressionInfo, ref bufferReadValid);
                int colorId2 = GameNetworkMessage.ReadIntFromPacket(CompressionBasic.BannerDataColorIndexCompressionInfo, ref bufferReadValid);
                Vec2 sizeP = new Vec2((float)GameNetworkMessage.ReadIntFromPacket(CompressionBasic.BannerDataSizeCompressionInfo, ref bufferReadValid), (float)GameNetworkMessage.ReadIntFromPacket(CompressionBasic.BannerDataSizeCompressionInfo, ref bufferReadValid));
                Vec2 position = new Vec2((float)GameNetworkMessage.ReadIntFromPacket(CompressionBasic.BannerDataSizeCompressionInfo, ref bufferReadValid), (float)GameNetworkMessage.ReadIntFromPacket(CompressionBasic.BannerDataSizeCompressionInfo, ref bufferReadValid));
                bool drowStroke = GameNetworkMessage.ReadBoolFromPacket(ref bufferReadValid);
                bool mirror = GameNetworkMessage.ReadBoolFromPacket(ref bufferReadValid);
                int rotation = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-360, 360, true), ref bufferReadValid);

                result[i] = meshId;
                result[i + 1] = colorId;
                result[i + 2] = colorId2;
                result[i + 3] = (int)sizeP.X;
                result[i + 4] = (int)sizeP.Y;
                result[i + 5] = (int)position.X;
                result[i + 6] = (int)position.Y;
                result[i + 7] = drowStroke ? 1 : 0;
                result[i + 8] = mirror ? 1 : 0;
                result[i + 9] = rotation;
            }
            return String.Join(".", result);

        }


        public static void WriteBannerCodeToPacket(string bannerCode)
        {
            String[] parts = bannerCode.Split('.');
            GameNetworkMessage.WriteIntToPacket((int)(parts.Count() / 10), new CompressionInfo.Integer(0, 100, true));
            for (int i = 0; i < parts.Length; i = i + 10)
            {
                int meshId = int.Parse(parts[i]);
                int colorId = int.Parse(parts[i + 1]);
                int colorId2 = int.Parse(parts[i + 2]);
                Vec2 size = new Vec2((float)int.Parse(parts[i + 3]), (float)int.Parse(parts[i + 4]));
                Vec2 position = new Vec2((float)int.Parse(parts[i + 5]), (float)int.Parse(parts[i + 6]));
                bool drawStroke = int.Parse(parts[i + 7]) == 1;
                bool mirror = int.Parse(parts[i + 8]) == 1;
                int rotation = int.Parse(parts[i + 9]);

                GameNetworkMessage.WriteIntToPacket(meshId, CompressionBasic.BannerDataMeshIdCompressionInfo);
                GameNetworkMessage.WriteIntToPacket(colorId, CompressionBasic.BannerDataColorIndexCompressionInfo);
                GameNetworkMessage.WriteIntToPacket(colorId2, CompressionBasic.BannerDataColorIndexCompressionInfo);
                GameNetworkMessage.WriteIntToPacket((int)size.X, CompressionBasic.BannerDataSizeCompressionInfo);
                GameNetworkMessage.WriteIntToPacket((int)size.Y, CompressionBasic.BannerDataSizeCompressionInfo);
                GameNetworkMessage.WriteIntToPacket((int)position.X, CompressionBasic.BannerDataSizeCompressionInfo);
                GameNetworkMessage.WriteIntToPacket((int)position.Y, CompressionBasic.BannerDataSizeCompressionInfo);
                GameNetworkMessage.WriteBoolToPacket(drawStroke);
                GameNetworkMessage.WriteBoolToPacket(mirror);
                GameNetworkMessage.WriteIntToPacket(rotation, new CompressionInfo.Integer(-360, 360, true));
            }
        }


        public static bool PrefixReadBannerCodeFromPacket(ref bool bufferReadValid, ref string __result)
        {
            __result = ReadBannerCodeFromPacket(ref bufferReadValid);
            return false;
        }

        public static bool PrefixWriteBannerCodeToPacket(string bannerCode)
        {
            WriteBannerCodeToPacket(bannerCode);
            return false;
        }
    }
}

using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.Helpers
{
    public static class PENetworkModule
    {
        public static void WriteInventoryPlayer(Inventory PlayerInventory)
        {
            GameNetworkMessage.WriteIntToPacket(PlayerInventory.Slots.Count, new CompressionInfo.Integer(0, 10, true));
            for (int i = 0; i < PlayerInventory.Slots.Count; i++)
            {
                GameNetworkMessage.WriteStringToPacket(PlayerInventory.Slots[i].Item != null ? PlayerInventory.Slots[i].Item.StringId : "");
                GameNetworkMessage.WriteIntToPacket(PlayerInventory.Slots[i].Count, new CompressionInfo.Integer(0, 20, true));
            }
        }
        public static Inventory ReadInventoryPlayer(ref bool result)
        {
            int playerInventorySlot = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 10, true), ref result);
            Inventory PlayerInventory = new Inventory(playerInventorySlot, 10, "PlayerInventory");
            for (int i = 0; i < PlayerInventory.Slots.Count; i++)
            {
                string itemId = GameNetworkMessage.ReadStringFromPacket(ref result);
                int count = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 20, true), ref result);
                ItemObject Item = itemId == "" ? null : MBObjectManager.Instance.GetObject<ItemObject>(itemId);
                PlayerInventory.Slots[i].Item = Item;
                PlayerInventory.Slots[i].Count = count;
            }
            return PlayerInventory;
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

        public static void WriteCustomInventory(Inventory RequestedInventory)
        {
            GameNetworkMessage.WriteIntToPacket(RequestedInventory.Slots.Count, new CompressionInfo.Integer(0, 256, true));
        }

        public static void WriteInventorySlots(Inventory RequestedInventory, NetworkCommunicator peer)
        {
            for (int i = 0; i < RequestedInventory.Slots.Count; i++)
            {
                if (RequestedInventory.Slots[i].Item == null || RequestedInventory.Slots[i].Count == 0) continue;
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new UpdateInventorySlot(RequestedInventory.InventoryId + "_" + i, RequestedInventory.Slots[i].Item, RequestedInventory.Slots[i].Count));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        public static Inventory ReadCustomInventory(string InventoryId, ref bool result)
        {
            int requestedInventorySlot = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 256, true), ref result);
            Inventory RequestedInventory = new Inventory(requestedInventorySlot, 100, InventoryId);
            return RequestedInventory;
        }
    }
}

using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class ExecuteInventoryTransfer : GameNetworkMessage
    {
        public string DraggedSlot { get; set; }
        public string DroppedSlot { get; set; }
        public ItemObject DraggedSlotItem { get; set; }
        public ItemObject DroppedSlotItem { get; set; }
        public int DraggedSlotCount { get; set; }
        public int DroppedSlotCount { get; set; }
        public ExecuteInventoryTransfer()
        {
        }

        public ExecuteInventoryTransfer(string draggedSlot, string droppedSlot, ItemObject draggedSlotItem, ItemObject droppedSlotItem, int draggedSlotCount, int droppedSlotCount)
        {
            this.DraggedSlot = draggedSlot;
            this.DroppedSlot = droppedSlot;
            this.DraggedSlotItem = draggedSlotItem;
            this.DroppedSlotItem = droppedSlotItem;
            this.DraggedSlotCount = draggedSlotCount;
            this.DroppedSlotCount = droppedSlotCount;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Equipment;
        }

        protected override string OnGetLogFormat()
        {
            return "Equipment execute inventory";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.DraggedSlot = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.DroppedSlot = GameNetworkMessage.ReadStringFromPacket(ref result);
            string draggedSlotItemString = GameNetworkMessage.ReadStringFromPacket(ref result);
            string droppedSlotItemString = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.DraggedSlotItem = draggedSlotItemString != "" ? MBObjectManager.Instance.GetObject<ItemObject>(draggedSlotItemString) : null;
            this.DroppedSlotItem = droppedSlotItemString != "" ? MBObjectManager.Instance.GetObject<ItemObject>(droppedSlotItemString) : null;
            this.DraggedSlotCount = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 256, true), ref result);
            this.DroppedSlotCount = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 256, true), ref result);


            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.DraggedSlot);
            GameNetworkMessage.WriteStringToPacket(this.DroppedSlot);
            GameNetworkMessage.WriteStringToPacket(this.DraggedSlotItem != null ? this.DraggedSlotItem.StringId : "");
            GameNetworkMessage.WriteStringToPacket(this.DroppedSlotItem != null ? this.DroppedSlotItem.StringId : "");
            GameNetworkMessage.WriteIntToPacket(this.DraggedSlotCount, new CompressionInfo.Integer(0, 256, true));
            GameNetworkMessage.WriteIntToPacket(this.DroppedSlotCount, new CompressionInfo.Integer(0, 256, true));
        }
    }
}

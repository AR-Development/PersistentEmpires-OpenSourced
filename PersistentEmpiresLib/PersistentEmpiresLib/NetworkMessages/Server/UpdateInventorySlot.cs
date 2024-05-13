using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class UpdateInventorySlot : GameNetworkMessage
    {
        public string Slot { get; set; }
        public ItemObject Item { get; set; }
        public int Count { get; set; }

        public UpdateInventorySlot() { }
        public UpdateInventorySlot(string slot, ItemObject item, int count)
        {
            this.Slot = slot;
            this.Item = item;
            this.Count = count;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Equipment;
        }

        protected override string OnGetLogFormat()
        {
            return "Inventory update received";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Slot = GameNetworkMessage.ReadStringFromPacket(ref result);
            string itemId = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.Item = itemId != "" ? MBObjectManager.Instance.GetObject<ItemObject>(itemId) : null;
            this.Count = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 256, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.Slot);
            GameNetworkMessage.WriteStringToPacket(this.Item == null ? "" : this.Item.StringId);
            GameNetworkMessage.WriteIntToPacket(this.Count, new CompressionInfo.Integer(0, 256, true));
        }
    }
}

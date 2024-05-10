using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class ClosedInventory : GameNetworkMessage
    {
        public string InventoryId { get; set; }
        public ClosedInventory() { }
        public ClosedInventory(string InventoryId)
        {
            this.InventoryId = InventoryId;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Equipment;
        }

        protected override string OnGetLogFormat()
        {
            return "Inventory closed";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.InventoryId = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.InventoryId);
        }
    }
}

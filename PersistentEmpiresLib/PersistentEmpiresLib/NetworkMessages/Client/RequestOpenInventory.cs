using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestOpenInventory : GameNetworkMessage
    {
        public string InventoryId { get; set; }
        public RequestOpenInventory() { }
        public RequestOpenInventory(string InventoryId, bool PlayerSelf)
        {
            this.InventoryId = InventoryId;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Player request an inventory open";
        }

        protected override bool OnRead()
        {
            bool result = false;
            this.InventoryId = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.InventoryId);
        }
    }
}

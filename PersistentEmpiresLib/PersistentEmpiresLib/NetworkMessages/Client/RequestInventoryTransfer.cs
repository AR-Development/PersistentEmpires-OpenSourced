using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestInventoryTransfer : GameNetworkMessage
    {
        public string DraggedTag { get; set; }
        public string DroppedTag { get; set; }
        public RequestInventoryTransfer() { }
        public RequestInventoryTransfer(string draggedTag, string droppedTag)
        {
            this.DraggedTag = draggedTag;
            this.DroppedTag = droppedTag;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Player request an inventory transfer";
        }

        protected override bool OnRead()
        {
            bool result = false;
            this.DraggedTag = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.DroppedTag = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.DraggedTag);
            GameNetworkMessage.WriteStringToPacket(this.DroppedTag);
        }
    }
}

using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class InventorySplitItem : GameNetworkMessage
    {
        public InventorySplitItem() { }
        public InventorySplitItem(string ClickedTag)
        {
            this.ClickedTag = ClickedTag;
        }
        public string ClickedTag { get; set; }
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
            this.ClickedTag = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.ClickedTag);
        }
    }
}

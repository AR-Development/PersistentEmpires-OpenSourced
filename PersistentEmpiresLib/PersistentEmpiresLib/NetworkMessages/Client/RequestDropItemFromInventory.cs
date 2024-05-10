using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]

    public sealed class RequestDropItemFromInventory : GameNetworkMessage
    {
        public RequestDropItemFromInventory() { }
        public RequestDropItemFromInventory(string tag)
        {
            this.DropTag = tag;
        }
        public string DropTag { get; set; }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Request drop item";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.DropTag = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.DropTag);
        }
    }
}

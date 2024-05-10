using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class MyDiscordId : GameNetworkMessage
    {
        public string Id;
        public MyDiscordId() { }
        public MyDiscordId(string id)
        {
            this.Id = id;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Discord Id Received";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Id = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.Id);
        }
    }
}

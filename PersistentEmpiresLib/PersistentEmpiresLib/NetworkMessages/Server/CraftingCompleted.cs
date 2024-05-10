using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class CraftingCompleted : GameNetworkMessage
    {
        public NetworkCommunicator Player;
        public CraftingCompleted() { }
        public CraftingCompleted(NetworkCommunicator Player)
        {
            this.Player = Player;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Crafting completed";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
        }
    }
}

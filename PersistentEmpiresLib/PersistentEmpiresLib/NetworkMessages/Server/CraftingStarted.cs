using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class CraftingStarted : GameNetworkMessage
    {
        public MissionObject CraftingStation;
        public NetworkCommunicator Player;
        public int CraftIndex;

        public CraftingStarted() { }
        public CraftingStarted(MissionObject craftingStation, NetworkCommunicator player, int craftIndex)
        {
            this.CraftingStation = craftingStation;
            this.Player = player;
            this.CraftIndex = craftIndex;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Crafting Started";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.CraftingStation = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            this.CraftIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 1024, true), ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.CraftingStation.Id);
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
            GameNetworkMessage.WriteIntToPacket(this.CraftIndex, new CompressionInfo.Integer(-1, 1024, true));
        }
    }
}

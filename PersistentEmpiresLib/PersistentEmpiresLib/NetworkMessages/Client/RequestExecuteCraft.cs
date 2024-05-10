using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestExecuteCraft : GameNetworkMessage
    {
        public MissionObject CraftingStation { get; set; }
        public int CraftIndex { get; set; }
        public RequestExecuteCraft() { }
        public RequestExecuteCraft(MissionObject CraftingStation, int CraftIndex)
        {
            this.CraftingStation = CraftingStation;
            this.CraftIndex = CraftIndex;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "execute craft";
        }

        protected override bool OnRead()
        {
            bool res = true;
            this.CraftingStation = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref res));
            this.CraftIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 1024, true), ref res);
            return res;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.CraftingStation.Id);
            GameNetworkMessage.WriteIntToPacket(this.CraftIndex, new CompressionInfo.Integer(-1, 1024, true));
        }
    }
}

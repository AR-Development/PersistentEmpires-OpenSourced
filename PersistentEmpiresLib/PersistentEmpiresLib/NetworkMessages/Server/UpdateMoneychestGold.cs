using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class UpdateMoneychestGold : GameNetworkMessage
    {
        public MissionObject Chest;
        public long Gold;

        public UpdateMoneychestGold() { }
        public UpdateMoneychestGold(MissionObject chest, long gold)
        {
            this.Chest = chest;
            this.Gold = gold;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Server UpdateMoneychestGold";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Chest = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.Gold = GameNetworkMessage.ReadLongFromPacket(new CompressionInfo.LongInteger(0, 9999999999, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.Chest.Id);
            GameNetworkMessage.WriteLongToPacket(this.Gold, new CompressionInfo.LongInteger(0, 9999999999, true));
        }
    }
}

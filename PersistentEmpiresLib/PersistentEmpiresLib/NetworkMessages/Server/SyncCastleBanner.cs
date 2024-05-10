using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SyncCastleBanner : GameNetworkMessage
    {
        public PE_CastleBanner CastleBanner;
        public int FactionIndex;
        public SyncCastleBanner() { }
        public SyncCastleBanner(PE_CastleBanner castleBanner, int factionIndex)
        {
            this.CastleBanner = castleBanner;
            this.FactionIndex = factionIndex;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Mission;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync castle banner";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.CastleBanner = (PE_CastleBanner)Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 200, true), ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.CastleBanner.Id);
            GameNetworkMessage.WriteIntToPacket(this.FactionIndex, new CompressionInfo.Integer(0, 200, true));
        }
    }
}

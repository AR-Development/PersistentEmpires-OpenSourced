using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestCloseStockpileMarket : GameNetworkMessage
    {
        public MissionObject StockpileMarketEntity;
        public RequestCloseStockpileMarket() { }
        public RequestCloseStockpileMarket(MissionObject missionObject)
        {
            this.StockpileMarketEntity = missionObject;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Close Stockpile Market";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.StockpileMarketEntity = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.StockpileMarketEntity.Id);
        }
    }
}

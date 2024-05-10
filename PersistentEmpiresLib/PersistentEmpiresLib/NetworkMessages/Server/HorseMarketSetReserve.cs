using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class HorseMarketSetReserve : GameNetworkMessage
    {
        public PE_HorseMarket Market { get; private set; }
        public int Stock { get; private set; }

        public HorseMarketSetReserve(PE_HorseMarket Market, int Stock)
        {
            this.Market = Market;
            this.Stock = Stock;
        }
        public HorseMarketSetReserve() { }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Update Horse market reserve";
        }

        protected override bool OnRead()
        {
            bool result = true;
            // Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.Market = (PE_HorseMarket)Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.Stock = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, PE_StockpileMarket.MAX_STOCK_COUNT, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.Market.Id);
            GameNetworkMessage.WriteIntToPacket(this.Stock, new CompressionInfo.Integer(0, PE_StockpileMarket.MAX_STOCK_COUNT, true));
        }
    }
}

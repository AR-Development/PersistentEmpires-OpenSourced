using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestBuyItem : GameNetworkMessage
    {
        public RequestBuyItem() { }
        public RequestBuyItem(MissionObject stockPileMarket, int itemIndex)
        {
            this.StockpileMarket = stockPileMarket;
            this.ItemIndex = itemIndex;
        }
        public MissionObject StockpileMarket;
        public int ItemIndex { get; set; }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Stockpile Market Buy Request";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.StockpileMarket = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.ItemIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 4096, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.StockpileMarket.Id);
            GameNetworkMessage.WriteIntToPacket(this.ItemIndex, new CompressionInfo.Integer(0, 4096, true));
        }
    }
}

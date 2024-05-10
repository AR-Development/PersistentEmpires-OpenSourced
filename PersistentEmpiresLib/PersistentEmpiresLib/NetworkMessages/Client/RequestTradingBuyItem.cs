using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestTradingBuyItem : GameNetworkMessage
    {
        public RequestTradingBuyItem() { }
        public RequestTradingBuyItem(MissionObject tradingCenter, int itemIndex)
        {
            this.TradingCenter = tradingCenter;
            this.ItemIndex = itemIndex;
        }
        public MissionObject TradingCenter;
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
            this.TradingCenter = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.ItemIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 4096, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.TradingCenter.Id);
            GameNetworkMessage.WriteIntToPacket(this.ItemIndex, new CompressionInfo.Integer(0, 4096, true));
        }
    }
}

using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestTradingPrices : GameNetworkMessage
    {
        public RequestTradingPrices() { }
        public RequestTradingPrices(PE_TradeCenter center, int index) {
            this.TradingCenter = center;
            this.ItemIndex = index;
        }
        public MissionObject TradingCenter;
        public int ItemIndex { get; set; }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "RequestTradingPrices";
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

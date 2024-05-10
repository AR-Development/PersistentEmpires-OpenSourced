using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class StockpileUnpackBox : GameNetworkMessage
    {
        public PE_StockpileMarket StockpileMarket;
        public int SlotId;
        public StockpileUnpackBox(int SlotId, PE_StockpileMarket StockpileMarket)
        {
            this.SlotId = SlotId;
            this.StockpileMarket = StockpileMarket;
        }
        public StockpileUnpackBox()
        {
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "StockpileUnpackBox";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.SlotId = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 100, true), ref result);
            this.StockpileMarket = (PE_StockpileMarket)Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.SlotId, new CompressionInfo.Integer(0, 100, true));
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.StockpileMarket.Id);
        }
    }
}

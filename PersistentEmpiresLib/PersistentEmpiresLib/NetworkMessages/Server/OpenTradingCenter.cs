using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class OpenTradingCenter : GameNetworkMessage
    {
        public MissionObject TradingCenterEntity;
        public Inventory PlayerInventory;

        public OpenTradingCenter() { }
        public OpenTradingCenter(PE_TradeCenter tradeCenter, Inventory playerInventory)
        {
            this.TradingCenterEntity = tradeCenter;
            this.PlayerInventory = playerInventory;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "OpenTradingCenter";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.TradingCenterEntity = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.PlayerInventory = PENetworkModule.ReadInventoryPlayer(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.TradingCenterEntity.Id);
            PENetworkModule.WriteInventoryPlayer(this.PlayerInventory);
        }
    }
}

using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class UpgradeableBuildingSetTier : GameNetworkMessage
    {
        public UpgradeableBuildingSetTier() { }
        public UpgradeableBuildingSetTier(int tier, MissionObject upgradingObject)
        {
            this.Tier = tier;
            this.UpgradingObject = upgradingObject;
        }
        public int Tier { get; set; }
        public MissionObject UpgradingObject;
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Upgrade tier changed";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Tier = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 4, true), ref result);
            this.UpgradingObject = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.Tier, new CompressionInfo.Integer(-1, 4, true));
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.UpgradingObject.Id);
        }
    }
}

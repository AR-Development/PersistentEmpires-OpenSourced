using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class UpgradeableBuildingUpgrading : GameNetworkMessage
    {
        public UpgradeableBuildingUpgrading() { }
        public UpgradeableBuildingUpgrading(bool isUpgrading, MissionObject upgradingObject)
        {
            this.IsUpgrading = isUpgrading;
            this.UpgradingObject = upgradingObject;
        }
        public bool IsUpgrading { get; set; }
        public MissionObject UpgradingObject;
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Upgrade status changed";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.IsUpgrading = GameNetworkMessage.ReadBoolFromPacket(ref result);
            this.UpgradingObject = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteBoolToPacket(this.IsUpgrading);
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.UpgradingObject.Id);
        }
    }
}

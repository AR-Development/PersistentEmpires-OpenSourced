using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class OpenImportExport : GameNetworkMessage
    {
        public MissionObject ImportExportEntity;
        public Inventory PlayerInventory;
        public OpenImportExport() { }
        public OpenImportExport(MissionObject importExportEntity, Inventory playerInventory)
        {
            this.ImportExportEntity = importExportEntity;
            this.PlayerInventory = playerInventory;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Open Import Export";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.ImportExportEntity = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.PlayerInventory = PENetworkModule.ReadInventoryPlayer(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.ImportExportEntity.Id);
            PENetworkModule.WriteInventoryPlayer(this.PlayerInventory);
        }
    }
}

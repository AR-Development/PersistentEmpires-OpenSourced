using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestImportItem : GameNetworkMessage
    {
        public ItemObject Item;
        public MissionObject ImportExportEntity;
        public RequestImportItem() { }
        public RequestImportItem(ItemObject item, MissionObject ImportExportEntity)
        {
            this.Item = item;
            this.ImportExportEntity = ImportExportEntity;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Request Import Item";
        }

        protected override bool OnRead()
        {
            bool result = true;
            string itemObjId = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.Item = MBObjectManager.Instance.GetObject<ItemObject>(itemObjId);
            this.ImportExportEntity = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.Item.StringId);
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.ImportExportEntity.Id);
        }
    }
}

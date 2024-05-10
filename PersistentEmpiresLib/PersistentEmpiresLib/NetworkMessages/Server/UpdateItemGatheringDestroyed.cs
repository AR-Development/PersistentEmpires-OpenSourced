using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class UpdateItemGatheringDestroyed : GameNetworkMessage
    {
        public PE_ItemGathering MissionObject;
        public bool IsDestroyed { get; set; }

        public UpdateItemGatheringDestroyed() { }
        public UpdateItemGatheringDestroyed(PE_ItemGathering missionObject, bool isDestroyed)
        {
            this.IsDestroyed = isDestroyed;
            this.MissionObject = missionObject;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Received UpdateItemGatheringDestroyed";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.MissionObject = (PE_ItemGathering)Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.IsDestroyed = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.MissionObject.Id);
            GameNetworkMessage.WriteBoolToPacket(this.IsDestroyed);
        }
    }
}

using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class AddMissionObjectBodyFlagPE : GameNetworkMessage
    {
        public MissionObject MissionObject { get; private set; }

        public BodyFlags BodyFlags { get; private set; }

        public bool ApplyToChildren { get; private set; }

        public AddMissionObjectBodyFlagPE(MissionObject missionObject, BodyFlags bodyFlags, bool applyToChildren)
        {
            this.MissionObject = missionObject;
            this.BodyFlags = bodyFlags;
            this.ApplyToChildren = applyToChildren;
        }

        // Token: 0x06000360 RID: 864 RVA: 0x00006AE4 File Offset: 0x00004CE4
        public AddMissionObjectBodyFlagPE()
        {
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjectsDetailed;
        }
        protected override bool OnRead()
        {
            bool result = true;
            this.MissionObject = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.BodyFlags = (BodyFlags)GameNetworkMessage.ReadIntFromPacket(CompressionBasic.FlagsCompressionInfo, ref result);
            this.ApplyToChildren = GameNetworkMessage.ReadBoolFromPacket(ref result);

            return result;
        }
        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.MissionObject.Id);
            GameNetworkMessage.WriteIntToPacket((int)this.BodyFlags, CompressionBasic.FlagsCompressionInfo);
            GameNetworkMessage.WriteBoolToPacket(this.ApplyToChildren);
        }
        protected override string OnGetLogFormat()
        {
            return string.Concat(new object[]
            {
                "Add bodyflags: ",
                this.BodyFlags,
                " to MissionObject with ID: ",
                this.MissionObject.Id,
                " and with name: ",
                this.MissionObject.GameEntity.Name,
                this.ApplyToChildren ? "" : " and to all of its children."
            });
        }

    }
}

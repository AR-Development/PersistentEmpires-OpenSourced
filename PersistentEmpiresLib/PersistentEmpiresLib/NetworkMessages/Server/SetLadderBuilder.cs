using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SetLadderBuilder : GameNetworkMessage
    {
        public PE_LadderBuilder LadderBuilder;
        public bool LadderBuilt;

        public SetLadderBuilder() { }

        public SetLadderBuilder(PE_LadderBuilder ladderBuilder, bool ladderBuilt)
        {
            this.LadderBuilder = ladderBuilder;
            this.LadderBuilt = ladderBuilt;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "SetLadderBuilder";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.LadderBuilder = (PE_LadderBuilder)Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.LadderBuilt = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.LadderBuilder.Id);
            GameNetworkMessage.WriteBoolToPacket(this.LadderBuilt);
        }
    }
}

using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class ResetBatteringRam : GameNetworkMessage
    {
        public ResetBatteringRam() { }
        public ResetBatteringRam(MissionObject siegeTower)
        {
            this.BatteringRam = siegeTower;
        }

        public MissionObject BatteringRam { get; private set; }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "ResetBatteringRam";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.BatteringRam = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.BatteringRam.Id);
        }
    }
}

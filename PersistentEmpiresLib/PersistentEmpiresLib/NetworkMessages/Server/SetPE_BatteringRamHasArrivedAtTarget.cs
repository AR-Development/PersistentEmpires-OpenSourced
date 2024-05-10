using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SetPE_BatteringRamHasArrivedAtTarget : GameNetworkMessage
    {
        public MissionObjectId BatteringRamId { get; private set; }
        public SetPE_BatteringRamHasArrivedAtTarget() { }
        public SetPE_BatteringRamHasArrivedAtTarget(MissionObjectId batteringRamId)
        {
            this.BatteringRamId = batteringRamId;
        }
        protected override bool OnRead()
        {
            bool flag = true;
            this.BatteringRamId = GameNetworkMessage.ReadMissionObjectIdFromPacket(ref flag);
            return flag;
        }

        // Token: 0x06000660 RID: 1632 RVA: 0x0000B369 File Offset: 0x00009569
        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.BatteringRamId);
        }

        // Token: 0x06000661 RID: 1633 RVA: 0x0000B376 File Offset: 0x00009576
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.SiegeWeaponsDetailed;
        }

        // Token: 0x06000662 RID: 1634 RVA: 0x0000B37E File Offset: 0x0000957E
        protected override string OnGetLogFormat()
        {
            return "Battering Ram with ID: " + this.BatteringRamId + " has arrived at its target.";
        }
    }
}

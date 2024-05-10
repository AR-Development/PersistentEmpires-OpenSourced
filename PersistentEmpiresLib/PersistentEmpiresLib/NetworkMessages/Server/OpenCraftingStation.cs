using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class OpenCraftingStation : GameNetworkMessage
    {
        public MissionObject Station { get; set; }
        public Inventory PlayerInventory { get; set; }
        public OpenCraftingStation() { }
        public OpenCraftingStation(MissionObject station, Inventory playerInventory)
        {
            this.Station = station;
            this.PlayerInventory = playerInventory;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "User opened craft station";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Station = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.PlayerInventory = PENetworkModule.ReadInventoryPlayer(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.Station.Id);
            PENetworkModule.WriteInventoryPlayer(this.PlayerInventory);
        }
    }
}

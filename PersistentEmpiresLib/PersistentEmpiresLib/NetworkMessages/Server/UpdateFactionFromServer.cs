using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class UpdateFactionFromServer : GameNetworkMessage
    {
        public string Name { get; set; }
        public string BannerCode { get; set; }
        public int FactionIndex { get; set; }
        public UpdateFactionFromServer() { }
        public UpdateFactionFromServer(Faction updatedFaction, int factionIndex)
        {
            this.BannerCode = updatedFaction.banner.Serialize();
            this.Name = updatedFaction.name;
            this.FactionIndex = factionIndex;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Server updated faction";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            this.Name = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.BannerCode = PENetworkModule.ReadBannerCodeFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.FactionIndex, new CompressionInfo.Integer(-1, 200, true));
            GameNetworkMessage.WriteStringToPacket(this.Name);
            PENetworkModule.WriteBannerCodeToPacket(this.BannerCode);
        }
    }
}

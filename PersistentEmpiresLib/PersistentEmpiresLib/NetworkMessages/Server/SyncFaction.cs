using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SyncFaction : GameNetworkMessage
    {
        public int FactionIndex;
        public Faction Faction;
        public SyncFaction() { }
        public SyncFaction(int factionIndex, Faction faction)
        {
            this.FactionIndex = factionIndex;
            this.Faction = faction;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Mission;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync Faction";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            String name = GameNetworkMessage.ReadStringFromPacket(ref result);
            Team team = Mission.MissionNetworkHelper.GetTeamFromTeamIndex(GameNetworkMessage.ReadTeamIndexFromPacket(ref result));
            string BannerKey = PENetworkModule.ReadBannerCodeFromPacket(ref result);
            string LordId = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.Faction = new Faction(new Banner(BannerKey), name);
            this.Faction.lordId = LordId;
            this.Faction.team = team;

            int warDeclerationLength = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 200, true), ref result);
            for (int j = 0; j < warDeclerationLength; j++)
            {
                this.Faction.warDeclaredTo.Add(GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 200, true), ref result));
            }
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.FactionIndex, new CompressionInfo.Integer(-1, 200, true));
            GameNetworkMessage.WriteStringToPacket(this.Faction.name);
            GameNetworkMessage.WriteTeamIndexToPacket(this.Faction.team.TeamIndex);
            PENetworkModule.WriteBannerCodeToPacket(this.Faction.banner.Serialize());
            GameNetworkMessage.WriteStringToPacket(this.Faction.lordId);
            int warDeclerationLength = this.Faction.warDeclaredTo.Count;
            GameNetworkMessage.WriteIntToPacket(warDeclerationLength, new CompressionInfo.Integer(0, 200, true));
            for (int j = 0; j < warDeclerationLength; j++)
            {
                GameNetworkMessage.WriteIntToPacket(this.Faction.warDeclaredTo[j], new CompressionInfo.Integer(0, 200, true));
            }
        }
    }
}

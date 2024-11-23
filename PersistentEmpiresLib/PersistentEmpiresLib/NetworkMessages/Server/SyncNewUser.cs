using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SyncNewUser : GameNetworkMessage
    {
        public Dictionary<int, Faction> Factions { get; set; }
        public List<PE_CastleBanner> CastleBanners { get; set; }
        public SyncNewUser()
        {
        }


        public SyncNewUser(Dictionary<int, Faction> factions, List<PE_CastleBanner> castleBanners)
        {
            this.Factions = factions;
            this.CastleBanners = castleBanners;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Mission;
        }

        protected override string OnGetLogFormat()
        {
            return "Sending all data to client";
        }

        protected override bool OnRead()
        {
            this.Factions = new Dictionary<int, Faction>();
            this.CastleBanners = new List<PE_CastleBanner>();
            bool result = true;
            /// FACTION START
            int factionLength = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 201, true), ref result);
            for (int i = 0; i < factionLength; i++)
            {
                int factionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
                BasicCultureObject bco = (BasicCultureObject)GameNetworkMessage.ReadObjectReferenceFromPacket(MBObjectManager.Instance, CompressionBasic.GUIDCompressionInfo, ref result);
                String name = GameNetworkMessage.ReadStringFromPacket(ref result);
                Team team = Mission.MissionNetworkHelper.GetTeamFromTeamIndex(GameNetworkMessage.ReadTeamIndexFromPacket(ref result));
                string BannerKey = PENetworkModule.ReadBannerCodeFromPacket(ref result);
                string LordId = GameNetworkMessage.ReadStringFromPacket(ref result);
                int memberLength = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 500, true), ref result);
                this.Factions[factionIndex] = new Faction(bco, new Banner(BannerKey), name);
                this.Factions[factionIndex].lordId = LordId;
                this.Factions[factionIndex].team = team;
                for (int j = 0; j < memberLength; j++)
                {
                    var member = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
                    if(!Factions[factionIndex].members.Contains(member))
                        Factions[factionIndex].members.Add(member); ;
                }
                int warDeclerationLength = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 200, true), ref result);
                for (int j = 0; j < warDeclerationLength; j++)
                {
                    this.Factions[factionIndex].warDeclaredTo.Add(GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 200, true), ref result));
                }
            }
            ///
            /// CASTLE START
            /*int castleLength = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 200, true), ref result);
            for (int i = 0; i < castleLength; i++)
            {
                int CastleIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 200, true), ref result);
                int FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
                PE_CastleBanner castleBanner = new PE_CastleBanner();
                castleBanner.CastleIndex = CastleIndex;
                castleBanner.FactionIndex = FactionIndex;
                this.CastleBanners.Add(castleBanner); // Don't rely on this object !!!
            }*/
            /// 

            return result;
        }

        protected override void OnWrite()
        {
            /// FACTION START
            GameNetworkMessage.WriteIntToPacket(this.Factions.Count, new CompressionInfo.Integer(0, 201, true));
            foreach (int i in this.Factions.Keys)
            {
                Faction faction = this.Factions[i];
                GameNetworkMessage.WriteIntToPacket(i, new CompressionInfo.Integer(-1, 200, true));
                GameNetworkMessage.WriteObjectReferenceToPacket(faction.basicCultureObject, CompressionBasic.GUIDCompressionInfo);
                GameNetworkMessage.WriteStringToPacket(faction.name);
                GameNetworkMessage.WriteTeamIndexToPacket(faction.team.TeamIndex);
                PENetworkModule.WriteBannerCodeToPacket(faction.banner.Serialize());
                GameNetworkMessage.WriteStringToPacket(faction.lordId);
                int memberLength = faction.members.Count;
                GameNetworkMessage.WriteIntToPacket(memberLength, new CompressionInfo.Integer(0, 500, true));
                for (int j = 0; j < memberLength; j++)
                {
                    GameNetworkMessage.WriteNetworkPeerReferenceToPacket(faction.members[j]);
                }
                int warDeclerationLength = faction.warDeclaredTo.Count;
                GameNetworkMessage.WriteIntToPacket(warDeclerationLength, new CompressionInfo.Integer(0, 200, true));
                for (int j = 0; j < warDeclerationLength; j++)
                {
                    GameNetworkMessage.WriteIntToPacket(faction.warDeclaredTo[j], new CompressionInfo.Integer(0, 200, true));
                }
            }
            ///
            /// CASTLE START
            /*GameNetworkMessage.WriteIntToPacket(this.CastleBanners.Count, new CompressionInfo.Integer(0, 200, true));
            for(int i = 0; i < this.CastleBanners.Count; i++)
            {
                PE_CastleBanner castleBanner = this.CastleBanners[i];
                GameNetworkMessage.WriteIntToPacket(castleBanner.CastleIndex, new CompressionInfo.Integer(0, 200, true));
                GameNetworkMessage.WriteIntToPacket(castleBanner.FactionIndex, new CompressionInfo.Integer(-1, 200, true));
            }*/
            ///
        }
    }
}

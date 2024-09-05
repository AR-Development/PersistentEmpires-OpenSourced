/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using System;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class AddFaction : GameNetworkMessage
    {
        public Faction faction { get; set; }
        public int factionIndex { get; set; }
        public AddFaction(Faction faction, int factionIndex)
        {
            this.faction = faction;
            this.factionIndex = factionIndex;
        }

        public AddFaction()
        {
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Faction added.";
        }

        protected override bool OnRead()
        {
            // throw new NotImplementedException();
            bool result = true;
            this.factionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            BasicCultureObject bco = (BasicCultureObject)GameNetworkMessage.ReadObjectReferenceFromPacket(MBObjectManager.Instance, CompressionBasic.GUIDCompressionInfo, ref result);
            String name = GameNetworkMessage.ReadStringFromPacket(ref result);
            Team team = Mission.MissionNetworkHelper.GetTeamFromTeamIndex(GameNetworkMessage.ReadTeamIndexFromPacket(ref result));
            string BannerKey = PENetworkModule.ReadBannerCodeFromPacket(ref result);
            int memberLength = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 500, true), ref result);
            this.faction = new Faction(bco, new Banner(BannerKey), name);
            this.faction.team = team;
            for (int i = 0; i < memberLength; i++)
            {
                this.faction.members.Add(GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result));
            }

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(factionIndex, new CompressionInfo.Integer(-1, 200, true));
            GameNetworkMessage.WriteObjectReferenceToPacket(faction.basicCultureObject, CompressionBasic.GUIDCompressionInfo);
            GameNetworkMessage.WriteStringToPacket(faction.name);
            GameNetworkMessage.WriteTeamIndexToPacket(faction.team.TeamIndex);
            PENetworkModule.WriteBannerCodeToPacket(faction.banner.Serialize());
            int memberLength = faction.members.Count;
            GameNetworkMessage.WriteIntToPacket(memberLength, new CompressionInfo.Integer(0, 500, true));
            for (int i = 0; i < memberLength; i++)
            {
                GameNetworkMessage.WriteNetworkPeerReferenceToPacket(faction.members[i]);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade;

namespace NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    internal class DiscordRoleRegister : GameNetworkMessage
    {

        public NetworkCommunicator Player;
        public string Role;
        public uint Color;

        public DiscordRoleRegister() { }

        public DiscordRoleRegister(NetworkCommunicator Player, string Tier, uint Color)
        {
            this.Player = Player;
            this.Role = Tier;
            this.Color = Color;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "DiscordRoleRegister";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            this.Role = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.Color = GameNetworkMessage.ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref result);


            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
            GameNetworkMessage.WriteStringToPacket(this.Role);
            GameNetworkMessage.WriteUintToPacket(this.Color, CompressionBasic.ColorCompressionInfo);
        }
    }
}


using PersistentEmpiresLib.Helpers;
using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class UpdateFactionBanner : GameNetworkMessage
    {
        public string BannerCode { get; set; }
        public UpdateFactionBanner() { }

        public UpdateFactionBanner(String bannerCode)
        {
            this.BannerCode = bannerCode;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Request update on faction banner";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.BannerCode = PENetworkModule.ReadBannerCodeFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            PENetworkModule.WriteBannerCodeToPacket(this.BannerCode);
        }
    }
}

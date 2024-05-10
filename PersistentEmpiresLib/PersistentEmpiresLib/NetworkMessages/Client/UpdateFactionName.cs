using System;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class UpdateFactionName : GameNetworkMessage
    {
        public UpdateFactionName()
        { }
        public UpdateFactionName(String newName)
        {
            this.NewName = newName;
        }

        public string NewName { get; private set; }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Change Faction Name Requested";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.NewName = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.NewName);

        }
    }
}

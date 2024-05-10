using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]

    public sealed class RequestStopPlaying : GameNetworkMessage
    {
        public RequestStopPlaying() { }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Mission;
        }

        protected override string OnGetLogFormat()
        {
            return "RequestStopPlaying";
        }

        protected override bool OnRead()
        {
            return true;
        }

        protected override void OnWrite()
        {

        }
    }
}

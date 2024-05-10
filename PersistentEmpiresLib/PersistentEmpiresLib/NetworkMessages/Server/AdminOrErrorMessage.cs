using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class AdminOrErrorMessage : GameNetworkMessage
    {

        public AdminOrErrorMessage() { }
        public AdminOrErrorMessage(string message)
        {
            this.Message = message;
        }

        public string Message { get; private set; }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.NormalLogging;
        }

        protected override string OnGetLogFormat()
        {
            return "Sended an Admin/Error message";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Message = GameNetworkMessage.ReadStringFromPacket(ref result);


            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.Message);
        }
    }
}

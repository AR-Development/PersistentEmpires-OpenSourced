using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SetHunger : GameNetworkMessage
    {
        public int Hunger;
        public SetHunger() { }
        public SetHunger(int hunger)
        {
            this.Hunger = hunger;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.AgentsDetailed;
        }

        protected override string OnGetLogFormat()
        {
            return "Hunger set";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Hunger = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 100, true), ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.Hunger, new CompressionInfo.Integer(0, 100, true));
        }
    }
}

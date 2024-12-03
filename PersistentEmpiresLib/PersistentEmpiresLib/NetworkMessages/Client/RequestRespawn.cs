using System;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestRespawn : GameNetworkMessage
    {
        public string PlayerId { get; private set; }

        public RequestRespawn(string playerId)
        {
            PlayerId = playerId;
        }

        public RequestRespawn()
        {
        }

        protected override bool OnRead()
        {
            try
            {
                bool bufferReadValid = true;
                PlayerId = ReadStringFromPacket(ref bufferReadValid);
                return bufferReadValid;
            }
            catch (Exception)
            {
            }
            return false;
        }

        protected override void OnWrite()
        {
            try
            {
                WriteStringToPacket(PlayerId);
            }
            catch (Exception)
            {

            }
        }

        public override string ToString()
        {
            return $"RequestRespawn: {PlayerId}";
        }

        protected override MultiplayerMessageFilter OnGetLogFilter() => MultiplayerMessageFilter.GameMode;

        protected override string OnGetLogFormat() => string.Format($"{this}");
    }
}
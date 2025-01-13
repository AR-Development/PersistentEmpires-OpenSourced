using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RegisterClientEquipmentOnWound : GameNetworkMessage
    {
        public List<string> Equipments;
        public string PlayerId;
        public RegisterClientEquipmentOnWound() { }

        public RegisterClientEquipmentOnWound(List<string> playerEquipment, string playerId)
        {
            Equipments = playerEquipment;
            PlayerId = playerId;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Client RegisterClientEquipmentOnWound";
        }

        protected override bool OnRead()
        {
            bool result = true;
            Equipments = new List<string>();
            for (int i = 0; i < 4; i++)
            {
                Equipments.Add(GameNetworkMessage.ReadStringFromPacket(ref result));
            }
            PlayerId = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {

            for (int i = 0; i < 4; i++)
            {
                GameNetworkMessage.WriteStringToPacket(Equipments[i]);
            }
            GameNetworkMessage.WriteStringToPacket(PlayerId);
        }
    }
}

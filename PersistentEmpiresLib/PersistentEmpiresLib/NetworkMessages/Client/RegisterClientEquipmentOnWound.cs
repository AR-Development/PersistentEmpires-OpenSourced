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
        static int Length = (int)EquipmentIndex.NumAllWeaponSlots;
        public RegisterClientEquipmentOnWound() { }

        public RegisterClientEquipmentOnWound(Equipment spawnEquipment)
        {
            Equipments = new List<string>();
            for (var equipmentIndex = (int)EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < Length; equipmentIndex++)
            {
                Equipments.Add(spawnEquipment[equipmentIndex].Item.StringId);
            }
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
            for (int i = 0; i < Length; i++)
            {
                Equipments.Add(GameNetworkMessage.ReadStringFromPacket(ref result));
            }
            return result;
        }

        protected override void OnWrite()
        {
            
            for (int i = 0; i < Length; i++)
            {
                GameNetworkMessage.WriteStringToPacket(Equipments[i]);
            }
        }
    }
}

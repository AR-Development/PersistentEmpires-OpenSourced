using PersistentEmpiresLib.ErrorLogging;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class OpenInventory : GameNetworkMessage
    {
        public string InventoryId { get; set; }
        public Inventory PlayerInventory;
        public Inventory RequestedInventory;
        public OpenInventory() { }

        public OpenInventory(string InventoryId, Inventory playerInventory, Inventory requestedInventory)
        {
            this.InventoryId = InventoryId;
            this.PlayerInventory = playerInventory;
            this.RequestedInventory = requestedInventory;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Equipment;
        }

        protected override string OnGetLogFormat()
        {
            return "Open inventory";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.InventoryId = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.PlayerInventory = PENetworkModule.ReadInventoryPlayer(ref result);
            if(this.InventoryId != "")
            {
                this.RequestedInventory = PENetworkModule.ReadCustomInventory(this.InventoryId, ref result);
            }
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.InventoryId);
            PENetworkModule.WriteInventoryPlayer(this.PlayerInventory);
            if(this.InventoryId != "")
            {
                PENetworkModule.WriteCustomInventory(this.RequestedInventory);
            }
        }
    }
}

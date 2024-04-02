using PersistentEmpiresLib.ErrorLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class Announcement : GameNetworkMessage
    {
        public string Message;
        public Announcement() { 
        }
        public Announcement(string Message)
        {
            this.Message = Message;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Announcement made";
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

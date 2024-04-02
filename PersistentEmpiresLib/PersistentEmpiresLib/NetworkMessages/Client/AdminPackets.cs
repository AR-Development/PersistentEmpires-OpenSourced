using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestBecameGodlike : GameNetworkMessage
    {


        public RequestBecameGodlike()
        {

        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestBecameGodlike";
        }

        protected override bool OnRead()
        {
            bool result = true;

            return result;
        }

        protected override void OnWrite()
        {
        }
    }
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestAdminGold : GameNetworkMessage
    {
        public int Gold { get; set; }

        public RequestAdminGold()
        {

        }
        public RequestAdminGold(int Gold)
        {
            this.Gold = Gold;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestAdminGold";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Gold = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 1000000, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.Gold, new CompressionInfo.Integer(0, 1000000, true));
        }
    }

    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestAdminJoinFaction : GameNetworkMessage
    {
        public int FactionIndex { get; set; }

        public RequestAdminJoinFaction()
        {

        }
        public RequestAdminJoinFaction(int FactionIndex)
        {
            this.FactionIndex = FactionIndex;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestAdminJoinFaction";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.FactionIndex, new CompressionInfo.Integer(-1, 200, true));
        }
    }
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestAdminResetFactionBanner : GameNetworkMessage
    {
        public int FactionIndex { get; set; }

        public RequestAdminResetFactionBanner()
        {

        }
        public RequestAdminResetFactionBanner(int FactionIndex)
        {
            this.FactionIndex = FactionIndex;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestAdminResetFactionBanner";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.FactionIndex, new CompressionInfo.Integer(-1, 200, true));
        }
    }

    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestAdminSetFactionName : GameNetworkMessage
    {
        public int FactionIndex { get; set; }
        public string FactionName { get; set; }

        public RequestAdminSetFactionName()
        {

        }
        public RequestAdminSetFactionName(int FactionIndex, string FactionName)
        {
            this.FactionIndex = FactionIndex;
            this.FactionName = FactionName;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestAdminSetFactionName";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.FactionIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-1, 200, true), ref result);
            this.FactionName = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteIntToPacket(this.FactionIndex, new CompressionInfo.Integer(-1, 200, true));
            GameNetworkMessage.WriteStringToPacket(this.FactionName);
        }
    }
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestItemSpawn : GameNetworkMessage
    {
        public RequestItemSpawn() { }

        public RequestItemSpawn(string itemId, int count)
        {
            this.ItemId = itemId;
            this.Count = count;
        }
        public string ItemId { get; set; }
        public int Count { get; set; }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestItemSpawn";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.ItemId = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.Count = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 100, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.ItemId);
            GameNetworkMessage.WriteIntToPacket(this.Count, new CompressionInfo.Integer(0, 100, true));
        }
    }

    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestTempBan : GameNetworkMessage
    {
        public NetworkCommunicator Player;
        public RequestTempBan() { }
        public RequestTempBan(NetworkCommunicator player)
        {
            this.Player = player;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestTempBan";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result, true);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
        }
    }
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestPermBan : GameNetworkMessage
    {
        public NetworkCommunicator Player;
        public RequestPermBan() { }
        public RequestPermBan(NetworkCommunicator player)
        {
            this.Player = player;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestPermBan";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result, true);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
        }
    }
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestKick : GameNetworkMessage
    {
        public NetworkCommunicator Player;
        public RequestKick() { }
        public RequestKick(NetworkCommunicator player)
        {
            this.Player = player;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestKick";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result, true);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
        }
    }
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestKill : GameNetworkMessage
    {
        public NetworkCommunicator Player;
        public RequestKill() { }
        public RequestKill(NetworkCommunicator player)
        {
            this.Player = player;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestKill";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result, true);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
        }
    }
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestFade : GameNetworkMessage
    {
        public NetworkCommunicator Player;
        public RequestFade() { }
        public RequestFade(NetworkCommunicator player)
        {
            this.Player = player;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestFade";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result, true);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
        }
    }
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestFreeze : GameNetworkMessage
    {
        public NetworkCommunicator Player;
        public RequestFreeze() { }
        public RequestFreeze(NetworkCommunicator player)
        {
            this.Player = player;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestFreeze";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result, true);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
        }
    }
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestTpToMe : GameNetworkMessage
    {
        public NetworkCommunicator Player;
        public RequestTpToMe() { }
        public RequestTpToMe(NetworkCommunicator player)
        {
            this.Player = player;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestTpToMe";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result, true);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
        }
    }
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestTpTo : GameNetworkMessage
    {
        public NetworkCommunicator Player;
        public RequestTpTo() { }
        public RequestTpTo(NetworkCommunicator player)
        {
            this.Player = player;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestTpTo";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result, true);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
        }
    }
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestHeal : GameNetworkMessage
    {
        public NetworkCommunicator Player;
        public RequestHeal() { }
        public RequestHeal(NetworkCommunicator player)
        {
            this.Player = player;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestHeal";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result, true);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
        }
    }

    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestUnWound: GameNetworkMessage
    {
        public NetworkCommunicator Player;
        public RequestUnWound() { }
        public RequestUnWound(NetworkCommunicator player)
        {
            this.Player = player;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Received RequestUnWound";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Player = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result, true);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Player);
        }
    }
}

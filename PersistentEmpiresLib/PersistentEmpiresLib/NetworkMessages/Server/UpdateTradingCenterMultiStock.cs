/*
 *  Persistent Empires Open Sourced - A Mount and Blade: Bannerlord Mod
 *  Copyright (C) 2024  Free Software Foundation, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.
 *
 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
 
using PersistentEmpiresLib.SceneScripts;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class UpdateTradingCenterMultiStock : GameNetworkMessage
    {
        public MissionObject TradingCenter;
        public List<int> Indexes;
        public List<int> Stocks;

        public UpdateTradingCenterMultiStock() { }

        public UpdateTradingCenterMultiStock(PE_TradeCenter tradingCenter, List<int> Indexes, List<int> Stocks)
        {
            this.TradingCenter = tradingCenter;
            this.Indexes = Indexes;
            this.Stocks = Stocks;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Received UpdateStockpileMultiStock";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.TradingCenter = Mission.MissionNetworkHelper.GetMissionObjectFromMissionObjectId(GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result));
            this.Indexes = new List<int>();
            this.Stocks = new List<int>();

            int indexLen = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 4096, true), ref result);
            for (int i = 0; i < indexLen; i++)
            {
                this.Indexes.Add(GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 4096, true), ref result));
            }

            int stocksLen = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 4096, true), ref result);
            for (int i = 0; i < stocksLen; i++)
            {
                this.Stocks.Add(GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 4096, true), ref result));
            }
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.TradingCenter.Id);
            GameNetworkMessage.WriteIntToPacket(this.Indexes.Count, new CompressionInfo.Integer(0, 4096, true));
            for (int i = 0; i < this.Indexes.Count; i++)
            {
                GameNetworkMessage.WriteIntToPacket(this.Indexes[i], new CompressionInfo.Integer(0, 4096, true));
            }

            GameNetworkMessage.WriteIntToPacket(this.Stocks.Count, new CompressionInfo.Integer(0, 4096, true));
            for (int i = 0; i < this.Stocks.Count; i++)
            {
                GameNetworkMessage.WriteIntToPacket(this.Stocks[i], new CompressionInfo.Integer(0, 4096, true));
            }
        }
    }
}

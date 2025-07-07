using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class TradingCenterBehavior : MissionNetwork
    {
        public delegate void TradingCenterOpenHandler(PE_TradeCenter stockpileMarket, Inventory playerInventory);
        public event TradingCenterOpenHandler OnTradingCenterOpen;

        public delegate void TradingCenterUpdateHandler(PE_TradeCenter stockpileMarket, int itemIndex, int newStock);
        public event TradingCenterUpdateHandler OnTradingCenterUpdate;

        public delegate void TradingCenterUpdateMultiHandler(PE_TradeCenter stockpileMarket, List<int> indexes, List<int> stocks);
        public event TradingCenterUpdateMultiHandler OnTradingCenterUpdateMultiHandler;

        Dictionary<MissionObject, List<NetworkCommunicator>> openedInventories;

        Dictionary<PE_TradeCenter, long> RandomizeTimer = new Dictionary<PE_TradeCenter, long>();

        private Random randomizer;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            openedInventories = new Dictionary<MissionObject, List<NetworkCommunicator>>();
            randomizer = new Random();
        }

        public override void AfterStart()
        {
            base.AfterStart();
            if (GameNetwork.IsServer)
            {
                List<PE_TradeCenter> tradeCenters = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_TradeCenter>().Select(g => g.GetFirstScriptOfType<PE_TradeCenter>()).ToList();
                foreach (PE_TradeCenter tradeCenter in tradeCenters)
                {
                    RandomizeTimer[tradeCenter] = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + tradeCenter.RandomizeDelayMinutes * 60;
                    tradeCenter.Randomize(randomizer);
                }
            }
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<OpenTradingCenter>(this.HandleOpenTradingCenterFromServer);
                networkMessageHandlerRegisterer.Register<UpdateTradingCenterStock>(this.HandleUpdateTradingCenterStockFromServer);
                networkMessageHandlerRegisterer.Register<UpdateTradingCenterMultiStock>(this.HandleUpdateTradingCenterMultiStock);
            }
            else
            {
                networkMessageHandlerRegisterer.Register<RequestTradingBuyItem>(this.HandleRequestTradingBuyItemFromClient);
                networkMessageHandlerRegisterer.Register<RequestTradingSellItem>(this.HandleRequestTradingSellItemFromClient);
                networkMessageHandlerRegisterer.Register<RequestCloseTradingCenter>(this.HandleRequestCloseTradingCenterFromClient);
                networkMessageHandlerRegisterer.Register<RequestTradingPrices>(this.HandleRequestTradingPricesFromClient);
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (GameNetwork.IsClient) return;

            foreach (PE_TradeCenter tradeCenter in this.RandomizeTimer.Keys.ToList())
            {
                if (this.RandomizeTimer[tradeCenter] < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    tradeCenter.Randomize(randomizer);
                    for (int i = 0; i < tradeCenter.MarketItems.Count; i++)
                    {
                        this.UpdateStockForPeers(tradeCenter, i);
                    }
                    this.RandomizeTimer[tradeCenter] = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + tradeCenter.RandomizeDelayMinutes * 60;
                }
            }

        }

        private bool HandleRequestTradingPricesFromClient(NetworkCommunicator peer, RequestTradingPrices message)
        {
            if (peer.ControlledAgent == null) return false;
            int skill = peer.ControlledAgent.Character.GetSkillValue(DefaultSkills.Trade);
            if (skill < 10)
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Not_Qualified", null).ToString(), Color.ConvertStringToColor("#FF0000FF").ToUnsignedInteger(), peer);
                return false;
            }

            PE_TradeCenter tradeCenter = (PE_TradeCenter)message.TradingCenter;
            MarketItem marketItem = tradeCenter.MarketItems[message.ItemIndex];

            List<PE_TradeCenter> otherTradeCenters = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_TradeCenter>().Select(g => g.GetFirstScriptOfType<PE_TradeCenter>()).Where(p => p != tradeCenter).OrderByDescending(p => p.MarketItems[message.ItemIndex].SellPrice()).ToList();

            InformationComponent.Instance.SendMessage(GameTexts.FindText("TradingCenterBehavior2", null).ToString(), Color.ConvertStringToColor("#03dac6ff").ToUnsignedInteger(), peer);
            foreach (PE_TradeCenter oTradeCenter in otherTradeCenters)
            {
                MarketItem otherMarketItem = oTradeCenter.MarketItems[message.ItemIndex];
                Color color = otherMarketItem.SellPrice() > marketItem.BuyPrice() ? Color.ConvertStringToColor("#00C853FF") : Color.ConvertStringToColor("#B71C1CFF");
                InformationComponent.Instance.SendMessage(otherMarketItem.SellPrice().ToString() + GameTexts.FindText("TradingCenterBehavior3", null).ToString() + oTradeCenter.GetCastleName(), color.ToUnsignedInteger(), peer);
            }

            return true;
        }

        public override void OnPlayerDisconnectedFromServer(NetworkCommunicator player)
        {
            if (GameNetwork.IsServer)
            {
                this.RemoveFromUpdateList(player);
            }
        }
        private void RemoveFromUpdateList(NetworkCommunicator peer, MissionObject StockpileMarketEntity = null)
        {
            if (StockpileMarketEntity != null && this.openedInventories.ContainsKey(StockpileMarketEntity) && this.openedInventories[StockpileMarketEntity].Contains(peer))
            {
                this.openedInventories[StockpileMarketEntity].Remove(peer);
                return;
            }


            foreach (MissionObject mObject in this.openedInventories.Keys.ToList())
            {
                if (this.openedInventories[mObject].Contains(peer))
                {
                    this.openedInventories[mObject].Remove(peer);
                }
            }
        }
        private bool HandleRequestCloseTradingCenterFromClient(NetworkCommunicator peer, RequestCloseTradingCenter message)
        {
            this.RemoveFromUpdateList(peer, message.TradingCenter);
            return true;
        }

        private bool HandleRequestTradingSellItemFromClient(NetworkCommunicator peer, RequestTradingSellItem message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            PE_TradeCenter stockpileMarket = (PE_TradeCenter)message.TradingCenter;
            if (peer.ControlledAgent == null || peer.ControlledAgent.IsActive() == false) return false;
            if (peer.ControlledAgent.Position.Distance(stockpileMarket.GameEntity.GlobalPosition) > stockpileMarket.Distance) return false;
            MarketItem marketItem = stockpileMarket.MarketItems[message.ItemIndex];

            if (!persistentEmpireRepresentative.GetInventory().IsInventoryIncludes(marketItem.Item, 1))
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("TradingCenterBehavior4", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                return false;
            }

            bool arrowCheckFlag = true;
            if (marketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Arrows ||
               marketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Bolts ||
               marketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Bullets
                )
            {
                foreach (InventorySlot slot in persistentEmpireRepresentative.GetInventory().Slots)
                {
                    if (slot.Item != null && slot.Item.StringId == marketItem.Item.StringId)
                    {
                        int maxAmmo = ItemHelper.GetMaximumAmmo(marketItem.Item);
                        arrowCheckFlag = maxAmmo == slot.Ammo;
                        if (!arrowCheckFlag) break;
                    }
                }
            }

            if (arrowCheckFlag == false)
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("TradingCenterBehavior5", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                return false;
            }

            List<int> updatedSlots = persistentEmpireRepresentative.GetInventory().RemoveCountedItemSynced(marketItem.Item, 1);
            foreach (int i in updatedSlots)
            {
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                GameNetwork.EndModuleEventAsServer();
            }
            persistentEmpireRepresentative.GoldGain(marketItem.SellPrice());
            // if (marketItem.Stock > 1000) marketItem.Stock = 1000;
            // LoggerHelper.LogAnAction(peer, LogAction.PlayerSellsStockpile, null, new object[] {
            //     marketItem
            // });
            marketItem.UpdateReserve(marketItem.Stock + 1);
            // SaveSystemBehavior.HandleCreateOrSaveStockpileMarket(stockpileMarket);
            this.UpdateStockForPeers(stockpileMarket, message.ItemIndex);


            return true;
        }

        private void UpdateStockForPeers(PE_TradeCenter stockpileMarket, int itemIndex)
        {
            MarketItem marketItem = stockpileMarket.MarketItems[itemIndex];
            if (this.openedInventories.ContainsKey(stockpileMarket))
            {
                foreach (NetworkCommunicator toPeer in this.openedInventories[stockpileMarket].ToArray())
                {
                    if (toPeer.IsConnectionActive)
                    {
                        GameNetwork.BeginModuleEventAsServer(toPeer);
                        GameNetwork.WriteMessage(new UpdateTradingCenterStock(stockpileMarket, marketItem.Stock, itemIndex));
                        GameNetwork.EndModuleEventAsServer();
                    }
                }
            }
        }

        private bool HandleRequestTradingBuyItemFromClient(NetworkCommunicator peer, RequestTradingBuyItem message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            PE_TradeCenter stockpileMarket = (PE_TradeCenter)message.TradingCenter;
            PE_TaxHandler taxHandler = stockpileMarket.GameEntity.GetFirstScriptOfType<PE_TaxHandler>();

            if (peer.ControlledAgent == null || peer.ControlledAgent.IsActive() == false) return false;
            if (peer.ControlledAgent.Position.Distance(stockpileMarket.GameEntity.GlobalPosition) > stockpileMarket.Distance) return false;

            MarketItem marketItem = stockpileMarket.MarketItems[message.ItemIndex];
            if (marketItem.Stock == 0)
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("TradingCenterBehavior6", null).ToString(), new Color(1f, 0f, 0f).ToUnsignedInteger(), peer);
                return false;
            }
            if (persistentEmpireRepresentative.GetInventory().HasEnoughRoomFor(marketItem.Item, 1) == false)
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("TradingCenterBehavior7", null).ToString(), new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                return false;
            }
            if (!persistentEmpireRepresentative.ReduceIfHaveEnoughGold(marketItem.BuyPrice()))
            {
                InformationComponent.Instance.SendMessage(GameTexts.FindText("PE_Not_Enough_Gold", null).ToString(), new Color(1f, 0f, 0f).ToUnsignedInteger(), peer);
                return false;
            }
            List<int> updatedSlots = persistentEmpireRepresentative.GetInventory().AddCountedItemSynced(marketItem.Item, 1, ItemHelper.GetMaximumAmmo(marketItem.Item));
            foreach (int i in updatedSlots)
            {
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                GameNetwork.EndModuleEventAsServer();
            }
            // LoggerHelper.LogAnAction(peer, LogAction.PlayerBuysStockpile, null, new object[] {
            //     marketItem
            // });
            if (taxHandler != null && taxHandler.CastleId != -1) taxHandler.AddTaxFeeToMoneyChest((marketItem.BuyPrice() * taxHandler.TaxPercentage) / 100);
            marketItem.UpdateReserve(marketItem.Stock - 1);
            // SaveSystemBehavior.HandleCreateOrSaveStockpileMarket(stockpileMarket);
            this.UpdateStockForPeers(stockpileMarket, message.ItemIndex);
            // Send a message to update ui
            return true;
        }

        private void HandleUpdateTradingCenterMultiStock(UpdateTradingCenterMultiStock message)
        {
            PE_TradeCenter stockpileMarket = (PE_TradeCenter)message.TradingCenter;
            for (int i = 0; i < message.Indexes.Count; i++)
            {
                int index = message.Indexes[i];
                int stock = message.Stocks[i];
                stockpileMarket.MarketItems[index].Stock = stock;
            }
            if (OnTradingCenterUpdateMultiHandler != null)
            {
                OnTradingCenterUpdateMultiHandler(stockpileMarket, message.Indexes, message.Stocks);
            }
        }

        private void HandleUpdateTradingCenterStockFromServer(UpdateTradingCenterStock message)
        {
            PE_TradeCenter stockpileMarket = (PE_TradeCenter)message.TradingCenter;
            stockpileMarket.MarketItems[message.ItemIndex].UpdateReserve(message.NewStock);
            if (this.OnTradingCenterUpdate != null)
            {
                this.OnTradingCenterUpdate(stockpileMarket, message.ItemIndex, message.NewStock);
            }
        }

        private void HandleOpenTradingCenterFromServer(OpenTradingCenter message)
        {
            PE_TradeCenter tradeCenter = (PE_TradeCenter)message.TradingCenterEntity;
            if (this.OnTradingCenterOpen != null)
            {
                this.OnTradingCenterOpen(tradeCenter, message.PlayerInventory);
            }
        }
        private static List<List<T>> ChunkBy<T>(List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public void OpenTradingCenterForPeer(PE_TradeCenter entity, NetworkCommunicator networkCommunicator)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkCommunicator.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;
            if (!this.openedInventories.ContainsKey(entity))
            {
                this.openedInventories[entity] = new List<NetworkCommunicator>();
            }
            this.openedInventories[entity].Add(networkCommunicator);

            List<int> indexes = new List<int>();
            for (int i = 0; i < entity.MarketItems.Count; i++)
            {

                MarketItem marketItem = entity.MarketItems[i];
                if (marketItem.Stock <= 0) continue;
                indexes.Add(i);
            }

            List<List<int>> chunks = ChunkBy<int>(indexes, 100);
            foreach (List<int> chunkIndex in chunks)
            {
                List<int> stocks = new List<int>();
                for (int i = 0; i < chunkIndex.Count; i++)
                {
                    int index = chunkIndex[i];
                    stocks.Add(entity.MarketItems[index].Stock);
                }

                GameNetwork.BeginModuleEventAsServer(networkCommunicator);
                GameNetwork.WriteMessage(new UpdateTradingCenterMultiStock(entity, chunkIndex, stocks));
                GameNetwork.EndModuleEventAsServer();
            }

            GameNetwork.BeginModuleEventAsServer(networkCommunicator);
            GameNetwork.WriteMessage(new OpenTradingCenter(entity, persistentEmpireRepresentative.GetInventory()));
            GameNetwork.EndModuleEventAsServer();
        }
    }
}

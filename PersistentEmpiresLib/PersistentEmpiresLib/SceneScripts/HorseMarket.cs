using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_HorseMarket : UsableMissionObject, IMissionObjectHash
    {
        public string HorseId = "mp_empire_horse_war";
        public string HorseHarness = "";
        private int MaxStock = 1000;
        public int Stock { private set; get; }
        public int MaximumPrice = 3000;
        public float CurrentPrice { private set; get; }
        public int MinimumPrice = 500;
        public int Constant { private set; get; }
        public int Stability = 10;

        public ItemObject HorseItem { get; private set; }


        public override bool IsDisabledForAgent(Agent agent)
        {
            return this.IsDeactivated || (this.IsDisabledForPlayers && !agent.IsAIControlled) || !agent.IsOnLand();
        }

        protected override void OnInit()
        {
            base.OnInit();

            this.HorseItem = MBObjectManager.Instance.GetObject<ItemObject>(this.HorseId);

            this.Constant = MaximumPrice;
            this.CurrentPrice = MathF.Pow(this.Constant, 1f / this.Stability);
            TextObject actionMessage = new TextObject(this.HorseItem.Name.ToString() + " Market");
            base.ActionMessage = actionMessage;
            TextObject descriptionMessage = new TextObject("Press {KEY} To Buy/Sell");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
        }
        public void UpdateReserve(int newStock)
        {
            if (newStock > 999) newStock = 999;
            if (newStock < 0) newStock = 0;
            if (newStock < 1)
            {
                this.CurrentPrice = MathF.Pow(this.Constant, 1f / this.Stability);
            }
            else
            {
                this.CurrentPrice = MathF.Pow(this.Constant / (float)newStock, 1f / this.Stability);
            }
            this.Stock = newStock;
        }
        public int BuyPrice()
        {
            float fakeStock = this.Stock;
            if (this.Stock < 2) return this.MaximumPrice;
            float denominator = MathF.Pow(fakeStock - 1, 1f / this.Stability);
            float numerator = this.Constant;
            int buyPrice = Math.Abs((int)((numerator / denominator) - MathF.Pow(this.CurrentPrice, 1f / this.Stability)));
            if (buyPrice < this.MinimumPrice) buyPrice = this.MinimumPrice;
            return buyPrice;
        }
        public int SellPrice()
        {
            float fakeStock = this.Stock;
            if (this.Stock < 1) fakeStock = 1;
            float denominator = MathF.Pow(fakeStock + 1, 1f / this.Stability);
            float numerator = this.Constant;
            int price = Math.Abs((int)(MathF.Pow(this.CurrentPrice, 1f / this.Stability) - (numerator / denominator)));
            if (price < this.MinimumPrice) price = (this.MinimumPrice * 90) / 100;
            return price;
        }

        public override void OnFocusGain(Agent userAgent)
        {
            base.OnFocusGain(userAgent);
            if (GameNetwork.IsClient)
            {
                InformationManager.ShowTooltip(typeof(ItemObject), new object[] {
                    new EquipmentElement(this.HorseItem),
                    this.BuyPrice().ToString(),
                    this.SellPrice().ToString(),
                    this.Stock.ToString()
                });
            }
        }
        public override void OnFocusLose(Agent userAgent)
        {
            base.OnFocusLose(userAgent);
            if (GameNetwork.IsClient)
            {
                InformationManager.HideTooltip();
            }
        }
        public override void OnUse(Agent userAgent)
        {
            base.OnUse(userAgent);
            if (GameNetwork.IsServer)
            {
                Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);
                userAgent.StopUsingGameObjectMT(false);

                NetworkCommunicator player = userAgent.MissionPeer.GetNetworkPeer();
                PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
                if (persistentEmpireRepresentative == null) return;
                if (userAgent.MountAgent != null)
                {
                    // Sell Mount.
                    ItemObject horseItem = userAgent.MountAgent.SpawnEquipment[EquipmentIndex.Horse].Item;
                    Debug.Print("HORSE ITEM IS " + horseItem.StringId);
                    if (horseItem.StringId != this.HorseId)
                    {
                        InformationComponent.Instance.SendMessage("You can't sell this horse here.", new Color(1f, 0f, 0f).ToUnsignedInteger(), player);
                        return;
                    }
                    if (userAgent.MountAgent.Health < (userAgent.MountAgent.HealthLimit * 75) / 100)
                    {
                        InformationComponent.Instance.SendMessage("Horse is injured. You can't sell it", new Color(1f, 0f, 0f).ToUnsignedInteger(), player);
                        return;
                    }
                    // Agent horse = userAgent.MountAgent;
                    // userAgent.Mount(horse); // Unmount first.
                    AgentHelpers.RespawnAgentOnPlace(userAgent);
                    // horse.FadeOut(true, false);
                    persistentEmpireRepresentative.GoldGain(this.SellPrice());
                    this.UpdateReserve(this.Stock + 1 > 1000 ? 1000 : this.Stock + 1);
                    SaveSystemBehavior.HandleCreateOrSaveHorseMarket(this);
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new HorseMarketSetReserve(this, this.Stock));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                }
                else
                {
                    if (this.Stock == 0)
                    {
                        InformationComponent.Instance.SendMessage("Stocks are empty.", new Color(1f, 0f, 0f).ToUnsignedInteger(), player);
                        return;
                    }
                    if (!persistentEmpireRepresentative.ReduceIfHaveEnoughGold(this.BuyPrice()))
                    {
                        InformationComponent.Instance.SendMessage("You need " + this.BuyPrice() + " gold to buy a horse.", new Color(1f, 0f, 0f).ToUnsignedInteger(), player);
                        return;
                    }
                    ItemRosterElement itemRosterElement = new ItemRosterElement(this.HorseItem, 0, null);
                    ItemRosterElement harnessElement = this.HorseHarness == "" ? default(ItemRosterElement) : new ItemRosterElement(MBObjectManager.Instance.GetObject<ItemObject>(this.HorseHarness), 1);
                    MatrixFrame matrixFrame = base.GameEntity.GetGlobalFrame();
                    Mission.Current.SpawnMonster(itemRosterElement, harnessElement, userAgent.Position, matrixFrame.rotation.f.AsVec2);
                    PE_TaxHandler taxHandler = this.GameEntity.GetFirstScriptOfType<PE_TaxHandler>();
                    if (taxHandler != null && taxHandler.CastleId != -1) taxHandler.AddTaxFeeToMoneyChest((this.BuyPrice() * taxHandler.TaxPercentage) / 100);
                    this.UpdateReserve(this.Stock - 1);


                    SaveSystemBehavior.HandleCreateOrSaveHorseMarket(this);
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new HorseMarketSetReserve(this, this.Stock));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                }
            }
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Horse Market";
        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            reportDamage = false;
            if (attackerAgent == null) return false;
            NetworkCommunicator player = attackerAgent.MissionPeer.GetNetworkPeer();
            bool isAdmin = Main.IsPlayerAdmin(player);
            if (isAdmin && weapon.Item != null && weapon.Item.StringId == "pe_adminstockfiller")
            {
                this.UpdateReserve(this.Stock + 10);
                InformationComponent.Instance.SendMessage("Stocks updated", Colors.Blue.ToUnsignedInteger(), player);
            }
            return true;
        }

        public MissionObject GetMissionObject()
        {
            return this;
        }
    }
}

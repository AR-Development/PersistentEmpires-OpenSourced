using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class MoneyChestBehavior : MissionNetwork
    {
        public delegate void UpdateGoldMCHandler(PE_MoneyChest mc, int newAmount);
        public event UpdateGoldMCHandler OnUpdateGoldMC;

        Dictionary<int, PE_MoneyChest> castleIdToMC;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);

            this.castleIdToMC = new Dictionary<int, PE_MoneyChest>();

            List<GameEntity> moneyChestEntities = new List<GameEntity>();
            base.Mission.Scene.GetAllEntitiesWithScriptComponent<PE_MoneyChest>(ref moneyChestEntities);
            List<PE_MoneyChest> moneyChests = moneyChestEntities.Select(chest => chest.GetFirstScriptOfType<PE_MoneyChest>()).ToList();

            foreach (PE_MoneyChest mc in moneyChests)
            {
                this.castleIdToMC[mc.CastleId] = mc;
            }
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        public void AddTaxFromHandler(PE_TaxHandler taxHandler, int amount)
        {
            if (taxHandler.CastleId == -1) return;
            if (this.castleIdToMC.ContainsKey(taxHandler.CastleId) == false) return;
            this.castleIdToMC[taxHandler.CastleId].AddTax(amount);
        }

        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsServer)
            {
                networkMessageHandlerRegisterer.Register<WithdrawDepositMoneychest>(this.HandleWithdrawDepositMoneychestFromClient);
            }
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<UpdateMoneychestGold>(this.HandleUpdateMoneychestGoldFromServer);
            }
        }
        private bool HandleWithdrawDepositMoneychestFromClient(NetworkCommunicator sender, WithdrawDepositMoneychest message)
        {
            PE_MoneyChest moneyChest = (PE_MoneyChest)message.MoneyChest;
            if (sender.ControlledAgent == null) return false;
            if (sender.ControlledAgent.Position.Distance(moneyChest.GameEntity.GlobalPosition) > moneyChest.Distance) return false;

            if (message.Withdraw)
            {
                moneyChest.WithdrawGold(sender, message.Amount);
            }
            else
            {
                moneyChest.DepositGold(sender, message.Amount);
            }
            return true;
        }

        private void HandleUpdateMoneychestGoldFromServer(UpdateMoneychestGold message)
        {
            PE_MoneyChest moneyChest = (PE_MoneyChest)message.Chest;
            moneyChest.UpdateGold(message.Gold);
            if (OnUpdateGoldMC != null)
            {
                OnUpdateGoldMC(moneyChest, (int)message.Gold);
            }
        }
    }
}

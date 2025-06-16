using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class BankingComponent : MissionLogic
    {
        public int BankAmountLimit { get; private set; }
        public int BankTaxRate { get; set; }

        public delegate void OpenBankHandler(PE_Bank Bank, int Amount, int BankTaxRate);
        public event OpenBankHandler OnOpenBank;

        public delegate void BankDepositEventHandler(NetworkCommunicator player, int amount);
        public static event BankDepositEventHandler OnBankDeposit;

        public delegate int BankWithdrawEventHandler(NetworkCommunicator player, int amount);
        public static event BankWithdrawEventHandler OnBankWithdraw;

        public delegate int BankQueryEventHandler(NetworkCommunicator player);
        public static event BankQueryEventHandler OnBankQuery;



        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
#if SERVER
            this.BankAmountLimit = ConfigManager.GetIntConfig("BankAmountLimit", 0);
            this.BankTaxRate = 100 - ConfigManager.GetIntConfig("BankTaxRate", 10);
#endif
        }

        private void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);

            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<OpenBank>(this.HandleOpenBankServer);
            }
            if (GameNetwork.IsServer)
            {
                networkMessageHandlerRegisterer.Register<RequestBankAction>(this.HandleRequestBankActionClient);
            }
        }

        private bool HandleRequestBankActionClient(NetworkCommunicator player, RequestBankAction message)
        {
            if (player.ControlledAgent == null) return false;
            PersistentEmpireRepresentative representative = player.GetComponent<PersistentEmpireRepresentative>();
            if (representative == null) return false;

            Vec3 playerPos = player.ControlledAgent.Position;
            Vec3 bankPos = message.Bank.GameEntity.GetGlobalFrame().origin;

            if (bankPos.Distance(playerPos) > 5) return false;
            if (BankingComponent.OnBankQuery == null) return false;

            if (message.Deposit && this.BankAmountLimit > 0)
            {
                int amount = BankingComponent.OnBankQuery(player);
                if (amount + message.Amount > this.BankAmountLimit)
                {
                    InformationComponent.Instance.SendMessage(GameTexts.FindText("BankingComponent1", null).ToString(), Colors.Red.ToUnsignedInteger(), player);
                    return false;
                }
            }

            if (message.Deposit && representative.ReduceIfHaveEnoughGold(message.Amount))
            {
                if (BankingComponent.OnBankDeposit != null)
                {
                    BankingComponent.OnBankDeposit(player, message.Amount);
                    LoggerHelper.LogAnAction(player, LogAction.PlayerDepositedToBank, new Database.DBEntities.AffectedPlayer[] { }, new object[] { message.Amount });
                }
            }
            else if (BankingComponent.OnBankQuery(player) >= message.Amount)
            {
                if (BankingComponent.OnBankWithdraw != null)
                {
                    int amount = BankingComponent.OnBankWithdraw(player, message.Amount);
                    representative.GoldGain(amount);
                    LoggerHelper.LogAnAction(player, LogAction.PlayerWithdrawToBank, new Database.DBEntities.AffectedPlayer[] { }, new object[] { amount });

                }
            }

            return true;
        }

        private void HandleOpenBankServer(OpenBank message)
        {
            if (this.OnOpenBank != null) this.OnOpenBank((PE_Bank)message.Bank, message.Amount, message.TaxRate);
        }

        public void OpenBankForPeer(NetworkCommunicator peer, PE_Bank Bank)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;

            if (BankingComponent.OnBankQuery == null) return;

            int amount = BankingComponent.OnBankQuery(peer);

            GameNetwork.BeginModuleEventAsServer(peer);
            GameNetwork.WriteMessage(new OpenBank(Bank, amount, BankTaxRate));
            GameNetwork.EndModuleEventAsServer();
        }

    }
}

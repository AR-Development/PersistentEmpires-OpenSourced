using System;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEMoneyChestVM : ViewModel
    {
        private int _amount;
        private int _balance;

        public Action<PEMoneyChestVM> OnDepositAmount;
        public Action<PEMoneyChestVM> OnWithdrawAmount;

        public PEMoneyChestVM(Action<PEMoneyChestVM> depositHandler, Action<PEMoneyChestVM> withdrawHandler)
        {
            this.OnDepositAmount = depositHandler;
            this.OnWithdrawAmount = withdrawHandler;
        }

        public void ExecuteDeposit()
        {
            this.OnDepositAmount(this);
        }
        public void ExecuteWithdraw()
        {
            this.OnWithdrawAmount(this);
        }
        [DataSourceProperty]
        public int Amount
        {
            get => this._amount;
            set
            {
                if (value != this._amount)
                {
                    this._amount = value;
                    base.OnPropertyChangedWithValue(value, "Amount");
                }
            }
        }

        [DataSourceProperty]
        public int Balance
        {
            get => this._balance;
            set
            {
                if (value != this._balance)
                {
                    this._balance = value;
                    base.OnPropertyChangedWithValue(value, "Balance");
                }
            }
        }

        public void RefreshValues(int amount)
        {
            this.Balance = amount;
        }
    }
}

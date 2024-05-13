using System;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PEBankVM : ViewModel
    {
        private int _amount;
        private int _balance;
        private int _taxrate;
        public Action<PEBankVM> OnDepositAmount;
        public Action<PEBankVM> OnWithdrawAmount;


        public PEBankVM(Action<PEBankVM> depositHandler, Action<PEBankVM> withdrawHandler)
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

        [DataSourceProperty]
        public int TaxRate
        {
            get => this._taxrate;
            set
            {
                if (value != this._taxrate)
                {
                    this._taxrate = value;
                    base.OnPropertyChangedWithValue(value, "TaxRate");
                }
            }
        }

        public void RefreshValues(int amount, int taxes)
        {
            this.Balance = amount;
            this.TaxRate = taxes;
        }
    }
}

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

using Dapper;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBBankingRepository
    {
        public static int Tax_Rate { get; set; }

        public class DBBank
        {
            public int Id { get; set; }
            public string PlayerId { get; set; }
            public int Amount { get; set; }
        }
        public static void Initialize()
        {
            BankingComponent.OnBankQuery += QueryBankBalance;
            BankingComponent.OnBankDeposit += DepositToBank;
            BankingComponent.OnBankWithdraw += WithdrawFromBank;
            Tax_Rate = (100 - PersistentEmpiresLib.ConfigManager.GetIntConfig("BankTaxRate", 10));

        }
        public static int QueryBankBalance(NetworkCommunicator player)
        {
            IEnumerable<DBPlayer> collection = DBConnection.Connection.Query<DBPlayer>("SELECT BankAmount FROM Players WHERE PlayerId = @PlayerId", new
            {
                PlayerId = player.VirtualPlayer.Id.ToString()
            });
            return collection.First().BankAmount;
        }

        public static void DepositToBank(NetworkCommunicator player, int amount)
        {
            DBConnection.Connection.Execute("UPDATE Players SET BankAmount = BankAmount + @Amount WHERE PlayerId = @PlayerId", new
            {
                PlayerId = player.VirtualPlayer.Id.ToString(),
                Amount = (amount * Tax_Rate) / 100
            });
        }

        public static int WithdrawFromBank(NetworkCommunicator player, int amount)
        {
            DBConnection.Connection.Execute("UPDATE Players SET BankAmount = BankAmount - @Amount WHERE PlayerId = @PlayerId", new
            {
                PlayerId = player.VirtualPlayer.Id.ToString(),
                Amount = amount
            });

            return amount;
        }
    }
}

using Dapper;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresSave.Database.Helpers;
using System.Collections.Generic;
using System.Linq;
using PersistentEmpiresLib.Helpers;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresServer.ServerMissions;
using System;

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
            try
            {
                IEnumerable<DBPlayer> collection = DBConnection.Connection.Query<DBPlayer>("SELECT BankAmount FROM Players WHERE PlayerId = @PlayerId", new
                {
                    PlayerId = player.VirtualPlayer.ToPlayerId()
                });
                return collection.First().BankAmount;
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
            }

            return 0;
        }

        public static void DepositToBank(NetworkCommunicator player, int amount)
        {
            try
            {
                DBConnection.Connection.Execute("UPDATE Players SET BankAmount = BankAmount + @Amount WHERE PlayerId = @PlayerId", new
                {
                    PlayerId = player.VirtualPlayer.ToPlayerId(),
                    Amount = (amount * Tax_Rate) / 100
                });
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
            }
        }

        public static int WithdrawFromBank(NetworkCommunicator player, int amount)
        {
            try
            {
                DBConnection.Connection.Execute("UPDATE Players SET BankAmount = BankAmount - @Amount WHERE PlayerId = @PlayerId", new
                {
                    PlayerId = player.VirtualPlayer.ToPlayerId(),
                    CustomName = player.VirtualPlayer.UserName.EncodeSpecialMariaDbChars(),
                    Amount = amount
                });

                return amount;
            }
            catch (Exception ex)
            {
                DiscordBehavior.NotifyException(ex);
            }

            return 0;
        }
    }
}
/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
using System;
using System.Collections.Generic;
using System.Linq;
using DCEMV.ServerShared;
using DCEMV.DemoServer.Persistence.Api.Entities;

namespace DCEMV.DemoServer.Persistence.Api.Repository
{
    public interface IAccountsRepository
    {
        AccountPM GetTransactionsForUser(string credentialsId);
        AccountPM GetAccountAndCardsForUser(string credentialsId);
        AccountPM GetAccountForUser(string credentialsId);
        void UpdateAccountState(string accountNumberId, AccountState accountState, string credentialsId);
        void UpdateAccount(AccountPM customer, string credentialsId);
        void AddAccount(string accountNumberId, string credentialsId);
    }

    public class AccountsRepository : IAccountsRepository
    {
        private readonly ApiDbContext _context = null;

        public AccountsRepository(ApiDbContext context)
        {
            _context = context;
        }

        public AccountPM GetAccountForUser(string credentialsId)
        {
            List<AccountPM> accounts = _context.Accounts.Where(w => w.CredentialsId == credentialsId).ToList();
            if (accounts.Count == 0)
                throw new ValidationException("No account found");
            if (accounts.Count > 1)
                throw new ValidationException("More than 1 account found");
            if(accounts[0].AccountState == AccountState.Cancelled || accounts[0].AccountState == AccountState.Locked)
                throw new ValidationException("Account not enabled");
            return accounts[0];
        }

        public AccountPM GetTransactionsForUser(string credentialsId)
        {
            AccountPM account = GetAccountForUser(credentialsId);
            //List<TransactionPM> txList = _context.Transactions.Where(w => 
            //    w.AccountNumberIdFromRef == account.AccountNumberId ||
            //    w.AccountNumberIdToRef == account.AccountNumberId
            //).ToList();

            account.TransactionsFrom = _context.Transactions.Where(w => w.AccountNumberIdFromRef == account.AccountNumberId).ToList();
            account.TransactionsTo = _context.Transactions.Where(w => w.AccountNumberIdToRef == account.AccountNumberId).ToList();
            account.CCTopUpTransactions = _context.TopUpTransactions.Where(w => w.AccountNumberIdToRef == account.AccountNumberId).ToList();
            account.POSTransactionsTo = _context.POSTransactions.Where(w =>
                w.AccountNumberIdToRef == account.AccountNumberId ||
                w.AccountNumberIdFromRef == account.AccountNumberId
            ).ToList();

            return account;
        }


        public AccountPM GetAccountAndCardsForUser(string credentialsId)
        {
            //this code creates sql queries that query the account table twice...
            //List<Account> accounts = _context.Accounts.Include(c => c.Cards).Where(w => w.CredentialsId == credentialsId).ToList();

            //if (accounts.Count == 0)
            //    throw new ValidationException("No account found");
            //if (accounts.Count > 1)
            //    throw new ValidationException("More than 1 account found");
            //return accounts[0];

            AccountPM account = GetAccountForUser(credentialsId);

            account.Cards = _context.Cards.Where(w => w.Account.AccountNumberId == account.AccountNumberId).ToList();

            return account;
        }

        public void AddAccount(string accountNumberId, string credentialsId)
        {
            AccountPM a = new AccountPM()
            {
                CustomerType = CustomerType.None,
                CredentialsId = credentialsId,
                AccountNumberId = accountNumberId,
                Balance = 0,
                BalanceUpdateTime = DateTime.Now,
                AccountState = AccountState.PendingUpdate,
            };
            _context.Accounts.Add(a);
            _context.SaveChanges();
        }

        public void UpdateAccount(AccountPM account, string credentialsId)
        {
            AccountPM accountLoggedIn = GetAccountForUser(credentialsId);
            
            if (accountLoggedIn.AccountNumberId != account.AccountNumberId)
                throw new ValidationException("Invalid AccountNumberId");

            if (accountLoggedIn.CustomerType != CustomerType.None && accountLoggedIn.CustomerType != account.CustomerType)
                throw new ValidationException("Cannot change customer type");

            account.AccountState = AccountState.Enabled;
            accountLoggedIn.UpdateAccount(account);
            _context.SaveChanges();
        }

        public void UpdateAccountState(string accountNumberId, AccountState accountState, string credentialsId)
        {
            AccountPM accountLoggedIn = GetAccountForUser(credentialsId);

            if (accountLoggedIn.AccountNumberId != accountNumberId)
                throw new ValidationException("Invalid AccountNumberId");

            accountLoggedIn.AccountState = accountState;
            _context.SaveChanges();
        }
    }
}

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
using Microsoft.EntityFrameworkCore.Storage;
using DCEMV.DemoServer.Persistence.Api.Entities;
using DCEMV.ServerShared;

namespace DCEMV.DemoServer.Persistence.Api.Repository
{
    public interface ITransactionsRepository
    {
        //Transaction GetTransaction(string transactionId);
        int AddTransaction(TransactionPM tx, string credentialsId,bool useTransaction = true);
        int AddTopUpTransaction(CCTopUpTransactionPM tx, string credentialsId);
    }

    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly ApiDbContext _context = null;
        private readonly IAccountsRepository _accountsRepository;
        private readonly ICardsRepository _cardsRepository;

        public TransactionsRepository(ApiDbContext context, IAccountsRepository accountsRepository, ICardsRepository cardsRepository)
        {
            _context = context;
            _accountsRepository = accountsRepository;
            _cardsRepository = cardsRepository;
        }
       
        public int AddTopUpTransaction(CCTopUpTransactionPM t, string credentialsId)
        {
            //TODO: reject top-ups with our cards

            if (t.Amount > ConfigSingleton.MaxTopUpTransactionAmount)
                throw new ValidationException("Invalid transaction, greater than max top up amount allowed");

            AccountPM accountLoggedIn = _accountsRepository.GetAccountForUser(credentialsId);

            t.AccountNumberIdToRef = accountLoggedIn.AccountNumberId;
            t.TransactionDateTime = DateTime.Now;

            using (IDbContextTransaction dbTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    _context.TopUpTransactions.Add(t);
                    UpdateAccountBalance(accountLoggedIn.AccountNumberId, accountLoggedIn.Balance = accountLoggedIn.Balance + t.Amount);
                    _context.SaveChanges();
                    dbTransaction.Commit();
                    return t.TopUpTransactionId;
                }
                catch
                {
                    dbTransaction.Rollback();
                    throw new TechnicalException("Error Occured during transaction db operation. Transaction Rolled Back");
                }
            }
        }

        public int AddTransaction(TransactionPM t, string credentialsId, bool useTransaction)
        {
            //TODO: Validate cardholder either by card crytogram of cardholder pin
            if(t.Amount > ConfigSingleton.MaxTransactionAmount)
                throw new ValidationException("Invalid transaction, amount greater than max transaction amount allowed");

            t.TransactionDateTime = DateTime.Now;

            AccountPM accountLoggedIn = _accountsRepository.GetAccountForUser(credentialsId);
            AccountPM accountFrom = null;
            AccountPM accountTo = null;
            CardPM cardTo = null;
            CardPM cardFrom = null;
            switch (t.TransactionType)
            {
                case TransactionType.SendMoneyFromAppToCard:
                    if (accountLoggedIn.AccountNumberId != t.AccountNumberIdFromRef)
                        throw new ValidationException("Invalid AccountNumberId");

                    cardTo = _cardsRepository.GetCard(t.CardSerialNumberIdTo);
                    if (cardTo.CardState != CardState.Active)
                        throw new ValidationException("Invalid card");

                    t.AccountNumberIdToRef = cardTo.AccountNumberIdRef;
                    accountTo = GetAccount(t.AccountNumberIdToRef);
                    accountFrom = accountLoggedIn; //_accountsRepository.GetAccount(transaction.AccountNumberIdFrom);
                    if (accountFrom.Balance < t.Amount)
                        throw new ValidationException("Insufficient funds");
                    break;

                case TransactionType.SendMoneyFromCardToApp:
                    if (accountLoggedIn.AccountNumberId != t.AccountNumberIdToRef)
                        throw new ValidationException("Invalid AccountNumberId");

                    cardFrom = _cardsRepository.GetCard(t.CardSerialNumberIdFrom);
                    if(cardFrom.CardState != CardState.Active)
                        throw new ValidationException("Invalid card");

                    if (cardFrom.AvailablegDailySpendLimit < t.Amount)
                        throw new ValidationException("Daily Spend Limit Exceeded"); 

                    if (cardFrom.MonthlySpendLimit < t.Amount)
                        throw new ValidationException("Monthly Spend Limit Exceeded");

                    t.AccountNumberIdFromRef = cardFrom.AccountNumberIdRef;
                    accountFrom = GetAccount(t.AccountNumberIdFromRef);
                    accountTo = accountLoggedIn; //_accountsRepository.GetAccount(transaction.AccountNumberIdTo);
                    if (accountFrom.Balance < t.Amount)
                        throw new ValidationException("Insufficient funds");
                    break;

                default:
                    throw new ValidationException("Invalid transaction type: " + t.TransactionType);
            }

            Action action = ()=> 
            {
                _context.Transactions.Add(t);
                UpdateAccountBalance(accountFrom.AccountNumberId, accountFrom.Balance = accountFrom.Balance - t.Amount);
                UpdateAccountBalance(accountTo.AccountNumberId, accountTo.Balance = accountTo.Balance + t.Amount);
                if (cardFrom != null)
                {
                    cardFrom.AvailablegDailySpendLimit = cardFrom.AvailablegDailySpendLimit - t.Amount;
                    cardFrom.AvailableMonthlySpendLimit = cardFrom.AvailableMonthlySpendLimit - t.Amount;
                }
            };

            if (!useTransaction)
            {
                try
                {
                    action.Invoke();
                    _context.SaveChanges();
                    return t.TransactionId;
                }
                catch
                {
                    throw new TechnicalException("Error Occured during transaction db operation. Transaction Rolled Back");
                }
            }
            else
            {
                using (IDbContextTransaction dbTransaction = _context.Database.BeginTransaction()) 
                {
                    try
                    {
                        action.Invoke();
                        _context.SaveChanges();
                        dbTransaction.Commit();
                        return t.TransactionId;
                    }
                    catch(Exception ex)
                    {
                        dbTransaction.Rollback();
                        throw new TechnicalException("Error Occured during transaction db operation. Transaction Rolled Back:" + ex.Message);
                    }
                }
            }
        }


        private AccountPM GetAccount(string accountNumberId)
        {
            return _context.Accounts.Find(accountNumberId);
        }

        private void UpdateAccountBalance(string accountNumberId, long balance)
        {
            AccountPM accountToUpdate = GetAccount(accountNumberId);
            accountToUpdate.Balance = balance;
            accountToUpdate.BalanceUpdateTime = DateTime.Now;
        }
    }
}

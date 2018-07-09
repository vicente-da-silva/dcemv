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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DCEMV.FormattingUtils;
using DCEMV.DemoServer.Persistence.Api.Repository;
using DCEMV.DemoServer.Persistence.Api.Entities;
using DCEMV.ServerShared;

namespace DCEMV.DemoServer.Controllers.Api
{
    [Authorize]
    //[Produces ("application/json")]
    public class AccountController : Controller
    {
        private readonly IAccountsRepository accountsRepository;

        public AccountController(IAccountsRepository accountsRepository)
        {
            this.accountsRepository = accountsRepository;
        }

        [HttpGet]
        [Route("transactions")]
        public Account GetAccountTransactions()
        {
            AccountPM account = accountsRepository.GetTransactionsForUser(GetCurrentUserId());

            Account accountDetails = new Account()
            {
                AccountNumberId = account.AccountNumberId,
                AccountState = account.AccountState,
                CustomerType = account.CustomerType,
                FirstName = account.FirstName,
                LastName = account.LastName,
                BusinessName = account.BusinessName,
                CompanyRegNumber = account.CompanyRegNumber,
                TaxNumber = account.TaxNumber,
                Balance = account.Balance,
            };

            account.TransactionsFrom.ForEach(x =>
            {
                TransferTransaction tx = new TransferTransaction()
                {
                    AccountFrom = x.AccountNumberIdFromRef,
                    //AccountTo = x.AccountNumberIdToRef,
                    Amount = x.Amount,
                    TransactionType = x.TransactionType,
                    DateTime = x.TransactionDateTime,
                    TransactionId = x.TransactionId,
                };
                accountDetails.TransferFromTransactions.Add(tx);
            });

            account.TransactionsTo.ForEach(x =>
            {
                TransferTransaction tx = new TransferTransaction()
                {
                    //AccountFrom = x.AccountNumberIdFromRef,
                    AccountTo = x.AccountNumberIdToRef,
                    Amount = x.Amount,
                    TransactionType = x.TransactionType,
                    DateTime = x.TransactionDateTime,
                    TransactionId = x.TransactionId,
                };
                accountDetails.TransferToTransactions.Add(tx);
            });

            account.POSTransactionsTo.ForEach(x =>
            {
                POSTransaction tx = new POSTransaction()
                {
                    AccountNumberId = x.AccountNumberIdToRef,
                    TransactionDateTime = x.TransactionDateTime,
                    TransactionId = x.TransactionIdRef,
                    POSTransactionId = x.POSTransactionId,
                };
                accountDetails.POSTransactions.Add(tx);
            });

            account.CCTopUpTransactions.ForEach(x =>
            {
                CCTopUpTransaction tx = new CCTopUpTransaction()
                {
                    AccountNumberId = x.AccountNumberIdToRef,
                    Amount = x.Amount,
                    DateTime = x.TransactionDateTime,
                };
                accountDetails.TopUpTransactions.Add(tx);
            });

            return accountDetails;
        }

        [HttpGet]
        [Route("account")]
        public Account GetAccount()
        {
            AccountPM account = accountsRepository.GetAccountAndCardsForUser(GetCurrentUserId());

            Account accountDetails = new Account()
            {
                AccountNumberId = account.AccountNumberId,
                AccountState = account.AccountState,
                CustomerType = account.CustomerType,
                FirstName = account.FirstName,
                LastName = account.LastName,
                BusinessName = account.BusinessName,
                CompanyRegNumber = account.CompanyRegNumber,
                TaxNumber = account.TaxNumber,
                Balance = account.Balance,
            };

            account.Cards.ForEach(x => 
            {
                Card cd = new Card()
                {
                    CardSerialNumberId = x.CardSerialNumberId,
                    CardState = x.CardState,
                    DailySpendLimit = x.DailySpendLimit,
                    MonthlySpendLimit = x.MonthlySpendLimit,
                    FreindlyName = x.FreindlyName,
                };
                accountDetails.Cards.Add(cd);
            });

            return accountDetails;
        }

        [HttpPost]
        [Route("account/updatebusinessaccountdetails")]
        public void UpdateBusinessAccountDetails(string json)
        {
            Account account = Account.FromJsonString(json);

            if (!Validate.GuidValidation(account.AccountNumberId))
                throw new ValidationException("Invalid AccountNumberId");

            accountsRepository.UpdateAccount(
                new AccountPM()
                {
                    CustomerType = CustomerType.Business,
                    AccountNumberId = account.AccountNumberId,
                    BusinessName = account.BusinessName,
                    CompanyRegNumber = account.CompanyRegNumber,
                    TaxNumber = account.TaxNumber
                }, GetCurrentUserId());
        }

        [HttpPost]
        [Route("account/updateindividualaccountdetails")]
        public void UpdateIndividualAccountDetails(string json)
        {
            Account account = Account.FromJsonString(json);

            if (!Validate.GuidValidation(account.AccountNumberId))
                throw new ValidationException("Invalid AccountNumberId");

            accountsRepository.UpdateAccount(
                new AccountPM()
                {
                    CustomerType = CustomerType.Individual,
                    AccountNumberId = account.AccountNumberId,
                    FirstName = account.FirstName,
                    LastName = account.LastName,
                }, GetCurrentUserId());
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst("sub").Value;
        }
    }
}

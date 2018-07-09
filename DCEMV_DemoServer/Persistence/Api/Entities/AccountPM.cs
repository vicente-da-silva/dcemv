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
using System.ComponentModel.DataAnnotations;
using DCEMV.ServerShared;

namespace DCEMV.DemoServer.Persistence.Api.Entities
{
    public class AccountPM
    {
        [Key]
        public string AccountNumberId { get; set; }

        [Required]
        public string CredentialsId { get; set; }

        [Required]
        public long Balance { get; set; }
        [Required]
        public DateTime BalanceUpdateTime { get; set; }

        public List<CardPM> Cards { get; set; }
        public List<InventoryItemPM> InventoryItems { get; set; }
        public List<InventoryGroupPM> InventoryGroups { get; set; }
        public List<CCTopUpTransactionPM> CCTopUpTransactions { get; set; }
        public List<POSTransactionPM> POSTransactionsTo { get; set; }
        public List<POSTransactionPM> POSTransactionsFrom { get; set; }
        public List<TransactionPM> TransactionsFrom { get; set; }
        public List<TransactionPM> TransactionsTo { get; set; }

        [Required]
        public CustomerType CustomerType { get; set; }

        [Required]
        public AccountState AccountState { get; set; }

        #region individual account
        [StringLength(200)]
        public string FirstName { get; set; }
        [StringLength(200)]
        public string LastName { get; set; }
        #endregion

        #region business account
        [StringLength(200)]
        public string BusinessName { get; set; }
        [StringLength(200)]
        public string CompanyRegNumber { get; set; }
        [StringLength(200)]
        public string TaxNumber { get; set; }
        #endregion

        public AccountPM()
        {
            Cards = new List<CardPM>();
        }

        internal void UpdateAccount(AccountPM account)
        {
            AccountState = account.AccountState;

            switch (account.CustomerType)
            {
                case CustomerType.Business:
                    CustomerType = account.CustomerType;
                    BusinessName = account.BusinessName;
                    CompanyRegNumber = account.CompanyRegNumber;
                    TaxNumber = account.TaxNumber;
                    break;

                case CustomerType.Individual:
                    CustomerType = account.CustomerType;
                    FirstName = account.FirstName;
                    LastName = account.LastName;
                    break;

                default:
                    throw new ValidationException("unknown customer type");
            }
        }
    }
}

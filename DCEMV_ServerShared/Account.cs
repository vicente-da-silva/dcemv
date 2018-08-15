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
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DCEMV.ServerShared
{
    public class Account
    {
        public string AccountNumberId { get; set; }
        public AccountState AccountState { get; set; }
        public CustomerType CustomerType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BusinessName { get; set; }
        public string CompanyRegNumber { get; set; }
        public string TaxNumber { get; set; }
        public long Balance { get; set; }

        public ObservableCollection<Card> Cards {get;set;}
        public List<CardTransferTransaction> TransferFromTransactions { get; set; }
        public List<CardTransferTransaction> TransferToTransactions { get; set; }
        public List<CCTopUpTransaction> TopUpTransactions { get; set; }
        public List<POSTransaction> POSTransactions { get; set; }

        public Account()
        {
            Cards = new ObservableCollection<Card>();
            TransferFromTransactions = new List<CardTransferTransaction>();
            TransferToTransactions = new List<CardTransferTransaction>();
            TopUpTransactions = new List<CCTopUpTransaction>();
            POSTransactions = new List<POSTransaction>();
        }

        //public string ToJsonString()
        //{
        //    string ser = JsonConvert.SerializeObject(this);
        //    return System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(ser));
        //}
        //public static Account FromJsonString(string json)
        //{
        //    byte[] deser = System.Convert.FromBase64String(json);
        //    return JsonConvert.DeserializeObject<Account>(System.Text.Encoding.ASCII.GetString(deser));
        //}

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static Account FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<Account>(json);
        }
    }
}

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
using Newtonsoft.Json;

namespace DCEMV.ServerShared
{
    public class POSTransaction
    {
        public List<POSTransactionItem> InvItems { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public string AccountNumberId { get; set; }
        public int TransactionId { get; set; }
        public int POSTransactionId { get; set; }

        public POSTransaction()
        {
            InvItems = new List<POSTransactionItem>();
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static POSTransaction FromJsonString(string posTx)
        {
            return JsonConvert.DeserializeObject<POSTransaction>(posTx);
        }
    }
}

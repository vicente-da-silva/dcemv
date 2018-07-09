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
using DCEMV.FormattingUtils;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public class TornTransactionLogManager
    {
        public TLVList TornTransactionLogs { get; }
        public TornTransactionLogManager()
        {
            TornTransactionLogs = new TLVList();
        }

        internal void AddTornTransactionLog(Kernels.K2.Kernel2Database database)
        {
            int mnttl = (int)Formatting.ConvertToInt32(database.GetDefault(EMVTagsEnum.MAX_NUMBER_OF_TORN_TRANSACTION_LOG_RECORDS_DF811D_KRN2).Value);

            if (TornTransactionLogs.Count == mnttl)
            {
                database.Get(EMVTagsEnum.TORN_RECORD_FF8101_KRN2).Value = TornTransactionLogs.GetLastAndRemoveFromList().Value;
            }
            TornTransactionLogs.AddToList(database.TornTempRecord);
        }
    }
}

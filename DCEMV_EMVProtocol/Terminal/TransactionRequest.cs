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
using DCEMV.EMVProtocol.Kernels;
using DCEMV.FormattingUtils;
using System;
using System.Text;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol
{
    /*
       Transaction Type '9C' See Table 5-6 regarding the values of these data
       Amount, Authorised (Numeric) '9F02' elements.
       Amount, Other (Numeric) '9F03'
       Unpredictable Number '9F37'
       Transaction Currency Code '5F2A' Transaction-specific only if multiple currencies are supported.
       */
    public class TransactionRequest
    {
        private TLVList TxData;

        public bool IsSingleUnitOfCurrency { get; }

        private long amount;
        private long amountOther;
        private TransactionTypeEnum transactionTypeEnum;
        private DateTime transactionDate;

        public TransactionRequest(long amount, long amountOther, TransactionTypeEnum transactionTypeEnum)
        {
            this.amount = amount;
            this.amountOther = amountOther;
            this.transactionTypeEnum = transactionTypeEnum;
            this.transactionDate = DateTime.Now;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TransactionRequest");
            sb.AppendLine(string.Format("{0,-30}:{1}", "Amount",        Convert.ToString(amount)));
            sb.AppendLine(string.Format("{0,-30}:{1}", "AmountOther" ,  Convert.ToString(amountOther) ));
            sb.AppendLine(string.Format("{0,-30}:{1}", "TransactionTypeEnum" , transactionTypeEnum ));
            sb.AppendLine(string.Format("{0,-30}:{1}", "TransactionDate" , transactionDate.ToUniversalTime() ));
            return sb.ToString();
        }

        public TLVList GetTxDataTLV()
        {
            TxData = new TLVList();
            TxData.AddToList(TLV.Create(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag, Formatting.StringToBcd(Convert.ToString(amount), EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.DataFormatter.GetMaxLength(), false)));
            TxData.AddToList(TLV.Create(EMVTagsEnum.AMOUNT_OTHER_NUMERIC_9F03_KRN.Tag, Formatting.StringToBcd(Convert.ToString(amountOther), EMVTagsEnum.AMOUNT_OTHER_NUMERIC_9F03_KRN.DataFormatter.GetMaxLength(), false)));
            TxData.AddToList(TLV.Create(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN.Tag, new byte[] { (byte)transactionTypeEnum }));
            TxData.AddToList(TLV.Create(EMVTagsEnum.TRANSACTION_DATE_9A_KRN.Tag, EMVTagsEnum.TRANSACTION_DATE_9A_KRN.FormatFromDateTime(transactionDate)));
            TxData.AddToList(TLV.Create(EMVTagsEnum.TRANSACTION_TIME_9F21_KRN.Tag, EMVTagsEnum.TRANSACTION_TIME_9F21_KRN.FormatFromDateTime(transactionDate)));
            TxData.AddToList(TLV.Create(EMVTagsEnum.POINTOFSERVICE_POS_ENTRY_MODE_9F39_KRN.Tag, Formatting.HexStringToByteArray("00")));
            TxData.AddToList(TLV.Create(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN.Tag, Formatting.StringToBcd(Convert.ToString(ISOCurrencyCodesEnum.ZAR.Code).PadLeft(4, '0'), false)));
            return TxData;
        }

        public long GetAmountAuthorized_9F02()
        {
            return amount;
        }

        public long GetAmounOther_9F03()
        {
            return amountOther;
        }

        public TransactionTypeEnum GetTransactionType_9C()
        {
            return transactionTypeEnum;
        }
    }
}



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
namespace DCEMV.SPDHProtocol
{

    internal class F00_NormalPurchase : TransactionBase
    {
        public F00_NormalPurchase()
            : base(SPDHTransactionTypeEnum.F00, "Normal purchase")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('B', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

    internal class F01_PreauthorizationPurchase : TransactionBase
    {
        public F01_PreauthorizationPurchase()
            : base(SPDHTransactionTypeEnum.F01, "Preauthorization purchase")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('B', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('i', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

    internal class F02_PreauthorizationPurchaseCompletion : TransactionBase
    {
        public F02_PreauthorizationPurchaseCompletion()
            : base(SPDHTransactionTypeEnum.F02, "Preauthorization purchase completion")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('B', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('i', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

    internal class F03_MailOrTelephoneOrder : TransactionBase
    {
        public F03_MailOrTelephoneOrder()
            : base(SPDHTransactionTypeEnum.F03, "Mail or telephone order")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('B', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

    internal class F04_MerchandiseReturn : TransactionBase
    {
        public F04_MerchandiseReturn()
            : base(SPDHTransactionTypeEnum.F04, "Merchandise return")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('B', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

    internal class F05_CashAdvance : TransactionBase
    {
        public F05_CashAdvance()
            : base(SPDHTransactionTypeEnum.F05, "Cash advance")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('B', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

    internal class F06_CardVerification : TransactionBase
    {
        public F06_CardVerification()
            : base(SPDHTransactionTypeEnum.F06, "Card verification")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

    internal class F07_BalanceInquiry : TransactionBase
    {
        public F07_BalanceInquiry()
            : base(SPDHTransactionTypeEnum.F07, "Balance inquiry")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

    internal class F08_PurchaseWithCashBack : TransactionBase
    {
        public F08_PurchaseWithCashBack()
            : base(SPDHTransactionTypeEnum.F08, "Purchase with cash back")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('B', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('C', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

    internal class F11_PurchaseAdjustment : TransactionBase
    {
        public F11_PurchaseAdjustment()
            : base(SPDHTransactionTypeEnum.F11, "Purchase adjustment")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('B', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('C', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

    internal class F12_MerchandiseReturnAdjustment : TransactionBase
    {
        public F12_MerchandiseReturnAdjustment()
            : base(SPDHTransactionTypeEnum.F12, "Merchandise return adjustment")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('B', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('C', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

    internal class F13_CashAdvanceAdjustment : TransactionBase
    {
        public F13_CashAdvanceAdjustment()
            : base(SPDHTransactionTypeEnum.F13, "Cash advance adjustment")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('B', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('C', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

    internal class F14_CashBackAdjustment : TransactionBase
    {
        public F14_CashBackAdjustment()
            : base(SPDHTransactionTypeEnum.F14, "Cash back adjustment")
        {

            MandatoryFidIds.Add(FidMetaList.FindMeta('B', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('C', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('q', ' '));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'E'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'I'));

            MandatoryFidIds.Add(FidMetaList.FindMeta('6', 'O'));

        }
    }

}

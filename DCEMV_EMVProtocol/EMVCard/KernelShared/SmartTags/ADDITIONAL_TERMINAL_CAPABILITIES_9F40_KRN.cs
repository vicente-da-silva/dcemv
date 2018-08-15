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
using DataFormatters;
using DCEMV.FormattingUtils;

namespace DCEMV.EMVProtocol.Kernels
{
    public class ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN : SmartTag
    {
        public enum Byte1
        {
            Cash = 8,
            Goods = 7,
            Services = 6,
            Cashback = 5,
            Inquiry = 4,
            Transfer = 3,
            Payment = 2,
            Administrative = 1,
        }
        public enum Byte2
        {
            CashDeposit = 8,
        }
        public enum Byte3
        {
            NumericKeys = 8,
            AlphabeticalAndSpecialCharactersKeys = 7,
            CommandKeys = 6,
            FunctionKeys = 5,
        }
        public enum Byte4
        {
            PrintAttendant = 8,
            PrintCardholder = 7,
            DisplayAttendant = 6,
            DisplayCardholder = 5,
            CodeTable10 = 2,
            CodeTable9 = 1,
        }
        public enum Byte5
        {
            CodeTable8 = 8,
            CodeTable7 = 7,
            CodeTable6 = 6,
            CodeTable5 = 5,
            CodeTable4 = 4,
            CodeTable3 = 3,
            CodeTable2 = 2,
            CodeTable1 = 1,
        }
        public class ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN_VALUE : SmartValue
        {
            public ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public bool IsCash { get;  set; }
            public bool IsGoods { get;  set; }
            public bool IsServices { get;  set; }
            public bool IsCashback { get;  set; }
            public bool IsInquiry { get;  set; }
            public bool IsTransfer { get;  set; }
            public bool IsPayment { get;  set; }
            public bool IsAdministrative { get;  set; }
            public bool IsCashDeposit { get;  set; }
            public bool IsNumericKeys { get;  set; }
            public bool IsAlphabeticalAndSpecialCharactersKeys { get;  set; }
            public bool IsCommandKeys { get;  set; }
            public bool IsFunctionKeys { get;  set; }
            public bool IsPrintAttendant { get;  set; }
            public bool IsPrintCardholder { get;  set; }
            public bool IsDisplayAttendant { get;  set; }
            public bool IsDisplayCardholder { get;  set; }
            public bool IsCodeTable10 { get;  set; }
            public bool IsCodeTable9 { get;  set; }
            public bool IsCodeTable8 { get;  set; }
            public bool IsCodeTable7 { get;  set; }
            public bool IsCodeTable6 { get;  set; }
            public bool IsCodeTable5 { get;  set; }
            public bool IsCodeTable4 { get;  set; }
            public bool IsCodeTable3 { get;  set; }
            public bool IsCodeTable2 { get;  set; }
            public bool IsCodeTable1 { get;  set; }

            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[4], IsCodeTable1, (int)Byte5.CodeTable1);
                Formatting.SetBitPosition(ref Value[4], IsCodeTable2, (int)Byte5.CodeTable2);
                Formatting.SetBitPosition(ref Value[4], IsCodeTable3, (int)Byte5.CodeTable3);
                Formatting.SetBitPosition(ref Value[4], IsCodeTable4, (int)Byte5.CodeTable4);
                Formatting.SetBitPosition(ref Value[4], IsCodeTable5, (int)Byte5.CodeTable5);
                Formatting.SetBitPosition(ref Value[4], IsCodeTable6, (int)Byte5.CodeTable6);
                Formatting.SetBitPosition(ref Value[4], IsCodeTable7, (int)Byte5.CodeTable7);
                Formatting.SetBitPosition(ref Value[4], IsCodeTable8, (int)Byte5.CodeTable8);

                Formatting.SetBitPosition(ref Value[3], IsCodeTable9, (int)Byte4.CodeTable9);
                Formatting.SetBitPosition(ref Value[3], IsCodeTable10, (int)Byte4.CodeTable10);
                Formatting.SetBitPosition(ref Value[3], IsDisplayAttendant, (int)Byte4.DisplayAttendant);
                Formatting.SetBitPosition(ref Value[3], IsDisplayCardholder, (int)Byte4.DisplayCardholder);
                Formatting.SetBitPosition(ref Value[3], IsPrintAttendant, (int)Byte4.PrintAttendant);
                Formatting.SetBitPosition(ref Value[3], IsPrintCardholder, (int)Byte4.PrintCardholder);

                Formatting.SetBitPosition(ref Value[2], IsAlphabeticalAndSpecialCharactersKeys, (int)Byte3.AlphabeticalAndSpecialCharactersKeys);
                Formatting.SetBitPosition(ref Value[2], IsCommandKeys, (int)Byte3.CommandKeys);
                Formatting.SetBitPosition(ref Value[2], IsFunctionKeys, (int)Byte3.FunctionKeys);
                Formatting.SetBitPosition(ref Value[2], IsNumericKeys, (int)Byte3.NumericKeys);

                Formatting.SetBitPosition(ref Value[1], IsCashDeposit, (int)Byte2.CashDeposit);

                Formatting.SetBitPosition(ref Value[0], IsAdministrative, (int)Byte1.Administrative);
                Formatting.SetBitPosition(ref Value[0], IsCash, (int)Byte1.Cash);
                Formatting.SetBitPosition(ref Value[0], IsCashback, (int)Byte1.Cashback);
                Formatting.SetBitPosition(ref Value[0], IsGoods, (int)Byte1.Goods);
                Formatting.SetBitPosition(ref Value[0], IsInquiry, (int)Byte1.Inquiry);
                Formatting.SetBitPosition(ref Value[0], IsPayment, (int)Byte1.Payment);
                Formatting.SetBitPosition(ref Value[0], IsServices, (int)Byte1.Services);
                Formatting.SetBitPosition(ref Value[0], IsTransfer, (int)Byte1.Transfer);

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                IsCodeTable1 = Formatting.GetBitPosition(Value[4],  (int)Byte5.CodeTable1);
                IsCodeTable2 = Formatting.GetBitPosition(Value[4],  (int)Byte5.CodeTable2);
                IsCodeTable3 = Formatting.GetBitPosition(Value[4],  (int)Byte5.CodeTable3);
                IsCodeTable4 = Formatting.GetBitPosition(Value[4],  (int)Byte5.CodeTable4);
                IsCodeTable5 = Formatting.GetBitPosition(Value[4],  (int)Byte5.CodeTable5);
                IsCodeTable6 = Formatting.GetBitPosition(Value[4],  (int)Byte5.CodeTable6);
                IsCodeTable7 = Formatting.GetBitPosition(Value[4],  (int)Byte5.CodeTable7);
                IsCodeTable8 = Formatting.GetBitPosition(Value[4],  (int)Byte5.CodeTable8);

                IsCodeTable9 = Formatting.GetBitPosition(Value[3],  (int)Byte4.CodeTable9);
                IsCodeTable10 = Formatting.GetBitPosition(Value[3],  (int)Byte4.CodeTable10);
                IsDisplayAttendant = Formatting.GetBitPosition(Value[3],  (int)Byte4.DisplayAttendant);
                IsDisplayCardholder = Formatting.GetBitPosition(Value[3],  (int)Byte4.DisplayCardholder);
                IsPrintAttendant = Formatting.GetBitPosition(Value[3],  (int)Byte4.PrintAttendant);
                IsPrintCardholder = Formatting.GetBitPosition(Value[3],  (int)Byte4.PrintCardholder);

                IsAlphabeticalAndSpecialCharactersKeys = Formatting.GetBitPosition(Value[2],  (int)Byte3.AlphabeticalAndSpecialCharactersKeys);
                IsCommandKeys = Formatting.GetBitPosition(Value[2],  (int)Byte3.CommandKeys);
                IsFunctionKeys = Formatting.GetBitPosition(Value[2],  (int)Byte3.FunctionKeys);
                IsNumericKeys = Formatting.GetBitPosition(Value[2],  (int)Byte3.NumericKeys);

                IsNumericKeys = Formatting.GetBitPosition(Value[1],  (int)Byte2.CashDeposit);

                IsAdministrative = Formatting.GetBitPosition(Value[0],  (int)Byte1.Administrative);
                IsCash = Formatting.GetBitPosition(Value[0],  (int)Byte1.Cash);
                IsCashback = Formatting.GetBitPosition(Value[0],  (int)Byte1.Cashback);
                IsGoods = Formatting.GetBitPosition(Value[0],  (int)Byte1.Goods);
                IsInquiry = Formatting.GetBitPosition(Value[0],  (int)Byte1.Inquiry);
                IsPayment = Formatting.GetBitPosition(Value[0],  (int)Byte1.Payment);
                IsServices = Formatting.GetBitPosition(Value[0],  (int)Byte1.Services);
                IsTransfer = Formatting.GetBitPosition(Value[0],  (int)Byte1.Transfer);

                return pos;
            }
        }
        public new ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN_VALUE Value { get { return (ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN_VALUE)Val; } }

        public ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN, 
                  new ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN_VALUE(EMVTagsEnum.ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN.DataFormatter))
        {
        }

        public ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN()
            : base(EMVTagsEnum.ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN,
                  new ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN_VALUE(EMVTagsEnum.ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN.DataFormatter))
        {
        }
    }
}

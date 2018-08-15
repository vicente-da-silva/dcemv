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
    public class APPLICATION_USAGE_CONTROL_9F07_KRN : SmartTag
    {
        public enum Byte1
        {
            ValidForDomesticCashTransactions = 8,
            ValidForInternationalCashTransactions = 7,
            ValidForDomesticGoods = 6,
            ValidForInternationalGoods = 5,
            ValidForDomesticServices = 4,
            ValidForInternationalServices = 3,
            ValidAtATMs = 2,
            ValidAtTerminalsOtherThanATMs = 1,
        }
        public enum Byte2
        {
            DomesticCashbackAllowed = 8,
            InternationalCashbackAllowed = 7,
        }
        
        public class APPLICATION_USAGE_CONTROL_9F07_KRN_VALUE : SmartValue
        {
            public APPLICATION_USAGE_CONTROL_9F07_KRN_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public bool IsValidForDomesticCashTransactions { get; protected set; }
            public bool IsValidForInternationalCashTransactions { get; protected set; }
            public bool IsValidForDomesticGoods { get; protected set; }
            public bool IsValidForInternationalGoods { get; protected set; }
            public bool IsValidForDomesticServices { get; protected set; }
            public bool IsValidForInternationalServices { get; protected set; }
            public bool IsValidAtATMs { get; protected set; }
            public bool IsValidAtTerminalsOtherThanATMs { get; protected set; }
            public bool IsDomesticCashbackAllowed { get; protected set; }
            public bool IsInternationalCashbackAllowed { get; protected set; }

            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], IsValidAtATMs, (int)Byte1.ValidAtATMs);
                Formatting.SetBitPosition(ref Value[0], IsValidAtTerminalsOtherThanATMs, (int)Byte1.ValidAtTerminalsOtherThanATMs);
                Formatting.SetBitPosition(ref Value[0], IsValidForDomesticCashTransactions, (int)Byte1.ValidForDomesticCashTransactions);
                Formatting.SetBitPosition(ref Value[0], IsValidForDomesticGoods, (int)Byte1.ValidForDomesticGoods);
                Formatting.SetBitPosition(ref Value[0], IsValidForDomesticServices, (int)Byte1.ValidForDomesticServices);
                Formatting.SetBitPosition(ref Value[0], IsValidForInternationalCashTransactions, (int)Byte1.ValidForInternationalCashTransactions);
                Formatting.SetBitPosition(ref Value[0], IsValidForInternationalGoods, (int)Byte1.ValidForInternationalGoods);
                Formatting.SetBitPosition(ref Value[0], IsValidForInternationalServices, (int)Byte1.ValidForInternationalServices);

                Formatting.SetBitPosition(ref Value[1], IsDomesticCashbackAllowed, (int)Byte2.DomesticCashbackAllowed);
                Formatting.SetBitPosition(ref Value[1], IsInternationalCashbackAllowed, (int)Byte2.InternationalCashbackAllowed);

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);
                
                IsValidAtATMs = Formatting.GetBitPosition(Value[0],  (int)Byte1.ValidAtATMs);
                IsValidAtTerminalsOtherThanATMs = Formatting.GetBitPosition(Value[0],  (int)Byte1.ValidAtTerminalsOtherThanATMs);
                IsValidForDomesticCashTransactions = Formatting.GetBitPosition(Value[0],  (int)Byte1.ValidForDomesticCashTransactions);
                IsValidForDomesticGoods = Formatting.GetBitPosition(Value[0],  (int)Byte1.ValidForDomesticGoods);
                IsValidForDomesticServices = Formatting.GetBitPosition(Value[0],  (int)Byte1.ValidForDomesticServices);
                IsValidForInternationalCashTransactions = Formatting.GetBitPosition(Value[0],  (int)Byte1.ValidForInternationalCashTransactions);
                IsValidForInternationalGoods = Formatting.GetBitPosition(Value[0],  (int)Byte1.ValidForInternationalGoods);
                IsValidForInternationalServices = Formatting.GetBitPosition(Value[0],  (int)Byte1.ValidForInternationalServices);

                IsDomesticCashbackAllowed = Formatting.GetBitPosition(Value[1], (int)Byte2.DomesticCashbackAllowed);
                IsInternationalCashbackAllowed = Formatting.GetBitPosition(Value[1], (int)Byte2.InternationalCashbackAllowed);

                return pos;
            }
        }
        public new APPLICATION_USAGE_CONTROL_9F07_KRN_VALUE Value { get { return (APPLICATION_USAGE_CONTROL_9F07_KRN_VALUE)Val; } }

        public APPLICATION_USAGE_CONTROL_9F07_KRN(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.APPLICATION_USAGE_CONTROL_9F07_KRN, 
                  new APPLICATION_USAGE_CONTROL_9F07_KRN_VALUE(EMVTagsEnum.APPLICATION_USAGE_CONTROL_9F07_KRN.DataFormatter))
        {
        }
    }
}

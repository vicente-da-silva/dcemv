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

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public class DS_ODS_INFO_FOR_READER_DF810A_KRN2 : SmartTag
    {
        public class DS_ODS_INFO_FOR_READER_DF810A_KRN2_VALUE : SmartValue
        {
            public DS_ODS_INFO_FOR_READER_DF810A_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }
            public bool UsableForTC { get; set; }
            public bool UsableForARQC { get; set; }
            public bool UsableForAAC { get; set; }


            public bool StopIfNoDSODSTerm { get; set; }
            public bool StopIFWriteFailed { get; set; }
            
            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], UsableForTC, 8);
                Formatting.SetBitPosition(ref Value[0], UsableForARQC, 7);
                Formatting.SetBitPosition(ref Value[0], UsableForAAC, 6);
                Formatting.SetBitPosition(ref Value[0], StopIfNoDSODSTerm, 3);
                Formatting.SetBitPosition(ref Value[0], StopIFWriteFailed, 2);

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                UsableForTC = Formatting.GetBitPosition(Value[0], 8);
                UsableForARQC = Formatting.GetBitPosition(Value[0], 7);
                UsableForAAC = Formatting.GetBitPosition(Value[0], 6);
                StopIfNoDSODSTerm = Formatting.GetBitPosition(Value[0], 3);
                StopIFWriteFailed = Formatting.GetBitPosition(Value[0], 2);

                return pos;
            }
        }
        public new DS_ODS_INFO_FOR_READER_DF810A_KRN2_VALUE Value { get { return (DS_ODS_INFO_FOR_READER_DF810A_KRN2_VALUE)Val; } }

        public DS_ODS_INFO_FOR_READER_DF810A_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.DS_ODS_INFO_FOR_READER_DF810A_KRN2, 
                  new DS_ODS_INFO_FOR_READER_DF810A_KRN2_VALUE(EMVTagsEnum.DS_ODS_INFO_FOR_READER_DF810A_KRN2.DataFormatter))
        {
        }
    }
}

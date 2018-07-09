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
    public enum IDSStatusEnum
    {
        READ = 0x80,
        WRITE = 0x40,
    }

    public class IDS_STATUS_DF8128_KRN2 : SmartTag
    {
        public class IDS_STATUS_DF8128_KRN2_VALUE : SmartValue
        {
            public IDS_STATUS_DF8128_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public bool IsRead { get; set; }
            public bool IsWrite { get; set; }
            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], IsWrite, 8);
                Formatting.SetBitPosition(ref Value[0], IsRead, 7);
                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                IsWrite = Formatting.GetBitPosition(Value[0], 8);
                IsRead = Formatting.GetBitPosition(Value[0], 7);

                return pos;
            }
        }

        public new IDS_STATUS_DF8128_KRN2_VALUE Value { get { return (IDS_STATUS_DF8128_KRN2_VALUE)Val; } }
        public IDS_STATUS_DF8128_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.IDS_STATUS_DF8128_KRN2, 
                  new IDS_STATUS_DF8128_KRN2_VALUE(EMVTagsEnum.IDS_STATUS_DF8128_KRN2.DataFormatter))
        {
        }
    }
}

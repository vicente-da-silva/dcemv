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

namespace DCEMV.EMVProtocol.Kernels
{
    public enum ACIVersionNumberEnum
    {
        VERSION0 = 0x00,
    }
    public enum DataStorageVersionNumberEnum
    {
        DATA_STORAGE_NOT_SUPPORTED = 0x00,
        VERSION_1 = 0x01,
        VERSION_2 = 0x02,
    }
    public enum CDAIndicatorEnum
    {
        CDA_SUPPORTED_IN_EMV = 0x00,
        CDA_SUPPORTED_OVER_TC_ARQC_AAC = 0x01
    }
    public enum SDSSchemeIndicatorEnum
    {
        UNDEFINED_SDS_CONFIGURATION = 0x00,
        ALL_10_TAGS_32_BYTES = 0x01,
        ALL_10_TAGS_48_BYTES = 0x02,
        ALL_10_TAGS_64_BYTES = 0x03,
        ALL_10_TAGS_96_BYTES = 0x04,
        ALL_10_TAGS_128_BYTES = 0x05,
        ALL_10_TAGS_160_BYTES = 0x06,
        ALL_10_TAGS_192_BYTES = 0x07,
        ALL_SDS_TAGS_32_BYTES_EXCEPT_9F78_WHICH_IS_64_BYTES = 0x08,
    }

    public class APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2 : SmartTag
    {
        public class APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2_VALUE : SmartValue
        {
            public ACIVersionNumberEnum ACIVersionNumberEnum { get; set; }
            public DataStorageVersionNumberEnum DataStorageVersionNumberEnum { get; set; }
            public bool SupportForFieldOffDetection { get; set; }
            public bool SupportForBalanceReading { get; set; }
            public CDAIndicatorEnum CDAIndicatorEnum { get; set; }
            public SDSSchemeIndicatorEnum SDSSchemeIndicatorEnum { get; set; }

            public APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }
            public override byte[] Serialize()
            {
                Value[0] = (byte)(((byte)ACIVersionNumberEnum << 4) | (byte)DataStorageVersionNumberEnum);
                byte x = SupportForFieldOffDetection ? (byte)0x01 : (byte)0x00;
                byte y = SupportForBalanceReading ? (byte)0x01 : (byte)0x00;
                Value[1] = (byte)(x << 2 | y << 1 | (byte)CDAIndicatorEnum);
                Value[2] = (byte)SDSSchemeIndicatorEnum;

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                ACIVersionNumberEnum = (ACIVersionNumberEnum)GetEnum(typeof(ACIVersionNumberEnum), Value[0] & 0XF0);
                DataStorageVersionNumberEnum = (DataStorageVersionNumberEnum)GetEnum(typeof(DataStorageVersionNumberEnum), Value[0] & 0x0F);
                SupportForFieldOffDetection = (Value[1] & 0x04) == 0x04 ? true : false;
                SupportForBalanceReading = (Value[1] & 0x02) == 0x02 ? true : false;
                CDAIndicatorEnum = (CDAIndicatorEnum)GetEnum(typeof(CDAIndicatorEnum), Value[1] & 0X01);
                SDSSchemeIndicatorEnum = (SDSSchemeIndicatorEnum)GetEnum(typeof(SDSSchemeIndicatorEnum), Value[2]);

                return pos;
            }
        }

        public new APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2_VALUE Value { get { return (APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2_VALUE)Val; } }

        public APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2, 
                  new APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2_VALUE(EMVTagsEnum.APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2.DataFormatter))
        {
        }
    }
}

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

    public class THIRD_PARTY_DATA_9F6E_KRN : SmartTag
    {
        public class THIRD_PARTY_DATA_9F6E_KRN_VALUE : SmartValue
        {
            public THIRD_PARTY_DATA_9F6E_KRN_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public string CountryCode { get; set; }
            public byte[] UniqueIdentifier { get; set; }
            public byte[] DeviceType { get; set; }
            public string ProprietaryData { get; set; }
            
            public override byte[] Serialize()
            {
                string output;
                if (!Formatting.GetBitPosition(UniqueIdentifier[0], 8))
                {
                    output = CountryCode + UniqueIdentifier + DeviceType + ProprietaryData;
                }
                else
                {
                    output = CountryCode + UniqueIdentifier + ProprietaryData;
                }
                Value = Formatting.HexStringToByteArray(output);
                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                string input = Formatting.ByteArrayToHexString(Value);

                CountryCode = input.Substring(0, 2);
                UniqueIdentifier = Formatting.HexStringToByteArray(input.Substring(2,2));
                if(!Formatting.GetBitPosition(UniqueIdentifier[0],8))
                {
                    DeviceType = Formatting.ASCIIStringToByteArray(input.Substring(4, 2));
                    ProprietaryData = input.Substring(6);
                }
                else
                    ProprietaryData = input.Substring(4);

                return pos;
            }
        }

        public new THIRD_PARTY_DATA_9F6E_KRN_VALUE Value { get { return (THIRD_PARTY_DATA_9F6E_KRN_VALUE)Val; } }
        public THIRD_PARTY_DATA_9F6E_KRN(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.THIRD_PARTY_DATA_9F6E_KRN2, 
                  new THIRD_PARTY_DATA_9F6E_KRN_VALUE(EMVTagsEnum.THIRD_PARTY_DATA_9F6E_KRN2.DataFormatter))
        {
        }
    }
}

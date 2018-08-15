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
    public class TRACK_2_EQUIVALENT_DATA_57_KRN : SmartTag
    {
        public class TRACK_2_EQUIVALENT_DATA_57_KRN_VALUE : SmartValue
        {
            public TRACK_2_EQUIVALENT_DATA_57_KRN_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public string PAN { get; set; }
            //public char FieldSeperator { get; set; }
            public string ExpirationDate { get; set; } //YYMM
            public string ServiceCode { get; set; }
            public string DiscretionaryData { get; set; }

            public override byte[] Serialize()
            {
                string output = PAN + 'D' + ExpirationDate + ServiceCode + DiscretionaryData;
                Value = Formatting.StringToBcd(output, false);
                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                string input = Formatting.BcdToString(Value);
                string[] inputs = input.Split('D');

                PAN = inputs[0];
                ExpirationDate = inputs[1].Substring(0, 4);
                ServiceCode = inputs[1].Substring(4, 3);
                DiscretionaryData = inputs[1].Substring(7, inputs[1].Length - 7);

                return pos;
            }
        }
        public new TRACK_2_EQUIVALENT_DATA_57_KRN_VALUE Value { get { return (TRACK_2_EQUIVALENT_DATA_57_KRN_VALUE)Val; } }
        public TRACK_2_EQUIVALENT_DATA_57_KRN(KernelDatabaseBase database)
             : base(database, EMVTagsEnum.TRACK_2_EQUIVALENT_DATA_57_KRN, 
                   new TRACK_2_EQUIVALENT_DATA_57_KRN_VALUE(EMVTagsEnum.TRACK_2_EQUIVALENT_DATA_57_KRN.DataFormatter))
        {
        }
    }
}

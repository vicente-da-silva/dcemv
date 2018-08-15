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
    public class TRACK_2_DATA_9F6B_KRN2 : SmartTag
    {
        public class TRACK_2_DATA_9F6B_KRN2_VALUE : SmartValue
        {
            public TRACK_2_DATA_9F6B_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public string PAN { get; set; }
            public string ExpirationDate { get; set; } //YYMM
            public string ServiceCode { get; set; }
            public byte[] DiscretionaryData { get; set; }

            public override byte[] Serialize()
            {
                //List<byte[]> result = new List<byte[]>();
                //result.Add(Formatting.StringToBcd(PAN,false));
                //result.Add(new byte[] { (byte)'D' });
                //result.Add(Formatting.StringToBcd(ExpirationDate, false));
                //result.Add(Formatting.StringToBcd(ServiceCode, false));
                //result.Add(DiscretionaryData);

                string output = PAN + 'D' + ExpirationDate + ServiceCode + Formatting.BcdToString(DiscretionaryData);
                Value = Formatting.StringToBcd(output, false);

                //Value = result.SelectMany(x => x).ToArray();
                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                string input = Formatting.ByteArrayToHexString(Value);
                //input = input.Replace("F", "");
                string[] inputs = input.Split('D');

                PAN = inputs[0];
                ExpirationDate = inputs[1].Substring(0, 4);
                ServiceCode = inputs[1].Substring(4, 3);
                DiscretionaryData = Formatting.StringToBcd(inputs[1].Substring(7, inputs[1].Length - 7), false);

                return pos;
            }
        }
        public new TRACK_2_DATA_9F6B_KRN2_VALUE Value { get { return (TRACK_2_DATA_9F6B_KRN2_VALUE)Val; } }
        public TRACK_2_DATA_9F6B_KRN2(KernelDatabaseBase database)
             : base(database, EMVTagsEnum.TRACK_2_DATA_9F6B_KRN2, 
                   new TRACK_2_DATA_9F6B_KRN2_VALUE(EMVTagsEnum.TRACK_2_DATA_9F6B_KRN2.DataFormatter))
        {
        }
    }
}

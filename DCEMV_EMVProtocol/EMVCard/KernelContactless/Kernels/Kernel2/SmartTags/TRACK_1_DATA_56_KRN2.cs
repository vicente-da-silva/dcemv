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
using System;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public class TRACK_1_DATA_56_KRN2 : SmartTag
    {
        public class TRACK_1_DATA_56_KRN2_VALUE : SmartValue
        {
            public TRACK_1_DATA_56_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }
            public byte FormatCode { get; protected set; }
            public string Name { get; protected set; }
            public byte FieldSeperator { get; protected set; }
            public string PAN { get; protected set; }
            public string ExpirationDate { get; protected set; } //YYMM
            public string ServiceCode { get; protected set; }
            public byte[] DiscretionaryData { get; set; }

            public override byte[] Serialize()
            {
                FormatCode = 0x42;
                FieldSeperator = 0x5E;
                string output = Formatting.ByteArrayToASCIIString(new byte[] { FormatCode }) 
                    + PAN +
                    Formatting.ByteArrayToASCIIString(new byte[] { FieldSeperator }) + 
                    Name +
                    Formatting.ByteArrayToASCIIString(new byte[] { FieldSeperator }) + 
                    ExpirationDate + 
                    ServiceCode +
                    Formatting.ByteArrayToASCIIString(DiscretionaryData);
                Value = Formatting.ASCIIStringToByteArray(output);
                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                FieldSeperator = 0x5E;

                pos = base.Deserialize(rawTlv, pos);

                string input = Formatting.ByteArrayToASCIIString(Value);
                string[] inputs = input.Split(new string[] { Formatting.ByteArrayToASCIIString(new byte[] { FieldSeperator }) }, StringSplitOptions.None);
                PAN = inputs[0].Substring(1);
                Name = inputs[1];
                ExpirationDate = inputs[2].Substring(0, 4);
                ServiceCode = inputs[2].Substring(4, 3);
                DiscretionaryData = Formatting.ASCIIStringToByteArray(inputs[2].Substring(7, inputs[2].Length - 7));

                return pos;
            }
        }
        public new TRACK_1_DATA_56_KRN2_VALUE Value { get { return (TRACK_1_DATA_56_KRN2_VALUE)Val; } }
        public TRACK_1_DATA_56_KRN2(KernelDatabaseBase database)
             : base(database, EMVTagsEnum.TRACK_1_DATA_56_KRN2, 
                   new TRACK_1_DATA_56_KRN2_VALUE(EMVTagsEnum.TRACK_1_DATA_56_KRN2.DataFormatter))
        {
        }
    }
}

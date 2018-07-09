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
using DCEMV.ISO7816Protocol;
using System.Text;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{

    public enum L2Enum
    {
        NOT_SET = 0xFF,

        OK = 0x00,
        CARD_DATA_MISSING = 0x01,
        CAM_FAILED = 0x02,
        STATUS_BYTES = 0x03,
        PARSING_ERROR = 0x04,
        MAX_LIMIT_EXCEEDED = 0x05,
        CARD_DATA_ERROR = 0x06,
        MAGSTRIPE_NOT_SUPPORTED = 0x07,
        NO_PPSE = 0x08,
        PPSE_FAULT = 0x09,
        EMPTY_CANDIDATE_LIST = 0x0A,
        IDS_READ_ERROR = 0x0B,
        IDS_WRITE_ERROR = 0x0C,
        IDS_DATA_ERROR = 0x0D,
        IDS_NO_MATCHING_AC = 0x0E,
        TERMINAL_DATA_ERROR = 0x0F,
    }
    public enum L3Enum
    {
        NOT_SET = 0xFF,

        OK = 0x00,
        TIME_OUT = 0x01,
        STOP = 0x02,
        AMOUNT_NOT_PRESENT = 0x3,
    }

    public class ERROR_INDICATION_DF8115_KRN2 : SmartTag
    {
        public class ERROR_INDICATION_DF8115_KRN2_VALUE : SmartValue
        {
            public ERROR_INDICATION_DF8115_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {
                SW12 = new byte[2];
            }
            public byte[] SW12 { get; set; }
            public L1Enum L1Enum { get; set; }
            public L2Enum L2Enum { get; set; }
            public L3Enum L3Enum { get; set; }
            public KernelMessageidentifierEnum MsgOnError { get; set; }

            
            public override byte[] Serialize()
            {
                Value[0] = (byte)L1Enum;
                Value[1] = (byte)L2Enum;
                Value[2] = (byte)L3Enum;
                Value[3] = SW12[0];
                Value[4] = SW12[1];
                Value[5] = (byte)MsgOnError;

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                L1Enum = (L1Enum)GetEnum(typeof(L1Enum), Value[0]);
                L2Enum = (L2Enum)GetEnum(typeof(L2Enum), Value[1]);
                L3Enum = (L3Enum)GetEnum(typeof(L3Enum), Value[2]);
                SW12[0] = rawTlv[3];
                SW12[1] = rawTlv[4];
                if (Value[5] != 0x00)
                    MsgOnError = (KernelMessageidentifierEnum)GetEnum(typeof(KernelMessageidentifierEnum), Value[5]);
                else
                    MsgOnError = KernelMessageidentifierEnum.N_A;

                return pos;
            }
        }

        public new ERROR_INDICATION_DF8115_KRN2_VALUE Value { get { return (ERROR_INDICATION_DF8115_KRN2_VALUE)Val; } }
        public ERROR_INDICATION_DF8115_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.ERROR_INDICATION_DF8115_KRN2, 
                  new ERROR_INDICATION_DF8115_KRN2_VALUE(EMVTagsEnum.ERROR_INDICATION_DF8115_KRN2.DataFormatter))
        {
            
        }

        public ERROR_INDICATION_DF8115_KRN2(TLV tlv)
            : base(tlv, EMVTagsEnum.ERROR_INDICATION_DF8115_KRN2,
                  new ERROR_INDICATION_DF8115_KRN2_VALUE(EMVTagsEnum.ERROR_INDICATION_DF8115_KRN2.DataFormatter))
        {

        }

        public override string ToPrintString(ref int depth)
        {
            StringBuilder sb = new StringBuilder();
            string tagName = TLVMetaDataSourceSingleton.Instance.DataSource.GetName(Tag.TagLable);

            string formatter = "{0,-75}";

            sb.AppendLine(string.Format(formatter, Tag.ToString() + " " + tagName + " L:[" + Val.GetLength().ToString() + "]"));
            sb.AppendLine("V:[");
            sb.AppendLine("\tL1Enum->" + Value.L1Enum);
            sb.AppendLine("\tL2Enum->" + Value.L2Enum);
            sb.AppendLine("\tL3Enum->" + Value.L3Enum);
            sb.AppendLine("\tSW1->" + Value.SW12[0]);
            sb.AppendLine("\tSW2->" + Value.SW12[1]);
            sb.AppendLine("\tMsg->" + Value.MsgOnError);
            sb.AppendLine("]");
            return sb.ToString();
        }
    }
}

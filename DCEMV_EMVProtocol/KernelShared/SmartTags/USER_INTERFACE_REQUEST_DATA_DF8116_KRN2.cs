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
using System.Text;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{

    public enum KernelMessageidentifierEnum
    {
        CARD_READ_OK = 0x17,
        TRY_AGAIN = 0x21,
        APPROVED = 0x03,
        APPROVED_SIGN = 0x1A,
        DECLINED = 0x07,
        ERROR_OTHER_CARD = 0x1C,
        INSERT_CARD = 0x1D,
        SEE_PHONE = 0x20,
        AUTHORISING_PLEASE_WAIT = 0x1B,
        CLEAR_DISPLAY = 0x1E,
        N_A = 0xFF,
    }
    public enum KernelStatusEnum
    {
        NOT_READY = 0x00,
        IDLE = 0x01,
        READY_TO_READ = 0x02,
        PROCESSING = 0x03,
        CARD_READ_SUCCESSFULLY = 0x04,
        PROCESSING_ERROR = 0x05,
        N_A = 0xFF,
    }
    public enum ValueQualifierEnum
    {
        NONE = 0x00,
        AMOUNT = 0x10,
        BALANCE = 0x20,
    }
    public class USER_INTERFACE_REQUEST_DATA_DF8116_KRN2 : SmartTag
    {
        public class USER_INTERFACE_REQUEST_DATA_DF8116_KRN2_VALUE : SmartValue
        {
            public USER_INTERFACE_REQUEST_DATA_DF8116_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {
                HoldTime = new byte[3];
                LanguagePreference = new byte[8];// Formatting.HexStringToByteArray("0000000000000000");
                ValueQualifierEnum = ValueQualifierEnum.NONE;
                ValueQualifier = new byte[6];////ValueQualifier = Formatting.HexStringToByteArray("000000000000");
                CurrencyCode = new byte[2];// Formatting.HexStringToByteArray("0000");

                KernelMessageidentifierEnum = KernelMessageidentifierEnum.N_A;
                KernelStatusEnum = KernelStatusEnum.N_A;
                ValueQualifierEnum = ValueQualifierEnum.NONE;
            }

            public KernelMessageidentifierEnum KernelMessageidentifierEnum { get; set; }
            public KernelStatusEnum KernelStatusEnum { get; set; }
            public byte[] HoldTime { get; set; } //l 3 and f n6
            public byte[] LanguagePreference { get; set; }//l 8 and an padded with hex 00
            public ValueQualifierEnum ValueQualifierEnum { get; set; }
            public byte[] ValueQualifier { get; set; } //l 6 and f n12
            public byte[] CurrencyCode { get; set; } //l 2 and f n3

            public override byte[] Serialize()
            {
                Value[0] = (byte)KernelMessageidentifierEnum;
                Value[1] = (byte)KernelStatusEnum;
                Array.Copy(HoldTime, 0, Value, 2, 3);
                Array.Copy(LanguagePreference, 0, Value, 5, 8);
                Value[13] = (byte)ValueQualifierEnum;
                Array.Copy(ValueQualifier, 0, Value, 14, 6);
                Array.Copy(CurrencyCode, 0, Value, 20, 2);

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);
                KernelMessageidentifierEnum = (KernelMessageidentifierEnum)GetEnum(typeof(KernelMessageidentifierEnum), Value[0]);
                KernelStatusEnum = (KernelStatusEnum)GetEnum(typeof(KernelStatusEnum), Value[1]);
                Array.Copy(Value, 2, HoldTime, 0, 3);
                Array.Copy(Value, 5, LanguagePreference, 0, 8);
                ValueQualifierEnum = (ValueQualifierEnum)GetEnum(typeof(ValueQualifierEnum), Value[13]);
                Array.Copy(Value, 14, ValueQualifier, 0, 6);
                Array.Copy(Value, 20, CurrencyCode, 0, 2);
                return pos;
            }
        }

        public new USER_INTERFACE_REQUEST_DATA_DF8116_KRN2_VALUE Value { get { return (USER_INTERFACE_REQUEST_DATA_DF8116_KRN2_VALUE)Val; } }
        public USER_INTERFACE_REQUEST_DATA_DF8116_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.USER_INTERFACE_REQUEST_DATA_DF8116_KRN2, 
                  new USER_INTERFACE_REQUEST_DATA_DF8116_KRN2_VALUE(EMVTagsEnum.USER_INTERFACE_REQUEST_DATA_DF8116_KRN2.DataFormatter))
        {
        }

        

        public USER_INTERFACE_REQUEST_DATA_DF8116_KRN2(TLV tlv)
            : base(tlv, EMVTagsEnum.USER_INTERFACE_REQUEST_DATA_DF8116_KRN2,
                  new USER_INTERFACE_REQUEST_DATA_DF8116_KRN2_VALUE(EMVTagsEnum.USER_INTERFACE_REQUEST_DATA_DF8116_KRN2.DataFormatter))
        {
        }

        public USER_INTERFACE_REQUEST_DATA_DF8116_KRN2()
            : base(EMVTagsEnum.USER_INTERFACE_REQUEST_DATA_DF8116_KRN2,
                  new USER_INTERFACE_REQUEST_DATA_DF8116_KRN2_VALUE(EMVTagsEnum.USER_INTERFACE_REQUEST_DATA_DF8116_KRN2.DataFormatter))
        {
        }

        public override string ToPrintString(ref int depth)
        {
            StringBuilder sb = new StringBuilder();
            string tagName = TLVMetaDataSourceSingleton.Instance.DataSource.GetName(Tag.TagLable);

            string formatter = "{0,-75}";

            sb.AppendLine(string.Format(formatter, Tag.ToString() + " " + tagName + " L:[" + Val.GetLength().ToString() + "]"));
            sb.AppendLine("V:[");
            sb.AppendLine("\tKernel1MessageidentifierEnum->" + Value.KernelMessageidentifierEnum);
            sb.AppendLine("\tKernel1StatusEnum->" + Value.KernelStatusEnum);
            sb.AppendLine("\tHoldTime->" + Formatting.ByteArrayToHexString(Value.HoldTime));
            sb.AppendLine("\tValueQualifierEnum->" + Value.ValueQualifierEnum);
            sb.AppendLine("\tLanguagePreference->" + Formatting.ByteArrayToHexString(Value.LanguagePreference));
            sb.AppendLine("\tValueQualifier->" + Formatting.ByteArrayToHexString(Value.ValueQualifier));
            sb.AppendLine("\tCurrencyCode->" + Formatting.ByteArrayToHexString(Value.CurrencyCode));
            sb.AppendLine("]");
            return sb.ToString();
        }
    }
}

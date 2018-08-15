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
using DCEMV.TLVProtocol;
using System.Text;

namespace DCEMV.EMVProtocol.Kernels
{
    public enum CVMResult
    {
        Unknown = 0x00,
        Failed = 0x01,
        Success = 0x02,
    }
    public class CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN : SmartTag
    {
        public class CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN_VALUE : SmartValue
        {
            public CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public byte CVMPerformed { get; set; }
            public byte CVMCondition { get; set; }
            public byte CVMResult { get; set; }

            public override byte[] Serialize()
            {
                Value = new byte[] { CVMPerformed , CVMCondition , CVMResult };

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                CVMResult = Value[2];
                CVMCondition = Value[1];
                CVMPerformed = Value[0];

                return pos;
            }

            public CVMCode GetCVMPerformed()
            {
                return (CVMCode)GetEnum(typeof(CVMCode), CVMPerformed & 0x3F);
            }
            public CVMConditionCode GetCVMCondition()
            {
                return (CVMConditionCode)GetEnum(typeof(CVMConditionCode), CVMCondition);
            }
            public CVMResult GetCVMResult()
            {
                return (CVMResult)GetEnum(typeof(CVMResult), CVMResult);
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("CVM Performed: " + (CVMCode)GetEnum(typeof(CVMCode), CVMPerformed & 0x3F));
                sb.Append("|Condition: " + (CVMConditionCode)GetEnum(typeof(CVMConditionCode), CVMCondition));
                sb.Append("|Result: " + (CVMResult)GetEnum(typeof(CVMResult), CVMResult));
                return sb.ToString();
            }
        }
        public new CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN_VALUE Value { get { return (CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN_VALUE)Val; } }
        public CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN(KernelDatabaseBase database)
             : base(database, EMVTagsEnum.CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN, 
                   new CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN_VALUE(EMVTagsEnum.CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN.DataFormatter))
        {
        }

        public CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN(TLV tlv)
             : base(tlv, EMVTagsEnum.CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN,
                   new CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN_VALUE(EMVTagsEnum.CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN.DataFormatter))
        {
        }
    }
}

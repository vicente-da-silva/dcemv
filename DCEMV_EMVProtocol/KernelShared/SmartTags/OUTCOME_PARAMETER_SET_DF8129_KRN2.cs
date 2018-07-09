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
using System.Text;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{

    public enum Kernel2OnlineResponseDataEnum
    {
        N_A = 0x0F,
    }
   
    public enum Kernel2AlternativeInterfacePreferenceEnum
    {
        N_A = 0x0F,
    }
    public class OUTCOME_PARAMETER_SET_DF8129_KRN2 : SmartTag
    {
        public class OUTCOME_PARAMETER_SET_DF8129_KRN2_VALUE : SmartValue
        {
            public OUTCOME_PARAMETER_SET_DF8129_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {
                Status = Kernel2OutcomeStatusEnum.N_A;
                Start = Kernel2StartEnum.N_A;
                OnlineResponseData = Kernel2OnlineResponseDataEnum.N_A;
                CVM = KernelCVMEnum.N_A;
                AlternateInterfacePreference = Kernel2AlternativeInterfacePreferenceEnum.N_A;
            }

            //b8-5
            public Kernel2OutcomeStatusEnum Status { get; set; }
            //b8-5
            public Kernel2StartEnum Start { get; set; }
            //b8-5
            public Kernel2OnlineResponseDataEnum OnlineResponseData { get; set; }
            //b8-5
            public KernelCVMEnum CVM { get; set; }

            //b8-4
            public bool UIRequestOnOutcomePresent { get; set; } //8
            public bool UIRequestOnRestartPresent { get; set; } //7
            public bool DataRecordPresent { get; set; } //6
            public bool DiscretionaryDataPresent { get; set; } //5
            public bool Receipt { get; set; } //4

            //b8-5
            public Kernel2AlternativeInterfacePreferenceEnum AlternateInterfacePreference { get; set; }

            //b8-1 //11111111: N/A //Other values: Hold time in units of 100 ms
            public byte FieldOffRequest { get; set; }

            //b8-1
            public byte RemovalTimeout { get; set; }

            public OUTCOME_PARAMETER_SET_DF8129_KRN2_VALUE()
                : base(EMVTagsEnum.OUTCOME_PARAMETER_SET_DF8129_KRN2.DataFormatter)
            {
            }

            public override byte[] Serialize()
            {
                //byte b1 = UIRequestOnOutcomePresent ? (byte)0x01 : (byte)0x00;
                //byte b2 = UIRequestOnRestartPresent ? (byte)0x01 : (byte)0x00;
                //byte b3 = DataRecordPresent ? (byte)0x01 : (byte)0x00;
                //byte b4 = DiscretionaryDataPresent ? (byte)0x01 : (byte)0x00;
                //byte b5 = Receipt ? (byte)0x01 : (byte)0x00;

                Value[0] = (byte)Status;
                Value[1] = (byte)Start;
                Value[2] = (byte)Kernel2OnlineResponseDataEnum.N_A;// (byte)OnlineResponseData;
                Value[3] = (byte)CVM;
                Formatting.SetBitPosition(ref Value[4], UIRequestOnOutcomePresent, 8); //Value[3] = (byte)(((b1 << 8) | (b1 << 7) | (b1 << 6) | (b1 << 5) | (b1 << 4)) & 0xF8);
                Formatting.SetBitPosition(ref Value[4], UIRequestOnRestartPresent, 7);
                Formatting.SetBitPosition(ref Value[4], DataRecordPresent, 6);
                Formatting.SetBitPosition(ref Value[4], DiscretionaryDataPresent, 5);
                Formatting.SetBitPosition(ref Value[4], Receipt, 4);
                Value[5] = (byte)Kernel2AlternativeInterfacePreferenceEnum.N_A;//(byte)AlternateInterfacePreference;
                Value[6] = FieldOffRequest;
                Value[7] = RemovalTimeout;

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                Status = (Kernel2OutcomeStatusEnum)GetEnum(typeof(Kernel2OutcomeStatusEnum), Value[0]);
                Start = (Kernel2StartEnum)GetEnum(typeof(Kernel2StartEnum), Value[1]);
                OnlineResponseData = Kernel2OnlineResponseDataEnum.N_A;// (Kernel1OnlineResponseDataEnum)GetEnum(typeof(Kernel1OnlineResponseDataEnum), Value[2]);
                CVM = (KernelCVMEnum)GetEnum(typeof(KernelCVMEnum), Value[3]);

                UIRequestOnOutcomePresent = Formatting.GetBitPosition(Value[4], 8); // ((Value[3] >> 8) & 0x01) == 0x01 ? true : false;
                UIRequestOnRestartPresent = Formatting.GetBitPosition(Value[4], 7);//((Value[3] >> 7) & 0x01) == 0x01 ? true : false;
                DataRecordPresent = Formatting.GetBitPosition(Value[4], 6); // ((Value[3] >> 6) & 0x01) == 0x01 ? true : false;
                DiscretionaryDataPresent = Formatting.GetBitPosition(Value[4], 5);//((Value[3] >> 5) & 0x01) == 0x01 ? true : false;
                Receipt = Formatting.GetBitPosition(Value[4], 4);//((Value[3] >> 4) & 0x01) == 0x01 ? true : false;

                AlternateInterfacePreference = Kernel2AlternativeInterfacePreferenceEnum.N_A;// (Kernel1AlternativeInterfacePreferenceEnum)GetEnum(typeof(Kernel1AlternativeInterfacePreferenceEnum), Value[5]);
                FieldOffRequest = Value[6];
                RemovalTimeout = Value[7];

                return pos;
            }
        }

        public new OUTCOME_PARAMETER_SET_DF8129_KRN2_VALUE Value { get { return (OUTCOME_PARAMETER_SET_DF8129_KRN2_VALUE)Val; } }
        public OUTCOME_PARAMETER_SET_DF8129_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.OUTCOME_PARAMETER_SET_DF8129_KRN2, 
                  new OUTCOME_PARAMETER_SET_DF8129_KRN2_VALUE(EMVTagsEnum.OUTCOME_PARAMETER_SET_DF8129_KRN2.DataFormatter))
        {
        }

        public OUTCOME_PARAMETER_SET_DF8129_KRN2(TLV tlv)
            : base(tlv, EMVTagsEnum.OUTCOME_PARAMETER_SET_DF8129_KRN2,
                  new OUTCOME_PARAMETER_SET_DF8129_KRN2_VALUE(EMVTagsEnum.OUTCOME_PARAMETER_SET_DF8129_KRN2.DataFormatter))
        {
        }

        public override void UpdateDB()
        {
            base.UpdateDB();
        }

        public override string ToPrintString(ref int depth)
        {
            StringBuilder sb = new StringBuilder();
            string tagName = TLVMetaDataSourceSingleton.Instance.DataSource.GetName(Tag.TagLable);

            string formatter = "{0,-75}";

            sb.AppendLine(string.Format(formatter, Tag.ToString() + " " + tagName + " L:[" + Val.GetLength().ToString() + "]"));
            sb.AppendLine("V:[");
            sb.AppendLine("\tStatus->" + Value.Status);
            sb.AppendLine("\tStart->" + Value.Start);
            sb.AppendLine("\tOnlineResponseData->" + Value.OnlineResponseData);
            sb.AppendLine("\tCVM->" + Value.CVM);

            sb.AppendLine("\tUIRequestOnOutcomePresent->" + Value.UIRequestOnOutcomePresent);
            sb.AppendLine("\tUIRequestOnRestartPresent->" + Value.UIRequestOnRestartPresent);
            sb.AppendLine("\tDataRecordPresent->" + Value.DataRecordPresent);
            sb.AppendLine("\tDiscretionaryDataPresent->" + Value.DiscretionaryDataPresent);
            sb.AppendLine("\tReceipt->" + Value.Receipt);

            sb.AppendLine("\tAlternateInterfacePreference->" + Value.AlternateInterfacePreference);
            sb.AppendLine("\tFieldOffRequest->" + Formatting.ByteArrayToHexString(new byte[] { Value.FieldOffRequest }));
            sb.AppendLine("\tRemovalTimeout->" + Formatting.ByteArrayToHexString(new byte[]{ Value.RemovalTimeout}));
            sb.AppendLine("]");
            return sb.ToString();
        }
    }
}

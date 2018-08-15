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
using System;
using System.Collections.Generic;
using System.Linq;
using DCEMV.TLVProtocol;
using DCEMV.FormattingUtils;
using DataFormatters;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public class PhoneMessageTableEntry_DF8131
    {
        public byte[] PCIIMask { get; set; }
        public byte[] PCIIValue { get; set; }
        public KernelMessageidentifierEnum MessageIdentifier { get; set; }
        public KernelStatusEnum Status { get; set; }

        public PhoneMessageTableEntry_DF8131(byte[] PCIIMask, byte[] PCIIValue, KernelMessageidentifierEnum MessageIdentifier, KernelStatusEnum Status)
        {
            this.PCIIMask = PCIIMask;
            this.PCIIValue = PCIIValue;
            this.MessageIdentifier = MessageIdentifier;
            this.Status = Status;
        }

        public byte[] serialize()
        {
            byte[] result = new byte[8];
            result[0] = PCIIMask[0];
            result[1] = PCIIMask[1];
            result[2] = PCIIMask[2];
            result[3] = PCIIValue[0];
            result[4] = PCIIValue[1];
            result[5] = PCIIValue[2];
            result[6] = (byte)MessageIdentifier;
            result[7] = (byte)Status;
            return result;
        }

        public int deserialize(byte[] rawTlv, int pos)
        {
            PCIIMask[0] = rawTlv[0];
            PCIIMask[1] = rawTlv[1];
            PCIIMask[2] = rawTlv[2];
            PCIIValue[0] = rawTlv[3];
            PCIIValue[1] = rawTlv[4];
            PCIIValue[2] = rawTlv[5];
            MessageIdentifier = (KernelMessageidentifierEnum)GetEnum(typeof(KernelMessageidentifierEnum), rawTlv[6]);
            Status = (KernelStatusEnum)GetEnum(typeof(KernelStatusEnum), rawTlv[7]);

            pos = pos + 8;
            return pos;
        }

        public static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }

    public class PHONE_MESSAGE_TABLE_DF8131_KRN2 : SmartTag
    {
        public class PHONE_MESSAGE_TABLE_DF8131_KRN2_VALUE : SmartValue
        {
            public PHONE_MESSAGE_TABLE_DF8131_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {
                Entries = new List<PhoneMessageTableEntry_DF8131>();
            }

            public List<PhoneMessageTableEntry_DF8131> Entries { get; set; }

            public override byte[] Serialize()
            {
                List<byte[]> result = new List<byte[]>();

                foreach (PhoneMessageTableEntry_DF8131 pte in Entries)
                    result.Add(pte.serialize());

                Value = result.SelectMany(x=>x).ToArray();

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                int numberOfEntries = Value.Length / 8;
                int valueCounter = 0;
                for (int i = 0; i < numberOfEntries; i++)
                {
                    byte[] PCIIMask = new byte[3];
                    byte[] PCIIValue = new byte[3];
                    KernelMessageidentifierEnum MessageIdentifier;
                    KernelStatusEnum Status;

                    Array.Copy(Value, valueCounter + 0, PCIIMask, 0, 3);
                    Array.Copy(Value, valueCounter + 3, PCIIValue, 0, 3);
                    valueCounter = valueCounter + 6;
                    MessageIdentifier = (KernelMessageidentifierEnum)GetEnum(typeof(KernelMessageidentifierEnum), Value[valueCounter]);
                    valueCounter = valueCounter + 1;
                    Status = (KernelStatusEnum)GetEnum(typeof(KernelStatusEnum), Value[valueCounter]);
                    valueCounter = valueCounter + 1;

                    Entries.Add(new PhoneMessageTableEntry_DF8131(PCIIMask, PCIIValue, MessageIdentifier, Status));
                }
                return pos;
            }

        }

        public new PHONE_MESSAGE_TABLE_DF8131_KRN2_VALUE Value { get { return (PHONE_MESSAGE_TABLE_DF8131_KRN2_VALUE)Val; } }


        public PHONE_MESSAGE_TABLE_DF8131_KRN2(TLV tlv)
            : base(tlv, EMVTagsEnum.PHONE_MESSAGE_TABLE_DF8131_KRN2,
                  new PHONE_MESSAGE_TABLE_DF8131_KRN2_VALUE(EMVTagsEnum.PHONE_MESSAGE_TABLE_DF8131_KRN2.DataFormatter))
        {
        }

        public PHONE_MESSAGE_TABLE_DF8131_KRN2()
            : base(EMVTagsEnum.PHONE_MESSAGE_TABLE_DF8131_KRN2,
                  new PHONE_MESSAGE_TABLE_DF8131_KRN2_VALUE(EMVTagsEnum.PHONE_MESSAGE_TABLE_DF8131_KRN2.DataFormatter))
        {
            BuildDefaults();
        }

        public void BuildDefaults()
        {
            List<PhoneMessageTableEntry_DF8131> lpe = new List<PhoneMessageTableEntry_DF8131>();
            lpe.Add(new PhoneMessageTableEntry_DF8131(Formatting.HexStringToByteArray("000800"), Formatting.HexStringToByteArray("000800"), KernelMessageidentifierEnum.SEE_PHONE, KernelStatusEnum.NOT_READY));
            lpe.Add(new PhoneMessageTableEntry_DF8131(Formatting.HexStringToByteArray("000400"), Formatting.HexStringToByteArray("000400"), KernelMessageidentifierEnum.SEE_PHONE, KernelStatusEnum.NOT_READY));
            lpe.Add(new PhoneMessageTableEntry_DF8131(Formatting.HexStringToByteArray("000100"), Formatting.HexStringToByteArray("000100"), KernelMessageidentifierEnum.SEE_PHONE, KernelStatusEnum.NOT_READY));
            lpe.Add(new PhoneMessageTableEntry_DF8131(Formatting.HexStringToByteArray("000200"), Formatting.HexStringToByteArray("000200"), KernelMessageidentifierEnum.SEE_PHONE, KernelStatusEnum.NOT_READY));
            lpe.Add(new PhoneMessageTableEntry_DF8131(Formatting.HexStringToByteArray("000000"), Formatting.HexStringToByteArray("000000"), KernelMessageidentifierEnum.DECLINED, KernelStatusEnum.NOT_READY));
            Value.Entries = lpe;
            this.Serialize();
        }
    }
}

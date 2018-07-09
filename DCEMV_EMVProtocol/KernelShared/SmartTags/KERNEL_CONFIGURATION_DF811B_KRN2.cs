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
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{
    public class KERNEL_CONFIGURATION_DF811B_KRN2 : SmartTag
    {
        public class KERNEL_CONFIGURATION_DF811B_KRN2_VALUE : SmartValue
        {
            public KERNEL_CONFIGURATION_DF811B_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }
            public bool MagStripeModeContactlessTransactionsNotSupported { get; set; }
            public bool EMVModeContactlessTransactionsNotSupported { get; set; }
            public bool OnDeviceCardholderVerificationSupported { get; set; }
            public bool RelayResistanceProtocolSupported { get; set; }

            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], MagStripeModeContactlessTransactionsNotSupported, 8);
                Formatting.SetBitPosition(ref Value[0], EMVModeContactlessTransactionsNotSupported, 7);
                Formatting.SetBitPosition(ref Value[0], OnDeviceCardholderVerificationSupported, 6);
                Formatting.SetBitPosition(ref Value[0], RelayResistanceProtocolSupported, 5);

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                MagStripeModeContactlessTransactionsNotSupported = Formatting.GetBitPosition(Value[0], 8);
                EMVModeContactlessTransactionsNotSupported = Formatting.GetBitPosition(Value[0], 7);
                OnDeviceCardholderVerificationSupported = Formatting.GetBitPosition(Value[0], 6);
                RelayResistanceProtocolSupported = Formatting.GetBitPosition(Value[0], 5);

                return pos;
            }
        }

        public new KERNEL_CONFIGURATION_DF811B_KRN2_VALUE Value { get { return (KERNEL_CONFIGURATION_DF811B_KRN2_VALUE)Val; } }
        public KERNEL_CONFIGURATION_DF811B_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.KERNEL_CONFIGURATION_DF811B_KRN2, 
                  new KERNEL_CONFIGURATION_DF811B_KRN2_VALUE(EMVTagsEnum.KERNEL_CONFIGURATION_DF811B_KRN2.DataFormatter))
        {
        }

        public KERNEL_CONFIGURATION_DF811B_KRN2(TLV tlv)
            : base(tlv, EMVTagsEnum.KERNEL_CONFIGURATION_DF811B_KRN2,
                  new KERNEL_CONFIGURATION_DF811B_KRN2_VALUE(EMVTagsEnum.KERNEL_CONFIGURATION_DF811B_KRN2.DataFormatter))
        {
        }

        public KERNEL_CONFIGURATION_DF811B_KRN2()
            : base(EMVTagsEnum.KERNEL_CONFIGURATION_DF811B_KRN2,
                  new KERNEL_CONFIGURATION_DF811B_KRN2_VALUE(EMVTagsEnum.KERNEL_CONFIGURATION_DF811B_KRN2.DataFormatter))
        {
        }
    }
}

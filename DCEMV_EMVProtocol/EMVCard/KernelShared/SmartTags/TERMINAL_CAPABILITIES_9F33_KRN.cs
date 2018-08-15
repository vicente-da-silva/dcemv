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
using DCEMV.EMVProtocol.Kernels.K2;
using DCEMV.FormattingUtils;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{
    public class TERMINAL_CAPABILITIES_9F33_KRN : SmartTag
    {
        public class TERMINAL_CAPABILITIES_9F33_KRN_VALUE : SmartValue
        {
            public TERMINAL_CAPABILITIES_9F33_KRN_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public bool ManualKeyEntryCapable { get; set; }
            public bool MagneticStripeCapable { get; set; }
            public bool ICWithContactsCapable { get; set; }


            public bool PlainTextPINforICCVerificationCapable { get; set; }
            public bool EncipheredPINForOnlineVerificationCapable { get; set; }
            public bool SignaturePaperCapable { get; set; }
            public bool EncipheredPINForOfflineVerificationCapable { get; set; }
            public bool NoCVMRequiredCapable { get; set; }


            public bool SDACapable { get; set; }
            public bool DDACapable { get; set; }
            public bool CardCaptureCapable { get; set; }
            public bool RFUCapable { get; set; }
            public bool CDACapable { get; set; }

            public void SetCardDataInputCapabilityValue(CARD_DATA_INPUT_CAPABILITY_DF8117_KRN2 cdicv)
            {
                Value[0] = cdicv.Value.Serialize()[0];
            }

            public void SetSecurityCapabilityValue(SECURITY_CAPABILITY_DF811F_KRN2 scv)
            {
                Value[2] = scv.Value.Serialize()[0];
            }

            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], ManualKeyEntryCapable, 8);
                Formatting.SetBitPosition(ref Value[0], MagneticStripeCapable, 7);
                Formatting.SetBitPosition(ref Value[0], ICWithContactsCapable, 6);

                Formatting.SetBitPosition(ref Value[1], PlainTextPINforICCVerificationCapable, 8);
                Formatting.SetBitPosition(ref Value[1], EncipheredPINForOnlineVerificationCapable, 7);
                Formatting.SetBitPosition(ref Value[1], SignaturePaperCapable, 6);
                Formatting.SetBitPosition(ref Value[1], EncipheredPINForOfflineVerificationCapable, 5);
                Formatting.SetBitPosition(ref Value[1], NoCVMRequiredCapable, 4);

                Formatting.SetBitPosition(ref Value[2], SDACapable, 8);
                Formatting.SetBitPosition(ref Value[2], DDACapable, 7);
                Formatting.SetBitPosition(ref Value[2], CardCaptureCapable, 6);
                Formatting.SetBitPosition(ref Value[2], RFUCapable, 5);
                Formatting.SetBitPosition(ref Value[2], CDACapable, 4);

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                ManualKeyEntryCapable = Formatting.GetBitPosition(Value[0], 8);
                MagneticStripeCapable = Formatting.GetBitPosition(Value[0], 7);
                ICWithContactsCapable = Formatting.GetBitPosition(Value[0], 6);

                PlainTextPINforICCVerificationCapable = Formatting.GetBitPosition(Value[1], 8);
                EncipheredPINForOnlineVerificationCapable = Formatting.GetBitPosition(Value[1], 7);
                SignaturePaperCapable = Formatting.GetBitPosition(Value[1], 6);
                EncipheredPINForOfflineVerificationCapable = Formatting.GetBitPosition(Value[1], 5);
                NoCVMRequiredCapable = Formatting.GetBitPosition(Value[1], 4);

                SDACapable = Formatting.GetBitPosition(Value[2], 8);
                DDACapable = Formatting.GetBitPosition(Value[2], 7);
                CardCaptureCapable = Formatting.GetBitPosition(Value[2], 6);
                RFUCapable = Formatting.GetBitPosition(Value[2], 5);
                CDACapable = Formatting.GetBitPosition(Value[2], 4);

                return pos;
            }
        }
        public new TERMINAL_CAPABILITIES_9F33_KRN_VALUE Value { get { return (TERMINAL_CAPABILITIES_9F33_KRN_VALUE)Val; } }

        public TERMINAL_CAPABILITIES_9F33_KRN(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.TERMINAL_CAPABILITIES_9F33_KRN, 
                  new TERMINAL_CAPABILITIES_9F33_KRN_VALUE(EMVTagsEnum.TERMINAL_CAPABILITIES_9F33_KRN.DataFormatter))
        {
        }
        public TERMINAL_CAPABILITIES_9F33_KRN(TLV tlv)
            : base(tlv, EMVTagsEnum.TERMINAL_CAPABILITIES_9F33_KRN,
                  new TERMINAL_CAPABILITIES_9F33_KRN_VALUE(EMVTagsEnum.TERMINAL_CAPABILITIES_9F33_KRN.DataFormatter))
        {
        }

        public TERMINAL_CAPABILITIES_9F33_KRN()
            : base(EMVTagsEnum.TERMINAL_CAPABILITIES_9F33_KRN,
                  new TERMINAL_CAPABILITIES_9F33_KRN_VALUE(EMVTagsEnum.TERMINAL_CAPABILITIES_9F33_KRN.DataFormatter))
        {
        }
    }
}

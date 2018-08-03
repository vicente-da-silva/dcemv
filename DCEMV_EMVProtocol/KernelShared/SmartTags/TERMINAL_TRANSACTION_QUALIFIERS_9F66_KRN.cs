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
using System.Collections.Generic;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{
    public class BitDescription
    {
        public string DescriptionForSet { get; }
        public string DescriptionForNotSet { get; }

        public BitDescription(string descriptionForSet, string descriptionForNotSet)
        {
            DescriptionForSet = descriptionForSet;
            DescriptionForNotSet = descriptionForNotSet;
        }
    }
    public class TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN : SmartTag
    {
        public class TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN_VALUE : SmartValue
        {
            public bool MagStripeModeSupported { get; set; }
            public bool EMVModeSupported { get; set; }
            public bool EMVContactChipSupported { get; set; }
            public bool OfflineOnlyReader { get; set; }
            public bool OnlinePINSupported { get; set; }
            public bool SignatureSupported { get; set; }
            public bool OfflineDataAuthenticationForOnlineAuthorizationsSupported { get; set; }
            public bool OnlineCryptogramRequired { get; set; }
            public bool CVMRequired { get; set; }
            public bool ContactChipOfflinePINSupported { get; set; }
            public bool IssuerUpdateProcessingSupported { get; set; }
            public bool ConsumerDeviceCVMSupported { get; set; }

            public TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], MagStripeModeSupported, 8);
                Formatting.SetBitPosition(ref Value[0], EMVModeSupported, 6);
                Formatting.SetBitPosition(ref Value[0], EMVContactChipSupported, 5);
                Formatting.SetBitPosition(ref Value[0], OfflineOnlyReader, 4);
                Formatting.SetBitPosition(ref Value[0], OnlinePINSupported, 3);
                Formatting.SetBitPosition(ref Value[0], SignatureSupported, 2);
                Formatting.SetBitPosition(ref Value[0], OfflineDataAuthenticationForOnlineAuthorizationsSupported, 1);

                Formatting.SetBitPosition(ref Value[1], OnlineCryptogramRequired, 8);
                Formatting.SetBitPosition(ref Value[1], CVMRequired, 7);
                Formatting.SetBitPosition(ref Value[1], ContactChipOfflinePINSupported, 6);

                Formatting.SetBitPosition(ref Value[2], IssuerUpdateProcessingSupported, 8);
                Formatting.SetBitPosition(ref Value[2], ConsumerDeviceCVMSupported, 7);

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                MagStripeModeSupported = Formatting.GetBitPosition(Value[0], 8);
                EMVModeSupported = Formatting.GetBitPosition(Value[0], 6);
                EMVContactChipSupported = Formatting.GetBitPosition(Value[0], 5);
                OfflineOnlyReader = Formatting.GetBitPosition(Value[0], 4);
                OnlinePINSupported = Formatting.GetBitPosition(Value[0], 3);
                SignatureSupported = Formatting.GetBitPosition(Value[0], 2);
                OfflineDataAuthenticationForOnlineAuthorizationsSupported = Formatting.GetBitPosition(Value[0], 1);

                OnlineCryptogramRequired = Formatting.GetBitPosition(Value[1], 8);
                CVMRequired = Formatting.GetBitPosition(Value[1], 7);
                ContactChipOfflinePINSupported = Formatting.GetBitPosition(Value[1], 6);

                IssuerUpdateProcessingSupported = Formatting.GetBitPosition(Value[2], 8);
                ConsumerDeviceCVMSupported = Formatting.GetBitPosition(Value[2], 7);

                return pos;
            }

            private static BitDescription[] FirstByteDescriptions = {
                                            new BitDescription("Offline Data Authentication for Online Authorizations supported","Offline Data Authentication for Online Authorizations not supported"),
                                            new BitDescription("Signature supported","Signature not supported"),
                                            new BitDescription("Online PIN supported","Online PIN not supported"),
                                            new BitDescription("Offline-only reader","Online capable reader"),
                                            new BitDescription("EMV contact chip supported","EMV contact chip not supported"),
                                            new BitDescription("EMV mode supported","EMV mode not supported"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("Mag-stripe mode supported","Mag-stripe mode not supported") };

            private static BitDescription[] SecondByteDescriptions = {
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("(Contact Chip) Offline PIN supported"," (Contact Chip) Offline PIN not supported"),
                                            new BitDescription("CVM required","CVM not required"),
                                            new BitDescription("Online cryptogram required"," Online cryptogram not required") };

            private static BitDescription[] ThirdByteDescriptions = {
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("Consumer Device CVM supported","Consumer Device CVM not supported"),
                                            new BitDescription("Issuer Update Processing supported","Issuer Update Processing not supported") };

            private static BitDescription[] FourthByteDescriptions = {
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU") };

            private static List<string> GetInterpretation(byte b, BitDescription[] descriptions)
            {
                List<string> result = new List<string>();
                for (int i = 0; i < 8; i++)
                    if (Formatting.IsBitSet(b, i)) //bit 0 is lsb, bit 7 is msb
                        result.Add(descriptions[i].DescriptionForSet);
                    else
                        result.Add(descriptions[i].DescriptionForNotSet);

                return result;
            }

            public List<string> GetInterpretationFistByte()
            {
                return GetInterpretation(Value[3], FirstByteDescriptions);
            }

            public List<string> GetInterpretationSecondByte()
            {
                return GetInterpretation(Value[2], SecondByteDescriptions);
            }

            public List<string> GetInterpretationThirdByte()
            {
                return GetInterpretation(Value[1], ThirdByteDescriptions);
            }

            public List<string> GetInterpretationFourthByte()
            {
                return GetInterpretation(Value[0], FourthByteDescriptions);
            }
        }

        public override TLV Clone()
        {
            TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN ttq = new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN();
            ttq.Val.Deserialize(Val.Serialize(), 0);
            return ttq;
        }

        public new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN_VALUE Value { get { return (TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN_VALUE)Val; } }

        public TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN, 
                  new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN_VALUE(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN.DataFormatter))
        {

        }

        public TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN(TLV tlv)
            : base(tlv, EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN,
                  new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN_VALUE(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN.DataFormatter))
        {

        }


        public TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN()
            : base(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN,
                  new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN_VALUE(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN.DataFormatter))
        {

        }
    }

}

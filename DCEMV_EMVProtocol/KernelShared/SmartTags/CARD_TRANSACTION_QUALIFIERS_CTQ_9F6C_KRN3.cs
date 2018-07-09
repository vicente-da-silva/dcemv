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
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{

    public class CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3 : SmartTag
    {
        public class CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3_VALUE : SmartValue
        {
            public bool OnlinePINRequired { get; set; }
            public bool SignatureRequired { get; set; }
            public bool GoOnlineIfOfflineDataAuthenticationFailsAndReaderIsOnlineCapable { get; set; }
            public bool SwitchInterfaceIfOfflineDataAuthenticationFailsAndReaderSupportsVIS { get; set; }
            public bool GoOnlineIfApplicationExpired { get; set; }
            public bool SwitchInterfaceForCashTransactions { get; set; }
            public bool SwitchInterfaceForCashBackTransactions { get; set; }
            public bool ValidForContactlessATMTransactions { get; set; }

            public bool ConsumerDeviceCVMPerformed { get; set; }
            public bool CardSupportsIssuerUpdateProcessingAtThePOS { get; set; }

            public CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], OnlinePINRequired, 8);
                Formatting.SetBitPosition(ref Value[0], SignatureRequired, 7);
                Formatting.SetBitPosition(ref Value[0], GoOnlineIfOfflineDataAuthenticationFailsAndReaderIsOnlineCapable, 6);
                Formatting.SetBitPosition(ref Value[0], SwitchInterfaceIfOfflineDataAuthenticationFailsAndReaderSupportsVIS, 5);
                Formatting.SetBitPosition(ref Value[0], GoOnlineIfApplicationExpired, 4);
                Formatting.SetBitPosition(ref Value[0], SwitchInterfaceForCashTransactions, 3);
                Formatting.SetBitPosition(ref Value[0], SwitchInterfaceForCashBackTransactions, 2);
                Formatting.SetBitPosition(ref Value[0], ValidForContactlessATMTransactions, 1);
                

                Formatting.SetBitPosition(ref Value[1], ConsumerDeviceCVMPerformed, 8);
                Formatting.SetBitPosition(ref Value[1], CardSupportsIssuerUpdateProcessingAtThePOS, 7);

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                OnlinePINRequired = Formatting.GetBitPosition(Value[0], 8);
                SignatureRequired = Formatting.GetBitPosition(Value[0], 7);
                GoOnlineIfOfflineDataAuthenticationFailsAndReaderIsOnlineCapable = Formatting.GetBitPosition(Value[0], 6);
                SwitchInterfaceIfOfflineDataAuthenticationFailsAndReaderSupportsVIS = Formatting.GetBitPosition(Value[0], 5);
                GoOnlineIfApplicationExpired = Formatting.GetBitPosition(Value[0], 4);
                SwitchInterfaceForCashTransactions = Formatting.GetBitPosition(Value[0], 3);
                SwitchInterfaceForCashBackTransactions = Formatting.GetBitPosition(Value[0], 2);
                ValidForContactlessATMTransactions = Formatting.GetBitPosition(Value[0], 1);

                SwitchInterfaceForCashBackTransactions = Formatting.GetBitPosition(Value[1], 8);
                CardSupportsIssuerUpdateProcessingAtThePOS = Formatting.GetBitPosition(Value[1], 7);

                return pos;
            }
        }

        public override TLV Clone()
        {
            byte[] TTQCopy = new byte[4];
            Array.Copy(Value.Value, TTQCopy, TTQCopy.Length);
            TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN ttq = new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN();
            ttq.Value.Value = TTQCopy;
            return ttq;
        }

        public new CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3_VALUE Value { get { return (CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3_VALUE)Val; } }

        public CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3, 
                  new CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3_VALUE(EMVTagsEnum.CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3.DataFormatter))
        {

        }

        public CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3()
            : base(EMVTagsEnum.CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3,
                  new CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3_VALUE(EMVTagsEnum.CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3.DataFormatter))
        {

        }
    }

}

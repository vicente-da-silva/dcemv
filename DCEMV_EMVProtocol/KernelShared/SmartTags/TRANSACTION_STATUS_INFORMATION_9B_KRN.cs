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

namespace DCEMV.EMVProtocol.Kernels
{
    public class TRANSACTION_STATUS_INFORMATION_9B_KRN : SmartTag
    {
        public class TRANSACTION_STATUS_INFORMATION_9B_KRN_VALUE : SmartValue
        {
            public bool OfflineDataAuthenticationWasPerformed { get; set; }
            public bool CardholderVerificationWasPerformed { get; set; }
            public bool CardRiskManagementWasPerformed { get; set; }
            public bool IssuerAuthenticationWasPerformed { get; set; }
            public bool TerminalRiskmanagementWasPerformed { get; set; }
            public bool ScriptProcessingWasPerformed { get; set; }

            public TRANSACTION_STATUS_INFORMATION_9B_KRN_VALUE(DataFormatterBase dataFormatter)
                : base(dataFormatter)
            {

            }

            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], OfflineDataAuthenticationWasPerformed, 8);
                Formatting.SetBitPosition(ref Value[0], CardholderVerificationWasPerformed, 7);
                Formatting.SetBitPosition(ref Value[0], CardRiskManagementWasPerformed, 6);
                Formatting.SetBitPosition(ref Value[0], IssuerAuthenticationWasPerformed, 5);
                Formatting.SetBitPosition(ref Value[0], TerminalRiskmanagementWasPerformed, 4);
                Formatting.SetBitPosition(ref Value[0], ScriptProcessingWasPerformed, 3);
                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                OfflineDataAuthenticationWasPerformed = Formatting.GetBitPosition(Value[0], 8);
                CardholderVerificationWasPerformed = Formatting.GetBitPosition(Value[0], 7);
                CardRiskManagementWasPerformed = Formatting.GetBitPosition(Value[0], 6);
                IssuerAuthenticationWasPerformed = Formatting.GetBitPosition(Value[0], 5);
                TerminalRiskmanagementWasPerformed = Formatting.GetBitPosition(Value[0], 4);
                ScriptProcessingWasPerformed = Formatting.GetBitPosition(Value[0], 3);
                
                return pos;
            }
        }

        public new TRANSACTION_STATUS_INFORMATION_9B_KRN_VALUE Value { get { return (TRANSACTION_STATUS_INFORMATION_9B_KRN_VALUE)Val; } }
        public TRANSACTION_STATUS_INFORMATION_9B_KRN(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.TRANSACTION_STATUS_INFORMATION_9B_KRN,
                  new TRANSACTION_STATUS_INFORMATION_9B_KRN_VALUE(EMVTagsEnum.TRANSACTION_STATUS_INFORMATION_9B_KRN.DataFormatter))
        {
        }
    }
}

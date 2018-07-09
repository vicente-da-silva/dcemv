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
    public class APPLICATION_INTERCHANGE_PROFILE_82_KRN : SmartTag
    {
        public class APPLICATION_INTERCHANGE_PROFILE_82_KRN_VALUE : SmartValue
        {
            public APPLICATION_INTERCHANGE_PROFILE_82_KRN_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public bool SDASupported { get; protected set; }
            public bool DDAsupported { get; protected set; }
            public bool CardholderVerificationIsSupported { get; protected set; }
            public bool TerminalRiskManagementIsToBePerformed { get; protected set; }
            public bool IssuerAuthenticationIsSupported { get; protected set; }
            public bool OnDeviceCardholderVerificationIsSupported { get; protected set; }
            public bool CDASupported { get; protected set; }

            public bool EMVModeIsSupported { get; protected set; }
            public bool RelayResistanceProtocolIsSupported { get; protected set; }

            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], SDASupported, 7);
                Formatting.SetBitPosition(ref Value[0], DDAsupported, 6);
                Formatting.SetBitPosition(ref Value[0], CardholderVerificationIsSupported, 5);
                Formatting.SetBitPosition(ref Value[0], TerminalRiskManagementIsToBePerformed, 4);
                Formatting.SetBitPosition(ref Value[0], IssuerAuthenticationIsSupported, 3);
                Formatting.SetBitPosition(ref Value[0], OnDeviceCardholderVerificationIsSupported, 2);
                Formatting.SetBitPosition(ref Value[0], CDASupported, 1);

                Formatting.SetBitPosition(ref Value[1], EMVModeIsSupported, 8);
                Formatting.SetBitPosition(ref Value[1], RelayResistanceProtocolIsSupported, 1);

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                SDASupported = Formatting.GetBitPosition(Value[0], 7);
                DDAsupported = Formatting.GetBitPosition(Value[0], 6);
                CardholderVerificationIsSupported = Formatting.GetBitPosition(Value[0], 5);
                TerminalRiskManagementIsToBePerformed = Formatting.GetBitPosition(Value[0], 4);
                IssuerAuthenticationIsSupported = Formatting.GetBitPosition(Value[0], 3);
                OnDeviceCardholderVerificationIsSupported = Formatting.GetBitPosition(Value[0], 2);
                CDASupported = Formatting.GetBitPosition(Value[0], 1);

                EMVModeIsSupported = Formatting.GetBitPosition(Value[1], 8);
                RelayResistanceProtocolIsSupported = Formatting.GetBitPosition(Value[1], 1);

                return pos;
            }
        }

        public new APPLICATION_INTERCHANGE_PROFILE_82_KRN_VALUE Value { get { return (APPLICATION_INTERCHANGE_PROFILE_82_KRN_VALUE)Val; } }
        public APPLICATION_INTERCHANGE_PROFILE_82_KRN(KernelDatabaseBase database)
            :base(database, EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN, 
                 new APPLICATION_INTERCHANGE_PROFILE_82_KRN_VALUE(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.DataFormatter))
        {
        }
    }
}

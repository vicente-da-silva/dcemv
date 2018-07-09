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

namespace DCEMV.EMVProtocol.Kernels
{
    public class TERMINAL_TYPE_9F35_KRN : SmartTag
    {
        public enum AttendedUnattended
        {
            Attended,
            Unattended,
        }
        public enum OnlineOffline
        {
            OnlineOnly,
            OfflineWithOnlineCapability,
            OfflineOnly,
        }
        public enum OpsControl
        {
            FinancialInstitution,
            Merchant,
            CardHolder,
        }

        public class TerminalType
        {
            public AttendedUnattended AttendedUnattended { get; }
            public OnlineOffline OnlineOffline { get; }
            public OpsControl OpsControl { get; }
            public byte Code { get; }

            public TerminalType(byte Code)
            {
                this.Code = Code;
                switch (Code)
                {
                    case 0x11:
                        AttendedUnattended = AttendedUnattended.Attended;
                        OnlineOffline = OnlineOffline.OnlineOnly;
                        OpsControl = OpsControl.FinancialInstitution;
                        break;

                    case 0x12:
                        AttendedUnattended = AttendedUnattended.Attended;
                        OnlineOffline = OnlineOffline.OfflineWithOnlineCapability;
                        OpsControl = OpsControl.FinancialInstitution;
                        break;

                    case 0x13:
                        AttendedUnattended = AttendedUnattended.Attended;
                        OpsControl = OpsControl.FinancialInstitution;
                        OnlineOffline = OnlineOffline.OfflineOnly;
                        break;

                    case 0x14:
                        AttendedUnattended = AttendedUnattended.Attended;
                        OnlineOffline = OnlineOffline.OnlineOnly;
                        OpsControl = OpsControl.FinancialInstitution;
                        break;

                    case 0x15:
                        AttendedUnattended = AttendedUnattended.Attended;
                        OnlineOffline = OnlineOffline.OfflineWithOnlineCapability;
                        OpsControl = OpsControl.FinancialInstitution;
                        break;

                    case 0x16:
                        AttendedUnattended = AttendedUnattended.Attended;
                        OnlineOffline = OnlineOffline.OfflineOnly;
                        OpsControl = OpsControl.FinancialInstitution;
                        break;

                    case 0x21:
                        AttendedUnattended = AttendedUnattended.Attended;
                        OnlineOffline = OnlineOffline.OnlineOnly;
                        OpsControl = OpsControl.Merchant;
                        break;

                    case 0x22:
                        AttendedUnattended = AttendedUnattended.Attended;
                        OnlineOffline = OnlineOffline.OfflineWithOnlineCapability;
                        OpsControl = OpsControl.Merchant;
                        break;

                    case 0x23:
                        AttendedUnattended = AttendedUnattended.Attended;
                        OnlineOffline = OnlineOffline.OfflineOnly;
                        OpsControl = OpsControl.Merchant;
                        break;

                    case 0x24:
                        AttendedUnattended = AttendedUnattended.Unattended;
                        OnlineOffline = OnlineOffline.OnlineOnly;
                        OpsControl = OpsControl.Merchant;
                        break;

                    case 0x25:
                        AttendedUnattended = AttendedUnattended.Unattended;
                        OnlineOffline = OnlineOffline.OfflineWithOnlineCapability;
                        OpsControl = OpsControl.Merchant;
                        break;

                    case 0x26:
                        AttendedUnattended = AttendedUnattended.Unattended;
                        OnlineOffline = OnlineOffline.OfflineOnly;
                        OpsControl = OpsControl.Merchant;
                        break;

                    case 0x34:
                        AttendedUnattended = AttendedUnattended.Unattended;
                        OnlineOffline = OnlineOffline.OnlineOnly;
                        OpsControl = OpsControl.CardHolder;
                        break;

                    case 0x35:
                        AttendedUnattended = AttendedUnattended.Unattended;
                        OnlineOffline = OnlineOffline.OfflineWithOnlineCapability;
                        OpsControl = OpsControl.CardHolder;
                        break;

                    case 0x36:
                        AttendedUnattended = AttendedUnattended.Unattended;
                        OnlineOffline = OnlineOffline.OfflineOnly;
                        OpsControl = OpsControl.CardHolder;
                        break;

                    default:
                        throw new EMVProtocolException("Invalid Terminal Type Code:" + Code);
                }
            }
        }

        public class TERMINAL_TYPE_9F35_KRN_VALUE : SmartValue
        {

            public TERMINAL_TYPE_9F35_KRN_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public TerminalType TerminalType { get; protected set; }

            public override byte[] Serialize()
            {
                Value = new byte[] { TerminalType.Code };

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                if(Value.Length > 0)
                    TerminalType = new TerminalType(Value[0]);

                return pos;
            }
        }
        public new TERMINAL_TYPE_9F35_KRN_VALUE Value { get { return (TERMINAL_TYPE_9F35_KRN_VALUE)Val; } }

        public TERMINAL_TYPE_9F35_KRN(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.TERMINAL_TYPE_9F35_KRN, 
                  new TERMINAL_TYPE_9F35_KRN_VALUE(EMVTagsEnum.TERMINAL_TYPE_9F35_KRN.DataFormatter))
        {
            
        }
    }
}

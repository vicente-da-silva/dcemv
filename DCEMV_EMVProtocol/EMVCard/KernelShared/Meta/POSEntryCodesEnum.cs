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
using DCEMV.Shared;
using System.Collections.Generic;

namespace DCEMV.EMVProtocol.Kernels
{

    public static class POSEntryCodesEnum
    {
        public static List<EnumBase> EnumList = new List<EnumBase>();

        public static EMVPOSEntryCode Unknown = new EMVPOSEntryCode(0x00);
        public static EMVPOSEntryCode ManualKeyEntry = new EMVPOSEntryCode(0x01);
        public static EMVPOSEntryCode MagneticStripeRead = new EMVPOSEntryCode(0x02);
        public static EMVPOSEntryCode BarcodeRead = new EMVPOSEntryCode(0x03);
        public static EMVPOSEntryCode RFU1 = new EMVPOSEntryCode(0x04);
        public static EMVPOSEntryCode ICCReadCVVPossible = new EMVPOSEntryCode(0x05);
        public static EMVPOSEntryCode RFU2 = new EMVPOSEntryCode(0x06);
        public static EMVPOSEntryCode ContactlessPaymentVSDCRules = new EMVPOSEntryCode(0x07);
        public static EMVPOSEntryCode CompleteMagneticStripeRead = new EMVPOSEntryCode(0x90);
        public static EMVPOSEntryCode ContactlessPaymentMagneticStripeRules = new EMVPOSEntryCode(0x91);
        public static EMVPOSEntryCode ICCReadCVVNotPossible = new EMVPOSEntryCode(0x95);

        static POSEntryCodesEnum()
        {
            EnumList.Add(Unknown);
            EnumList.Add(ManualKeyEntry);
            EnumList.Add(MagneticStripeRead);
            EnumList.Add(BarcodeRead);
            EnumList.Add(RFU1);
            EnumList.Add(ICCReadCVVPossible);
            EnumList.Add(RFU2);
            EnumList.Add(ContactlessPaymentVSDCRules);
            EnumList.Add(CompleteMagneticStripeRead);
            EnumList.Add(ContactlessPaymentMagneticStripeRules);
            EnumList.Add(ICCReadCVVNotPossible);
        }
    }

    public class EMVPOSEntryCode : EnumBase
    {
        public EMVPOSEntryCode(int code)
        {
            this.Code = code;
        }
    }
}

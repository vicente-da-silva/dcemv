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
using DCEMV.EMVProtocol.Kernels;
using System;
using System.Collections.Generic;

namespace DCEMV.EMVProtocol.Contactless
{
    public class AIDKernelID
    {
        public RIDEnum RIDEnum { get; set; }
        public byte KernelID { get; set; }
    }

    public static class AIDKernelIDMapDefaults
    {
        private static List<AIDKernelID> data = new List<AIDKernelID>();

        public static AIDKernelID AmericanExpress = new AIDKernelID() { RIDEnum = RIDEnum.A000000025, KernelID = 0x04 };
        public static AIDKernelID Discover = new AIDKernelID() { RIDEnum = RIDEnum.A000000324, KernelID = 0x06 };
        public static AIDKernelID JCB = new AIDKernelID() { RIDEnum = RIDEnum.A000000065, KernelID = 0x05 };
        public static AIDKernelID MasterCard = new AIDKernelID() { RIDEnum = RIDEnum.A000000004, KernelID = 0x02 };
        public static AIDKernelID UnionPay = new AIDKernelID() { RIDEnum = RIDEnum.A000000333, KernelID = 0x07 };
        public static AIDKernelID Visa = new AIDKernelID() { RIDEnum = RIDEnum.A000000003, KernelID = 0x03 };
        public static AIDKernelID Other = new AIDKernelID() { RIDEnum = RIDEnum.None, KernelID = 0x00 };

        static AIDKernelIDMapDefaults()
        {
            data.Add(AmericanExpress);
            data.Add(Discover);
            data.Add(JCB);
            data.Add(MasterCard);
            data.Add(UnionPay);
            data.Add(Visa);
            data.Add(Other);
        }

        public static AIDKernelID FindEntry(string rid)
        {
            foreach (AIDKernelID k in data)
                if (Enum.GetName(typeof(RIDEnum), k.RIDEnum) == rid)
                    return k;
            return Other;
        }
    }
}

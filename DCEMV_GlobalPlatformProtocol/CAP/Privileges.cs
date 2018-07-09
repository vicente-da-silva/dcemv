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

namespace DCEMV.GlobalPlatformProtocol
{
    public class Privileges
    {
        private List<Privilege> privs = new List<Privilege>();

        public Privileges set(params Privilege[] privs)
        {
            Privileges p = new Privileges();
            foreach (Privilege pv in privs)
            {
                p.add(pv);
            }
            return p;
        }

        public static List<Privilege> getEnumList()
        {
            List<Privilege> result = new List<Privilege>();
            result.Add(Privilege.SecurityDomain);
            result.Add(Privilege.DAPVerification);
            result.Add(Privilege.DelegatedManagement);
            result.Add(Privilege.CardLock);
            result.Add(Privilege.CardTerminate);
            result.Add(Privilege.CardReset);
            result.Add(Privilege.CVMManagement);
            result.Add(Privilege.MandatedDAPVerification);
            result.Add(Privilege.TrustedPath);
            result.Add(Privilege.AuthorizedManagement);
            result.Add(Privilege.TokenVerification);
            result.Add(Privilege.GlobalDelete);
            result.Add(Privilege.GlobalLock);
            result.Add(Privilege.GlobalRegistry);
            result.Add(Privilege.FinalApplication);
            result.Add(Privilege.GlobalService);
            result.Add(Privilege.ReceiptGeneration);
            result.Add(Privilege.CipheredLoadFileDataBlock);
            result.Add(Privilege.ContactlessActivation);
            result.Add(Privilege.ContactlessSelfActivation);

            return result;
        }

        // TODO: implement GP 2.2 table 6.2
        // TODO: bitmasks as symbolics, KAT tests
        // See GP 2.2.1 Tables 11-7, 11-8, 11-9
        // See GP 2.1.1 Table 9-7 (matches 2.2 Table 11-7)
        public static Privileges fromBytes(byte[] data)
        {
            if (data.Length != 1 && data.Length != 3)
            {
                throw new Exception("Privileges must be encoded on 1 or 3 bytes");
            }
            Privileges p = new Privileges();
            // Process first byte
            int b1 = data[0] & 0xFF;
            if ((b1 & 0x80) == 0x80)
            {
                p.privs.Add(Privilege.SecurityDomain);
            }
            if ((b1 & 0xC1) == 0xC0)
            {
                p.privs.Add(Privilege.DAPVerification);
            }
            if ((b1 & 0xA0) == 0xA0)
            {
                p.privs.Add(Privilege.DelegatedManagement);
            }
            if ((b1 & 0x10) == 0x10)
            {
                p.privs.Add(Privilege.CardLock);
            }
            if ((b1 & 0x8) == 0x8)
            {
                p.privs.Add(Privilege.CardTerminate);
            }
            if ((b1 & 0x4) == 0x4)
            {
                p.privs.Add(Privilege.CardReset);
            }
            if ((b1 & 0x2) == 0x2)
            {
                p.privs.Add(Privilege.CVMManagement);
            }
            if ((b1 & 0xC1) == 0xC1)
            {
                p.privs.Add(Privilege.MandatedDAPVerification);
            }
            if (data.Length > 1)
            {
                int b2 = data[1] & 0xFF;
                if ((b2 & 0x80) == 0x80)
                {
                    p.privs.Add(Privilege.TrustedPath);
                }
                if ((b2 & 0x40) == 0x40)
                {
                    p.privs.Add(Privilege.AuthorizedManagement);
                }
                if ((b2 & 0x20) == 0x20)
                {
                    p.privs.Add(Privilege.TokenVerification); // XXX: mismatch in spec
                }
                if ((b2 & 0x10) == 0x10)
                {
                    p.privs.Add(Privilege.GlobalDelete);
                }
                if ((b2 & 0x8) == 0x8)
                {
                    p.privs.Add(Privilege.GlobalLock);
                }
                if ((b2 & 0x4) == 0x4)
                {
                    p.privs.Add(Privilege.GlobalRegistry);
                }
                if ((b2 & 0x2) == 0x2)
                {
                    p.privs.Add(Privilege.FinalApplication);
                }
                if ((b2 & 0x1) == 0x1)
                {
                    p.privs.Add(Privilege.GlobalService);
                }
                int b3 = data[2] & 0xFF;
                if ((b3 & 0x80) == 0x80)
                {
                    p.privs.Add(Privilege.ReceiptGeneration);
                }
                if ((b3 & 0x40) == 0x40)
                {
                    p.privs.Add(Privilege.CipheredLoadFileDataBlock);
                }
                if ((b3 & 0x20) == 0x20)
                {
                    p.privs.Add(Privilege.ContactlessActivation);
                }
                if ((b3 & 0x10) == 0x10)
                {
                    p.privs.Add(Privilege.ContactlessSelfActivation);
                }
                if ((b3 & 0xF) != 0x0)
                {
                    // RFU
                    throw new Exception("RFU bits set in privileges!");
                }
            }
            return p;
        }

        public static Privileges fromByte(byte b)
        {
            return fromBytes(new byte[] { b });
        }

        public byte[] toBytes()
        {
            List<Privilege> p = new List<Privilege>(privs);
            int b1 = 0x00;
            if (p.Remove(Privilege.SecurityDomain))
            {
                b1 |= 0x80;
            }
            if (p.Remove(Privilege.DAPVerification))
            {
                b1 |= 0xC0;
            }
            if (p.Remove(Privilege.DelegatedManagement))
            {
                b1 |= 0xA0;
            }
            if (p.Remove(Privilege.CardLock))
            {
                b1 |= 0x10;
            }
            if (p.Remove(Privilege.CardTerminate))
            {
                b1 |= 0x8;
            }
            if (p.Remove(Privilege.CardReset))
            {
                b1 |= 0x4;
            }
            if (p.Remove(Privilege.CVMManagement))
            {
                b1 |= 0x2;
            }
            if (p.Remove(Privilege.MandatedDAPVerification))
            {
                b1 |= 0xC1;
            }

            // Fits in one byte
            if (p.Count == 0)
            {
                return new byte[] { (byte)(b1 & 0xFF) };
            }

            // Second
            int b2 = 0x00;
            if (p.Remove(Privilege.TrustedPath))
            {
                b2 |= 0x80;
            }
            if (p.Remove(Privilege.AuthorizedManagement))
            {
                b2 |= 0x40;
            }
            if (p.Remove(Privilege.TokenVerification))
            {
                b2 |= 0x20;
            }
            if (p.Remove(Privilege.GlobalDelete))
            {
                b2 |= 0x10;
            }
            if (p.Remove(Privilege.GlobalLock))
            {
                b2 |= 0x8;
            }
            if (p.Remove(Privilege.GlobalRegistry))
            {
                b2 |= 0x4;
            }
            if (p.Remove(Privilege.FinalApplication))
            {
                b2 |= 0x2;
            }
            if (p.Remove(Privilege.GlobalService))
            {
                b2 |= 0x1;
            }

            // Third
            int b3 = 0x00;
            if (p.Remove(Privilege.ReceiptGeneration))
            {
                b3 |= 0x80;
            }
            if (p.Remove(Privilege.CipheredLoadFileDataBlock))
            {
                b3 |= 0x40;
            }
            if (p.Remove(Privilege.ContactlessActivation))
            {
                b3 |= 0x20;
            }
            if (p.Remove(Privilege.ContactlessSelfActivation))
            {
                b3 |= 0x10;
            }
            return new byte[] { (byte)(b1 & 0xFF), (byte)(b2 & 0xFF), (byte)(b3 & 0xFF) };
        }

        public byte toByte()
        {
            byte[] bytes = toBytes();
            if (bytes.Length == 1)
                return bytes[0];
            throw new Exception("This privileges set can not be encoded in one byte");
        }

        public override String ToString()
        {
            return String.Join(", ", privs);
        }
        public bool has(Privilege p)
        {
            return privs.Contains(p);
        }
        public void add(Privilege p)
        {
            privs.Add(p);
        }
        public bool isEmpty()
        {
            return privs.Count == 0;
        }
    }
}

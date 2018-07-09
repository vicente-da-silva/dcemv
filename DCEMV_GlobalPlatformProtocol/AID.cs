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
using DCEMV.FormattingUtils;
using Org.BouncyCastle.Utilities;
using System;

namespace DCEMV.GlobalPlatformProtocol
{
    public class AID
    {
        private byte[] aidBytes = null;

        public AID(byte[] bytes)
            : this(bytes, 0, bytes.Length)
        {

        }

        public AID(String str)
            : this(Formatting.HexStringToByteArray(str))
        {

        }

        public AID(byte[] bytes, int offset, int length)
        {
            if ((length < 5) || (length > 16))
            {
                throw new Exception("AID's are between 5 and 16 bytes, not " + Formatting.ByteArrayToHexString(BitConverter.GetBytes(length)));
            }
            aidBytes = new byte[length];
            Array.Copy(bytes, offset, aidBytes, 0, length);
        }

        public byte[] getBytes()
        {
            return aidBytes;
        }

        public int getLength()
        {
            return aidBytes.Length;
        }

        public override String ToString()
        {
            return Formatting.ByteArrayToHexString(aidBytes);
        }

        public int hashCode()
        {
            return Arrays.GetHashCode(aidBytes);
        }

        public bool equals(Object o)
        {
            if (o is AID)
            {
                return Arrays.AreEqual(((AID)o).aidBytes, aidBytes);
            }
            return false;
        }
    }
}

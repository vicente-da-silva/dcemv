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
    public abstract class SCPWrapper
    {
        protected int blockSize = 0;
        protected GPKeySet sessionKeys = null;
        protected bool mac = false;
        protected bool enc = false;
        protected bool rmac = false;

        public virtual void SetSecurityLevel(List<APDUMode> securityLevel)
        {
            mac = securityLevel.Contains(APDUMode.MAC);
            enc = securityLevel.Contains(APDUMode.ENC);
            rmac = securityLevel.Contains(APDUMode.RMAC);
        }

        public abstract byte[] Wrap(GPCommand command);
        public abstract byte[] Unwrap(GPResponse response);
        private static byte[] Pad80(byte[] text, int offset, int length, int blocksize)
        {
            if (length == -1)
            {
                length = text.Length - offset;
            }
            int totalLength = length;
            for (totalLength++; (totalLength % blocksize) != 0; totalLength++)
            {
                ;
            }
            int padlength = totalLength - length;
            byte[] result = new byte[totalLength];
            Array.Copy(text, offset, result, 0, length);
            result[length] = (byte)0x80;
            for (int i = 1; i < padlength; i++)
            {
                result[length + i] = (byte)0x00;
            }
            return result;
        }
        protected static byte[] Pad80(byte[] text, int blocksize)
        {
            return Pad80(text, 0, text.Length, blocksize);
        }
        public int getBlockSize()
        {
            int res = this.blockSize;
            if (mac)
                res = res - 8;
            if (enc)
                res = res - 8;
            return res;
        }
    }
}

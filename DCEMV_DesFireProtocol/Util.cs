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
using System.Collections;

namespace DCEMV.DesFireProtocol
{
    public class Util
    {
        public static byte ConvertToByte(BitArray bits)
        {
            if (bits.Length != 8)
            {
                throw new ArgumentException("illegal number of bits");
            }

            byte b = 0;
            if (bits.Get(7)) b++;
            if (bits.Get(6)) b += 2;
            if (bits.Get(5)) b += 4;
            if (bits.Get(4)) b += 8;
            if (bits.Get(3)) b += 16;
            if (bits.Get(2)) b += 32;
            if (bits.Get(1)) b += 64;
            if (bits.Get(0)) b += 128;
            return b;
        }
    }
}

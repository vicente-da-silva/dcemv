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

namespace DCEMV.FormattingUtils
{
    public class ByteArrayToHexUtil
    {
        private static readonly uint[] _lookup32 = CreateLookup32();

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("X2");
                result[i] = ((uint)s[0]) + ((uint)s[1] << 16);
            }
            return result;
        }

        public static string ByteArrayToHexViaLookup32(byte[] bytes)
        {
            var lookup32 = _lookup32;
            var result = new char[bytes.Length * 8 + (bytes.Length - 1)];
            int pos = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = lookup32[bytes[i]];
                result[pos] = '0';
                pos = pos + 1;
                result[pos] = 'x';
                pos = pos + 1;
                result[pos] = (char)val;
                pos = pos + 1;
                result[pos] = (char)(val >> 16);
                pos = pos + 1;
                result[pos] = '[';

                if (i > 99) //TODO: enhance this
                {
                    pos = pos + 1;
                    result[pos] = '.';
                    pos = pos + 1;
                    result[pos] = '.';
                }
                else
                {
                    if (i < 10)
                    {
                        pos = pos + 1;
                        result[pos] = ' ';
                        pos = pos + 1;
                        result[pos] = Convert.ToString(i)[0];
                    }
                    else
                    {
                        String iString = Convert.ToString(i);
                        pos = pos + 1;
                        result[pos] = iString[0];
                        pos = pos + 1;
                        result[pos] = iString[1];
                    }

                }

                pos = pos + 1;
                result[pos] = ']';


                if (result.Length > pos + 1)
                {
                    pos = pos + 1;
                    result[pos] = ' ';
                    pos = pos + 1;
                }
            }
            return new string(result);
        }
    }
}

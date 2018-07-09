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
using Org.BouncyCastle.Utilities;
using System;

namespace DCEMV.EMVSecurity
{
    public class Util
    {
        public static void AdjustDESParity(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                bytes[i] = (byte)(b & 0xfe | (b >> 1 ^ b >> 2 ^ b >> 3 ^ b >> 4 ^ b >> 5 ^ b >> 6 ^ b >> 7 ^ 0x01) & 0x01);
            }
        }

        public static bool IsDESParityAdjusted(byte[] bytes)
        {
            byte[] correct = Arrays.Clone(bytes);
            AdjustDESParity(correct);
            return Arrays.AreEqual(bytes, correct);
        }

        public static byte[] Trim(byte[] array, int length)
        {
            byte[] trimmedArray = new byte[length];
            Array.Copy(array, 0, trimmedArray, 0, length);
            return trimmedArray;
        }
    }
}

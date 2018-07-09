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
using System.Linq;
using System.Text;

namespace DCEMV.FormattingUtils
{
    public class Formatting
    {
        public static ulong ConvertToInt64(byte[] value)
        {
            int numberByteLEngth = 8;
            //assumption is that all byte arrays passed in to this method are big endian as
            //emv data byte arrays are big endian
            bool le = BitConverter.IsLittleEndian;
            if (value.Length > 8)
                throw new Exception("Cannot convert To Int64");

            byte[] newVal = new byte[numberByteLEngth];
            if (value.Length < numberByteLEngth)
            {
                if (le)
                    Array.Copy(value.Reverse().ToArray(), 0, newVal, 0, value.Length);
                else
                    Array.Copy(value, 0, newVal, 0, value.Length);

                return BitConverter.ToUInt64(newVal, 0);
            }
            else
            {
                if (le)
                    return BitConverter.ToUInt64(value.Reverse().ToArray(), 0);
                else
                    return BitConverter.ToUInt64(value, 0);
            }
        }
        public static uint ConvertToInt32(byte[] value)
        {
            int numberByteLEngth = 4;
            //assumption is that all byte arrays passed in to this method are big endian as
            //emv data byte arrays are big endian
            bool le = BitConverter.IsLittleEndian;
            if (value.Length > 8)
                throw new Exception("Cannot convert To Int32");

            byte[] newVal = new byte[numberByteLEngth];
            if (value.Length < numberByteLEngth)
            {
                if (le)
                    Array.Copy(value.Reverse().ToArray(), 0, newVal, 0, value.Length);
                else
                    Array.Copy(value, 0, newVal, 0, value.Length);

                return BitConverter.ToUInt32(newVal, 0);
            }
            else
            {
                if (le)
                    return BitConverter.ToUInt32(value.Reverse().ToArray(), 0);
                else
                    return BitConverter.ToUInt32(value, 0);
            }
        }
        public static ushort ConvertToInt16(byte[] value)
        {
            int numberByteLEngth = 2;
            //assumption is that all byte arrays passed in to this method are big endian as
            //emv data byte arrays are big endian
            bool le = BitConverter.IsLittleEndian;
            if (value.Length > 8)
                throw new Exception("Cannot convert To Int16");

            byte[] newVal = new byte[numberByteLEngth];
            if (value.Length < numberByteLEngth)
            {
                if (le)
                    Array.Copy(value.Reverse().ToArray(), 0, newVal, 0, value.Length);
                else
                    Array.Copy(value, 0, newVal, 0, value.Length);

                return BitConverter.ToUInt16(newVal, 0);
            }
            else
            {
                if (le)
                    return BitConverter.ToUInt16(value.Reverse().ToArray(), 0);
                else
                    return BitConverter.ToUInt16(value, 0);
            }
        }

        #region Hex Conversions
        public static string ByteArrayToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }
        public static byte[] HexStringToByteArray(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return new byte[0];

            if (hex.Length % 2 != 0)
                hex = '0' + hex;

            return Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
        }
        #endregion

        #region ASCII Conversions
        public static string ByteArrayToASCIIString(byte[] bytes)
        {
            return UTF8Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
        public static byte[] ASCIIStringToByteArray(string value)
        {
            return UTF8Encoding.UTF8.GetBytes(value);
        }
        public static byte[] ASCIIStringToByteArray(string value, int totalWidth, char padChar)
        {
            return ASCIIStringToByteArray(value.PadLeft(totalWidth, padChar));
        }
        #endregion

        #region Bit Manipulation
        public static bool IsBitSet(byte b, int index)
        {
            return ((b >> index) & 0x01) == 0x01;
        }
        public static bool GetBitPosition(byte input, int pos)
        {
            return ((input >> pos - 1) & 0x01) == 0x01 ? true : false;
        }
        public static void SetBitPosition(ref byte input, bool set, int pos)
        {
            byte b1 = (byte)(0x01 << pos - 1);
            if(set)
                input = (byte)(b1 | input);
            else
                input = (byte)((byte)(0xFF - b1) & input);

        }
        #endregion

        #region Bcd Conversions
        public static byte[] StringToBcd(string input, bool bytePadLeft)
        {
            //Extensions.Fill(buf, fill);
            StringBuilder sb = new StringBuilder();
            if (input.Length % 2 == 1) //odd
            {
                if (bytePadLeft)
                {
                    sb.Append('F');
                    sb.Append(input);
                }
                else
                {
                    sb.Append(input);
                    sb.Append('F');
                }
            }
            else
                sb.Append(input);

            List<byte[]> result = new List<byte[]>();
            for (int i = 0; i< sb.Length;)
            {
                result.Add(HexStringToByteArray("" + sb[i] + sb[i + 1]));
                i = i + 2;
            }
            return result.SelectMany(x => x).ToArray();
        }
        public static byte[] StringToBcd(string input, int outputLength, bool bytePadLeft)
        {
            StringBuilder sb = new StringBuilder();
            if (outputLength < input.Length / 2)
                throw new FormattingException("output length < input length");
            if (outputLength % 2 == 1) //odd\
                throw new FormattingException("output length cannot be odd");

            sb.Append(input.PadLeft(outputLength*2,'0'));

            List<byte[]> result = new List<byte[]>();
            for (int i = 0; i < sb.Length;)
            {
                result.Add(HexStringToByteArray("" + sb[i] + sb[i + 1]));
                i = i + 2;
            }
            return result.SelectMany(x => x).ToArray();
        }
        public static string BcdToString(byte[] input)
        {
            if (input.Length == 0)
                return "";

            int length;//length of output string
            bool paddedLeft = false;
            bool paddedRight = false;

            int val1 = (input[0] & 0xF0) >> 4;
            int val2 = (input[input.Length - 1] & 0x0F);

            if (val1 == 0xF)
                paddedLeft = true;

            if (val2 == 0xF)
                paddedRight = true;

            if (paddedLeft && paddedRight)
                throw new FormattingException("uBcdTostring input value invalid: it is padded left and right");

            length = input.Length * 2;
            if (paddedLeft || paddedRight)
                length--;

            //cannot be paddedLeft and paddedRight, but must be one or the other in odd buffer length scenarios
            //bool paddedLEft only checked in odd buffer length scenarios
            return BcdToString(input, 0, length, paddedLeft);
        }
        private static string BcdToString(byte[] input, int offset, int length, bool padLeft)
        {
            //convert the all the bytes to chars and then check if the str was a odd length, if so
            //check what kind of padding was applied and then remove the appropriate character from the buffer
            int bufpos = offset;
            int strLength = length - (offset * 2);
            StringBuilder b = new StringBuilder(strLength);

            for (int i = bufpos; i < input.Length; i++)
            {
                int val1 = (input[i] & 0xF0) >> 4;
                int val2 = (input[i] & 0x0F);
                char c1 = val1.ToString()[0];
                //if (c1 == 'd') c1 = '=';
                char c2 = val2.ToString()[0];
                //if (c2 == 'd') c2 = '=';
                b.Append(c1.ToString().ToUpper());
                b.Append(c2.ToString().ToUpper());
                bufpos += 2;
            }

            if (strLength % 2 == 1)
            { //if odd number of characters
                if (padLeft & (offset == 0))
                { //ignore padding if offset > 0
                    b.Remove(0, 1); //remove first char
                }
                else if (!padLeft)
                {
                    b.Remove(b.Length - 1, 1); //remove last char
                }
            }

            return b.ToString();
        }
        public static long BcdToLong(byte[] input)
        {
            long.TryParse(BcdToString(input), out long result);
            return result;
        }
        #endregion

        #region Dates
        public static DateTime ConvertStringToDate(string value, string format)
        {
            return DateTime.ParseExact(value, format, System.Globalization.CultureInfo.InvariantCulture);
        }
        public static string ConvertDateToString(DateTime value, string format)
        {
            return  string.Format("{0:" + format + "}", value);
        }
        #endregion

        public static string ByteArrayToBinaryString(byte[] input)
        {
            List<string> result = new List<string>();

            for (int j = 0; j < input.Length; j++)
            {
                for (int i = 7; i >= 0; i--)
                {
                    if (((input[j] >> i) & 0x01) == 0x01)
                        result.Add("1");
                    else
                        result.Add("0");
                }
            }
            StringBuilder sb = new StringBuilder();
            result.ForEach(x => sb.Append(x));
            return sb.ToString();
            //string binaryString = Convert.ToString(ConvertToInt16(input),2);
            //int zeroCount = (input.Length - (binaryString.Length / 8)) * 8;
            //return binaryString.PadLeft(zeroCount, '0');
        }
        public static byte[] ConvertToHexAscii(byte[] input)
        {
            return Formatting.ASCIIStringToByteArray(Formatting.ByteArrayToHexString(input));
        }
        public static byte[] PadArray(byte[] input, int newLength, bool padLeft, byte padChar)
        {
            byte[] result = new byte[newLength];
            for (int i = 0; i < result.Length; i++)
                result[i] = padChar;

            if (padLeft)
                Array.Copy(input, 0, result, result.Length - input.Length ,input.Length);
            else
                Array.Copy(input, 0, result, 0, input.Length);

            return result;
        }
        public static T[] ConcatArrays<T>(params T[][] list)
        {
            T[] result = new T[list.Sum(a => a.Length)];
            int offset = 0;
            for (int x = 0; x < list.Length; x++)
            {
                list[x].CopyTo(result, offset);
                offset += list[x].Length;
            }
            return result;
        }
        public static byte[] ConcatArrays(byte[] array1, int beginIndex1, int length1, byte[] array2, int beginIndex2, int length2)
        {
            byte[] concatArray = new byte[length1 + length2];
            Array.Copy(array1, beginIndex1, concatArray, 0, length1);
            Array.Copy(array2, beginIndex2, concatArray, length1, length2);
            return concatArray;
        }
        public static byte[] copyOfRange(byte[] original, int from, int to)
        {
            int newLength = to - from;
            if (newLength < 0)
                throw new Exception(from + " > " + to);
            byte[] copy = new byte[newLength];
            Array.Copy(original, from, copy, 0, Math.Min(original.Length - from, newLength));
            return copy;
        }
        public static byte[] GetRandomNumber()
        {
            byte[] randomNum = new byte[4];
            Random r = new Random();
            r.NextBytes(randomNum);
            return randomNum.Reverse().ToArray();//change endianess
        }
        public static byte[] GetRandomNumberNumeric(int numberMSBEqual0)
        {
            byte[] randomNum = new byte[4];
            Random r = new Random();
            r.NextBytes(randomNum);
            randomNum = randomNum.Reverse().ToArray();//change endianess
            randomNum[0] = (byte)(randomNum[0] >> numberMSBEqual0);
            return randomNum;
        }
        public static byte[] Xor(byte[] op1, byte[] op2)
        {
            byte[] result;
            // Use the smallest array
            if (op2.Length > op1.Length)
            {
                result = new byte[op1.Length];
            }
            else
            {
                result = new byte[op2.Length];
            }
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (byte)(op1[i] ^ op2[i]);
            }
            return result;
        }
        public static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
        public static void PadToMultipleOf(ref byte[] src, int pad)
        {
            int len = (src.Length + pad - 1) / pad * pad;
            Array.Resize(ref src, len);
        }
        public static byte[] RotateRight(byte[] input)
        {
            return input.Skip(1).Concat(input.Take(1)).ToArray();
        }
        public static byte[] RotateLeft(byte[] input)
        {
            return input.Skip(input.Length - 1).Concat(input.Take(input.Length - 1)).ToArray();
        }
        public static List<byte[]> SplitArray(byte[] array, int blockSize)
        {
            List<byte[]> result = new List<byte[]>();

            int len = array.Length;
            int offset = 0;
            int left = len - offset;
            while (left > 0)
            {
                int currentLen = 0;
                if (left >= blockSize)
                {
                    currentLen = blockSize;
                }
                else
                {
                    currentLen = left;
                }
                byte[] block = new byte[currentLen];
                Array.Copy(array, offset, block, 0, currentLen);
                result.Add(block);
                left -= currentLen;
                offset += currentLen;
            }
            return result;
        }
        public static bool IsNumeric(string s)
        {
            int output;
            return Int32.TryParse(s, out output);
        }
        public static byte[] ToByteArry(char[] input)
        {
            return input.Select(c => (byte)c).ToArray();
        }
    }
}

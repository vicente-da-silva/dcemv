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
using System;

namespace DataFormatters
{
    public enum DataFormats
    {
        _NUMERIC,
        _ALPHA_NUMERIC,
        _ALPHA_NUMERIC_SPECIAL,
        _ALPHA_NUMERIC_SPACE,
        _ALPHA,
        _CN,
        _TIME_HHMMSS,
        _DATE_YYMMDD,
        _BINARY,
        _BINARY_16,
    }

    
    public abstract class DataFormatterBase
    {
        public DataFormats Format { get; }

        public abstract bool CheckLength(byte[] data);

        public abstract int GetMaxLength();

        public DataFormatterBase(DataFormats df)
        {
            Format = df;
        }

        public string ConvertToString(byte[] data)
        {
            switch (Format)
            {
                //case DataFormats.UNKNOWN:
                //case DataFormats.VARIABLE_UNKNOWN:
                case DataFormats._BINARY:
                case DataFormats._BINARY_16:
                    //case DataFormats._BINARY_MULTIPLE_OF_4:
                    //case DataFormats.H:
                    //case DataFormats.A:
                    return Formatting.ByteArrayToHexString(data);

                case DataFormats._CN:
                case DataFormats._DATE_YYMMDD:
                case DataFormats._TIME_HHMMSS:
                case DataFormats._NUMERIC:
                    return Formatting.BcdToString(data);

                case DataFormats._ALPHA:
                case DataFormats._ALPHA_NUMERIC:
                case DataFormats._ALPHA_NUMERIC_SPECIAL:
                case DataFormats._ALPHA_NUMERIC_SPACE:
                    return Formatting.ByteArrayToASCIIString(data);

                default:
                    throw new Exception("Unknown DataFormat:" + Format);
            }
        }

        public bool Validate(byte[] data)
        {
            switch (Format)
            {
                //case DataFormats.UNKNOWN:
                //    throw new Exception("Unknown DataFormat:" + Format);
                //case DataFormats.VARIABLE_UNKNOWN:
                //    throw new Exception("Unknown DataFormat:" + Format);

                case DataFormats._CN:
                case DataFormats._DATE_YYMMDD:
                case DataFormats._TIME_HHMMSS:
                case DataFormats._NUMERIC:
                    string valueNumeric = Formatting.BcdToString(data);
                    for(int i = 0; i< valueNumeric.Length;i++)
                    {
                        if (!Char.IsNumber(valueNumeric, i))
                            return false;
                    }
                    return true;

                case DataFormats._BINARY:
                    return true;

                //case DataFormats._BINARY_MULTIPLE_OF_4:
                //    if (data.Length % 4 != 0)
                //        return false;
                //    else
                //        return true;
                
                case DataFormats._ALPHA:
                    string valueAlpha = Formatting.ByteArrayToASCIIString(data);
                    for (int i = 0; i < valueAlpha.Length; i++)
                    {
                        if (!Char.IsLetter(valueAlpha, i))
                            return false;
                    }
                    return true;

                case DataFormats._ALPHA_NUMERIC:
                    string valueAlphaNumeric = Formatting.ByteArrayToASCIIString(data);
                    for (int i = 0; i < valueAlphaNumeric.Length; i++)
                    {
                        if (!Char.IsLetter(valueAlphaNumeric, i) && !Char.IsNumber(valueAlphaNumeric, i))
                            return false;
                    }
                    return true;

                case DataFormats._ALPHA_NUMERIC_SPECIAL:
                    string valueAlphaNumericSpecial = Formatting.ByteArrayToASCIIString(data);
                    for (int i = 0; i < valueAlphaNumericSpecial.Length; i++)
                    {
                        if (!Char.IsLetter(valueAlphaNumericSpecial, i) && 
                            !Char.IsNumber(valueAlphaNumericSpecial, i) &&
                            !Char.IsWhiteSpace(valueAlphaNumericSpecial, i) &&
                            !Char.IsPunctuation(valueAlphaNumericSpecial, i) &&
                            !Char.IsControl(valueAlphaNumericSpecial, i) &&
                            !Char.IsSymbol(valueAlphaNumericSpecial, i))
                            return false;
                    }
                    return true;

                case DataFormats._ALPHA_NUMERIC_SPACE:
                    string valueAlphaNumericSpace = Formatting.ByteArrayToASCIIString(data);
                    for (int i = 0; i < valueAlphaNumericSpace.Length; i++)
                    {
                        if (!Char.IsLetter(valueAlphaNumericSpace, i) && 
                            !Char.IsNumber(valueAlphaNumericSpace, i) &&
                            !Char.IsWhiteSpace(valueAlphaNumericSpace, i))
                            return false;
                    }
                    return true;

                //case DataFormats.H:
                //    throw new Exception("Unknown DataFormat:" + Format);
                //case DataFormats.A:
                //    throw new Exception("Unknown DataFormat:" + Format);

                default:
                    throw new Exception("Unknown DataFormat:" + Format);
            }
        }
    }
    public class DataFormatterLengthList : DataFormatterBase
    {
        private int length1;
        private int length2;

        public DataFormatterLengthList(DataFormats df, int length1, int length2)
            : base(df)
        {
            this.length1 = length1;
            this.length2 = length2;
        }

        public override bool CheckLength(byte[] data)
        {
            if (data.Length != length1 && data.Length != length2)
                return false;
            else
                return true;
        }

        public override int GetMaxLength()
        {
            if (length1 > length2)
                return length1;
            else
                return length2;
        }
    }

    public class DataFormatterFixedOrRange : DataFormatterBase
    {
        private int fixedVal;
        private int min;
        private int max;

        public DataFormatterFixedOrRange(DataFormats df, int fixedVal, int min, int max)
            : base(df)
        {
            this.fixedVal = fixedVal;
            this.min = min;
            this.max = max;
        }

        public override bool CheckLength(byte[] data)
        {
            if (data.Length == fixedVal || (data.Length <= min && data.Length >= max))
                return true;
            else
                return false;
        }

        public override int GetMaxLength()
        {
            if (fixedVal > max)
                return fixedVal;
            else
                return max;
        }
    }

    public class DataFormatterLengthRange : DataFormatterBase
    {
        public int Min { get; }
        public int Max { get; }

        public DataFormatterLengthRange(DataFormats df, int min, int max)
            : base(df)
        {
            this.Min = min;
            this.Max = max;
        }

        public override bool CheckLength(byte[] data)
        {
            if (Min == -1 && Max == -1)
                return true;

            if (data.Length < Min || data.Length > Max)
                return false;
            else
                return true;
        }

        public override int GetMaxLength()
        {
            return Max;
        }
    }
    public class DataFormatterLengthRangeMultiple : DataFormatterLengthRange
    {
        public int Multiple { get; }

        public DataFormatterLengthRangeMultiple(DataFormats df, int min, int max, int multipe)
            : base(df,min,max)
        {
            this.Multiple = multipe;
        }
    }
    public class DataFormatterLengthFixed : DataFormatterBase
    {
        public int Max { get; }

        public DataFormatterLengthFixed(DataFormats df, int max)
            : base(df)
        {
            this.Max = max;
        }

        public override bool CheckLength(byte[] data)
        {
            if (data.Length != Max)
                return false;
            else
                return true;
        }
        public override int GetMaxLength()
        {
            return Max;
        }
    }
}

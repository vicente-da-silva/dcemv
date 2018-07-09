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
using DataFormatters;
using DCEMV.FormattingUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DCEMV.TLVProtocol
{
    public enum TagTypeEnum
    {
        Universal = 0x00,
        Application = 0x01,
        Context = 0x02,
        Private = 0x03,
    }

    public abstract class TLVElement
    {
        public abstract int Deserialize(byte[] rawTlv, int pos);
        public abstract byte[] Serialize();

        protected static byte[] TrimEnd(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0x00);
            Array.Resize(ref array, lastIndex + 1);
            return array;
        }
        protected static byte[] TrimStart(byte[] array)
        {
            int index = Array.FindIndex(array, b => b != 0x00);
            byte[] result = new byte[array.Length - index];
            Array.Copy(array, index, result, 0, array.Length - index);
            return result;
        }
        public static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }

    /*
     * https://en.wikipedia.org/wiki/X.690
     */
    public class TLV : TLVElement
    {
        private TLVList children;

        public class T : TLVElement
        {
            public int TagNumber { get; protected set; }
            public bool IsConstructed { get; protected set; }
            public TagTypeEnum TagType { get; protected set; }
            public string TagLable { get; protected set; }
            public string TagName { get; protected set; }

            internal T()
            {

            }

            public T(string tagLabel)
            {
                Deserialize(Formatting.HexStringToByteArray(tagLabel), 0);
            }

            public static T Create(byte[] rawTLV, ref int pos)
            {
                T t = new T();
                pos = t.Deserialize(rawTLV, pos);
                return t;
            }

            //Octet 1                           Octet 2
            //8 7           6       5 4 3 2 1   8       7 6 5 4 3 2 1
            //Tag class     P/C     Tag Number  More    Tag Number
            public override int Deserialize(byte[] rawTlv, int pos)
            {
                IsConstructed = (rawTlv[pos] & 0x20) != 0;
                TagType = (TagTypeEnum)GetEnum(typeof(TagTypeEnum), (rawTlv[pos] & 0xC0) >> 6);

                int start = pos;
                //universal class, primitive, tage field continues on one or more subsequent bytes 
                bool moreBytes = (rawTlv[pos] & 0x1F) == 0x1F;
                //each subsequent byte has bit 7 set to 1, unless it is the last subsequent byte
                while (moreBytes && (rawTlv[++pos] & 0x80) != 0) ;
                pos++;//unless it is the last subsequent byte

                if (moreBytes)
                {
                    int tagLength = pos - start;
                    byte[] tagNumberBytes = new byte[4];
                    int byteCounter = 1;
                    for (var i = start + 1; i < tagLength + start; i++) //skip first byte
                    {
                        tagNumberBytes[4 - byteCounter] = rawTlv[i]; //msb first in rawTlv, so reverse array
                        byteCounter++;
                    }
                    tagNumberBytes = TrimStart(tagNumberBytes);
                    byte[] padded = new byte[4];
                    Array.Copy(tagNumberBytes, 0, padded, 0, tagNumberBytes.Length);
                    TagNumber = BitConverter.ToInt32(padded, 0) + 30;
                    byte[] tagLabelBytes = new byte[tagLength];
                    Array.Copy(rawTlv, start, tagLabelBytes, 0, tagLength);
                    TagLable = Formatting.ByteArrayToHexString(tagLabelBytes);
                }
                else
                {
                    TagNumber = rawTlv[start] & 0x1F; //single byte tag
                    TagLable = Formatting.ByteArrayToHexString(new byte[] { rawTlv[start] });
                }
                return pos;
            }

            public override byte[] Serialize()
            {
                if (TagNumber == 0) //if tag number not precalculated, calculate it now
                {
                    byte[] tagAsBytes = Formatting.HexStringToByteArray(TagLable);
                    Deserialize(tagAsBytes, 0);
                }
                List<byte[]> result = new List<byte[]>();
                if (TagNumber > 30)
                {
                    result.Add(new byte[] { 0x1F });
                    byte[] tagBytes = BitConverter.GetBytes(TagNumber - 30).ToArray();
                    tagBytes = TrimEnd(tagBytes);
                    result.Add(tagBytes.Reverse().ToArray()); //reverse so msb first in array
                }
                else
                    result.Add(new byte[] { BitConverter.GetBytes(TagNumber)[0] });

                if (IsConstructed)
                    result[0][0] = (byte)(result[0][0] | 0x20);

                result[0][0] = (byte)(result[0][0] | (byte)TagType << 6);

                return result.SelectMany(a => a).ToArray();
            }

            public override string ToString()
            {
                return string.Format("T:[{0}]", TagLable);
            }
        }
        public class V : TLVElement
        {
            internal class L : TLVElement
            {
                public int Value { get; set; }

                public L(int length)
                {
                    Value = length;
                }

                public override int Deserialize(byte[] rawTlv, int pos)
                {
                    // parse Length
                    //if more than 1 byte, length has bit 7 set to 1 and then the number of bytes the length byte is
                    bool multiByteLength = (rawTlv[pos] & 0x80) != 0;
                    Value = multiByteLength ? GetLengthInt(rawTlv, pos) : rawTlv[pos]; //get length of data
                    pos = multiByteLength ? pos + (rawTlv[pos] & 0x7F) + 1 : pos + 1; //increment i by length of length bytes
                    return pos;
                }

                public int DeserializeNonMultiByte(byte[] rawTlv, int pos)
                {
                    Value = rawTlv[pos]; //get length of data
                    pos = pos + 1; //increment i by length of length bytes
                    return pos;
                }

                public override byte[] Serialize()
                {
                    List<byte[]> result = new List<byte[]>();
                    if (Value > 127)
                    {
                        byte[] lengthBytes = BitConverter.GetBytes(Value).Reverse().ToArray();
                        lengthBytes = TrimStart(lengthBytes);
                        result.Add(new byte[] { (byte)(0x80 | lengthBytes.Length) });
                        result.Add(lengthBytes);
                    }
                    else
                        result.Add(new byte[] { BitConverter.GetBytes(Value)[0] });

                    return result.SelectMany(a => a).ToArray();
                }

                private static int GetLengthInt(byte[] data, int offset)
                {
                    var result = 0;
                    int lengthVal = data[offset] & 0x7F;

                    for (var i = 0; i < lengthVal; i++)
                    {
                        result = (result << 8) | data[offset + i + 1]; //+1 skip first byte
                    }

                    return result;
                }

                public override string ToString()
                {
                    return string.Format("L:[{0}]", Value); //"L:[" + Length.ToString("X") + "]";
                }
            }

            private byte[] byteValue;

            public byte[] Value
            {
                get
                {
                    return byteValue;
                }
                set
                {
                    byteValue = value;
                    Length.Value = value.Length;
                }
            }
            internal L Length { get; set; }

            public int GetLength()
            {
                return Length.Value;
            }

            public DataFormatterBase DataFormatter { get; }

            internal V(DataFormatterBase dataFormatter)
            {
                this.DataFormatter = dataFormatter;
                Length = new L(0);
                Value = new byte[Length.Value];
            }
            public V(DataFormatterBase dataFormatter, byte[] value)
            {
                this.DataFormatter = dataFormatter;
                //if (this.dataFormatter.GetMaxLength() == -1)
                //    Length = 0;
                //else
                //    Length = this.dataFormatter.GetMaxLength();
                Length = new L(value.Length);
                Value = value;
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                if (rawTlv.Length == 0)
                    return pos;

                pos = Length.Deserialize(rawTlv, pos);
                Value = new byte[Length.Value];
                Array.Copy(rawTlv, pos, Value, 0, Length.Value);
                return pos + Length.Value;
            }

            public override byte[] Serialize()
            {
                if (!DataFormatter.Validate(Value))
                    throw new Exception("Data Format Error");

                List<byte[]> result = new List<byte[]>();
                Length.Value = Value.Length;
                result.Add(Length.Serialize());
                result.Add(Value);
                return result.SelectMany(a => a).ToArray();
            }

            public void PackValue(int length)
            {
                if (byteValue.Length > Length.Value)
                    throw new TLVException("byteValue length > Length.Value");

                if (byteValue.Length < length)
                {
                    switch (DataFormatter.Format)
                    {
                        case DataFormats._NUMERIC:
                        case DataFormats._BINARY:
                        case DataFormats._BINARY_16:
                        case DataFormats._CN:
                        case DataFormats._TIME_HHMMSS:
                        case DataFormats._DATE_YYMMDD:
                            byteValue = Formatting.PadArray(byteValue, length, true, 0x00);
                            break;

                        case DataFormats._ALPHA_NUMERIC:
                        case DataFormats._ALPHA_NUMERIC_SPECIAL:
                        case DataFormats._ALPHA_NUMERIC_SPACE:
                        case DataFormats._ALPHA:
                            byteValue = Formatting.PadArray(byteValue, length, false, Convert.ToByte(' '));
                            break;

                        default:
                            throw new TLVException("Unknown Data Format for Pack: " + DataFormatter.Format);
                    }
                    Value = byteValue;
                }
                //truncate data
                if (byteValue.Length > length)
                {
                    byte[] newByteValue = new byte[length];
                    newByteValue = Formatting.copyOfRange(byteValue, 0, length);
                    Value = newByteValue;
                }

            }

            public override string ToString()
            {
                return string.Format("V:[{0}]", DataFormatter.ConvertToString(Value));
            }

            public bool CheckLength()
            {
                return DataFormatter.CheckLength(Value);
            }
            public bool Validate()
            {
                return DataFormatter.Validate(Value);
            }
        }

        public T Tag { get; protected set; }
        public V Val { get; protected set; }
        
        public TLVList Children
        {
            get
            {
                if (Tag.IsConstructed)
                    return children;
                else
                    throw new Exception("Cannot add child to non constructed tag");
            }
            protected set
            {
                children = value;
            }
        }
        public byte[] Value
        {
            get
            {
                return Val.Value;
            }
            set
            {
                Val.Value = value;
            }
        }

        protected TLV()
        {
            children = new TLVList();
        }

        private TLV(string tag, byte[] value)
        {
            children = new TLVList();
            Tag = new T(tag);
            Val = new V(TLVMetaDataSourceSingleton.Instance.DataSource.GetFormatter(Tag.TagLable), value);
        }
        public static TLV Create(string tagLable, byte[] value)
        {
            TLV tlv = new TLV(tagLable, value);
            if (!tlv.Validate())
                throw new TLVException("Validation failed for tags value, Tag:" + tagLable);
            return tlv;
        }
        public static TLV Create(string tagLable)
        {
            TLV tlv = new TLV(tagLable, new byte[0]);
            if (!tlv.Validate())
                throw new TLVException("Validation failed for tags value, Tag:" + tagLable);
            return tlv;
        }
        public virtual TLV Clone()
        {
            TLV clone = new TLV();
            clone.Deserialize(Serialize(), 0);
            return clone;
        }
        public static TLV Create(byte[] rawTLV, ref int pos)
        {
            TLV tlv = new TLV();
            pos = tlv.Deserialize(rawTLV, pos);
            return tlv;
        }

        public override int Deserialize(byte[] rawTlv, int pos)
        {
            if (rawTlv.Length == 0)
                return 0;

            children = new TLVList();

            //some byte streams may have FF padding between tags ??
            while (rawTlv[pos] == 0xFF)
                pos++;

            Tag = new T();
            pos = Tag.Deserialize(rawTlv, pos);
            Val = new V(TLVMetaDataSourceSingleton.Instance.DataSource.GetFormatter(Tag.TagLable));
            pos = Val.Deserialize(rawTlv, pos);

            if (Tag.IsConstructed)
            {
                for (int i = 0; i < Value.Length;)
                {
                    TLV child = new TLV();
                    i = child.Deserialize(Value, i);
                    //TODO: Dont like this...had to change AddToList to allow duplicates, previously if a item in the list already existed its
                    //value was replaced, some cards may return blocks of 0x00 bytes
                    //in their read record byte streams, this is not valid TLV but we have to cater for it
                    //the TLV class sees a tag of 00 with length of 00 and so on, we add the 00 tags and allow duplicates so that when things 
                    //like static data for authentication is calcaulted the dups are serialized back into the origional
                    //blocks of 0x00, since the card has included this invalid data in its signature!!
                    //this should change, probably include the 0x00 blocks as some sort of custom padding tag, and switch dups back off?
                    Children.AddToList(child, true);
                }
            }
            return pos;
        }
        public static TLVList DeserializeChildrenWithNoV(byte[] rawTlv, int pos)
        {
            TLVList children = new TLVList();
            for (int i = pos; i < rawTlv.Length;)//dont increment i
            {
                TLV child = new TLV()
                {
                    Tag = new T()
                };
                pos = child.Tag.Deserialize(rawTlv, pos);
                child.Val = new V(TLVMetaDataSourceSingleton.Instance.DataSource.GetFormatter(child.Tag.TagLable));
                //pos = child.Val.Length.deserialize(rawTlv, pos);
                pos = child.Val.Length.DeserializeNonMultiByte(rawTlv, pos);
                children.AddToListIncludeDuplicates(child);
                i = pos;
            }
            return children;
        }
        public static TLVList DeserializeChildrenWithNoLV(byte[] rawTlv, int pos)
        {
            TLVList children = new TLVList();
            for (int i = pos; i < rawTlv.Length;)
            {
                T t = T.Create(rawTlv, ref i);
                children.AddToList(TLV.Create(t.TagLable));
            }
            return children;
        }
        
        public override byte[] Serialize()
        {
            List<byte[]> result = new List<byte[]>();
            int length = 0;

            result.Add(Tag.Serialize());
            if (children.Count > 0)
            {
                List<byte[]> resultChildren = new List<byte[]>();
                foreach (TLV tlv in children)
                {
                    byte[] tlvSer = tlv.Serialize();
                    resultChildren.Add(tlvSer);
                    length = length + tlvSer.Length;
                }
                Val.Length.Value = length;
                result.Add(Val.Length.Serialize());
                result.Add(resultChildren.SelectMany(a => a).ToArray());
            }
            else
            {
                result.Add(Val.Serialize());
            }
            return result.SelectMany(a => a).ToArray();
        }
        public virtual string ToPrintString(ref int depth)
        {
            StringBuilder sb = new StringBuilder();
            string tagName = TLVMetaDataSourceSingleton.Instance.DataSource.GetName(Tag.TagLable);

            string formatter = "{0,-75}";

            sb.Append(string.Format(formatter, Tag.ToString() + " " + tagName + " " + Val.Length.ToString()));

            if (children.Count == 0)
            {
                sb.Append(" " + Val.ToString());
            }
            else
            {
                depth++;
                sb.AppendLine(" V:[");
                sb.Append(children.ToPrintString(ref depth));
                sb.Append("]");
                depth--;
            }

            return sb.ToString();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string tagName = TLVMetaDataSourceSingleton.Instance.DataSource.GetName(Tag.TagLable);

            sb.Append(Tag.ToString());
            sb.Append(" ("+tagName+") ");
            sb.Append(Val.Length.ToString());

            if (children.Count == 0)
                sb.Append(" " + Val.ToString());
            else
                sb.Append(" V:[" + children.ToString() + "]");

            return sb.ToString();
        }
        public bool ValidateLength()
        {
            return Val.CheckLength();
        }
        public bool Validate()
        {
            return Val.Validate();
        }
        public void Initialize()
        {
            if (Tag.IsConstructed)
                Children.Initialize();
            else
                Val = new V(Val.DataFormatter);
        }
    }
}

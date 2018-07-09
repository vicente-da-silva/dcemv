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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DCEMV.SPDHProtocol
{
    internal enum FormatEnum
    {
        Numeric,
        Hex,
    }
    internal enum SPDHTransactionTypeEnum
    {
        F00,
        F01,
        F02,
        F03,
        F04,
        F05,
        F06,
        F07,
        F08,
        F11,
        F12,
        F13,
        F14,
    }

    internal class HeaderBase
    {
        public Dictionary<HeaderEntryEnum,HeaderEntryBase> Entries { get; protected set; }

        public HeaderBase()
        {
            Entries = new Dictionary<HeaderEntryEnum, HeaderEntryBase>();
        }

        public virtual byte[] Serialize()
        {
            return Entries.Select(x => x.Value.Serialize()).SelectMany(x => x).ToArray();
        }

        public virtual int Deserialize(byte[] input, int pos)
        {
            foreach (HeaderEntryBase he in Entries.Values)
            {
                pos = he.Deserialize(input, pos);
            }
            return pos;
        }

        public void SetValue(HeaderEntryEnum enumVal, byte[] value)
        {
            Entries.First(x=>x.Key == enumVal).Value.Value = value;
        }
        public byte[] GetValue(HeaderEntryEnum enumVal)
        {
            return Entries.First(x => x.Key == enumVal).Value.Value;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            Entries.Values.ToList().ForEach(x => sb.Append(x.ToString()));
            return sb.ToString();
        }
        public string ToPrintString(ref int depth)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbTab = new StringBuilder();
            for (int j = 0; j <= depth; j++)
                sbTab.Append("\t");

            int count = 0;
            foreach(HeaderEntryBase hb in Entries.Values)
            {
                if(count == Entries.Count - 1)
                    sb.Append(sbTab.ToString() + hb.ToString());
                else
                    sb.AppendLine(sbTab.ToString() + hb.ToString());
                count++;
            }

            return sb.ToString();
        }
    }

    internal class TransactionBase
    {
        public Header Header { get; private set; }
        public SPDHTransactionTypeEnum Type { get; protected set; }
        public string Name { get; protected set; }
        public List<FIDMeta> MandatoryFidIds { get; protected set; }
        public List<FIDBase> Fids { get; protected set; }

        public TransactionBase(SPDHTransactionTypeEnum type, string name)
        {
            Header = new Header();
            MandatoryFidIds = new List<FIDMeta>();
            Fids = new List<FIDBase>();
            Type = type;
            Name = name;

            Header.SetValue(HeaderEntryEnum.DeviceType, Formatting.ASCIIStringToByteArray(SPDHConstants.DialOrLeasedLineTerminalOrNetwork));
            Header.SetValue(HeaderEntryEnum.TransmissionNumber, Formatting.ASCIIStringToByteArray(SPDHConstants.TransmissionNumberNotChecked));

        }
        public TransactionBase(byte[] raw, ref int pos, SPDHTransactionTypeEnum type)
        {
            Header = new Header();
            MandatoryFidIds = new List<FIDMeta>();
            Fids = new List<FIDBase>();
            Type = type;
            Name = type.ToString();
            pos = Deserialize(raw, pos);

            Header.SetValue(HeaderEntryEnum.DeviceType, Formatting.ASCIIStringToByteArray(SPDHConstants.DialOrLeasedLineTerminalOrNetwork));
            Header.SetValue(HeaderEntryEnum.TransmissionNumber, Formatting.ASCIIStringToByteArray(SPDHConstants.TransmissionNumberNotChecked));
        }

        public Optional<FIDBase> FindFid(FIDMeta meta)
        {
            FIDBase fidFound = null;
            foreach (FIDBase fid in Fids)
            {
                if (fid.Children.Count == 0)
                {
                    if (fid.Id == meta.Id && fid.SubId == meta.SubId)
                        fidFound = fid;
                }
                else
                {
                    foreach (FIDBase fidChild in fid.Children)
                    {
                        if (fidChild.Id == meta.Id && fidChild.SubId == meta.SubId)
                            fidFound = fidChild;
                    }
                }
            }
            if(fidFound != null)
                return Optional<FIDBase>.Create(fidFound);
            else
                return Optional<FIDBase>.CreateEmpty();
        }

        public void SetHeaderValues(string transactionType, SPDHMessageType messageType, SPDHMessageSubType subType, DateTime datetime, string terminalID, string employeeId )
        {
            Header.SetValue(HeaderEntryEnum.ProcessingFlag1, Formatting.HexStringToByteArray(""));
            Header.SetValue(HeaderEntryEnum.ProcessingFlag2, Formatting.ASCIIStringToByteArray("5"));
            Header.SetValue(HeaderEntryEnum.ProcessingFlag3, Formatting.HexStringToByteArray(""));
            Header.SetValue(HeaderEntryEnum.ResponseCode, Formatting.HexStringToByteArray(""));

            Header.SetValue(HeaderEntryEnum.TransactionCode, Formatting.ASCIIStringToByteArray(transactionType));
            Header.SetValue(HeaderEntryEnum.CurrentDate, Formatting.ASCIIStringToByteArray(Formatting.ConvertDateToString(datetime, "yyMMdd")));
            Header.SetValue(HeaderEntryEnum.CurrentTime, Formatting.ASCIIStringToByteArray(Formatting.ConvertDateToString(datetime, "HHmmss")));
            Header.SetValue(HeaderEntryEnum.MessageType, new byte[] { (byte)messageType });
            Header.SetValue(HeaderEntryEnum.MessageSubtype, new byte[] { (byte)subType });
            Header.SetValue(HeaderEntryEnum.TerminalID, Formatting.ASCIIStringToByteArray(terminalID));
            Header.SetValue(HeaderEntryEnum.EmployeeID, Formatting.ASCIIStringToByteArray(employeeId));
        }

        public bool Validate()
        {
            foreach(FIDMeta fm in MandatoryFidIds)
            {
                bool found = false;
                foreach(FIDBase fb in Fids)
                    if(fb.Id == fm.Id && fm.SubId == fb.SubId)
                        found = true;

                if (!found)
                    return false;
            }
            return true;
        }

        public virtual byte[] Serialize()
        {
            List<byte[]> result = new List<byte[]>
            {
                Header.Serialize(),
                Fids.Select(x => x.Serialize()).SelectMany(x => x).ToArray()
            };
            return result.SelectMany(x => x).ToArray();
        }

        public virtual int Deserialize(byte[] input, int pos)
        {
            pos = Header.Deserialize(input, pos);
            for(; pos < input.Length;)
                Fids.Add(new FIDBase(input, ref pos));
            return pos;
        }

        public override string ToString()
        {
            return string.Format("[{0}]", Name);
        }

        public virtual string ToPrintString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("[SPDH Transaction:{0}]", Name));

            int depth = 1;
            StringBuilder sbTab = new StringBuilder();
            for (int j = 0; j <= depth; j++)
                sbTab.Append("\t");

            depth++;
            sb.AppendLine(sbTab + "Header:[");
            sb.AppendLine(Header.ToPrintString(ref depth));
            sb.AppendLine(sbTab + "]");

            sb.AppendLine(sbTab + "Fids:[");
            foreach (FIDBase fb in Fids)
                sb.AppendLine(fb.ToPrintString(ref depth));
            sb.Append(sbTab + "]");

            return sb.ToString();
        }
        
    }

    internal class HeaderEntryBase
    {
        public int Start { get; protected set; }
        public int End { get; protected set; }
        public int Length { get; protected set; }
        public FormatEnum Type { get; protected set; }
        public string Name { get; protected set; }
        public byte[] Value { get; set; }

        public HeaderEntryBase(string name, int start, int end, int length, FormatEnum type)
        {
            Start = start;
            End = end;
            Length = length;
            Type = type;
            Name = name;
        }

        public virtual byte[] Serialize()
        {
            switch (Type)
            {
                case FormatEnum.Hex:
                    Value = Formatting.PadArray(Value, Length, false, (byte)' ');
                    break;

                case FormatEnum.Numeric:
                    Value = Formatting.PadArray(Value, Length, true, (byte)'0');
                    break;
            }
            return Value;
        }

        public virtual int Deserialize(byte[] input, int pos)
        {
            Value = new byte[Length];
            Array.Copy(input, pos, Value, 0, Value.Length);
            return pos + Length;
        }

        public override string ToString()
        {
            return string.Format("[{0,-50}] V:[{1}]", Name, Formatting.ByteArrayToASCIIString(Value));
        }
    }

    internal class FIDMeta
    {
        public char Id { get; protected set; }
        public char SubId { get; protected set; }
        public string Description { get; protected set; }
        public FormatEnum FidFormat { get; protected set; }
        public int Min { get; protected set; }
        public int Max { get; protected set; }

        public FIDMeta(char id, char subId, string description, FormatEnum fidFormat, int min, int max)
        {
            Id = id;
            SubId = subId;
            Description = description;
            FidFormat = fidFormat;
            Min = min;
            Max = max;
        }

        public override string ToString()
        {
            return string.Format("[{0}][{1}:{2}]", Description, Id, SubId);
        }
    }

    internal class FIDBase
    {
        public char Id { get; protected set; }
        public char SubId { get; protected set; }
        public byte[] Value { get; protected set; }

        public FIDMeta FidMeta { get; protected set; }

        public List<FIDBase> Children { get; protected set; }

        public FIDBase(char id, char subId, byte[] value)
        {
            Children = new List<FIDBase>();
            Id = id;
            SubId = subId;
            Value = value;

            FidMeta = FidMetaList.FindMeta(id, subId);
        }
        public FIDBase(byte[] rawValue, ref int pos)
        {
            Children = new List<FIDBase>();
            pos = Deserialize(rawValue, pos);
            if(Children.Count == 0)
                FidMeta = FidMetaList.FindMeta(Id, SubId);
        }

        public virtual byte[] Serialize()
        {
            List<byte[]> result = new List<byte[]>
            {
                new byte[] { 0x1C, (byte)Id }
            };

            if (Children.Count > 0)
            {
                return SerializeChildren();
            }
            else
            {
                if (SubId != ' ') result.Add(new byte[] { 0x1E, (byte)SubId });
                result.Add(Value);
                return result.SelectMany(x => x).ToArray();
            }
        }

        private byte[] SerializeChildren()
        {
            return Children.SelectMany(x => x.Serialize()).ToArray();
        }

        protected virtual int Deserialize(byte[] input, int pos)
        {
            byte delimiter = input[pos]; //should be FS char
            pos++;
            Id = (char)input[pos];
            pos++;

            if (input[pos] == (byte)0x1E) //RS char
            {
                pos++;
                return DeserializeChildren(input, pos);
            }
            else
                SubId = ' ';

            int lastPos = Array.FindIndex(input, pos, (x) => x == 0x1C);
            if (lastPos == -1)
                lastPos = input.Length;
            int valLength = lastPos - pos;
            Value = new byte[valLength];
            Array.Copy(input, pos, Value, 0, Value.Length);
            pos = pos + Value.Length;
            return pos;
        }

        private int DeserializeChildren(byte[] input, int pos)
        {
            bool breakLoop = false;
            while (1 == 1)
            {
                char childSubId = (char)input[pos];
                pos++;
                int lastPos = Array.FindIndex(input, pos, (x) => x == 0x1E);
                if (lastPos == -1)
                {
                    lastPos = Array.FindIndex(input, pos, (x) => x == 0x1C);
                    if (lastPos == -1)
                        lastPos = input.Length;
                    breakLoop = true;
                }
                int valLength = lastPos - pos;
                byte[] childValue = new byte[valLength];
                Array.Copy(input, pos, childValue, 0, childValue.Length);
                pos = pos + childValue.Length;

                FIDBase child = new FIDBase(Id, childSubId, childValue);
                Children.Add(child);

                if (breakLoop)
                    return pos;

                pos++;//increment past delimeter
            }
        }
        

        public override string ToString()
        {
            if(Children.Count == 0)
                return string.Format("[{0,-50}][{1}:{2}] V:[{3}]", FidMeta.Description,Id,SubId,Formatting.ByteArrayToASCIIString(Value));
            else
                return string.Format("[{0,-50}][{1}] V:[Child Count={2}]","Parent", Id, Children.Count);
        }

        public string ToPrintString(ref int depth)
        {
            StringBuilder sbTab = new StringBuilder();
            for (int j = 0; j <= depth; j++)
                sbTab.Append("\t");

            if (Children.Count == 0)
                return sbTab + string.Format("[{0,-50}][{1}:{2}] V:[{3}]", FidMeta.Description, Id, SubId, Formatting.ByteArrayToASCIIString(Value));
            else
            {
                StringBuilder childs = new StringBuilder();
                foreach (FIDBase child in Children)
                {
                    childs.AppendLine(sbTab + child.ToPrintString(ref depth));
                }
                return childs.ToString();
            }
        }
    }
}

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
using System.Text;

namespace DCEMV.CardReaders.NCIDriver
{
    public class ConfigParameter
    {
        private byte identifier;
        private byte? reservedIdentifier;
        private byte length;
        private byte[] value;

        private ConfigParamTag Identifier
        {
            get
            {
                return (ConfigParamTag)EnumUtil.GetEnum(typeof(ConfigParamTag), identifier);
            }
        }

        public ConfigParameter()
        {
        }

        public ConfigParameter(byte identifier, byte? reservedIdentifier, byte length, byte[] value)
        {
            this.identifier = identifier;
            this.length = length;
            this.value = value;
            this.reservedIdentifier = reservedIdentifier;
        }

        public byte deserialize(byte[] packet, byte pos)
        {
            identifier = packet[pos];
            pos++;
            if (identifier >= (byte)ConfigParamTag.RESERVED_START && identifier <= (byte)ConfigParamTag.RESERVED_END) //extension
            {
                reservedIdentifier = identifier;
                identifier = packet[pos];
                pos++;
            }
            length = packet[pos];
            pos++;
            if (length > 0)
            {
                value = new byte[length];
                Array.Copy(packet, pos, value, 0, length);
                pos = (byte)(pos + length);
            }
            return pos;
        }

        public byte[] serialize()
        {
            byte[] param;
            byte pos = 0;
            if (reservedIdentifier.HasValue)
            {
                param = new byte[1 + 1 + 1 + length];
                param[pos] = reservedIdentifier.Value;
                pos++;
                param[pos] = identifier;
                pos++;
                param[pos] = length;
                if (length > 0)
                    Array.Copy(value, 0, param, 3, length);
            }
            else
            {
                param = new byte[1 + 1 + length];
                param[pos] = identifier;
                pos++;
                param[pos] = length;
                if (length > 0)
                    Array.Copy(value, 0, param, 2, length);
            }
            return param;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (reservedIdentifier.HasValue)
            {
                sb.AppendLine("Reserved Identifier:" + String.Format("0x{0:X2}", reservedIdentifier.Value));
                sb.AppendLine("Identifier:" + String.Format("0x{0:X2}", identifier));
            }
            else
                try
                {
                    sb.AppendLine("Identifier:" + Identifier);
                }
                catch
                {
                    sb.AppendLine("Identifier:" + String.Format("0x{0:X2}", identifier));
                }
            sb.AppendLine("Length:" + length);
            if(length>0)
                sb.AppendLine("Value:" + BitConverter.ToString(value, 0));
            return sb.ToString();
        }
    }
}

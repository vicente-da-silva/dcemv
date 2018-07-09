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
using System.Text;

namespace DCEMV.CardReaders.NCIDriver
{
    public class MappingConfiguration
    {
        public RFProtocolEnum RFProtocol { get; set; }
        public RFModeEnum RFMode { get; set; }
        public RFInterfaceEnum RFInterface { get; set; }

        public static byte getSize()
        {
            return 3;
        }

        public byte deserialize(byte[] packet, byte pos)
        {
            RFProtocol = (RFProtocolEnum)EnumUtil.GetEnum(typeof(RFProtocolEnum), packet[pos]);
            pos++;
            RFMode = (RFModeEnum)EnumUtil.GetEnum(typeof(RFModeEnum), packet[pos]);
            pos++;
            RFInterface = (RFInterfaceEnum)EnumUtil.GetEnum(typeof(RFInterfaceEnum), packet[pos]);
            pos++;
            return pos;
        }

        public byte[] serialize()
        {
            byte[] ret = new byte[getSize()];
            ret[0] = (byte)RFProtocol;
            ret[1] = (byte)RFMode;
            ret[2] = (byte)RFInterface;
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("RFProtocol: " + RFProtocol);
            sb.AppendLine("Mode: " + RFMode);
            sb.AppendLine("RFInterface: " + RFInterface);
            return sb.ToString();
        }
    }
}

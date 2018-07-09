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
    public class DataPacket : Packet
    {
        public byte ConnIdentifier
        {
            get
            {
                return identifier;
            }
        }

        public DataPacket() { }

        public DataPacket(PacketBoundryFlagEnum pbf, byte connectionIdentifier)
            : base(PacketTypeEnum.Data, pbf, connectionIdentifier)
        {
            
        }

        public DataPacket(PacketBoundryFlagEnum pbf, byte connectionIdentifier, byte[] payLoad)
            : base(PacketTypeEnum.Data, pbf, connectionIdentifier, payLoad)
        {

        }

        public byte[] getPayLoad()
        {
            return payLoad;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.AppendLine(MessageType + " with " + PacketBoundryFlag + " on CONID " + String.Format("0x{0:x2}", ConnIdentifier));
            sb.AppendLine("[" + getPLL() + "] HEX[" + BitConverter.ToString(payLoad, 0) + "]");
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }
    }
}

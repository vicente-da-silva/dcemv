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
    public class ControlPacket : Packet
    {
        protected byte opcodeIdentifier;
        public GroupIdentifierEnum GroupIdentifier
        {
            get
            {
                return (GroupIdentifierEnum)EnumUtil.GetEnum(typeof(GroupIdentifierEnum), identifier);
            }
        }

        public ControlPacket() { }

        public ControlPacket(PacketTypeEnum packetType,PacketBoundryFlagEnum pbf, GroupIdentifierEnum groupIdentifier, byte opcodeIdentifier)
            : base(packetType, pbf, (byte)groupIdentifier)
        {
            this.opcodeIdentifier = opcodeIdentifier;
        }

        public ControlPacket(PacketBoundryFlagEnum pbf, GroupIdentifierEnum groupIdentifier, byte opcodeIdentifier)
            : base(PacketTypeEnum.ControlCommand, pbf, (byte)groupIdentifier)
        {
            this.opcodeIdentifier = opcodeIdentifier;
        }

        public override byte[] serialize()
        {
            byte[] packet = base.serialize();
            packet[1] = (byte)opcodeIdentifier;
            return packet;
        }

        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);
            opcodeIdentifier = packet[1];
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(MessageType + " with " + PacketBoundryFlag + " on GID " + GroupIdentifier + " and OID " + opcodeIdentifier);
            sb.AppendLine("[" + getPLL() + "] HEX[" + BitConverter.ToString(payLoad, 0) + "]");
            return sb.ToString();
        }
    }
}

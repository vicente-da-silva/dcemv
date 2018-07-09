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
    public class Packet
    {
        public PacketTypeEnum MessageType { get; set; }
        public PacketBoundryFlagEnum PacketBoundryFlag { get; set; }
        protected byte identifier;
        protected byte[] payLoad;

        public Packet()
        {
            if (payLoad == null)
                this.payLoad = new byte[0];
        }

        public Packet(PacketTypeEnum messageType, PacketBoundryFlagEnum packetBoundryFlag, byte identifier, byte[] payLoad)
        {
            if(payLoad==null)
                this.payLoad = new byte[0];
            else
                this.payLoad = payLoad;
            this.MessageType = messageType;
            this.PacketBoundryFlag = packetBoundryFlag;
            this.identifier = identifier;
        }

        public Packet(PacketTypeEnum messageType, PacketBoundryFlagEnum packetBoundryFlag, byte identifier)
        {
            this.payLoad = new byte[0];
            this.MessageType = messageType;
            this.PacketBoundryFlag = packetBoundryFlag;
            this.identifier = identifier;
        }

        public virtual byte[] serialize()
        {
            byte[] packet = new byte[1 + 1 + 1 + getPLL()]; //[header + [gid/rfu]] + [oid/connid] + [l] + [payload]
            packet[0] = (byte)((((byte)MessageType | (byte)PacketBoundryFlag) << 4) | (identifier & 0x0F));
            packet[2] = getPLL();
            Array.Copy(payLoad, 0, packet, 3, getPLL());
            return packet;
        }

        public virtual void deserialize(byte[] packet)
        {
            MessageType = (PacketTypeEnum)EnumUtil.GetEnum(typeof(PacketTypeEnum), (byte)((packet[0] >> 4) & 0x0E));
            PacketBoundryFlag = (PacketBoundryFlagEnum)EnumUtil.GetEnum(typeof(PacketBoundryFlagEnum), (byte)((packet[0] >> 4) & 0x01));
            identifier = (byte)(packet[0] & 0x0F);
            payLoad = new byte[packet[2]];
            Array.Copy(packet, 3, payLoad, 0, packet[2]);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MT[" + MessageType + "] PBF[" + PacketBoundryFlag + "]");
            sb.AppendLine("Id[" + String.Format("0x{0:x2}",identifier) + "]");
            sb.AppendLine("PLL[" + getPLL() + "] PLHEX[" + BitConverter.ToString(payLoad, 0) + "]");
            return sb.ToString();
        }

        public byte getPLL()
        {
            return (byte)payLoad.Length;
        }

        protected short ToShort(byte msb, byte lsb)
        {
            return (short)((msb << 8) | lsb);
        }
        protected byte FromShortMSB(short value)
        {
            return (byte)(value >> 8);
        }
        protected byte FromShortLSB(short value)
        {
            return (byte)(value & 0x0F);
        }

    }
}

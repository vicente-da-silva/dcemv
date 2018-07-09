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
    public class ControlResponse : ControlPacket
    {
        public ControlResponse() { }

        public ControlResponse(PacketBoundryFlagEnum pbf, GroupIdentifierEnum groupIdentifier, byte opcodeIdentifier)
            : base(PacketTypeEnum.ControlResponse, pbf, groupIdentifier, opcodeIdentifier)
        {
            if(payLoad == null || getPLL() == 0)
                payLoad = new byte[1];
        }

        public ReponseCode Status { get; set; }

        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);
            Status = (ReponseCode)EnumUtil.GetEnum(typeof(ReponseCode), payLoad[0]);
            byte[] newPayLoad = new byte[payLoad.Length - 1];
            Array.Copy(payLoad, 1, newPayLoad, 0, payLoad.Length - 1);
            payLoad = newPayLoad;
        }

        public override byte[] serialize()
        {
            if (payLoad == null || getPLL() == 0)
            {
                payLoad = new byte[1];
                payLoad[0] = (byte)Status;
            }
            else { 
                byte[] newPayLoad = new byte[payLoad.Length + 1];
                Array.Copy(payLoad, 0, newPayLoad, 1, payLoad.Length);
                newPayLoad[0] = (byte)Status;
                payLoad = newPayLoad;
            }

            return base.serialize();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(MessageType + " with " + PacketBoundryFlag + " on GID " + GroupIdentifier + " and OID " + opcodeIdentifier);
            sb.AppendLine("[" + getPLL() + "] HEX[" + BitConverter.ToString(payLoad, 0) + "]");
            sb.AppendLine("Status: " + Status);
            return sb.ToString();
        }
    }
}

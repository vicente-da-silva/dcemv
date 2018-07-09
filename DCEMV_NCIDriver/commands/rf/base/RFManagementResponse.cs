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
    public class RFManagementResponse : ControlResponse
    {
        public OpcodeRFIdentifierEnum OpcodeIdentifier
        {
            get
            {
                return (OpcodeRFIdentifierEnum)EnumUtil.GetEnum(typeof(OpcodeRFIdentifierEnum), opcodeIdentifier);
            }

        }

        public RFManagementResponse() : base() { }

        public RFManagementResponse(PacketBoundryFlagEnum pbf, OpcodeRFIdentifierEnum opcodeIdentifier) 
            : base(pbf, GroupIdentifierEnum.RFMANAGEMENT, (byte)opcodeIdentifier) { }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.AppendLine(MessageType + " with " + PacketBoundryFlag + " on GID " + GroupIdentifier + " and OID " + OpcodeIdentifier);
            sb.AppendLine("[" + getPLL() + "] HEX[" + BitConverter.ToString(payLoad, 0) + "]");
            sb.AppendLine("Status: " + Status);
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }
    }
}

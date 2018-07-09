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
    public class RFManagementNotification : NotificationPacket
    {
        public byte RFDiscoveryId { get; set; }
        public RFProtocolEnum RFProtocol { get; set; }
        public RFTechnologiesAndModeEnum RFTechnologiesAndMode { get; set; }
        public TechSpecificParamsBase TechSpecificParam { get; set; }

        public new OpcodeRFIdentifierEnum OpcodeIdentifier
        {
            get
            {
                return (OpcodeRFIdentifierEnum)EnumUtil.GetEnum(typeof(OpcodeRFIdentifierEnum), base.OpcodeIdentifier);
            }
        }

        public RFManagementNotification() : base() { }

        public RFManagementNotification(PacketBoundryFlagEnum pbf, OpcodeRFIdentifierEnum opcode) 
            : base(pbf,GroupIdentifierEnum.RFMANAGEMENT,(byte)opcode)
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(MessageType + " with " + PacketBoundryFlag + " on GID " + GroupIdentifier + " and OID " + OpcodeIdentifier);
            sb.AppendLine("[" + getPLL() + "] HEX[" + BitConverter.ToString(payLoad, 0) + "]");
            sb.AppendLine("RFDiscoveryId: " + RFDiscoveryId);
            sb.AppendLine("RFProtocol: " + RFProtocol);
            sb.AppendLine("RFTechnologiesAndMode: " + RFTechnologiesAndMode);
            sb.AppendLine("---TechSpecificParam---");
            if (TechSpecificParam!=null)
                sb.Append(TechSpecificParam.ToString());
            else
                sb.AppendLine("NULL");
            sb.AppendLine("---");
            return sb.ToString();
        }
    }
}

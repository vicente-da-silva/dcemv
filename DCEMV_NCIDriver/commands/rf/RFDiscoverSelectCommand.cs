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
    public class RFDiscoverSelectCommand : RFManagementCommand
    {
        public byte RFDiscoveryId { get; set; }
        public RFProtocolEnum RFProtocol { get; set; }
        public RFInterfaceEnum RFInterface { get; set; }

        public RFDiscoverSelectCommand() { }

        public RFDiscoverSelectCommand(PacketBoundryFlagEnum pbf) : base(pbf,OpcodeRFIdentifierEnum.RF_DISCOVER_SELECT_CMD) 
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.Append(base.ToString());
            sb.AppendLine("RFDiscoveryId: " + RFDiscoveryId);
            sb.AppendLine("RFProtocol: " + RFProtocol);
            sb.AppendLine("RFInterface: " + RFInterface);
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }

        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);
            RFDiscoveryId = payLoad[0];
            RFProtocol = (RFProtocolEnum)EnumUtil.GetEnum(typeof(RFProtocolEnum), payLoad[1]);
            RFInterface = (RFInterfaceEnum)EnumUtil.GetEnum(typeof(RFInterfaceEnum), payLoad[2]);
        }

        public override byte[] serialize()
        {
            payLoad = new byte[3];
            payLoad[0] = RFDiscoveryId;
            payLoad[1] = (byte)RFProtocol;
            payLoad[2] = (byte)RFInterface;
            return base.serialize();
        }
    }
}

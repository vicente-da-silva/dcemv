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
namespace DCEMV.CardReaders.NCIDriver
{
    public class RFSetListenModeRoutingPacket : RFManagementCommand
    {
        public RFSetListenModeRoutingPacket(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeRFIdentifierEnum.RF_SET_LISTEN_MODE_ROUTING_CMD) { }
    }
    public class RFGetListenModeRoutingPacket : RFManagementCommand
    {
        public RFGetListenModeRoutingPacket(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeRFIdentifierEnum.RF_GET_LISTEN_MODE_ROUTING_CMD) { }
    }
    public class RFINTFActivatedPacket : RFManagementCommand
    {
        public RFINTFActivatedPacket(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeRFIdentifierEnum.RF_INTF_ACTIVATED_NTF) { }
    }
    public class RFFieldInfoPacket : RFManagementCommand
    {
        public RFFieldInfoPacket(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeRFIdentifierEnum.RF_FIELD_INFO_NTF) { }
    }
    public class RFParameterUpdatePacket : RFManagementCommand
    {
        public RFParameterUpdatePacket(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeRFIdentifierEnum.RF_PARAMETER_UPDATE_CMD) { }
    }

    public class RFNFCEEActionPacket : RFManagementCommand
    {
        public RFNFCEEActionPacket(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeRFIdentifierEnum.RF_NFCEE_ACTION_NTF) { }
    }
    public class RFNFCEEDiscoveryReqPacket : RFManagementCommand
    {
        public RFNFCEEDiscoveryReqPacket(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeRFIdentifierEnum.RF_NFCEE_DISCOVERY_REQ_NTF) { }
    }
}

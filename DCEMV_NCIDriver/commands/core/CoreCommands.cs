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
    public class CoreConnCreatePacket : CoreCommand
    {
        public CoreConnCreatePacket(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeCoreIdentifierEnum.CORE_CONN_CREATE_CMD) { }
    }
    public class CoreConnClosePacket : CoreCommand
    {
        public CoreConnClosePacket(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeCoreIdentifierEnum.CORE_CONN_CLOSE_CMD) { }
    }
    public class CoreConnCreditsPacket : CoreCommand
    {
        public CoreConnCreditsPacket(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeCoreIdentifierEnum.CORE_CONN_CREDITS_NTF) { }
    }
    public class CoreGenericErrorPacket : CoreCommand
    {
        public CoreGenericErrorPacket(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeCoreIdentifierEnum.CORE_GENERIC_ERROR_NTF) { }
    }
    public class CoreInterfaceErrorPacket : CoreCommand
    {
        public CoreInterfaceErrorPacket(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeCoreIdentifierEnum.CORE_INTERFACE_ERROR_NTF) { }
    }
}

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
    public class GenericResponse : CoreCommand
    {
        private ReponseCode status;

        public GenericResponse(PacketBoundryFlagEnum pbf) : base(pbf,(byte)OpcodeRFIdentifierEnum.RF_DISCOVER_MAP_CMD)//TODO: insert correct opcode
        {
        }

        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);
            status = (ReponseCode)EnumUtil.GetEnum(typeof(ReponseCode), payLoad[0]);
        }

        public override byte[] serialize()
        {
            payLoad = new byte[1];//TODO: insert correct size of payload
            payLoad[0] = (byte)status;

            return base.serialize();
        }
    }
}

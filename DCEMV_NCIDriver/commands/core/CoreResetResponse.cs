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
    public class CoreResetResponse : CoreResponse
    {
        public NCIVersionEnum NCIVersion { get; set; }
        public ConfigStatusEnum ConfigStatus { get; set; }

        public CoreResetResponse() { }

        public CoreResetResponse(PacketBoundryFlagEnum pbf) : base(pbf,(byte)OpcodeCoreIdentifierEnum.CORE_RESET_CMD) { }

        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);
            NCIVersion = (NCIVersionEnum)Enum.ToObject(typeof(NCIVersionEnum), payLoad[0]);
            ConfigStatus = (ConfigStatusEnum)Enum.ToObject(typeof(ConfigStatusEnum), payLoad[1]);
        }

        public override byte[] serialize()
        {
            payLoad = new byte[2];
            payLoad[0] = (byte)NCIVersion;
            payLoad[1] = (byte)ConfigStatus;
            return base.serialize();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.Append(base.ToString());
            sb.AppendLine("NCIVersion: " + NCIVersion);
            sb.AppendLine("ConfigStatus: " + ConfigStatus);
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }
    }
}



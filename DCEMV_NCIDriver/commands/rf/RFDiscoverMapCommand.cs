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
    public class RFDiscoverMapCommand : RFManagementCommand
    {
        public MappingConfiguration[] MappingConfigurations { get; set; }

        public RFDiscoverMapCommand() { }

        public RFDiscoverMapCommand(PacketBoundryFlagEnum pbf) : base(pbf,OpcodeRFIdentifierEnum.RF_DISCOVER_MAP_CMD)
        {
        }
        
        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);

            byte noOfMappingconfigurations = payLoad[0];
            byte pos = 1;
            MappingConfigurations = new MappingConfiguration[noOfMappingconfigurations];
            for (int i = 0; i < noOfMappingconfigurations; i++)
            {
                MappingConfiguration mc = new MappingConfiguration();
                pos = mc.deserialize(payLoad, pos);
                MappingConfigurations[i] = mc;
            }
        }

        public override byte[] serialize()
        {
            payLoad = new byte[(MappingConfigurations.Length * MappingConfiguration.getSize())+1];
            for (int i = 0; i < MappingConfigurations.Length; i++)
            {
                MappingConfiguration mc = MappingConfigurations[i];
                byte[] ser = mc.serialize();
                Array.Copy(ser, 0, payLoad, (MappingConfiguration.getSize() * i) + 1, MappingConfiguration.getSize());
            }
            payLoad[0] = (byte)MappingConfigurations.Length;
            return base.serialize();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.Append(base.ToString());
            for (int i = 0; i < MappingConfigurations.Length; i++)
                sb.AppendLine("MappingConfiguration:" + MappingConfigurations[i].ToString());
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }
    }
}

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
    public class RFDiscoverCommand : RFManagementCommand
    {
        public DiscoverConfiguration[] DiscoverConfigurations { get; set; }

        public RFDiscoverCommand() { DiscoverConfigurations = new DiscoverConfiguration[0]; }

        public RFDiscoverCommand(PacketBoundryFlagEnum pbf) : base(pbf,OpcodeRFIdentifierEnum.RF_DISCOVER_CMD)
        {
            DiscoverConfigurations = new DiscoverConfiguration[0];
        }
        
        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);

            byte noOfConfigurations = payLoad[0];
            DiscoverConfigurations = new DiscoverConfiguration[noOfConfigurations];
            byte pos = 1;
            for(int i = 0; i < noOfConfigurations; i++)
            {
                DiscoverConfiguration mc = new DiscoverConfiguration();
                pos = mc.deserialize(payLoad, pos);
                DiscoverConfigurations[i] = mc;
            }
        }

        public override byte[] serialize()
        {
            payLoad = new byte[(DiscoverConfigurations.Length * DiscoverConfiguration.getSize())+1];
            for (int i = 0; i < DiscoverConfigurations.Length; i++)
            {
                DiscoverConfiguration mc = DiscoverConfigurations[i];
                byte[] ser = mc.serialize();
                Array.Copy(ser, 0, payLoad, (DiscoverConfiguration.getSize() * i) + 1, DiscoverConfiguration.getSize());
            }
            payLoad[0] = (byte)DiscoverConfigurations.Length;
            return base.serialize();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.Append(base.ToString());
            for (int i = 0; i < DiscoverConfigurations.Length; i++)
                sb.AppendLine("DiscoverConfiguration:" + DiscoverConfigurations[i].ToString());
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }
    }
}

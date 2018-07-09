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
using System.Linq;
using System.Text;

namespace DCEMV.CardReaders.NCIDriver
{
    public class CoreInitResponse : CoreResponse
    {
        public NCCFeatures NCCFeatures { get; set; }
        public SupportedInterfacesEnum[] SupportedInterfaces { get; set; }
        public byte MaxLogicalConnections { get; set; }
        public short MaxRoutingTableSize { get; set; }
        public byte MaxControlPacketPayloadSize { get; set; }
        public short MaxSizeForLargeParameters { get; set; }
        public byte ManufacturerId { get; set; }
        public byte[] ManufacturerSpecificInformation { get; set; }

        public CoreInitResponse(PacketBoundryFlagEnum pbf) : base(pbf,OpcodeCoreIdentifierEnum.CORE_INIT_CMD) {}

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.Append(base.ToString());
            sb.Append(NCCFeatures.ToString());

            foreach (SupportedInterfacesEnum e in SupportedInterfaces)
                sb.AppendLine("SupportedInterface: " + e);

            sb.AppendLine("MaxLogicalConnections: " + MaxLogicalConnections);
            sb.AppendLine("MaxRoutingTableSize: " + MaxRoutingTableSize);
            sb.AppendLine("MaxControlPacketPayloadSize: " + MaxControlPacketPayloadSize);
            sb.AppendLine("MaxSizeForLargeParameters: " + MaxSizeForLargeParameters);
            sb.AppendLine("ManufacturerId: " + ManufacturerId);

            sb.AppendLine("ManufacturerSpecificInformation: HEX[" + BitConverter.ToString(ManufacturerSpecificInformation, 0) + "]");
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }

        public override void deserialize(byte[] packet)
        {
            NCCFeatures = new NCCFeatures();

            base.deserialize(packet);
            NCCFeatures.deserialize(payLoad[0], payLoad[1], payLoad[2], payLoad[3]);
            byte numberSupportedInterfaces = payLoad[4];
            SupportedInterfaces = new SupportedInterfacesEnum[numberSupportedInterfaces];
            short pos = 5;
            for(int i = 0; i < numberSupportedInterfaces; i++)
            {
                SupportedInterfaces[i] = (SupportedInterfacesEnum)Enum.ToObject(typeof(SupportedInterfacesEnum), payLoad[pos]);
                pos++;
            }
            MaxLogicalConnections = payLoad[pos];
            pos++;
            MaxRoutingTableSize = ToShort(payLoad[pos],payLoad[pos + 1]);
            pos = (short)(pos + 2);
            MaxControlPacketPayloadSize = payLoad[pos];
            pos++;
            MaxSizeForLargeParameters = ToShort(payLoad[pos], payLoad[pos + 1]);
            pos = (short)(pos + 2);
            ManufacturerId = payLoad[pos];
            pos++;
            ManufacturerSpecificInformation = new byte[4];
            Array.Copy(payLoad,pos,ManufacturerSpecificInformation,0,4);
            pos = (short)(pos + 4);
        }

        public override byte[] serialize()
        {
            if (SupportedInterfaces == null)
                SupportedInterfaces = new SupportedInterfacesEnum[0];

            if (ManufacturerSpecificInformation == null)
                ManufacturerSpecificInformation = new byte[4];

            if(NCCFeatures == null)
                throw new Exception("NCCFeatures not initialised");

            if (ManufacturerSpecificInformation.Length != 4)
                throw new Exception("ManufacturerSpecificInformation > 4");

            byte[][] serOut = new byte[4][];
            serOut[0] = NCCFeatures.serialize();

            serOut[1] = new byte[SupportedInterfaces.Length + 1];
            serOut[1][0] = (byte)SupportedInterfaces.Length;
            for (int i = 0; i < SupportedInterfaces.Length; i++)
                serOut[1][i+1] = (byte)SupportedInterfaces[i];

            serOut[2] = new byte[7];
            serOut[2][0] = MaxLogicalConnections;
            serOut[2][1] = FromShortMSB(MaxRoutingTableSize); ;
            serOut[2][2] = FromShortLSB(MaxRoutingTableSize); ;
            serOut[2][3] = MaxControlPacketPayloadSize;
            serOut[2][4] = FromShortMSB(MaxSizeForLargeParameters); ;
            serOut[2][5] = FromShortLSB(MaxSizeForLargeParameters); ;
            serOut[2][6] = ManufacturerId;

            serOut[3] = ManufacturerSpecificInformation;

            payLoad = serOut.SelectMany(x => x).ToArray();

            return base.serialize();
        }
    }
}

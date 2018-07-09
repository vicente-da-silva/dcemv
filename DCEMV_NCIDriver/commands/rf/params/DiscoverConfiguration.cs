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
    public class DiscoverConfiguration
    {
        public RFTechnologiesAndModeEnum RFTechnologiesAndMode { get; set; }
        public byte RFDiscoverFrequency { get; set; }

        public static byte getSize()
        {
            return 2;
        }


        public byte deserialize(byte[] packet, byte pos)
        {
            RFTechnologiesAndMode = (RFTechnologiesAndModeEnum)EnumUtil.GetEnum(typeof(RFTechnologiesAndModeEnum), packet[pos]);
            pos++;
            RFDiscoverFrequency = packet[pos];
            pos++;
            return pos;
        }

        public byte[] serialize()
        {
            byte[] ret = new byte[getSize()];
            ret[0] = (byte)RFTechnologiesAndMode;
            ret[1] = (byte)RFDiscoverFrequency;
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("RFTechnologiesAndMode: " + RFTechnologiesAndMode);
            if(RFDiscoverFrequency == 0x01)
                sb.AppendLine("DiscoverFrequency: " + "RF Technology and Mode will be executed in every discovery period.");
            else if(RFDiscoverFrequency>=0x02 && RFDiscoverFrequency <= 0x0A)
                sb.AppendLine("DiscoverFrequency: " + String.Format("(Poll Mode Only)Polling will be executed in every {0} discovery period.", RFDiscoverFrequency));
            else
                sb.AppendLine("DiscoverFrequency: RFU Found");
            return sb.ToString();
        }
    }
}

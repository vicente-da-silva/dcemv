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
    public class NCCFeatures
    {
        private byte byte0;
        private byte byte1;
        private byte byte2;
        private byte byte3;

        public NCCFeatures(){ }

        public NCCFeatures(byte byte0, byte byte1, byte byte2, byte byte3)
        {
            this.byte0 = byte0;
            this.byte1 = byte1;
            this.byte2 = byte2;
            this.byte3 = byte3;
        }

        public void deserialize(byte[] packet)
        {
            byte0 = packet[0];
            byte1 = packet[1];
            byte2 = packet[2];
            byte3 = packet[3];
        }

        public void deserialize(byte byte0, byte byte1, byte byte2, byte byte3)
        {
            this.byte0 = byte0;
            this.byte1 = byte1;
            this.byte2 = byte2;
            this.byte3 = byte3;
        }

        public byte[] serialize()
        {
            byte[] ret = new byte[4];
            ret[0] = byte0;
            ret[1] = byte1;
            ret[2] = byte2;
            ret[3] = byte3;
            return ret;
        }

        public override string ToString()
        {
            StringBuilder ret = new StringBuilder();
            ret.AppendLine("--- NCCFeatures Start ---");
            if ((byte0 & 0x01) == 0x01)
                ret.AppendLine("Discovery Frequency configuration supported");
            else
                ret.AppendLine("Discovery Frequency configuration ignored, using 0x01");

            if ((byte0 & 0x06) == 0x00)
                ret.AppendLine("DH is the only entity that configures the NFCC");

            if ((byte0 & 0x06) == 0x02)
                ret.AppendLine("NFCC can manage or merge multiple sets of RF configuration parameters");

            if ((byte1 & 0x08) == 0x08)
                ret.AppendLine("AID based routing");
            else if ((byte1 & 0x04) == 0x04)
                ret.AppendLine("Protocol based routing");
            else if ((byte1 & 0x02) == 0x02)
                ret.AppendLine("Technology based Routing");
            else
                ret.AppendLine("Listen Mode Routing not supported");

            if ((byte2 & 0x02) == 0x02)
                ret.AppendLine("Switched Off state");

            if ((byte2 & 0x01) == 0x01)
                ret.AppendLine("Battery Off state");

            ret.AppendLine("--- NCCFeatures Stop ---");

            return ret.ToString();
        }
    }
}

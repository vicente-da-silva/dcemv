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
    public class RFT3TPollingCommand : RFManagementCommand
    {
        public byte[] SensFReqParams { get; internal set; }

        public RFT3TPollingCommand() { SensFReqParams = new byte[getSize()]; }

        public RFT3TPollingCommand(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeRFIdentifierEnum.RF_T3T_POLLING_CMD) { SensFReqParams = new byte[getSize()]; }

        public static byte getSize()
        {
            return 4;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.Append(base.ToString());
            sb.AppendLine("SensFReqParams: [" + getSize() + "] HEX[" + BitConverter.ToString(SensFReqParams, 0) + "]");
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }


        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);
            Array.Copy(payLoad, 0, SensFReqParams, 0, getSize());
        }

        public override byte[] serialize()
        {
            payLoad = new byte[SensFReqParams.Length];
            Array.Copy(SensFReqParams, 0, payLoad, 0, SensFReqParams.Length);
            return base.serialize();
        }
    }
}

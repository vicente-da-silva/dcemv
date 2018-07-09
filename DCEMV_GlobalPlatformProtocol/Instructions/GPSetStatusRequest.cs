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
using DCEMV.TLVProtocol;

namespace DCEMV.GlobalPlatformProtocol
{

    public class GPSetStatusRequest : GPCommand
    {
        public GPSetStatusRequest()
        {
        }

        public GPSetStatusRequest(byte[] aid, bool doLock) : base(ISO7816Protocol.Cla.ProprietaryCla8x, GPInstructionEnum.SetStatus, aid, 0x40, doLock ? (byte)0x80 : (byte)0x00)
        {
            ApduResponseType = typeof(GPSetStatusResponse);
        }
    }
    public class GPSetStatusResponse : GPResponse
    {
        //private TLV tlvResponse;

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
        }
       
        protected override TLV GetTLVResponse()
        {
            return null;// tlvResponse;
        }
    }
}

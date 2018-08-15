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
using DCEMV.Shared;
using DCEMV.EMVProtocol.Kernels;

namespace DCEMV.EMVProtocol
{
    public class EMVComputeCryptographicChecksumRequest : EMVCommand
    {
        public EMVComputeCryptographicChecksumRequest(byte[] udolData) : base(ISO7816Protocol.Cla.ProprietaryCla8x, EMVInstructionEnum.ComputeCryptographickChecksum, null, 0x8E, 0x80)
        {
            ApduResponseType = typeof(EMVComputeCryptographicChecksumResponse);
            CommandData = udolData;
            Logger.Log(ToPrintString());
        }
    }

    public class EMVComputeCryptographicChecksumResponse : EMVResponse
    {
        private TLV tlvResponse;

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
            if (!Succeeded) return;
            tlvResponse = TLV.Create(EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_2_77_KRN.Tag);
            tlvResponse.Deserialize(ResponseData,0);

            Logger.Log(ToPrintString());
        }
        
        protected override TLV GetTLVResponse()
        {
            return tlvResponse;
        }
    }
}

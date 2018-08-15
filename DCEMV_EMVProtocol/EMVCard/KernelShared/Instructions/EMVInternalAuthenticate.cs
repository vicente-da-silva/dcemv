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
using DCEMV.Shared;
using DCEMV.EMVProtocol.Kernels;
using DCEMV.TLVProtocol;


namespace DCEMV.EMVProtocol
{
    public class EMVInternalAuthenticateRequest : EMVCommand
    {
        public EMVInternalAuthenticateRequest(byte[] authenticationRelatedData) : base(ISO7816Protocol.Cla.CompliantCmd0x, EMVInstructionEnum.InternalAuthenticate, null, 0x00, 0x00)
        {
            ApduResponseType = typeof(EMVInternalAuthenticateResponse);

            CommandData = authenticationRelatedData;

            Logger.Log(ToPrintString());

        }

        
    }

    public class EMVInternalAuthenticateResponse : EMVResponse
    {
        private TLV tlvResponse;
        public TLV SignedApplicationData { get; set; }

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
            if (!Succeeded) return;
            if (ResponseData[0] == 0x77)
                tlvResponse = TLV.Create(EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_2_77_KRN.Tag);
            else
                tlvResponse = TLV.Create(EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_1_80_KRN.Tag);
            tlvResponse.Deserialize(ResponseData, 0);
            Logger.Log(ToPrintString());

            if (ResponseData[0] == 0x80)  //format 2
            {
                SignedApplicationData = TLV.Create(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag, tlvResponse.Value);
            }
            if (ResponseData[0] == 0x77)  //format 1
            {
                SignedApplicationData = tlvResponse.Children.Get(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag);
            }
        }
        public TLV GetTLVSignedApplicationData()
        {
            return SignedApplicationData;
        }
        protected override TLV GetTLVResponse()
        {
            return tlvResponse;
        }
    }
}

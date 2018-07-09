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
using System;
using DCEMV.TLVProtocol;


namespace DCEMV.EMVProtocol
{
    public class EMVGetProcessingOptionsRequest : EMVCommand
    {
        public EMVGetProcessingOptionsRequest(TLV pdolRelatedData) : base(ISO7816Protocol.Cla.ProprietaryCla8x, EMVInstructionEnum.GetProcessingOptions, null, 0x00, 0x00)
        {
            ApduResponseType = typeof(EMVGetProcessingOptionsResponse);

            TLV tlv = TLV.Create(EMVTagsEnum.COMMAND_TEMPLATE_83_KRN.Tag, pdolRelatedData.Value);
            CommandData = tlv.Serialize();
            Logger.Log(ToPrintString());
        }

       
    }

    public class EMVGetProcessingOptionsResponse : EMVResponse
    {
        private TLV tlvResponse;

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
            if (!Succeeded) return;
            if (ResponseData[0]==0x80)
                tlvResponse = TLV.Create(EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_1_80_KRN.Tag);
            else
                tlvResponse = TLV.Create(EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_2_77_KRN.Tag);
            tlvResponse.Deserialize(ResponseData,0);
            Logger.Log(ToPrintString());
        }

        public TLVList GetResponseTags()
        {
            try
            {
                TLVList result = new TLVList();
                if (tlvResponse.Tag.TagLable == EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_1_80_KRN.Tag)
                {
                    byte[] aip = EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.InitValue();
                    byte[] afl = new byte[tlvResponse.Value.Length - aip.Length];
                    Array.Copy(tlvResponse.Value, 0, aip,0, aip.Length);
                    Array.Copy(tlvResponse.Value, aip.Length, afl, 0, afl.Length);
                    result.AddToList(TLV.Create(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag, aip));
                    result.AddToList(TLV.Create(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN.Tag, afl));
                }
                else if (tlvResponse.Tag.TagLable == EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_2_77_KRN.Tag)
                {
                    return tlvResponse.Children;
                }
                else
                    throw new EMVProtocolException("Unrecognised template received from Get Processing Options");

                return result;
            }
            catch (Exception ex)
            { throw new EMVProtocolException("APPLICATION_IDENTIFIER_CARD_4F Tag not found:" + ex.Message); }
        }
        
        protected override TLV GetTLVResponse()
        {
            return tlvResponse;
        }
    }
}

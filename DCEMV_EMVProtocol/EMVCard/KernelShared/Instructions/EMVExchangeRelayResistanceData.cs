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
using DCEMV.EMVProtocol.Kernels;
using System;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol
{
    public class EMVExchangeRelayResistanceDataRequest : EMVCommand
    {
        public EMVExchangeRelayResistanceDataRequest(byte[] entropy) : base(ISO7816Protocol.Cla.ProprietaryCla8x, EMVInstructionEnum.ExchangeRelayResistanceData, entropy, 0x00, 0x00)
        {
            ApduResponseType = typeof(EMVExchangeRelayResistanceDataResponse);
        }
    }

    public class EMVExchangeRelayResistanceDataResponse : EMVResponse
    {
        private TLV tlvResponse;

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
            if (!Succeeded) return;
            tlvResponse = TLV.Create(EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_1_80_KRN.Tag);
            tlvResponse.Deserialize(ResponseData,0);
        }

        public TLVList GetResponseTags()
        {
            try
            {
                TLVList result = new TLVList();
                if (tlvResponse.Tag.TagLable == EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_1_80_KRN.Tag)
                {
                    byte[] DeviceRelayResistanceEntropy = new byte[4];
                    byte[] MinTimeForProcessingRelayResistanceAPDU = new byte[2];
                    byte[] MaxTimeForProcessingRelayResistanceAPDU = new byte[2];
                    byte[] DeviceEstimatedTransmissionTimeForRelayResistanceRAPDU = new byte[2];

                    Array.Copy(tlvResponse.Value, 0, DeviceRelayResistanceEntropy, 0, 4);
                    Array.Copy(tlvResponse.Value, 4, MinTimeForProcessingRelayResistanceAPDU, 0, 2);
                    Array.Copy(tlvResponse.Value, 6, MaxTimeForProcessingRelayResistanceAPDU, 0, 2);
                    Array.Copy(tlvResponse.Value, 8, DeviceEstimatedTransmissionTimeForRelayResistanceRAPDU, 0, 2);

                    result.AddToList(TLV.Create(EMVTagsEnum.DEVICE_RELAY_RESISTANCE_ENTROPY_DF8302_KRN2.Tag, DeviceRelayResistanceEntropy));
                    result.AddToList(TLV.Create(EMVTagsEnum.MIN_TIME_FOR_PROCESSING_RELAY_RESISTANCE_APDU_DF8303_KRN2.Tag, MinTimeForProcessingRelayResistanceAPDU));
                    result.AddToList(TLV.Create(EMVTagsEnum.MAX_TIME_FOR_PROCESSING_RELAY_RESISTANCE_APDU_DF8304_KRN2.Tag, MaxTimeForProcessingRelayResistanceAPDU));
                    result.AddToList(TLV.Create(EMVTagsEnum.DEVICE_ESTIMATED_TRANSMISSION_TIME_FOR_RELAY_RESISTANCE_RAPDU_DF8305_KRN2.Tag, DeviceEstimatedTransmissionTimeForRelayResistanceRAPDU));
                }
                else
                    throw new EMVProtocolException("Unrecognised template received from Get Processing Options");

                return result;
            }
            catch (Exception ex)
            { throw new EMVProtocolException("RESPONSE_MESSAGE_TEMPLATE_FORMAT_1_80_KRN Tag not found:" + ex.Message); }
        }

        protected override TLV GetTLVResponse()
        {
            return tlvResponse;
        }
    }
}

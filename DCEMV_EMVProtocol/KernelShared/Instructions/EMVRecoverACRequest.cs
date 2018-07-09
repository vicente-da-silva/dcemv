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
    public class EMVRecoverACRequest : EMVCommand
    {
        public EMVRecoverACRequest(TLV drdolRelatedData) : base(ISO7816Protocol.Cla.CompliantCmd0x, EMVInstructionEnum.RecoverAC, null, 0x00, 0x00)
        {
            ApduResponseType = typeof(EMVRecoverACResponse);
            CommandData = drdolRelatedData.Value;
        }
    }

    public class EMVRecoverACResponse : EMVResponse
    {
        private TLV tlvResponse;

        public TLV CryptogramInformationData { get; protected set; }
        public TLV ApplicationTransactionCounter { get; protected set; }
        public TLV ApplicationCryptogram { get; protected set; }
        public TLV IssuerApplicationData { get; protected set; }
        public TLV POSCardholderInteractionInformation { get; protected set; }

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
            if (!Succeeded) return;
            if (ResponseData[0] == 0x77)
                tlvResponse = TLV.Create(EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_2_77_KRN.Tag);
            else
                tlvResponse = TLV.Create(EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_1_80_KRN.Tag);
            tlvResponse.Deserialize(ResponseData,0);

            if (!tlvResponse.Tag.IsConstructed)
            {
                CryptogramInformationData = TLV.Create(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag, GetData(tlvResponse.Value,0,1));
                ApplicationTransactionCounter = TLV.Create(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag, GetData(tlvResponse.Value, 0+1, 2));
                ApplicationCryptogram = TLV.Create(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag, GetData(tlvResponse.Value, 0+1+2, 8));
                IssuerApplicationData = TLV.Create(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag, GetData(tlvResponse.Value, 0+1+2+8, 32));
            }
            else
            {
                CryptogramInformationData = tlvResponse.Children.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag);
                ApplicationTransactionCounter = tlvResponse.Children.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag);
                ApplicationCryptogram = tlvResponse.Children.Get(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag);
                IssuerApplicationData = tlvResponse.Children.Get(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag);
                POSCardholderInteractionInformation = tlvResponse.Children.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2.Tag);
            }
        }

        private byte[] GetData(byte[] source, int start, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(source, start, result, 0, length);
            return result;
        }

        protected override TLV GetTLVResponse()
        {
            return tlvResponse;
        }
    }
}

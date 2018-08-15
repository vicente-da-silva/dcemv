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
using DCEMV.FormattingUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCEMV.TLVProtocol;


namespace DCEMV.EMVProtocol
{
    public class EMVGenerateACRequest : EMVCommand
    {
        byte[] cdolData;
        byte[] dsdolData;
        public EMVGenerateACRequest(TLV cdolRelatedData, TLV dsdolRelatedData, TLV referenceControlParameter) : base(ISO7816Protocol.Cla.ProprietaryCla8x, EMVInstructionEnum.GenerateAC, null, 0x00, 0x00)
        {
            ApduResponseType = typeof(EMVGenerateACResponse);

            cdolData = cdolRelatedData.Value;
            if (dsdolRelatedData != null) dsdolData = dsdolRelatedData.Value;

            List<byte[]> result = new List<byte[]>
            {
                cdolData
            };
            if (dsdolData != null)
                result.Add(dsdolData);
            CommandData = result.SelectMany(a => a).ToArray();

            P1 = referenceControlParameter.Value[0];//rcp is a 1 byte value

            Logger.Log(ToPrintString());
        }
        public EMVGenerateACRequest(byte[] cdol2Data, TLV dsdolRelatedData, TLV referenceControlParameter) : base(ISO7816Protocol.Cla.ProprietaryCla8x, EMVInstructionEnum.GenerateAC, null, 0x00, 0x00)
        {
            ApduResponseType = typeof(EMVGenerateACResponse);

            cdolData = cdol2Data;
            
            if (dsdolRelatedData != null) dsdolData = dsdolRelatedData.Value;

            List<byte[]> result = new List<byte[]>
            {
                cdolData
            };
            if (dsdolData != null)
                result.Add(dsdolData);
            CommandData = result.SelectMany(a => a).ToArray();

            P1 = referenceControlParameter.Value[0];//rcp is a 1 byte value

            Logger.Log(ToPrintString());
        }
        public EMVGenerateACRequest(byte[] cdol1RelatedData) : base(ISO7816Protocol.Cla.ProprietaryCla8x, EMVInstructionEnum.GenerateAC, null, 0x00, 0x00)
        {
            ApduResponseType = typeof(EMVGenerateACResponse);

            cdolData = cdol1RelatedData;

            CommandData = cdol1RelatedData;

            P1 = 0x80; //ARQC and No CDA Requested

            Logger.Log(ToPrintString());
        }
        public override string ToPrintString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Start ADPU Request: " + this.GetType().Name);

            bool CDARequested = Formatting.GetBitPosition(P1, 5);
            ACTypeEnum ACType = (ACTypeEnum)Formatting.GetEnum(typeof(ACTypeEnum), (P1 >> 6));
            sb.AppendLine("CDA Requested: " + CDARequested + " ACType:" + ACType);
            sb.AppendLine("CDOL Data: [" + Formatting.ByteArrayToHexString(cdolData) + "]");
            if(dsdolData!= null) sb.AppendLine("DSDOL Data: " + Formatting.ByteArrayToHexString(dsdolData));

            sb.AppendLine("End ADPU Request: " + this.GetType().Name);
            
            return sb.ToString();
        }
    }


    public class EMVGenerateACResponse : EMVResponse
    {
        private TLV tlvResponse;

        public TLV CryptogramInformationData { get; protected set; }
        public TLV ApplicationTransactionCounter { get; protected set; }
        public TLV ApplicationCryptogram { get; protected set; }
        public TLV IssuerApplicationData { get; protected set; }
        public TLV POSCardholderInteractionInformation { get; protected set; }
        public TLV SignedDynamicApplicationData { get; protected set; }

        public bool CDAPerformed { get; protected set; }

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
            if (!Succeeded) return;
            if (ResponseData[0] == 0x77)
                tlvResponse = TLV.Create(EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_2_77_KRN.Tag);
            else
                tlvResponse = TLV.Create(EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_1_80_KRN.Tag);
            tlvResponse.Deserialize(ResponseData,0);
            Logger.Log(ToPrintString());

            if (!tlvResponse.Tag.IsConstructed) //Format 1 is not used if CDA is performed. 
            {
                CDAPerformed = false;
                CryptogramInformationData = TLV.Create(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag, GetData(tlvResponse.Value, 0, 1));
                ApplicationTransactionCounter = TLV.Create(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag, GetData(tlvResponse.Value, 0 + 1, 2));
                ApplicationCryptogram = TLV.Create(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag, GetData(tlvResponse.Value, 0 + 1 + 2, 8));
                IssuerApplicationData = TLV.Create(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag, GetData(tlvResponse.Value, 0 + 1 + 2 + 8, tlvResponse.Value.Length - (0 + 1 + 2 + 8)));
            }
            else //format 2 
            {
                //CDA Not Performed
                if (tlvResponse.Children.IsNotPresent(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag))
                {
                    CDAPerformed = false;
                    CryptogramInformationData = tlvResponse.Children.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag);
                    ApplicationTransactionCounter = tlvResponse.Children.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag);
                    ApplicationCryptogram = tlvResponse.Children.Get(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag);
                    IssuerApplicationData = tlvResponse.Children.Get(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag);
                    POSCardholderInteractionInformation = tlvResponse.Children.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2.Tag);
                }
                else//CDA Performed
                {
                    CDAPerformed = true;
                    CryptogramInformationData = tlvResponse.Children.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag);
                    ApplicationTransactionCounter = tlvResponse.Children.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag);
                    SignedDynamicApplicationData = tlvResponse.Children.Get(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag);
                    IssuerApplicationData = tlvResponse.Children.Get(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag);
                    POSCardholderInteractionInformation = tlvResponse.Children.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2.Tag);
                }
            }
        }

        public TLVList GetResponseTags()
        {
            return tlvResponse.Children;
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

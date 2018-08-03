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
using System.Text;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol
{
    public enum SelectPPSERequest_P1
    {
        SelectByName = 0x04,
        None = 0x00
    }
    public enum SelectPPSERequest_P2
    {
        NextOccurrence = 0x20,
        FirstOrOnlyOccurrence = 0x00
    }
    public class EMVSelectPPSERequest : EMVCommand
    {
        string fileName;
        public EMVSelectPPSERequest(string fileName) : base(EMVInstructionEnum.Select, null, 
            (byte)SelectPPSERequest_P1.SelectByName,
            (byte)SelectPPSERequest_P2.FirstOrOnlyOccurrence)
        {
            this.fileName = fileName;
            ApduResponseType = typeof(EMVSelectPPSEResponse);
            CommandData = Formatting.ASCIIStringToByteArray(fileName);

            Logger.Log(ToPrintString());
        }

        public override string ToPrintString()
        {
            string header = "Start ADPU Request: " + this.GetType().Name;
            string body = "Filename: " + fileName + "[" + Formatting.ByteArrayToHexString(CommandData) + "]";
            string footer = "End ADPU Request: " + this.GetType().Name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header).AppendLine(body).Append(footer);
            return sb.ToString();
        }
    }

   
    public class EMVSelectPPSEResponse : EMVResponse
    {
        private TLV tlvResponse;

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
            if (!Succeeded) return;
            tlvResponse = TLV.Create(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag);
            tlvResponse.Deserialize(ResponseData,0);
            
            Logger.Log(ToPrintString());
        }

        protected override TLV GetTLVResponse()
        {
            if(tlvResponse == null)
            {
                tlvResponse = TLV.Create(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag);
                tlvResponse.Deserialize(ResponseData, 0);
            }
            return tlvResponse;
        }

        public string GetDFName()
        {
            try {
                if (GetTLVResponse().Tag.TagLable != EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                    throw new EMVProtocolException("No FILE_CONTROL_INFO_TEMPLATE_6F tag found");

                TLV y = GetTLVResponse().Children.Get(EMVTagsEnum.DEDICATED_FILE_DF_NAME_84_KRN.Tag);

                return Encoding.UTF8.GetString(y.Value, 0, y.Value.Length);
            }
            catch(Exception ex)
            { throw new EMVProtocolException("DEDICATED_FILE_NAME_84 Tag not found:" + ex.Message);}
        }

        public List<string> GetADFNames()
        {
            try
            {
                if (GetTLVResponse().Tag.TagLable != EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                    throw new EMVProtocolException("No FILE_CONTROL_INFO_TEMPLATE_6F tag found");

                TLV y = GetTLVResponse().Children.Get(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_ISSUER_DISCRETIONARY_DATA_BF0C_KRN.Tag);

                TLV z = y.Children.Get(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_ISSUER_DISCRETIONARY_DATA_BF0C_KRN.Tag);

                TLVList a = z.Children.FindAll(EMVTagsEnum.APPLICATION_TEMPLATE_61_KRN.Tag);

                List<string> result = new List<string>();
                foreach(TLV tlv in a)
                    result.Add(Formatting.ByteArrayToHexString(tlv.Children.Get(EMVTagsEnum.APPLICATION_DEDICATED_FILE_ADF_NAME_4F_KRN.Tag).Value));

                return result;
            }
            catch (Exception ex)
            { throw new EMVProtocolException("APPLICATION_IDENTIFIER_CARD_4F Tag not found:" + ex.Message); }
        }

        public TLV GetSFI_88()
        {
            try
            {
                if (GetTLVResponse().Tag.TagLable != EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                    throw new EMVProtocolException("No FILE_CONTROL_INFO_TEMPLATE_6F tag found");

                TLV y = GetTLVResponse().Children.Get(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_PROPRIETARY_TEMPLATE_A5_KRN.Tag);

                TLV z = y.Children.Get(EMVTagsEnum.SHORT_FILE_IDENTIFIER_SFI_88_KRN.Tag);

                return z;
            }
            catch (Exception ex)
            { throw new EMVProtocolException("GetSFI_88 Error:" + ex.Message); }
        }

        public TLVList GetDirectoryEntries_61()
        {
            try
            {
                if (GetTLVResponse().Tag.TagLable != EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                    throw new EMVProtocolException("No FILE_CONTROL_INFO_TEMPLATE_6F tag found");

                TLV y = GetTLVResponse().Children.Get(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_PROPRIETARY_TEMPLATE_A5_KRN.Tag);

                TLV z = y.Children.Get(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_ISSUER_DISCRETIONARY_DATA_BF0C_KRN.Tag);

                return z.Children.FindAll(EMVTagsEnum.APPLICATION_TEMPLATE_61_KRN.Tag);
            }
            catch (Exception ex)
            { throw new EMVProtocolException("GetDirectoryEntries_61 Error:" + ex.Message); }
        }
    }
}

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
using DCEMV.FormattingUtils;
using System;
using System.Text;
using DCEMV.TLVProtocol;
using DCEMV.Shared;
using DCEMV.EMVProtocol.Kernels;

namespace DCEMV.EMVProtocol
{
    public class EMVSelectApplicationRequest : EMVCommand
    {
        private string aid;
        public EMVSelectApplicationRequest(string aid, bool isNext = false) : 
            base(EMVInstructionEnum.SelectPPSE, null, 0x04, isNext?(byte)0x02:(byte)0x00)
        {
            this.aid = aid;

            ApduResponseType = typeof(EMVSelectApplicationResponse);
            CommandData = Formatting.HexStringToByteArray(aid);

            Logger.Log(ToPrintString());
        }
        
        public override string ToPrintString()
        {
            string header = "Start ADPU Request: " + this.GetType().Name;
            string body = "AID: " + aid;
            string footer = "End ADPU Request: " + this.GetType().Name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header).AppendLine(body).Append(footer);
            return sb.ToString();
        }
    }

    public class EMVSelectApplicationResponse : EMVResponse
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
            if (tlvResponse == null)
            {
                tlvResponse = TLV.Create(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag);
                tlvResponse.Deserialize(ResponseData, 0);
            }
            return tlvResponse;
        }

        public TLV GetFCITemplateTag()
        {
            if (GetTLVResponse().Tag.TagLable != EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                throw new EMVProtocolException("No FILE_CONTROL_INFO_TEMPLATE_6F tag found");

            TLV y = GetTLVResponse().Children.Get(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_PROPRIETARY_TEMPLATE_A5_KRN.Tag);

            TLV pdol = y.Children.Get(EMVTagsEnum.PROCESSING_OPTIONS_DATA_OBJECT_LIST_PDOL_9F38_KRN.Tag);
            if (pdol == null)
                y.Children.AddToList(TLV.Create(EMVTagsEnum.PROCESSING_OPTIONS_DATA_OBJECT_LIST_PDOL_9F38_KRN.Tag));

            return GetTLVResponse();
        }

        public TLV GetDFNameTag()
        {
            try
            {
                if (GetTLVResponse().Tag.TagLable != EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                    throw new EMVProtocolException("No FILE_CONTROL_INFO_TEMPLATE_6F tag found");

                return GetTLVResponse().Children.Get(EMVTagsEnum.DEDICATED_FILE_DF_NAME_84_KRN.Tag);
            }
            catch (Exception ex)
            { throw new EMVProtocolException("DEDICATED_FILE_NAME_84 Tag not found:" + ex.Message); }
        }

        public TLV GetPreferredNameTag()
        {
            try
            {
                if (GetTLVResponse().Tag.TagLable != EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                    throw new EMVProtocolException("No FILE_CONTROL_INFO_TEMPLATE_6F tag found");

                TLV y = GetTLVResponse().Children.Get(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_PROPRIETARY_TEMPLATE_A5_KRN.Tag);

                return y.Children.Get(EMVTagsEnum.APPLICATION_PREFERRED_NAME_9F12_KRN.Tag);
            }
            catch (Exception ex)
            { throw new EMVProtocolException("APPLICATION_PREFERRED_NAME_9F12_KRN Tag not found:" + ex.Message); }
        }

        public TLV GetApplicationLabelTag()
        {
            try
            {
                if (GetTLVResponse().Tag.TagLable != EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                    throw new EMVProtocolException("No FILE_CONTROL_INFO_TEMPLATE_6F tag found");

                TLV y = GetTLVResponse().Children.Get(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_PROPRIETARY_TEMPLATE_A5_KRN.Tag);

                return y.Children.Get(EMVTagsEnum.APPLICATION_LABEL_50_KRN.Tag);
            }
            catch (Exception ex)
            { throw new EMVProtocolException("APPLICATION_LABEL_50_KRN Tag not found:" + ex.Message); }
        }

        public TLV GetApplicationPriorityindicatorTag()
        {
            try
            {
                if (GetTLVResponse().Tag.TagLable != EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                    throw new EMVProtocolException("No FILE_CONTROL_INFO_TEMPLATE_6F tag found");

                TLV y = GetTLVResponse().Children.Get(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_PROPRIETARY_TEMPLATE_A5_KRN.Tag);

                return y.Children.Get(EMVTagsEnum.APPLICATION_PRIORITY_INDICATOR_87_KRN.Tag);
            }
            catch (Exception ex)
            { throw new EMVProtocolException("DEDICATED_FILE_NAME_84 Tag not found:" + ex.Message); }
        }

        public string GetDFName()
        {
            try {
                if (GetTLVResponse().Tag.TagLable != EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                    throw new EMVProtocolException("No FILE_CONTROL_INFO_TEMPLATE_6F tag found");

                var y = GetTLVResponse().Children.Get(EMVTagsEnum.DEDICATED_FILE_DF_NAME_84_KRN.Tag);

                return Formatting.ByteArrayToHexString(y.Value);
            }
            catch(Exception ex)
            { throw new EMVProtocolException("DEDICATED_FILE_NAME_84 Tag not found:" + ex.Message);}
        }

        public TLVList GetPDOLTags()
        {
            return TLV.DeserializeChildrenWithNoV(GetPDOL().Value, 0);
        }

        public TLV GetPDOL()
        {
            try
            {
                if (GetFCITemplateTag().Tag.TagLable != EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                    throw new EMVProtocolException("No FILE_CONTROL_INFO_TEMPLATE_6F tag found");

                TLV y = GetTLVResponse().Children.Get(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_PROPRIETARY_TEMPLATE_A5_KRN.Tag);
               
                return y.Children.Get(EMVTagsEnum.PROCESSING_OPTIONS_DATA_OBJECT_LIST_PDOL_9F38_KRN.Tag);
            }
            catch (Exception ex)
            { throw new EMVProtocolException("PROCESSING_OPTIONS_DATA_OBJECT_LIST_9F38 Tag not found:" + ex.Message); }
        }
    }
}

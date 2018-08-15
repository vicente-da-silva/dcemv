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
    public class EMVReadRecordRequest : EMVCommand
    {
        private byte sfiNumber;
        private byte recordNumber;

        public EMVReadRecordRequest(byte sfiNumber,byte recordNumber) : base(ISO7816Protocol.Cla.CompliantCmd0x, EMVInstructionEnum.ReadRecord, null, 0x00, 0x00)
        {
            ApduResponseType = typeof(EMVReadRecordResponse);

            this.recordNumber = recordNumber;
            this.sfiNumber = sfiNumber;

            P1 = recordNumber;
            P2 = (byte)((sfiNumber << 3) | 0x04);//0x04 indicates P1 is a record number

            Logger.Log(ToPrintString());
        }

        public override string ToPrintString()
        {
            string header = "Start ADPU Request: " + this.GetType().Name;
            string body = "recordNumber: " + Formatting.ByteArrayToHexString(new byte[] { recordNumber }) + " sfiNumber: " + Formatting.ByteArrayToHexString(new byte[] { sfiNumber });
            string footer = "End ADPU Request: " + this.GetType().Name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header).AppendLine(body).Append(footer);
            return sb.ToString();
        }
    }

    public class EMVReadRecordResponse : EMVResponse
    {
        private TLV tlvResponse;

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
            if (!Succeeded)return;
            
            tlvResponse = TLV.Create(EMVTagsEnum.READ_RECORD_RESPONSE_MESSAGE_TEMPLATE_70_KRN.Tag);
            try
            {
                tlvResponse.Deserialize(ResponseData, 0);
            }
            catch
            {
                tlvResponse = TLV.Create(EMVTagsEnum.READ_RECORD_RESPONSE_MESSAGE_TEMPLATE_70_KRN.Tag);
                ResponseData = tlvResponse.Serialize();
            }
            Logger.Log(ToPrintString());
        }

        public TLVList GetResponseTags()
        {
            try
            {
                return tlvResponse.Children;
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

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

namespace DCEMV.EMVProtocol
{
    public class EMVPutDataRequest : EMVCommand
    {
        public EMVPutDataRequest(TLV tagToPut) : base(ISO7816Protocol.Cla.ProprietaryCla8x, EMVInstructionEnum.PutData, null, 0x00, 0x00)
        {
            ApduResponseType = typeof(EMVPutDataResponse);
            CommandData = tagToPut.Value;

            P1 = Formatting.HexStringToByteArray(tagToPut.Tag.TagLable.Substring(0,2))[0];
            P2 = Formatting.HexStringToByteArray(tagToPut.Tag.TagLable.Substring(2, 2))[0];

            Logger.Log(ToPrintString());
        }

        public override string ToPrintString()
        {
            string header = "Start ADPU Request: " + this.GetType().Name;
            string body = "P1: " + Formatting.ByteArrayToHexString(new byte[] { P1 }) + " P2: " + Formatting.ByteArrayToHexString(new byte[] { P2 });
            string footer = "End ADPU Request: " + this.GetType().Name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header).AppendLine(body).Append(footer);
            return sb.ToString();
        }
    }

    public class EMVPutDataResponse : EMVResponse
    {
        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
            Logger.Log(ToPrintString());
        }

        public TLVList GetResponseTags()
        {
            throw new Exception("No tags retured from this call");
        }
        protected override TLV GetTLVResponse()
        {
            return null;
        }
    }
}

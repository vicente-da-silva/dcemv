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
using DCEMV.FormattingUtils;
using System.Text;
using DCEMV.TLVProtocol;


namespace DCEMV.EMVProtocol
{
    public class EMVGetDataRequest : EMVCommand
    {
        public EMVGetDataRequest(byte[] tag) : base(ISO7816Protocol.Cla.ProprietaryCla8x, EMVInstructionEnum.GetData, null, 0x00, 0x00)
        {
            ApduResponseType = typeof(EMVGetDataResponse);

            if(tag.Length == 1)
            {
                P2 = tag[0];
            }
            else
            {
                P1 = tag[0];
                P2 = tag[1];
            }
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

    public class EMVGetDataResponse : EMVResponse
    {
        private TLV tlvResponse;

        public EMVGetDataResponse()
        {
        }

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
            if (!Succeeded) return;
            int pos = 0;
            TLV.T t = TLV.T.Create(ResponseData, ref pos);
            tlvResponse = TLV.Create(t.TagLable);
            tlvResponse.Deserialize(ResponseData,0);
            Logger.Log(ToPrintString());
        }

        public TLV GetResponseTag()
        {
            return tlvResponse;
        }

        public TLVList GetResponseTags()
        {
            TLVList ret = new TLVList();
            ret.AddToList(GetTLVResponse());
            return ret;
        }

        protected override TLV GetTLVResponse()
        {
            return tlvResponse;
        }
    }
}

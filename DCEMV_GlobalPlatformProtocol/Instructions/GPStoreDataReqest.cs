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

namespace DCEMV.GlobalPlatformProtocol
{
    public enum StoreDataRequestP1Enum
    {
        LastOrOnlyCommand = 0x80,
        MoreBlocks = 0x00,
        Nogeneralencryptioninformationornonencrypteddata = 0x00,
        Applicationdependentencryptionofthedata = 0x20,
        Encrypteddata = 0x60,
        Nogeneraldatastructureinformation=0x00,
        DGIformatofthecommanddatafield=0x08,
        BERTLVformatofthecommanddatafield=0x10,
        ISOcase3commandnoresponsedataexpected = 0x00,
        ISOcase4commandresponsedatamaybereturned=0x01,
    }
    public class GPStoreData
    {
        public byte[] DGI { get; set; }
        private byte DGILength { get; set; }
        DGIMeta DGIMeta { get; set; }
        public TLVList Data { get; set; }
        public byte[] DataBytes { get; set; }
        private byte[] MAC { get; set; }
        public byte DataBlock { get; set; }
        public bool IsLastBlock { get; set; }

        public virtual byte[] Serialize()
        {
            byte[] data;
            if (Data != null)
                data = Data.Serialize();
            else
                data = DataBytes;

            byte[] result = Formatting.ConcatArrays(
                DGI,
                new byte[] { BitConverter.GetBytes(data.Length)[0] },
                data
                );

            if (MAC != null)
                result = Formatting.ConcatArrays(result, MAC);

            return result;
        }
        public virtual void Deserialize(byte[] input)
        {
            int pos = 0;
            DGI = new byte[2];
            Array.Copy(input, pos, DGI, 0, 2);
            pos = pos + 2;

            DGILength = input[pos];
            pos++;

            DataBytes = new byte[DGILength];
            Array.Copy(input, pos, DataBytes, 0, DGILength);
            pos = pos + DGILength;

            DGIMeta = DGITagsList.GetMeta(Formatting.ByteArrayToHexString(DGI), DataBytes[0]);
            if (DGIMeta.IsTLVFormatted)
            {
                Data = new TLVList();
                Data.Deserialize(DataBytes);
            }

            if(pos != input.Length)
            {
                MAC = new byte[8];
                Array.Copy(input, pos, MAC, 0, 8);
                pos = pos + 8;
            }
        }

        public override string ToString()
        {
            bool asXML = false;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("{0}:{1}", "DGI:" +  Formatting.ByteArrayToHexString(DGI), DGIMeta.Description));
            int depth = 0;
            
            if (Data != null)
            {
                if (asXML)
                    sb.Append(TLVListXML.XmlSerialize(Data));
                else
                    sb.Append(Data.ToPrintString(ref depth));
            }
            else
                sb.Append(string.Format("{0,-30}:{1}", "DGIBytes", Formatting.ByteArrayToHexString(DataBytes)));

            return sb.ToString();
        }
    }

    public class GPStoreDataReqest : GPCommand
    {
        public GPStoreDataReqest()
            : base(ISO7816Protocol.Cla.ProprietaryCla8x, GPInstructionEnum.StoreData, null, 0x00, 0x00)
        {
            ApduResponseType = typeof(GPStoreDataResponse);
        }
        public GPStoreDataReqest(byte[] data, byte p1,byte p2) 
            : base(ISO7816Protocol.Cla.ProprietaryCla8x, GPInstructionEnum.StoreData, data, p1, p2)
        {
            ApduResponseType = typeof(GPStoreDataResponse);
        }
    }
    public class GPStoreDataResponse : GPResponse
    {
        //private TLV tlvResponse;

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
        }
       
        protected override TLV GetTLVResponse()
        {
            return null;// tlvResponse;
        }
    }
}

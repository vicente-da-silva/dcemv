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
using System;
using DCEMV.TLVProtocol;

namespace DCEMV.GlobalPlatformProtocol
{
    public class GPInitializeUpdateReqest : GPCommand
    {
        public GPInitializeUpdateReqest()
        {

        }

        public GPInitializeUpdateReqest(byte keyVersion, byte keyId, byte[] host_challenge)
            : base(ISO7816Protocol.Cla.ProprietaryCla8x, GPInstructionEnum.InitializeUpdate, host_challenge, keyVersion, keyId)
        {
            ApduResponseType = typeof(GPInitializeUpdateResponse);
        }
    }
    public class GPInitializeUpdateResponse : GPResponse
    {
        public byte[] KeyDiversificationData { get; set; }
        public byte KeyVersionNumber { get; set; }
        public byte SCPId { get; set; }
        public byte SCPI { get; set; }
        public byte[] CardChallenge { get; set; }
        public byte[] CardCryptogram { get; set; }
        public byte[] CardChallengeSeq { get; set; }

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);

            if (response.Length == 2)
                return;

            KeyDiversificationData = new byte[10];
            int pos = 0;
            Array.Copy(ResponseData, pos, KeyDiversificationData, 0, 10);
            pos = pos + 10;

            KeyVersionNumber = ResponseData[pos];
            pos++;
            SCPId = ResponseData[pos];
            pos++;

            if (SCPId == 1)
            {
                CardChallenge = new byte[8];
                CardCryptogram = new byte[8];
                Array.Copy(ResponseData, pos, CardChallenge, 0, 8);
                pos = pos + 8;
                Array.Copy(ResponseData, pos, CardCryptogram, 0, 8);
                pos = pos + 8;
            }
            if (SCPId == 2)
            {
                CardChallengeSeq = new byte[2];
                CardChallenge = new byte[6];
                CardCryptogram = new byte[8];
                
                Array.Copy(ResponseData, pos, CardChallengeSeq, 0, 2);
                pos = pos + 2;
                Array.Copy(ResponseData, pos, CardChallenge, 0, 6);
                pos = pos + 6;
                Array.Copy(ResponseData, pos, CardCryptogram, 0, 8);
                pos = pos + 6;
            }
            if (SCPId == 3)
            {
                SCPI = ResponseData[pos];
                pos++;
                CardChallenge = new byte[8];
                CardCryptogram = new byte[8];
                Array.Copy(ResponseData, pos, CardChallenge, 0, 8);
                pos = pos + 8;
                Array.Copy(ResponseData, pos, CardCryptogram, 0, 8);
                pos = pos + 8;
                if (ResponseData.Length == 32)
                {
                    CardChallengeSeq = new byte[3];
                    Array.Copy(ResponseData, pos, CardChallengeSeq, 0, 3);
                    pos = pos + 3;
                }
            }
        }
       
        protected override TLV GetTLVResponse()
        {
            return null;
        }
    }
}

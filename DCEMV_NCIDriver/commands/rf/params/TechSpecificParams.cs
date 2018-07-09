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
using System.Text;

namespace DCEMV.CardReaders.NCIDriver
{
    public abstract class TechSpecificParamsBase
    {
        /**
         * The ALL_REQ and SENS_REQ Commands are sent by the NFC Forum Device in Poll Mode to 
            probe the Operating Field for NFC Forum Devices in Listen Mode configured for NFC-A 
            Technology. 
         4.6.3  SENS_RES Response
            In response to an ALL_REQ and SENS_REQ Command from the NFC Forum Device in Poll 
            Mode, an NFC Forum Device in Listen Mode returns a SENS_RES Response with a length of 2
            bytes, depending on its state. 
         **/
        public byte[] SensRes { get; set; }

        public abstract byte deserialize(byte[] packet, byte pos);
        public abstract byte[] serialize();
    }

    public class TechSpecificParamsNFCAPollMode : TechSpecificParamsBase
    {
        public byte NfcIdLen { get; internal set; }
        public byte[] NfcId { get; set; }
        public byte SelResLen { get; internal set; }
        /**
         * Use the SEL_REQ Command to select the NFC Forum Device in Listen Mode by means of its
         * NFCID1.
         **/
        public byte[] SelRes { get; set; }

        public override byte deserialize(byte[] packet, byte pos)
        {
            SensRes = new byte[2];//always 2 for this class
            Array.Copy(packet, pos, SensRes, 0, 2);
            pos = (byte)(pos + 2);
            NfcIdLen = packet[pos];
            pos++;
            NfcId = new byte[NfcIdLen];
            Array.Copy(packet, pos, NfcId, 0, NfcIdLen);
            pos = (byte)(pos + NfcIdLen);
            SelResLen = packet[pos];
            pos++;
            SelRes = new byte[SelResLen];
            Array.Copy(packet, pos, SelRes, 0, SelResLen);
            pos = (byte)(pos + SelResLen);
            return pos;
        }

        public override byte[] serialize()
        {
            NfcIdLen = (byte)NfcId.Length;
            SelResLen = (byte)SelRes.Length;
            byte length = (byte)(2 + 1 + NfcIdLen + 1 + SelResLen);
            byte[] ret = new byte[length];
            byte pos = 0;
            Array.Copy(SensRes, 0, ret, pos, 2);
            pos = (byte)(pos + 2);
            ret[pos] = NfcIdLen;
            pos++;
            Array.Copy(NfcId, 0, ret, pos, NfcIdLen);
            pos = (byte)(pos + NfcIdLen);
            ret[pos] = SelResLen;
            pos++;
            Array.Copy(SelRes, 0, ret, pos, SelResLen);
            pos = (byte)(pos + SelResLen);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SENS_RES = " + BitConverter.ToString(SensRes, 0));
            sb.AppendLine("NFCID = " + BitConverter.ToString(NfcId, 0));
            if (SelResLen != 0)
                sb.AppendLine("SEL_RES = " + BitConverter.ToString(SelRes, 0));
            return sb.ToString();
        }
    }

    public class TechSpecificParamsNFCBPollMode : TechSpecificParamsBase
    {
        public override byte deserialize(byte[] packet, byte pos)
        {
            byte SensResLen = packet[0];
            pos++;
            SensRes = new byte[SensResLen];
            Array.Copy(packet, pos, SensRes, 0, SensResLen);
            pos = (byte)(pos + SensResLen);
            return pos;
        }

        public override byte[] serialize()
        {
            byte length = (byte)(1 + SensRes.Length);
            byte[] ret = new byte[length];
            ret[0] = (byte)SensRes.Length;
            byte pos = 1;
            Array.Copy(SensRes, 0, ret, pos, SensRes.Length);
            pos = (byte)(pos + SensRes.Length);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (SensRes.Length != 0)
                sb.AppendLine("SENS_RES = " + BitConverter.ToString(SensRes, 0));

            return sb.ToString();
        }
    }

    public class TechSpecificParamsNFCFPollMode : TechSpecificParamsBase
    {
        public byte BitRate { get; set; }

        public override byte deserialize(byte[] packet, byte pos)
        {
            BitRate = packet[0];
            pos++;
            byte SensResLen = packet[1];
            pos++;
            SensRes = new byte[SensResLen];
            Array.Copy(packet, pos, SensRes, 0, SensResLen);
            pos = (byte)(pos + SensResLen);
            return pos;
        }

        public override byte[] serialize()
        {
            byte length = (byte)(2 + SensRes.Length);
            byte[] ret = new byte[length];
            ret[0] = BitRate;
            ret[1] = (byte)SensRes.Length;
            byte pos = 2;
            Array.Copy(SensRes, 0, ret, pos, SensRes.Length);
            pos = (byte)(pos + SensRes.Length);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("\tBitrate = {0}", (BitRate == 1) ? "212" : "424"));
            if (SensRes.Length != 0)
                sb.AppendLine("\tSENS_RES = " + BitConverter.ToString(SensRes, 0));

            return sb.ToString();
        }
    }

    public class TechSpecificParamsNFCFListenMode : TechSpecificParamsBase
    {
        public byte[] NfcId { get; internal set; }

        public override byte deserialize(byte[] packet, byte pos)
        {
            byte NfcIdLen = packet[pos];
            pos++;
            NfcId = new byte[NfcIdLen];
            Array.Copy(packet, pos, NfcId, 0, NfcIdLen);
            pos = (byte)(pos + NfcIdLen);
            return pos;
        }

        public override byte[] serialize()
        {
            byte pos = 0;
            byte[] ret = new byte[NfcId.Length];
            ret[pos] = (byte)NfcId.Length;
            pos++;
            Array.Copy(NfcId, 0, ret, pos, NfcId.Length);
            pos = (byte)(pos + NfcId.Length);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\tNFCID = " + BitConverter.ToString(NfcId, 0));
            return sb.ToString();
        }
    }
}

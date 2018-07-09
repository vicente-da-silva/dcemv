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
    public abstract class ActivationParameterBase
    {
        public abstract byte deserialize(byte[] packet, byte pos);
        public abstract byte[] serialize();
    }

    public class ActivationParameterNFCA_ISODEP_POLL : ActivationParameterBase
    {
        public byte[] RATSResponse { get; set; }

        public override byte deserialize(byte[] packet, byte pos)
        {
            byte length = packet[pos];
            pos++;
            RATSResponse = new byte[length];
            Array.Copy(packet, pos, RATSResponse, 0, length);
            pos = (byte)(pos + length);
            return pos;
        }

        public override byte[] serialize()
        {
            byte[] ret = new byte[RATSResponse.Length + 1];
            ret[0] = (byte)RATSResponse.Length;
            Array.Copy(RATSResponse, 0, ret, 1, RATSResponse.Length);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("RATSResponse: " + BitConverter.ToString(RATSResponse,0));
            return sb.ToString();
        }
    }
    public class ActivationParameterNFCB_ISODEP_POLL : ActivationParameterBase
    {
        public byte[] ATTRIBResponse { get; set; }

        public override byte deserialize(byte[] packet, byte pos)
        {
            byte length = packet[pos];
            pos++;
            ATTRIBResponse = new byte[length];
            Array.Copy(packet, pos, ATTRIBResponse, 0, length);
            pos = (byte)(pos + length);
            return pos;
        }

        public override byte[] serialize()
        {
            byte[] ret = new byte[ATTRIBResponse.Length + 1];
            ret[0] = (byte)ATTRIBResponse.Length;
            Array.Copy(ATTRIBResponse, 0, ret, 1, ATTRIBResponse.Length);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ATTRIBResponse: " + BitConverter.ToString(ATTRIBResponse, 0));
            return sb.ToString();
        }
    }
    public class ActivationParameterNFCA_ISODEP_LISTEN : ActivationParameterBase
    {
        public byte RATSCommandParam { get; set; }

        public override byte deserialize(byte[] packet, byte pos)
        {
            RATSCommandParam = packet[pos];
            pos++;
            return pos;
        }

        public override byte[] serialize()
        {
            return new byte[] { RATSCommandParam };
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("RATSCommandParam: " + RATSCommandParam);
            return sb.ToString();
        }
    }
    public class ActivationParameterNFCB_ISODEP_LISTEN : ActivationParameterBase
    {
        public byte[] ATTRIBCommand { get; set; }

        public override byte deserialize(byte[] packet, byte pos)
        {
            byte length = packet[pos];
            pos++;
            ATTRIBCommand = new byte[length];
            Array.Copy(packet, pos, ATTRIBCommand, 0, length);
            pos = (byte)(pos + length);
            return pos;
        }

        public override byte[] serialize()
        {
            byte[] ret = new byte[ATTRIBCommand.Length + 1];
            ret[0] = (byte)ATTRIBCommand.Length;
            Array.Copy(ATTRIBCommand, 0, ret, 1, ATTRIBCommand.Length);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ATTRIBCommand: " + BitConverter.ToString(ATTRIBCommand, 0));
            return sb.ToString();
        }
    }
    public class ActivationParameterNFCA_NFCF__DEP_POLL: ActivationParameterBase
    {
        public byte[] ALTRESResponse { get; set; }

        public override byte deserialize(byte[] packet, byte pos)
        {
            byte length = packet[pos];
            pos++;
            ALTRESResponse = new byte[length];
            Array.Copy(packet, pos, ALTRESResponse, 0, length);
            pos = (byte)(pos + length);
            return pos;
        }

        public override byte[] serialize()
        {
            byte[] ret = new byte[ALTRESResponse.Length + 1];
            ret[0] = (byte)ALTRESResponse.Length;
            Array.Copy(ALTRESResponse, 0, ret, 1, ALTRESResponse.Length);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ALTRESResponse: " + BitConverter.ToString(ALTRESResponse, 0));
            return sb.ToString();
        }
    }
    public class ActivationParameterNFCA_NFCF__DEP_LISTEN : ActivationParameterBase
    {
        public byte[] ALTREQCommand { get; set; }

        public override byte deserialize(byte[] packet, byte pos)
        {
            byte length = packet[pos];
            pos++;
            ALTREQCommand = new byte[length];
            Array.Copy(packet, pos, ALTREQCommand, 0, length);
            pos = (byte)(pos + length);
            return pos;
        }

        public override byte[] serialize()
        {
            byte[] ret = new byte[ALTREQCommand.Length + 1];
            ret[0] = (byte)ALTREQCommand.Length;
            Array.Copy(ALTREQCommand, 0, ret, 1, ALTREQCommand.Length);
            return ret;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ALTREQCommand: " + BitConverter.ToString(ALTREQCommand, 0));
            return sb.ToString();
        }
    }
}

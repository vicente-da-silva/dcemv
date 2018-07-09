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
using System;
using System.Collections.Generic;
using System.Linq;

namespace DCEMV.ISO7816Protocol
{
    /// <summary>
    /// Class ApduCommand implments the ISO 7816 apdu commands
    /// </summary>
    public class ApduCommand
    {
        private bool encodeLength = true;

        public ApduCommand()
        {

        }
        public ApduCommand(byte cla, byte ins, byte p1, byte p2, byte[] commandData, byte? le, bool encodeLength)
        {
            if (commandData != null && commandData.Length > 255)
            {
                throw new NotImplementedException();
            }
            this.encodeLength = encodeLength;
            CLA = cla;
            INS = ins;
            P1 = p1;
            P2 = p2;
            CommandData = commandData;
            Le = le;

            ApduResponseType = typeof(ISO7816Protocol.ApduResponse);
        }
        public ApduCommand(byte cla, byte ins, byte p1, byte p2, byte[] commandData, byte? le)
        {
            if (commandData != null && commandData.Length > 255)
            {
                throw new NotImplementedException();
            }
            CLA = cla;
            INS = ins;
            P1 = p1;
            P2 = p2;
            CommandData = commandData;
            Le = le;

            ApduResponseType = typeof(ISO7816Protocol.ApduResponse);
        }
        /// <summary>
        /// Class of instructions
        /// </summary>
        public byte CLA { get; set; }
        /// <summary>
        /// Instruction code
        /// </summary>
        public byte INS { get; set; }
        /// <summary>
        /// Instruction parameter 1
        /// </summary>
        public byte P1 { get; set; }
        /// <summary>
        /// Instruction parameter 2
        /// </summary>
        public byte P2 { get; set; }
        /// <summary>
        /// Maximum number of bytes expected in the response ot this command
        /// </summary>
        public byte? Le { get; set; }
        /// <summary>
        /// Contiguous array of bytes representing commands data
        /// </summary>
        public byte[] CommandData { get; set; }
        /// <summary>
        /// Expected response type for this command.
        /// Provides mechanism to bind commands to responses
        /// </summary>
        public Type ApduResponseType { set; get; }
        /// <summary>
        /// Packs the current command into contiguous buffer bytes
        /// </summary>
        /// <returns>
        /// buffer holds the current wire/air format of the command
        /// </returns>
        public virtual byte[] Serialize()
        {
            List<byte[]> result = new List<byte[]>
            {
                new byte[] { CLA },
                new byte[] { INS },
                new byte[] { P1 },
                new byte[] { P2 }
            };
            if (encodeLength)
            {
                if (CommandData != null && CommandData.Length > 0)
                {
                    result.Add(new byte[] { (byte)CommandData.Length });
                    result.Add(CommandData);
                }
            }

            if (Le != null)
            {
                result.Add(new byte[] { (byte)Le });
            }

            return result.SelectMany(x => x).ToArray();
        }
        public virtual void Deserialize(byte[] input)
        {
            CLA = input[0];
            INS = input[1];
            P1 = input[2];
            P2 = input[3];

            int length = input[4];
            CommandData = new byte[length];
            Array.Copy(input, 5, CommandData, 0, CommandData.Length);

            if(5 + CommandData.Length < input.Length)
                Le = input[5 + CommandData.Length];
        }
        /// <summary>
        /// Helper method to print the command in a readable format
        /// </summary>
        /// <returns>
        /// return string formatted command
        /// </returns>
        public override string ToString()
        {
            return "ApduCommand CLA=" + CLA.ToString("X2") + ",INS=" + INS.ToString("X2") + ",P1=" + P1.ToString("X2") + ",P2=" + P2.ToString("X2") + ((CommandData != null && CommandData.Length > 0) ? (",Data=" + BitConverter.ToString(CommandData).Replace("-", "")) : "");
        }
    }
    /// <summary>
    /// Class ApduResponse implments handler for the ISO 7816 apdu response
    /// </summary>
    public class ApduResponse
    {
        public const byte TAG_MULTI_BYTE_MASK = 0x1F;
        public const byte TAG_COMPREHENSION_MASK = 0x80;
        public const byte TAG_LENGTH_MULTI_BYTE_MASK = 0x80;

        /// <summary>
        /// Class constructor
        /// </summary>
        public ApduResponse() { }
        /// <summary>
        /// method to extract the response data, status and qualifier
        /// </summary>
        /// <param name="response"></param>
        public virtual void Deserialize(byte[] response)
        {
            if (response.Length < 2)
            {
                throw new InvalidOperationException("APDU response must be at least 2 bytes");
            }

            ResponseData = new byte[response.Length - 2];
            Array.Copy(response, 0, ResponseData, 0, ResponseData.Length);

            SW1 = response[response.Length-2];
            SW2 = response[response.Length-1];

            //using (DataReader reader = DataReader.FromBuffer(response))
            //{
            //    ResponseData = new byte[response.Length - 2];
            //    reader.ReadBytes(ResponseData);
            //    SW1 = reader.ReadByte();
            //    SW2 = reader.ReadByte();
            //}
        }
        public virtual byte[] Serialize()
        {
            List<byte[]> result = new List<byte[]>
            {
                ResponseData,
                new byte[] { SW1 },
                new byte[] { SW2 }
            };
            return result.SelectMany(x => x).ToArray();

            //using (DataWriter writer = new DataWriter())
            //{
            //    writer.WriteBytes(ResponseData);
            //    writer.WriteByte(SW1);
            //    writer.WriteByte(SW2);

            //    return writer.DetachBuffer();
            //}
        }
        /// <summary>
        /// Detects if the command has succeeded
        /// </summary>
        /// <returns></returns>
        public virtual bool Succeeded
        {
            get
            {
                return SW == 0x9000;
            }
        }
        /// <summary>
        /// command processing status
        /// </summary>
        public byte SW1 { get; set; }
        /// <summary>
        /// command processing qualifier
        /// </summary>
        public byte SW2 { get; set; }

        public byte[] SW12 { get { return new byte[] { SW1, SW2 }; } }
        /// <summary>
        /// Wrapper property to read both response status and qualifer
        /// </summary>
        public ushort SW
        {
            get
            {
                return (ushort)(((ushort)SW1 << 8) | (ushort)SW2);
            }
            set
            {
                SW1 = (byte)(value >> 8);
                SW2 = (byte)(value & 0xFF);
            }
        }
        /// <summary>
        /// Response data
        /// </summary>
        public byte[] ResponseData { get; set; }
        /// <summary>
        /// Mapping response status and qualifer to human readable format
        /// </summary>
        public virtual string SWTranslation
        {
            get
            {
                switch (SW)
                {
                    case 0x9000:
                        return "Success";

                    case 0x6700:
                        return "Incorrect length or address range error";

                    case 0x6800:
                        return "The requested function is not supported by the card";

                    default:
                        return "Unknown";
                }
            }
        }
        /// <summary>
        /// Helper method to print the response in a readable format
        /// </summary>
        /// <returns>
        /// return string formatted response
        /// </returns>
        public override string ToString()
        {
            return "ApduResponse SW=" + SW.ToString("X4") + " (" + SWTranslation + ")" + ((ResponseData != null && ResponseData.Length > 0) ? (",Data=" + BitConverter.ToString(ResponseData).Replace("-", "")) : "");
        }
    }

    public enum ReturnType
    {
        Info,
        Warning,
        Error,
        Security
    }

    public class AdpuReturnResult : EnumBase
    {
        public byte SW1 { get; }
        public SW2Range SW2 { get; }
        public string Description { get; }
        public ReturnType ReturnType { get; }

        public AdpuReturnResult(byte sw1, SW2Range sw2, ReturnType returnType, string description)
        {
            this.SW1 = sw1;
            this.SW2 = sw2;
            this.ReturnType = returnType;
            this.Description = description;
        }
    }

    public class SW2Range
    {
        byte Start { get; }
        byte End { get; }

        public SW2Range(byte start, byte end)
        {
            this.Start = start;
            this.End = end;
        }

        public SW2Range(byte single)
        {
            this.Start = single;
            this.End = single;
        }

        public bool inRange(byte val)
        {
            if (val >= Start && val <= End)
                return true;
            else
                return false;
        }
    }
}

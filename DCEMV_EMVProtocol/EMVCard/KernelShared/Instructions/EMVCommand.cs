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
using System.Text;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol
{
    public abstract class EMVCommand : ISO7816Protocol.ApduCommand
    {
        public static Logger Logger = new Logger(typeof(EMVCommand));

        public byte Instruction
        {
            set { base.INS = value; }
            get { return base.INS; }
        }
        public byte[] Data
        {
            set { base.CommandData = value; }
            get { return base.CommandData; }
        }
        

        public EMVCommand(EMVInstructionEnum ins, byte[] data, byte p1, byte p2)
            : base((byte)ISO7816Protocol.Cla.CompliantCmd0x, (byte)ins, p1, p2, data, 0x00)
        {
            ApduResponseType = typeof(EMVResponse);
        }

        public EMVCommand(ISO7816Protocol.Cla cla, EMVInstructionEnum ins, byte[] data, byte p1, byte p2)
            : base((byte)cla, (byte)ins, p1, p2, data, 0x00)
        {
            ApduResponseType = typeof(EMVResponse);
        }

        public virtual string ToPrintString()
        {
            string header = "Start ADPU Request: " + this.GetType().Name;
            string body = CommandData == null ? "No Data" : Formatting.ByteArrayToHexString(CommandData);
            string footer = "End ADPU Request: " + this.GetType().Name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header).AppendLine(body).Append(footer);
            return sb.ToString();
        }
    }

    public abstract class EMVResponse : ISO7816Protocol.ApduResponse
    {
        public static Logger Logger = new Logger(typeof(EMVResponse));

        public EMVResponse()
            : base()
        { }

        public override string SWTranslation
        {
            get
            {
                return EMVReturnCodesEnum.FindReturnCode(SW1,SW2).Description;
            }
        }
        public override bool Succeeded
        {
            get
            {
                return SW == 0x9000;
            }
        }
        public bool SubsequentFrame
        {
            get
            {
                return SW == 0x91AF;
            }
        }
        public bool BoundaryError
        {
            get
            {
                return SW == 0x91BE;
            }
        }

        public virtual string ToPrintString()
        {
            int depth = 0;
            string header = "Start ADPU Response:" + this.GetType().Name;
            string status = "Status: " + SWTranslation;
            string body;
            if(GetTLVResponse() == null)
            {
                if (ResponseData == null || ResponseData.Length == 0)
                {
                    body = "[No tags - No Data]";
                }
                else
                {
                    body = "[No tags - Raw Data:" + Formatting.ByteArrayToHexString(ResponseData) + "]";
                }
            }
            else
            {
                body = GetTLVResponse().ToPrintString(ref depth);
            }
            string footer = "End ADPU Response:" + this.GetType().Name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header).AppendLine(status).AppendLine(body).Append(footer);
            return sb.ToString();
        }
        
        protected abstract TLV GetTLVResponse();
    }
}

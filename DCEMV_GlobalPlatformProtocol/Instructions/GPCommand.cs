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

namespace DCEMV.GlobalPlatformProtocol
{
    public enum GPInstructionEnum : byte
    {
        Install = 0xE6,
        Load = 0xE8,
        StoreData = 0xE2,
        Delete = 0xE4,
        InitializeUpdate = 0x50,
        ExternalAuthenticate = 0x82,
        GetStatus = 0xF2,
        Select = 0xA4,
        GetData = 0xCA,
        SetStatus = 0xF0,
    };

    public abstract class GPCommand : ISO7816Protocol.ApduCommand
    {
        public static Logger Logger = new Logger(typeof(GPCommand));

        public GPCommand()
        {
            
        }

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


        //public GPCommand(GPInstructionEnum ins, byte[] data, byte p1, byte p2)
        //    : base((byte)ISO7816Protocol.Cla.CompliantCmd0x, (byte)ins, p1, p2, data, 0x00)
        //{
        //    ApduResponseType = typeof(GPResponse);
        //}
        
        public GPCommand(ISO7816Protocol.Cla cla, GPInstructionEnum ins, byte[] data, byte p1, byte p2, byte le = 0x00)
            : base((byte)cla, (byte)ins, p1, p2, data, le)
        {
            ApduResponseType = typeof(GPResponse);
        }

        public virtual string ToPrintString()
        {
            string header = "+--- Start ADPU Request: " + this.GetType().Name;
            header = string.Format("{0} [Cla:{1:X2} Ins:{2:X2} P1:{3:X2} P2:{4:X2} Le:{5:X2}]", header, CLA, INS, P1, P2, Le);
            string body =   "|    Command Data: " + Formatting.ByteArrayToHexString(CommandData);
            string footer = "+--- End ADPU Request: " + this.GetType().Name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header).AppendLine(body).AppendLine(footer);
            return sb.ToString();
        }
    }

    public abstract class GPResponse : ISO7816Protocol.ApduResponse
    {
        public static Logger Logger = new Logger(typeof(GPResponse));

        public GPResponse()
            : base()
        { }

        public override string SWTranslation
        {
            get
            {
                return EMVReturnCodesEnum.FindReturnCode(SW1, SW2).Description;
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
            string body = GetTLVResponse() == null ? "No tags retured from this call" : GetTLVResponse().ToPrintString(ref depth);
            string footer = "End ADPU Response:" + this.GetType().Name;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(header).AppendLine(body).Append(footer);
            return sb.ToString();
        }

        protected abstract TLV GetTLVResponse();
    }
}

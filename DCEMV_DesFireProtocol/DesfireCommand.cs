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
namespace DCEMV.DesFireProtocol
{
    /// <summary>
    /// Class DesfireCommand extends the Iso7816.ApduCommand and provides
    /// mappings to Iso7816 command fields
    /// </summary>
    public class DesfireCommand : ISO7816Protocol.ApduCommand
    {
        public byte Command
        {
            set { base.INS = value; }
            get { return base.INS; }
        }
        public byte[] Data
        {
            set { base.CommandData = value; }
            get { return base.CommandData; }
        }
        public enum CommandType : byte
        {
            GetVersion = 0x60,
            GetAdditionalFrame = 0xAF,
            SelectApplication = 0x5A,
            ReadData = 0xBD,
            ReadRecord = 0xBB,
            AuthenticateAES = 0xAA,
            CreateApplication = 0xCA,
            CreateStdDataFile = 0xCD,
            WriteData = 0x3D,
            GetFileIDs = 0x6F,
            GetApplicationIDs = 0x6A,
            GetKeySettings = 0x45,
            GetKeyVersion = 0x64,
        };

        public DesfireCommand()
            : base((byte)ISO7816Protocol.Cla.ProprietaryCla9x, 0, 0, 0, null, 0)
        {
            ApduResponseType = typeof(DesFireProtocol.DesfireResponse);
        }
        public DesfireCommand(CommandType cmd, byte[] data)
            : base((byte)ISO7816Protocol.Cla.ProprietaryCla9x, (byte)cmd, 0x00, 0x00, data, 0x00)
        {
            ApduResponseType = typeof(DesFireProtocol.DesfireResponse);
        }
    }
    /// <summary>
    /// Class DesfireResponse extends the Iso7816.ApduResponse.
    /// </summary>
    public class DesfireResponse : ISO7816Protocol.ApduResponse
    {
        public DesfireResponse()
            : base()
        { }
        public override string SWTranslation
        {
            get
            {
                if (SW1 != 0x91)
                {
                    return "Unknown";
                }
                switch (SW2)
                {
                    case 0x00:
                        return "Success";

                    case 0xAF:
                        return "Additional frames expected";

                    default:
                        return "Unknown";
                }
            }
        }
        public override bool Succeeded
        {
            get
            {
                return SW == 0x9100;
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
    }
}

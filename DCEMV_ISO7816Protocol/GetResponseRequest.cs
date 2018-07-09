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
namespace DCEMV.ISO7816Protocol
{
    public class GetResponseRequest : ISO7816Protocol.ApduCommand
    {
        private byte length;
        public GetResponseRequest(byte length) : base((byte)Cla.CompliantCmd0x, 0xC0, 0x00, 0x00, new byte[] { length }, length, false)
        {
            this.length = length;

            ApduResponseType = typeof(GetResponseResponse);
        }
    }

    public class GetResponseResponse : ISO7816Protocol.ApduResponse
    {
        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
        }
    }
}

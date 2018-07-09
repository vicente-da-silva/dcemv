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
using System.Collections;

namespace DCEMV.DesFireProtocol
{
    public class AccessRights
    {
        public byte ChangeAccess { get; set; }
        public byte ReadWriteAccess { get; set; }
        public byte WriteAccess { get; set; }
        public byte ReadAccess { get; set; }

        public byte[] getValue()
        {
            BitArray ca = new BitArray(new byte[] { ChangeAccess });
            BitArray rwa = new BitArray(new byte[] { ReadWriteAccess });
            BitArray wa = new BitArray(new byte[] { WriteAccess });
            BitArray ra = new BitArray(new byte[] { ReadAccess });

            BitArray upper = new BitArray(new bool[] {
                ra.Get(3),
                ra.Get(2),
                ra.Get(1),
                ra.Get(0),
                wa.Get(3),
                wa.Get(2),
                wa.Get(1),
                wa.Get(0),
                
            });

            BitArray lower = new BitArray(new bool[] {
                rwa.Get(3),
                rwa.Get(2),
                rwa.Get(1),
                rwa.Get(0),
                ca.Get(3),
                ca.Get(2),
                ca.Get(1),
                ca.Get(0),
            });

            return new byte[] { Util.ConvertToByte(upper) , Util.ConvertToByte(lower)};
        }
    }
}

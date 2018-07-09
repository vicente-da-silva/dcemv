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
using System.IO;

namespace DCEMV.GlobalPlatformProtocol
{
    public class ByteArrayOutputStream : IDisposable
    {
        private MemoryStream ms;
        private BinaryWriter br;

        public ByteArrayOutputStream() 
        {
            ms = new MemoryStream();
            br = new BinaryWriter(ms);
        }

        public void Reset()
        {
            br.Dispose();
            ms.Dispose();

            ms = new MemoryStream();
            br = new BinaryWriter(ms);
        }

        public void Write(byte[] input)
        {
            br.Write(input);
        }
        public void Write(byte[] input, int offset, int length)
        {
            br.Write(input, offset, length);
        }
        public void Write(byte input)
        {
            br.Write(input);
        }
        public void Write(int input)
        {
            br.Write(input);
        }
        public byte[] ToByteArray()
        {
            return ms.ToArray();
        }

        public int Size()
        {
            return this.ToByteArray().Length;
        }

        public void Dispose()
        {
            br.Dispose();
            ms.Dispose();
        }
    }
}

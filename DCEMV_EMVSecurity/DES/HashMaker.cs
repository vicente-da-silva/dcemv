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
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using System;

namespace DCEMV.EMVSecurity
{
    public class HashMaker
    {
        private IDigest digest;

        public HashMaker(DigestMode type)
        {
            switch (type)
            {
                case DigestMode.SHA1:
                    digest = new Sha1Digest();
                    break;

                default:
                    throw new Exception("Unknown DigestMode:" + type);
            }
        }
        public static HashMaker GetInstance(DigestMode type)
        {
            return new HashMaker(type);
        }

        public byte[] Digest(byte[] data)
        {
            byte[] outBff = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(data, 0, data.Length);
            digest.DoFinal(outBff, 0);
            return outBff;
        }
    }
}

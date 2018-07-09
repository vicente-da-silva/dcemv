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

namespace DCEMV.EMVSecurity
{
    public interface IKey
    {
        String GetAlgorithm();
        String GetFormat();
        byte[] GetEncoded();
    }
    public interface ISecretKey : IKey
    {
    }
    public class SecretKeySpec : ISecretKey
    {
        private byte[] key;
        private String algorithm;

        public SecretKeySpec(byte[] var1, String var2)
        {
            if (var1 != null && var2 != null)
            {
                if (var1.Length == 0)
                {
                    throw new Exception("Empty key");
                }
                else
                {
                    this.key = (byte[])var1.Clone();
                    this.algorithm = var2;
                }
            }
            else
            {
                throw new Exception("Missing argument");
            }
        }

        public String GetAlgorithm()
        {
            return this.algorithm;
        }

        public String GetFormat()
        {
            return "RAW";
        }

        public byte[] GetEncoded()
        {
            return (byte[])this.key.Clone();
        }


    }
}

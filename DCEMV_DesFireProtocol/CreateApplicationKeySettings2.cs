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
using System.Collections;

namespace DCEMV.DesFireProtocol
{
    public enum CryptoMethodEnum
    {
        Crypto_DESAnd2K3DES, //00
        Crypto_3K3DES, //01
        Crypto_AES, //10
    }
    public class CreateApplicationKeySettings2
    {
        public byte NumberOfKeysThatCanbeStoredinApplicationForCryptographicPurposes { get; set; }
        public bool TwoByteFileIdentifiersSupported { get; set; }
        public CryptoMethodEnum CryptoMethod { get; set; }

        public byte getValue()
        {
            if (NumberOfKeysThatCanbeStoredinApplicationForCryptographicPurposes > 14)
            {
                throw new Exception("NumberOfKeysThatCanbeStoredinApplicationForCryptographicPurposes > 14");
            }

            BitArray val_0_3 = new BitArray(new byte[] { NumberOfKeysThatCanbeStoredinApplicationForCryptographicPurposes });
            BitArray val_6_7 = new BitArray(2);
            switch (CryptoMethod)
            {
                case CryptoMethodEnum.Crypto_DESAnd2K3DES:
                    val_6_7.Set(1, false);//msb
                    val_6_7.Set(0, false);//lsb
                    break;
                case CryptoMethodEnum.Crypto_3K3DES:
                    val_6_7.Set(1, false);
                    val_6_7.Set(0, true);
                    break;

                case CryptoMethodEnum.Crypto_AES:
                    val_6_7.Set(1, true);
                    val_6_7.Set(0, false);
                    break;

                default:
                    throw new Exception("Invalid CryptoMethod");
            }

            BitArray ba = new BitArray(new bool[] {
                val_6_7.Get(1),
                val_6_7.Get(0),
                TwoByteFileIdentifiersSupported,
                false,//RFU
                val_0_3.Get(3),
                val_0_3.Get(2),
                val_0_3.Get(1),
                val_0_3.Get(0),
            });
            return Util.ConvertToByte(ba);
        }
    }
}

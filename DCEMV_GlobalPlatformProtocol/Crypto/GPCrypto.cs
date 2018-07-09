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
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using System;

namespace DCEMV.GlobalPlatformProtocol
{
    public class GPCrypto
    {
        public static byte[] null_bytes_8 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static byte[] null_bytes_16 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static byte[] one_bytes_16 = new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 };

        // List of used ciphers.
        public static String DES3_CBC_CIPHER = "DESede/CBC/NoPadding";
        public static String DES3_ECB_CIPHER = "DESede/ECB/NoPadding";
        public static String DES_CBC_CIPHER = "DES/CBC/NoPadding";
        public static String DES_ECB_CIPHER = "DES/ECB/NoPadding";
        public static String AES_CBC_CIPHER = "AES/CBC/NoPadding";

        //public static IvParameterSpec iv_null_des = new IvParameterSpec(null_bytes_8);
        //public static IvParameterSpec iv_null_aes = new IvParameterSpec(null_bytes_16);

        public static byte[] Kcv_3des(GPKey key)
        {
            try
            {
                byte[] check = DoEncrypt_DES3_ECB(key.GetKey().GetEncoded(), null_bytes_8);
                //Cipher cipher = Cipher.getInstance("DESede/ECB/NoPadding");
                //cipher.init(Cipher.ENCRYPT_MODE, key.getKey());
                //byte check[] = cipher.doFinal(GPCrypto.null_bytes_8);
                return Arrays.CopyOf(check, 3);
            }
            catch (Exception e)
            {
                throw new Exception("Could not calculate KCV", e);
            }
        }

        public static byte[] DoEncrypt_DES_ECB(byte[] key, byte[] data)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher(DES_ECB_CIPHER);
            ICipherParameters keyparam = new KeyParameter(key);
            cipher.Init(true, keyparam);
            return cipher.DoFinal(data);
        }
        public static byte[] DoEncrypt_DES3_ECB(byte[] key, byte[] data)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher(DES3_ECB_CIPHER);
            ICipherParameters keyparam = new KeyParameter(key);
            cipher.Init(true, keyparam);
            return cipher.DoFinal(data);
        }
        public static byte[] DoEncrypt_DES_CBC(byte[] key, byte[] data, int offset, int length, byte[] iv = null)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher(DES_CBC_CIPHER);
            ICipherParameters keyparam = new KeyParameter(key);

            if (iv == null)
                iv = GPCrypto.null_bytes_8;

            ICipherParameters keyparamIV = new ParametersWithIV(keyparam, iv);
            cipher.Init(true, keyparamIV);
            return cipher.DoFinal(data, offset, length);
        }
        public static byte[] DoEncrypt_DES3_CBC(byte[] key, byte[] data, int offset, int length, byte[] iv = null)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher(DES3_CBC_CIPHER);
            ICipherParameters keyparam = new KeyParameter(key);

            if (iv == null)
                iv = GPCrypto.null_bytes_8;

            ICipherParameters keyparamIV = new ParametersWithIV(keyparam, iv);
            cipher.Init(true, keyparamIV);
            return cipher.DoFinal(data, offset, length);
        }
        public static byte[] DoEncrypt_DES3_CBC(byte[] key, byte[] data, byte[] iv = null)
        {
            return DoEncrypt_DES3_CBC(key, data, 0, data.Length);
        }
        public static byte[] DoEncrypt_AES_CBC(byte[] key, byte[] data, byte[] iv = null)
        {
            IBufferedCipher c = CipherUtilities.GetCipher(AES_CBC_CIPHER);
            ICipherParameters keyparam = new KeyParameter(key);

            if (iv == null)
                iv = GPCrypto.null_bytes_16;

            ICipherParameters ivparam = new ParametersWithIV(keyparam, iv);
            c.Init(true, keyparam);
            return c.DoFinal(data);
        }
    }
}

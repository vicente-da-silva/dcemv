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
using DCEMV.FormattingUtils;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;

namespace DCEMV.EMVSecurity
{
    public class JCEHandler
    {
        private const String ALG_TRIPLE_DES = "DESede";
        private const String ALG_DES = "DES";
        private const String DES_NO_PADDING = "NoPadding";
        
        public static IKey GenerateDESKey(short keyLength)
        {
            CipherKeyGenerator cipherKeyGenerator = new CipherKeyGenerator();
            cipherKeyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), keyLength));
            byte[] keyDES3 = cipherKeyGenerator.GenerateKey();

            return FormDESKey(keyLength, keyDES3);
        }

        public static byte[] EncryptData(byte[] data, IKey key)
        {
            return DoCryptStuff(data, key, CipherDirection.ENCRYPT_MODE);
        }

        public static byte[] DecryptData(byte[] encryptedData, IKey key)
        {
            return DoCryptStuff(encryptedData, key, CipherDirection.DECRYPT_MODE);
        }

        public static byte[] EncryptDataCBC(byte[] data, IKey key, byte[] iv)
        {
            return DoCryptStuff(data, key, CipherDirection.ENCRYPT_MODE, CipherMode.CBC, iv);
        }

        public static byte[] DecryptDataCBC(byte[] encryptedData, IKey key, byte[] iv)
        {
            return DoCryptStuff(encryptedData, key, CipherDirection.DECRYPT_MODE, CipherMode.CBC, iv);
        }

        private static byte[] DoCryptStuff(byte[] data, IKey key, CipherDirection direction)
        {
            return DoCryptStuff(data, key, direction, CipherMode.ECB, null);
        }

        public static byte[] ExtractDESKeyMaterial(short keyLength, IKey clearDESKey)
        {
            String keyAlg = clearDESKey.GetAlgorithm();
            String keyFormat = clearDESKey.GetFormat();
            if (keyFormat.CompareTo("RAW") != 0)
            {
                throw new Exception("Unsupported DES key encoding format: " + keyFormat);
            }
            if (!keyAlg.StartsWith(ALG_DES))
            {
                throw new Exception("Unsupported key algorithm: " + keyAlg);
            }
            byte[] clearKeyBytes = clearDESKey.GetEncoded();
            clearKeyBytes = Util.Trim(clearKeyBytes, GetBytesLength(keyLength));
            return clearKeyBytes;
        }

        private static byte[] DoCryptStuff(byte[] data, IKey key, CipherDirection direction, CipherMode cipherMode, byte[] iv)
        {
            byte[] result;
            String transformation = key.GetAlgorithm();
            if (key.GetAlgorithm().StartsWith(ALG_DES))
                transformation += "/" + ModetoString(cipherMode) + "/" + DES_NO_PADDING;

            ICipherParameters keyparam = new KeyParameter(key.GetEncoded());
            IBufferedCipher cipher = CipherUtilities.GetCipher(transformation);

            if (cipherMode != CipherMode.ECB)
                keyparam = new ParametersWithIV(keyparam, iv);

            byte[] output = new byte[cipher.GetOutputSize(data.Length)];
            cipher.Init(direction == CipherDirection.ENCRYPT_MODE ? true : false, keyparam);
            result = cipher.DoFinal(data);

            if (cipherMode != CipherMode.ECB)
                Array.Copy(result, result.Length - 8, iv, 0, iv.Length);

            //AlgorithmParameterSpec aps = null;
            //try
            //{
            //    Cipher c1 = Cipher.getInstance(transformation, provider.getName());
            //    if (cipherMode != CipherMode.ECB)
            //        aps = new IvParameterSpec(iv);
            //    c1.init(direction, key, aps);
            //    result = c1.doFinal(data);
            //    if (cipherMode != CipherMode.ECB)
            //        System.arraycopy(result, result.length - 8, iv, 0, iv.length);
            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}
            return result;
        }

        private static String ModetoString(CipherMode cipherMode)
        {
            switch (cipherMode)
            {
                case CipherMode.ECB:
                    return "ECB";
                case CipherMode.CBC:
                    return "CBC";
                case CipherMode.CFB8:
                    return "CFB8";
                case CipherMode.CFB64:
                    return "CFB64";
                default:
                    throw new Exception("Unsupported cipher mode " + cipherMode);
            }
        }

        public static IKey FormDESKey(short keyLength, byte[] clearKeyBytes)
        {
            IKey key = null;
            switch (keyLength)
            {
                case SMAdapter.LENGTH_DES:
                    key = new SecretKeySpec(clearKeyBytes, ALG_DES);
                    break;
                case SMAdapter.LENGTH_DES3_2KEY:
                    // make it 3 components to work with JCE
                    clearKeyBytes = Formatting.ConcatArrays(clearKeyBytes, 0, GetBytesLength(SMAdapter.LENGTH_DES3_2KEY), clearKeyBytes, 0, GetBytesLength(SMAdapter.LENGTH_DES));
                    key = new SecretKeySpec(clearKeyBytes, ALG_TRIPLE_DES);
                    break;
                case SMAdapter.LENGTH_DES3_3KEY:
                    key = new SecretKeySpec(clearKeyBytes, ALG_TRIPLE_DES);
                    break;
            }
            if (key == null)
                throw new Exception("Unsupported DES key length: " + keyLength + " bits");
            return key;
        }

        public static int GetBytesLength(short keyLength)
        {
            int bytesLength = 0;
            switch (keyLength)
            {
                case SMAdapter.LENGTH_DES:
                    bytesLength = 8;
                    break;
                case SMAdapter.LENGTH_DES3_2KEY:
                    bytesLength = 16;
                    break;
                case SMAdapter.LENGTH_DES3_3KEY:
                    bytesLength = 24;
                    break;
                default:
                    throw new Exception("Unsupported key length: " + keyLength + " bits");
            }
            return bytesLength;
        }
    }
}

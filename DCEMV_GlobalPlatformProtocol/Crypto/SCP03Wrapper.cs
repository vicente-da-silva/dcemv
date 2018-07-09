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
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;

namespace DCEMV.GlobalPlatformProtocol
{
    public class SCP03Wrapper : SCPWrapper
    {
        // Both are block size length
        byte[] chaining_value = new byte[16];
        byte[] encryption_counter = new byte[16];

        public SCP03Wrapper(GPKeySet sessionKeys, SCPVersions scp, List<APDUMode> securityLevel, byte[] icv, byte[] ricv, int bs)
        {
            this.sessionKeys = sessionKeys;
            this.blockSize = bs;
            // initialize chaining value.
            Array.Copy(GPCrypto.null_bytes_16, 0, chaining_value, 0, GPCrypto.null_bytes_16.Length);
            // initialize encryption counter.
            Array.Copy(GPCrypto.null_bytes_16, 0, encryption_counter, 0, GPCrypto.null_bytes_16.Length);

            SetSecurityLevel(securityLevel);
        }

        public override byte[] Wrap(GPCommand command)
        {
            byte[] cmd_mac = null;

            try
            {
                if (!mac && !enc)
                {
                    return command.Serialize();
                }

                int cla = command.CLA;
                int lc = command.CommandData.Length;
                byte[] data = command.CommandData;

                // Encrypt if needed
                if (enc)
                {
                    cla = 0x84;
                    // Counter shall always be incremented
                    Buffer_increment(encryption_counter);
                    if (command.CommandData.Length > 0)
                    {
                        byte[] d = Pad80(command.CommandData, 16);
                        // Encrypt with S-ENC, after increasing the counter
                        byte[] iv = GPCrypto.DoEncrypt_AES_CBC(sessionKeys.GetKeyFor(KeySessionType.ENC).GetEncoded(), encryption_counter);
                        data = GPCrypto.DoEncrypt_AES_CBC(sessionKeys.GetKeyFor(KeySessionType.ENC).GetEncoded(), d, iv);
                        
                        //Cipher c = Cipher.getInstance(GPCrypto.AES_CBC_CIPHER);
                        //c.init(Cipher.ENCRYPT_MODE, sessionKeys.getKeyFor(KeySessionType.ENC), GPCrypto.null_bytes_16);
                        //byte[] iv = c.doFinal(encryption_counter);

                        // Now encrypt the data with S-ENC.
                        //c.init(Cipher.ENCRYPT_MODE, sessionKeys.getKeyFor(KeySessionType.ENC), new IvParameterSpec(iv));
                        //data = c.doFinal(d);
                        lc = data.Length;
                    }
                }
                // Calculate C-MAC
                if (mac)
                {
                    cla = 0x84;
                    lc = lc + 8;

                    ByteArrayOutputStream bo = new ByteArrayOutputStream();
                    bo.Write(chaining_value);
                    bo.Write(cla);
                    bo.Write(command.INS);
                    bo.Write(command.P1);
                    bo.Write(command.P2);
                    bo.Write(lc);
                    bo.Write(data);
                    byte[] cmac_input = bo.ToByteArray();
                    byte[] cmac = Scp03_mac(sessionKeys.GetKey(KeySessionType.MAC), cmac_input, 128);
                    // Set new chaining value
                    Array.Copy(cmac, 0, chaining_value, 0, chaining_value.Length);
                    // 8 bytes for actual mac
                    cmd_mac = Arrays.CopyOf(cmac, 8);
                }
                // Construct new command
                ByteArrayOutputStream na = new ByteArrayOutputStream();
                na.Write(cla); // possibly fiddled
                na.Write(command.INS);
                na.Write(command.P1);
                na.Write(command.P2);
                na.Write(lc);
                na.Write(data);
                if (mac)
                    na.Write(cmd_mac);
                return na.ToByteArray();
            }
            catch (Exception e)
            {
                throw new Exception("APDU wrapping failed", e);
            }
        }

        public override byte[] Unwrap(GPResponse response)
        {
            return response.ResponseData;
        }

        // GP 2.2.1 Amendment D v 1.1.1
        public static byte[] Scp03_kdf(GPKey key, byte constant, byte[] context, int blocklen_bits)
        {
            return Scp03_kdf(key.GetValue(), constant, context, blocklen_bits);
        }

        // SCP03 related
        private static byte[] Scp03_mac(GPKey key, byte[] msg, int lengthbits)
        {
            return Scp03_mac(key.GetValue(), msg, lengthbits);
        }

        private static byte[] Scp03_mac(byte[] keybytes, byte[] msg, int lengthBits)
        {
            // FIXME: programmatically set the crypto backend
            IBlockCipher cipher = new AesEngine();
            CMac cmac = new CMac(cipher);
            cmac.Init(new KeyParameter(keybytes));
            cmac.BlockUpdate(msg, 0, msg.Length);
            byte[] outVal = new byte[cmac.GetMacSize()];
            cmac.DoFinal(outVal, 0);
            return Arrays.CopyOf(outVal, lengthBits / 8);
        }

        private static byte[] Scp03_kdf(byte[] key, byte constant, byte[] context, int blocklen_bits)
        {
            // 11 bytes
            byte[] label = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            //TODO: test, should order be reversed?
            byte[] bo = Arrays.ConcatenateAll(
                label, // 11 bytes of label
                new byte[] { constant }, // constant for the last byte
                new byte[] { 0x00 }, // separator
                new byte[] { (byte)((blocklen_bits >> 8) & 0xFF) }, // block size in two bytes
                new byte[] { (byte)(blocklen_bits & 0xFF) }
                );

            byte[] blocka = bo;
            byte[] blockb = context;

            IBlockCipher cipher = new AesEngine();
            CMac cmac = new CMac(cipher);

            KDFCounterBytesGenerator kdf = new KDFCounterBytesGenerator(cmac);
            kdf.Init(new KDFCounterParameters(key, blocka, blockb, 8)); // counter size in bits

            byte[] cgram = new byte[blocklen_bits / 8];
            kdf.GenerateBytes(cgram, 0, cgram.Length);
            return cgram;
        }

        private static byte[] Scp03_key_check_value(GPKey key)
        {
            try
            {
                byte[] cv = GPCrypto.DoEncrypt_AES_CBC(key.GetKey().GetEncoded(), GPCrypto.one_bytes_16);
                //Cipher c = Cipher.getInstance(AES_CBC_CIPHER);
                //c.init(Cipher.ENCRYPT_MODE, key.getKey(), iv_null_aes);
                //byte[] cv = c.doFinal(one_bytes_16);
                return Arrays.CopyOfRange(cv, 0, 3);
            }
            catch (Exception e)
            {
                throw new Exception("Could not calculate key check value: ", e);
            }

        }

        private static byte[] Scp03_encrypt_key(GPKey kek, GPKey key)
        {
            try
            {
                // Pad with random
                int n = key.GetLength() % 16 + 1;
                byte[] plaintext = new byte[n * 16];
                SecureRandom sr = new SecureRandom();
                sr.NextBytes(plaintext);
                Array.Copy(key.GetValue(), 0, plaintext, 0, key.GetValue().Length);
                // encrypt
                byte[] cgram = GPCrypto.DoEncrypt_AES_CBC(kek.GetValue(), plaintext);
                //Cipher c = Cipher.getInstance(AES_CBC_CIPHER);
                //c.init(Cipher.ENCRYPT_MODE, kek.GetEncoded(), null_bytes_16);
                //byte[] cgram = c.doFinal(plaintext);
                return cgram;
            }
            catch (Exception e)
            {
                throw new Exception("Could not calculate key check value: ", e);
            }

        }

        private static void Buffer_increment(byte[] buffer, int offset, int len)
        {
            if (len < 1)
                return;
            for (int i = offset + len - 1; i >= offset; i--)
            {
                if (buffer[i] != (byte)0xFF)
                {
                    buffer[i]++;
                    break;
                }
                else
                    buffer[i] = (byte)0x00;
            }
        }

        private static void Buffer_increment(byte[] buffer)
        {
            Buffer_increment(buffer, 0, buffer.Length);
        }

    }
}

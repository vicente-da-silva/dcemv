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
using DCEMV.EMVSecurity;
using DCEMV.FormattingUtils;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;

namespace DCEMV.GlobalPlatformProtocol
{
    public class GPKey
    {
        //private int version = 0;
        //private int id = 0;
        private int length = -1;
        private KeyType type;
        private byte[] value = null;

        public GPKey(byte[] v, KeyType type)
        {
            if (v.Length != 16 && v.Length != 24 && v.Length != 32)
                throw new Exception("A valid key should be 16/24/32 bytes long");
            this.value = Arrays.CopyOf(v, v.Length);
            this.length = v.Length;
            this.type = type;

            // Set default ID/version
            //id = 0x00;
            //version = 0x00;
        }

        public byte[] GetValue()
        {
            return value;
        }
        public int GetLength()
        {
            return length;
        }

        public ISecretKey GetKey(KeyType type)
        {
            if (type == KeyType.DES)
            {
                return new SecretKeySpec(Enlarge(value, 8), "DES");
            }
            else if (type == KeyType.DES3)
            {
                return new SecretKeySpec(Enlarge(value, 24), "DESede");
            }
            else if (type == KeyType.AES)
            {
                return new SecretKeySpec(value, "AES");
            }
            else
            {
                throw new Exception("Don't know how to handle " + type + " yet");
            }
        }

        public override string ToString()
        {
            return String.Format("Key Type:{0} Value:{1}", type, Formatting.ByteArrayToHexString(value));
        }

        private static byte[] Enlarge(byte[] key, int length)
        {
            if (length == 24)
            {
                byte[] key24 = new byte[24];
                Array.Copy(key, 0, key24, 0, 16);
                Array.Copy(key, 0, key24, 16, 8);
                return key24;
            }
            else
            {
                byte[] key8 = new byte[8];
                Array.Copy(key, 0, key8, 0, 8);
                return key8;
            }
        }

        public ISecretKey GetKey()
        {
            return GetKey(this.type);
        }
    }
    public class GPKeySet
    {
        private Dictionary<KeySessionType, GPKey> keys = new Dictionary<KeySessionType, GPKey>();
        private byte keyID = 0x00;
        private byte keyVersion = 0x00;

        public GPKeySet()
        {

        }

        public GPKeySet(GPKey master)
        {
            keys.Add(KeySessionType.ENC, master);
            keys.Add(KeySessionType.MAC, master);
            keys.Add(KeySessionType.KEK, master);
        }

        public GPKey GetKey(KeySessionType type)
        {
            return keys[type];
        }

        public void SetKey(KeySessionType type, GPKey k)
        {
            keys.Add(type, k);
        }

        public ISecretKey GetKeyFor(KeySessionType type)
        {
            return keys[type].GetKey();
        }

        public byte GetKeyVersion()
        {
            return keyVersion;
        }

        public byte GetKeyID()
        {
            return keyID;
        }

        public override String ToString()
        {
            String s =
            "\nENC: " + GetKey(KeySessionType.ENC).ToString() +
            "\nMAC: " + GetKey(KeySessionType.MAC).ToString() +
            "\nKEK: " + GetKey(KeySessionType.KEK).ToString();
            return s;
        }
    }
    public class GPPlaintextKeys
    {
        private GPKeySet staticKeys;
        private Diversification diversifier;
        protected GPKey master;

        private GPPlaintextKeys(GPKeySet keys, Diversification div)
        {
            staticKeys = keys;
            diversifier = div;
            System.Diagnostics.Debug.WriteLine(String.Format("static keys: {0}", staticKeys.ToString()));
        }

        public byte GetKeysetID()
        {
            return staticKeys.GetKeyID();
        }

        public byte GetKeysetVersion()
        {
            return staticKeys.GetKeyVersion();
        }

        public static GPPlaintextKeys FromMasterKey(GPKey master, Diversification div)
        {
            GPKeySet ks = new GPKeySet(master);
            GPPlaintextKeys p = new GPPlaintextKeys(ks, div)
            {
                master = master
            };
            return p;
        }

        public static GPPlaintextKeys FromMasterKey(GPKey master)
        {
            return FromMasterKey(master, Diversification.NONE);
        }
        public static GPPlaintextKeys FromKeySet(GPKeySet ks)
        {
            return new GPPlaintextKeys(ks, Diversification.NONE);
        }

        public GPKeySet GetSessionKeys(int scp, byte[] kdd, params byte[][] args)
        {
            GPKeySet cardKeys = staticKeys;

            if (diversifier != Diversification.NONE)
                cardKeys = Diversify(staticKeys, kdd, diversifier, scp);

            System.Diagnostics.Debug.WriteLine(String.Format("card manager keys are diversified from kmc: {0}", Formatting.ByteArrayToHexString(master.GetValue())));
            System.Diagnostics.Debug.WriteLine(String.Format("diversified card keys: {0}", cardKeys.ToString()));

            GPKeySet sessionKeys;
            if (scp == 1)
            {
                if (args.Length != 2)
                {
                    throw new Exception("SCP01 requires host challenge and card challenge");
                }
                sessionKeys = DeriveSessionKeysSCP01(cardKeys, args[0], args[1]);
            }
            else if (scp == 2)
            {
                if (args.Length != 1)
                {
                    throw new Exception("SCP02 requires sequence");
                }
                sessionKeys = DeriveSessionKeysSCP02(cardKeys, args[0], false);
            }
            else if (scp == 3)
            {
                if (args.Length != 2)
                {
                    throw new Exception("SCP03 requires host challenge and card challenge");
                }
                sessionKeys = DeriveSessionKeysSCP03(cardKeys, args[0], args[1]);
            }
            else
            {
                throw new Exception("Dont know how to handle: " + scp);
            }
            System.Diagnostics.Debug.WriteLine(String.Format("session keys: {0}", sessionKeys.ToString()));
            return sessionKeys;
        }

        public static GPKeySet Diversify(GPKeySet keys, byte[] diversification_data, Diversification mode, int scp)
        {
            try
            {
                GPKeySet result = new GPKeySet();
                //Cipher cipher = Cipher.getInstance("DESede/ECB/NoPadding");
                foreach (KeySessionType v in KeyTypeList.Values)
                {
                    if (v == KeySessionType.RMAC)
                        continue;
                    byte[] kv = null;
                    // shift around and fill initialize update data as required.
                    if (mode == Diversification.VISA2)
                    {
                        kv = FillVisa(diversification_data, v);
                    }
                    else if (mode == Diversification.EMV)
                    {
                        kv = FillEmv(diversification_data, v);
                    }

                    // Encrypt with current master key
                    //cipher.init(Cipher.ENCRYPT_MODE, keys.getKey(v).getKey(Type.DES3));
                    //byte[] keybytes = cipher.doFinal(kv);
                    byte[] keybytes = GPCrypto.DoEncrypt_DES3_ECB(keys.GetKey(v).GetKey(KeyType.DES3).GetEncoded(),kv);
                    
                    // Replace the key, possibly changing type. G&D SCE 6.0 uses EMV 3DES and resulting keys
                    // must be interpreted as AES-128
                    GPKey nk = new GPKey(keybytes, scp == 3 ? KeyType.AES : KeyType.DES3);
                    result.SetKey(v, nk);
                }
                return result;
            }
            //catch (BadPaddingException e)
            //{
            //    throw new Exception("Diversification failed.", e);
            //}
            //catch (InvalidKeyException e)
            //{
            //    throw new Exception("Diversification failed.", e);
            //}
            //catch (IllegalBlockSizeException e)
            //{
            //    throw new Exception("Diversification failed.", e);
            //}
            //catch (NoSuchAlgorithmException e)
            //{
            //    throw new Exception("Diversification failed.", e);
            //}
            //catch (NoSuchPaddingException e)
            //{
            //    throw new Exception("Diversification failed.", e);
            //}
            catch (Exception e)
            {
                throw new Exception("Diversification failed.", e);
            }
        }

        public static byte[] FillVisa(byte[] init_update_response, KeySessionType key)
        {
            byte[] data = new byte[16];
            Array.Copy(init_update_response, 0, data, 0, 2);
            Array.Copy(init_update_response, 4, data, 2, 4);
            data[6] = (byte)0xF0;
            data[7] = (byte)key;
            Array.Copy(init_update_response, 0, data, 8, 2);
            Array.Copy(init_update_response, 4, data, 10, 4);
            data[14] = (byte)0x0F;
            data[15] = (byte)key;
            return data;
        }

        public static byte[] FillVisa2(byte[] init_update_response, KeySessionType key)
        {
            byte[] data = new byte[16];
            Array.Copy(init_update_response, 0, data, 0, 4);
            Array.Copy(init_update_response, 8, data, 4, 2);
            data[6] = (byte)0xF0;
            data[7] = 0x01;
            Array.Copy(init_update_response, 0, data, 8, 4);
            Array.Copy(init_update_response, 8, data, 12, 2);
            data[14] = (byte)0x0F;
            data[15] = 0x01;
            return data;
        }

        public static byte[] FillEmv(byte[] init_update_response, KeySessionType key)
        {
            byte[] data = new byte[16];
            // 6 rightmost bytes of init update response (which is 10 bytes)
            Array.Copy(init_update_response, 4, data, 0, 6);
            data[6] = (byte)0xF0;
            data[7] = (byte)key;
            Array.Copy(init_update_response, 4, data, 8, 6);
            data[14] = (byte)0x0F;
            data[15] = (byte)key;
            return data;

        }

        static void Buffer_increment(byte[] buffer, int offset, int len)
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

        private GPKeySet DeriveSessionKeysSCP01(GPKeySet staticKeys, byte[] host_challenge, byte[] card_challenge)
        {
            GPKeySet sessionKeys = new GPKeySet();

            byte[] derivationData = new byte[16];
            Array.Copy(card_challenge, 4, derivationData, 0, 4);
            Array.Copy(host_challenge, 0, derivationData, 4, 4);
            Array.Copy(card_challenge, 0, derivationData, 8, 4);
            Array.Copy(host_challenge, 4, derivationData, 12, 4);

            try
            {
                //Cipher cipher = Cipher.getInstance("DESede/ECB/NoPadding");
                foreach (KeySessionType v in KeyTypeList.Values)
                {
                    if (v == KeySessionType.RMAC) // skip RMAC key
                        continue;
                    //cipher.init(Cipher.ENCRYPT_MODE, staticKeys.getKeyFor(v));
                    //GPKey nk = new GPKey(cipher.doFinal(derivationData), Type.DES3);
                    byte[] result = GPCrypto.DoEncrypt_DES3_ECB(staticKeys.GetKeyFor(v).GetEncoded(), derivationData);
                    GPKey nk = new GPKey(result, KeyType.DES3);

                    sessionKeys.SetKey(v, nk);
                }
                // KEK is the same
                sessionKeys.SetKey(KeySessionType.KEK, staticKeys.GetKey(KeySessionType.KEK));
                return sessionKeys;
            }
            //catch (NoSuchAlgorithmException e)
            //{
            //    throw new IllegalStateException("Session keys calculation failed.", e);
            //}
            //catch (NoSuchPaddingException e)
            //{
            //    throw new IllegalStateException("Session keys calculation failed.", e);
            //}
            //catch (InvalidKeyException e)
            //{
            //    throw new RuntimeException("Session keys calculation failed.", e);
            //}
            //catch (IllegalBlockSizeException e)
            //{
            //    throw new RuntimeException("Session keys calculation failed.", e);
            //}
            //catch (BadPaddingException e)
            //{
            //    throw new RuntimeException("Session keys calculation failed.", e);
            //}
            catch (Exception e)
            {
                throw new Exception("Session keys calculation failed.", e);
            }
        }

        private GPKeySet DeriveSessionKeysSCP02(GPKeySet staticKeys, byte[] sequence, bool implicitChannel)
        {
            GPKeySet sessionKeys = new GPKeySet();

            try
            {
                //Cipher cipher = Cipher.getInstance("DESede/CBC/NoPadding");
               
                byte[] derivationData = new byte[16];
                Array.Copy(sequence, 0, derivationData, 2, 2);

                byte[] constantMAC = new byte[] { (byte)0x01, (byte)0x01 };
                Array.Copy(constantMAC, 0, derivationData, 0, 2);
                //cipher.init(Cipher.ENCRYPT_MODE, staticKeys.getKeyFor(KeySessionType.MAC), GPCrypto.iv_null_des);
                //GPKey nk = new GPKey(cipher.doFinal(derivationData), Type.DES3);
                byte[] result = GPCrypto.DoEncrypt_DES3_CBC(staticKeys.GetKeyFor(KeySessionType.MAC).GetEncoded(), derivationData);
                GPKey nk = new GPKey(result, KeyType.DES3);

                sessionKeys.SetKey(KeySessionType.MAC, nk);

                // TODO: is this correct? - increment by one for all other than C-MAC
                if (implicitChannel)
                {
                    Buffer_increment(derivationData, 2, 2);
                }

                byte[] constantRMAC = new byte[] { (byte)0x01, (byte)0x02 };
                Array.Copy(constantRMAC, 0, derivationData, 0, 2);
                //cipher.init(Cipher.ENCRYPT_MODE, staticKeys.getKeyFor(KeySessionType.MAC), GPCrypto.iv_null_des);
                //nk = new GPKey(cipher.doFinal(derivationData), Type.DES3);
                byte[] result2 = GPCrypto.DoEncrypt_DES3_CBC(staticKeys.GetKeyFor(KeySessionType.MAC).GetEncoded(), derivationData);
                nk = new GPKey(result2, KeyType.DES3);

                sessionKeys.SetKey(KeySessionType.RMAC, nk);

                byte[] constantENC = new byte[] { (byte)0x01, (byte)0x82 };
                Array.Copy(constantENC, 0, derivationData, 0, 2);
                //cipher.init(Cipher.ENCRYPT_MODE, staticKeys.getKeyFor(KeySessionType.ENC), GPCrypto.iv_null_des);
                //nk = new GPKey(cipher.doFinal(derivationData), Type.DES3);
                byte[] result3 = GPCrypto.DoEncrypt_DES3_CBC(staticKeys.GetKeyFor(KeySessionType.ENC).GetEncoded(), derivationData);
                nk = new GPKey(result3, KeyType.DES3);

                sessionKeys.SetKey(KeySessionType.ENC, nk);

                byte[] constantDEK = new byte[] { (byte)0x01, (byte)0x81 };
                Array.Copy(constantDEK, 0, derivationData, 0, 2);
                //cipher.init(Cipher.ENCRYPT_MODE, staticKeys.getKeyFor(KeySessionType.KEK), GPCrypto.iv_null_des);
                //nk = new GPKey(cipher.doFinal(derivationData), Type.DES3);
                byte[] result4 = GPCrypto.DoEncrypt_DES3_CBC(staticKeys.GetKeyFor(KeySessionType.KEK).GetEncoded(), derivationData);
                nk = new GPKey(result4, KeyType.DES3);

                sessionKeys.SetKey(KeySessionType.KEK, nk);
                return sessionKeys;
            }
            //catch (NoSuchAlgorithmException e)
            //{
            //    throw new IllegalStateException("Session keys calculation failed.", e);
            //}
            //catch (NoSuchPaddingException e)
            //{
            //    throw new IllegalStateException("Session keys calculation failed.", e);
            //}
            //catch (InvalidKeyException e)
            //{
            //    throw new RuntimeException("Session keys calculation failed.", e);
            //}
            //catch (IllegalBlockSizeException e)
            //{
            //    throw new RuntimeException("Session keys calculation failed.", e);
            //}
            //catch (BadPaddingException e)
            //{
            //    throw new RuntimeException("Session keys calculation failed.", e);
            //}
            //catch (InvalidAlgorithmParameterException e)
            //{
            //    throw new RuntimeException("Session keys calculation failed.", e);
            //}
            catch (Exception e)
            {
                throw new Exception("Session keys calculation failed.", e);
            }
        }

        private GPKeySet DeriveSessionKeysSCP03(GPKeySet staticKeys, byte[] host_challenge, byte[] card_challenge)
        {
            GPKeySet sessionKeys = new GPKeySet();
            byte mac_constant = 0x06;
            byte enc_constant = 0x04;
            byte rmac_constant = 0x07;

            byte[] context = Arrays.Concatenate(host_challenge, card_challenge);

            // MAC
            byte[] kdf = SCP03Wrapper.Scp03_kdf(staticKeys.GetKey(KeySessionType.MAC), mac_constant, context, 128);
            sessionKeys.SetKey(KeySessionType.MAC, new GPKey(kdf, KeyType.AES));
            // ENC
            kdf = SCP03Wrapper.Scp03_kdf(staticKeys.GetKey(KeySessionType.ENC), enc_constant, context, 128);
            sessionKeys.SetKey(KeySessionType.ENC, new GPKey(kdf, KeyType.AES));
            // RMAC
            kdf = SCP03Wrapper.Scp03_kdf(staticKeys.GetKey(KeySessionType.MAC), rmac_constant, context, 128);
            sessionKeys.SetKey(KeySessionType.RMAC, new GPKey(kdf, KeyType.AES));

            // KEK remains the same
            sessionKeys.SetKey(KeySessionType.KEK, staticKeys.GetKey(KeySessionType.KEK));
            return sessionKeys;
        }
    }
}

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
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;

namespace DCEMV.GlobalPlatformProtocol
{
    public class SCP0102Wrapper : SCPWrapper
    {
        private ByteArrayOutputStream rMac = new ByteArrayOutputStream();

        private byte[] icv = null;
        private byte[] ricv = null;
        private int scp;
        private bool icvEnc = false;
        private bool preAPDU = false;
        private bool postAPDU = false;
        
        public SCP0102Wrapper(GPKeySet sessionKeys, SCPVersions scp, List<APDUMode> securityLevel, byte[] icv, byte[] ricv, int bs)
        {
            this.blockSize = bs;
            this.sessionKeys = sessionKeys;
            this.icv = icv;
            this.ricv = ricv;
            SetSCPVersion(scp);
            SetSecurityLevel(securityLevel);
        }

        public void SetSCPVersion(SCPVersions scp)
        {
            // Major version of wrapper
            this.scp = 2;
            if(scp == SCPVersions.SCP_01_05)// || scp == SCPVersions.SCP_01_15)
                this.scp = 1;
            
            // modes
            //if ((scp == SCPVersions.SCP_01_15) || (scp == SCPVersions.SCP_02_14) || (scp == SCPVersions.SCP_02_15) || (scp == SCPVersions.SCP_02_1A) || (scp == SCPVersions.SCP_02_1B))
            if (scp == SCPVersions.SCP_02_15)
                icvEnc = true;
            else
                icvEnc = false;

            //if ((scp == SCPVersions.SCP_01_05) || (scp == SCPVersions.SCP_01_15) || (scp == SCPVersions.SCP_02_04) || (scp == SCPVersions.SCP_02_05) || (scp == SCPVersions.SCP_02_14) || (scp == SCPVersions.SCP_02_15))
            if ((scp == SCPVersions.SCP_01_05) || (scp == SCPVersions.SCP_02_15))
                preAPDU = true;
            else
                preAPDU = false;

            //if ((scp == SCPVersions.SCP_02_0A) || (scp == SCPVersions.SCP_02_0B) || (scp == SCPVersions.SCP_02_1A) || (scp == SCPVersions.SCP_02_1B))
            //    postAPDU = true;
            //else
                postAPDU = false;
        }

        public byte[] GetIV()
        {
            return icv;
        }
        
        public override void SetSecurityLevel(List<APDUMode> securityLevel)
        {
            if (securityLevel.Contains(APDUMode.RMAC))
                ricv = icv;

            base.SetSecurityLevel(securityLevel);
        }

        private static byte ClearBits(byte b, byte mask)
        {
            return (byte)((b & ~mask) & 0xFF);
        }

        private static byte SetBits(byte b, byte mask)
        {
            return (byte)((b | mask) & 0xFF);
        }

        public override byte[] Wrap(GPCommand command)
        {

            try
            {
                if (!mac && !enc)
                {
                    return command.Serialize();
                }

                if (rmac)
                {
                    rMac.Reset();
                    rMac.Write(ClearBits((byte)command.CLA, (byte)0x07));
                    rMac.Write(command.INS);
                    rMac.Write(command.P1);
                    rMac.Write(command.P2);
                    if (command.CommandData.Length >= 0)
                    {
                        rMac.Write(command.CommandData.Length);
                        rMac.Write(command.CommandData);
                    }
                }

                byte origCLA = command.CLA;
                byte newCLA = origCLA;
                byte origINS = command.INS;
                byte origP1 = command.P1;
                byte origP2 = command.P2;
                byte[] origData = command.CommandData;
                byte origLc = (byte)command.CommandData.Length;
                byte newLc = origLc;
                byte[] newData = null;
                byte le = command.Le.Value;
                ByteArrayOutputStream t = new ByteArrayOutputStream();

                if (origLc > GetBlockSize())
                {
                    throw new Exception("APDU too long for wrapping.");
                }

                if (mac)
                {
                    if (icv == null)
                    {
                        icv = new byte[8];
                    }
                    else if (icvEnc)
                    {
                        //IBufferedCipher c = null;
                        if (scp == 1)
                        {
                            icv = GPCrypto.DoEncrypt_DES3_ECB(sessionKeys.GetKeyFor(KeySessionType.MAC).GetEncoded(), icv);
                            //c = Cipher.getInstance(GPCrypto.DES3_ECB_CIPHER);
                            //c.init(Cipher.ENCRYPT_MODE, sessionKeys.getKeyFor(KeyType.MAC));
                        }
                        else
                        {
                            icv = GPCrypto.DoEncrypt_DES_ECB(sessionKeys.GetKey(KeySessionType.MAC).GetKey(KeyType.DES).GetEncoded(), icv);
                            //c = Cipher.getInstance(GPCrypto.DES_ECB_CIPHER);
                            //c.init(Cipher.ENCRYPT_MODE, sessionKeys.getKey(KeyType.MAC).getKey(Type.DES));
                        }
                        // encrypts the future ICV ?
                        //icv = c.doFinal(icv);
                    }

                    if (preAPDU)
                    {
                        newCLA = SetBits((byte)newCLA, (byte)0x04);
                        newLc = (byte)(newLc + 8);
                    }
                    t.Write(newCLA);
                    t.Write(origINS);
                    t.Write(origP1);
                    t.Write(origP2);
                    t.Write(newLc);
                    t.Write(origData);

                    if (scp == 1)
                    {
                        icv = Mac_3des(sessionKeys.GetKey(KeySessionType.MAC), t.ToByteArray(), icv);
                    }
                    else if (scp == 2)
                    {
                        icv = Mac_des_3des(sessionKeys.GetKey(KeySessionType.MAC), t.ToByteArray(), icv);
                    }

                    if (postAPDU)
                    {
                        newCLA = SetBits((byte)newCLA, (byte)0x04);
                        newLc = (byte)(newLc + 8);
                    }
                    t.Reset();
                    newData = origData;
                }

                if (enc && (origLc > 0))
                {
                    if (scp == 1)
                    {
                        t.Write(origLc);
                        t.Write(origData);
                        if ((t.Size() % 8) != 0)
                        {
                            byte[] x = Pad80(t.ToByteArray(), 8);
                            t.Reset();
                            t.Write(x);
                        }
                    }
                    else
                    {
                        t.Write(Pad80(origData, 8));
                    }
                    newLc += (byte)(t.Size() - origData.Length);

                    //Cipher c = Cipher.getInstance(GPCrypto.DES3_CBC_CIPHER);
                    //c.init(Cipher.ENCRYPT_MODE, sessionKeys.getKeyFor(KeyType.ENC), GPCrypto.iv_null_des);
                    //newData = c.doFinal(t.toByteArray());
                    newData = GPCrypto.DoEncrypt_DES3_CBC(sessionKeys.GetKeyFor(KeySessionType.ENC).GetEncoded(), t.ToByteArray());

                    t.Reset();
                }
                t.Write(newCLA);
                t.Write(origINS);
                t.Write(origP1);
                t.Write(origP2);
                if (newLc > 0)
                {
                    t.Write(newLc);
                    t.Write(newData);
                }
                if (mac)
                {
                    t.Write(icv);
                }
                if (le > 0)
                {
                    t.Write(le);
                }
                return t.ToByteArray();
            }
            catch (Exception e)
            {
                throw new Exception("APDU wrapping failed", e);
            }

        }

        public override byte[] Unwrap(GPResponse response)
        {
            if (rmac)
            {
                if (response.ResponseData.Length < 8)
                {
                    throw new Exception("Wrong response length (too short).");
                }
                int respLen = response.ResponseData.Length - 8;
                rMac.Write(respLen);
                rMac.Write(response.ResponseData, 0, respLen);
                rMac.Write(response.SW1);
                rMac.Write(response.SW2);

                ricv = Mac_des_3des(sessionKeys.GetKey(KeySessionType.RMAC), Pad80(rMac.ToByteArray(), 8), ricv);

                byte[] actualMac = new byte[8];
                Array.Copy(response.ResponseData, respLen, actualMac, 0, 8);
                if (!Arrays.AreEqual(ricv, actualMac))
                {
                    throw new Exception("RMAC invalid.");
                }
                ByteArrayOutputStream o = new ByteArrayOutputStream();
                o.Write(response.ResponseData, 0, respLen);
                o.Write(response.SW1);
                o.Write(response.SW2);
                return o.ToByteArray();
            }
            return response.ResponseData;
        }

        public static byte[] Mac_3des_nulliv(GPKey key, byte[] d)
        {
            return Mac_3des(key, d, GPCrypto.null_bytes_8);
        }
        private static byte[] Mac_3des(GPKey key, byte[] text, byte[] iv)
        {
            byte[] d = Pad80(text, 8);
            return Mac_3des(key.GetKey(), d, 0, d.Length, iv);
        }
        private static byte[] Mac_3des(ISecretKey key, byte[] text, int offset, int length, byte[] iv)
        {
            if (length == -1)
            {
                length = text.Length - offset;
            }

            try
            {
                //Cipher cipher = Cipher.getInstance(DES3_CBC_CIPHER);
                //cipher.init(Cipher.ENCRYPT_MODE, key, new IvParameterSpec(iv));
                //byte[] res = cipher.doFinal(text, offset, length);
                byte[] res = GPCrypto.DoEncrypt_DES3_CBC(key.GetEncoded(), text, offset, length, iv);

                byte[] result = new byte[8];
                Array.Copy(res, res.Length - 8, result, 0, 8);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception("MAC computation failed.", e);
            }
        }
        private static byte[] Mac_des_3des(GPKey key, byte[] text, byte[] iv)
        {
            byte[] d = Pad80(text, 8);
            return Mac_des_3des(key, d, 0, d.Length, iv);
        }
        private static byte[] Mac_des_3des(GPKey key, byte[] text, int offset, int length, byte[] iv)
        {
            if (length == -1)
            {
                length = text.Length - offset;
            }

            try
            {
                //Cipher cipher1 = Cipher.getInstance(DES_CBC_CIPHER);
                //cipher1.init(Cipher.ENCRYPT_MODE, key.getKey(Type.DES), new IvParameterSpec(iv));
                //Cipher cipher2 = Cipher.getInstance(DES3_CBC_CIPHER);
                //cipher2.init(Cipher.ENCRYPT_MODE, key.getKey(Type.DES3), new IvParameterSpec(iv));

                byte[] result = Arrays.Clone(iv);
                byte[] temp;
                if (length > 8)
                {
                    temp = GPCrypto.DoEncrypt_DES_CBC(key.GetKey(KeyType.DES).GetEncoded(), text, offset, length - 8, iv);
                    Array.Copy(temp, temp.Length - 8, result, 0, 8);
                }
                temp = GPCrypto.DoEncrypt_DES3_CBC(key.GetKey(KeyType.DES3).GetEncoded(), text, (offset + length) - 8, 8, result);
                Array.Copy(temp, temp.Length - 8, result, 0, 8);
                return result;
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                throw new Exception("MAC computation failed.", e);
            }
        }

        private int GetBlockSize()
        {
            int res = this.blockSize;
            if (mac)
                res = res - 8;
            if (enc)
                res = res - 8;
            return res;
        }
    }
}

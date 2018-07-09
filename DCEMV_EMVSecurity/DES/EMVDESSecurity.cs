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
using DCEMV.EMVProtocol.Kernels;
using DCEMV.EMVSecurity.PIN;
using DCEMV.FormattingUtils;
using DCEMV.TLVProtocol;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace DCEMV.EMVSecurity
{
    public enum CrptoVersionEnum
    {
        _17,
        _10,
        _18,
    }
    public class CryptoMetaData
    {
        public SKDMethod SKDMethod { get; set; }
        public CrptoVersionEnum CryptoVersion { get; set; }
        public SecretKeySpec IMKACUnEncrypted { get; set; }
        public SecretKeySpec IMKMACUnEncrypted { get; set; }
        public SecretKeySpec IMKDEAUnEncrypted { get; set; }
        public byte KDI { get; set; }
        public byte[] CVR { get; set; }
        public byte[] Cryptogram { get; set; }
        public string PAN { get; set; }
        public string PSN { get; set; }
        public byte[] ATC { get; set; }
        public byte[] UPN { get; set; }
        public byte[] Amount { get; set; }
        public byte[] IAD { get; set; }
    }
    public class EMVDESSecurity //: EMVDESSecurityBase
    {
        private static byte[] fPaddingBlock = Formatting.HexStringToByteArray("FFFFFFFFFFFFFFFF");
        private static byte[] zeroBlock = Formatting.HexStringToByteArray("0000000000000000");

        private static Dictionary<String, int> keyTypeToLMKIndex;
        private static Dictionary<int, ISecretKey> lmks = new Dictionary<int, ISecretKey>(); //Count = LMK_PAIRS_NO * variants.Length * schemeVariants.Length = 15 * 10 * 6 = 900
        private static int LMK_PAIRS_NO = 0xe; //must = max value in LMK file
        private static int[] variants = { 0x00, 0xa6, 0x5a, 0x6a, 0xde, 0x2b, 0x50, 0x74, 0x9c, 0xfa }; //10
        private static int[] schemeVariants = { 0x00, 0xa6, 0x5a, 0x6a, 0xde, 0x2b }; //6
        private static short LMK_KEY_LENGTH = SMAdapter.LENGTH_DES3_2KEY;

        private static String KEY_TYPE_PATTERN = ("([^:;]*)([:;])?([^:;])?([^:;])?");

        private static int KEY_U_LEFT = 1;
        private static int KEY_U_RIGHT = 2;
        private static int KEY_T_LEFT = 3;
        private static int KEY_T_MEDIUM = 4;
        private static int KEY_T_RIGHT = 5;

        private static HashMaker SHA1_MESSAGE_DIGEST = HashMaker.GetInstance(DigestMode.SHA1);

        public EMVDESSecurity(String lmkFile)
        {
            //Objects.requireNonNull(lmkFile);
            Init(null, lmkFile, false);
        }

        private void Init(String jceProviderClassName, String lmkFile, bool lmkRebuild)
        {
            if (!File.Exists(lmkFile) && !lmkRebuild)
                throw new Exception("null lmkFile - needs rebuild");
            try
            {
                keyTypeToLMKIndex = new Dictionary<string, int>
                {
                    { SMAdapter.TYPE_ZMK, 0x000 }, //LMK0x00 variant 0
                    { SMAdapter.TYPE_ZPK, 0x001 },

                    { SMAdapter.TYPE_PVK, 0x002 },
                    { SMAdapter.TYPE_TPK, 0x002 },
                    { SMAdapter.TYPE_TMK, 0x002 },
                    { SMAdapter.TYPE_CVK, 0x402 }, //LMK0x02 variant 4

                    { SMAdapter.TYPE_TAK, 0x003 },
                    //            keyTypeToLMKIndex.put(PINLMKIndex,        0x004);

                    { SMAdapter.TYPE_ZAK, 0x008 },

                    { SMAdapter.TYPE_BDK, 0x009 }, //LMK0x09 variant 0
                    { SMAdapter.TYPE_MK_AC, 0x109 }, //LMK0x09 variant 1
                    { SMAdapter.TYPE_MK_SMI, 0x209 },
                    { SMAdapter.TYPE_MK_SMC, 0x309 },
                    { SMAdapter.TYPE_MK_DAC, 0x409 },
                    { SMAdapter.TYPE_MK_DN, 0x509 },
                    { SMAdapter.TYPE_MK_CVC3, 0x709 },

                    { SMAdapter.TYPE_ZEK, 0x00A },

                    { SMAdapter.TYPE_DEK, 0x00B },

                    { SMAdapter.TYPE_RSA_SK, 0x00C },
                    { SMAdapter.TYPE_HMAC, 0x10C },

                    { SMAdapter.TYPE_RSA_PK, 0x00D }
                };

                //jceHandler = new JCEHandler();
                if (lmkRebuild)
                {
                    // Creat new LMK file
                    // Generate New random Local Master Keys
                    GenerateLMK();
                    // Write the new Local Master Keys to file
                    WriteLMK(lmkFile);
                }
                ReadLMK(lmkFile);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private void ReadLMK(String lmkFile)
        {
            lmks.Clear();
            try
            {
                Properties lmkProps = new Properties();
                lmkProps.Load(lmkFile);
                byte[] lmkData;
                for (int i = 0; i <= LMK_PAIRS_NO; i++)
                {
                    lmkData = Formatting.HexStringToByteArray(lmkProps.GetProperty("LMK0x" + String.Format("{0:X}", i).PadLeft(2, '0').ToLower()).Substring(0, SMAdapter.LENGTH_DES3_2KEY / 4));
                    SpreadLMKVariants(lmkData, i);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Can't read Local Master Keys from file: " + lmkFile, e);
            }
        }

        private void WriteLMK(String fileName)
        {
            Properties lmkProps = new Properties();
            try
            {
                for (int i = 0; i <= LMK_PAIRS_NO; i++)
                {
                    lmkProps.SetProperty("LMK0x" + String.Format("{0:X}", i).PadLeft(2, '0').ToLower(), Formatting.ByteArrayToHexString(lmks[i].GetEncoded()));
                }

                lmkProps.Save(fileName);
            }
            catch (Exception e)
            {
                throw new Exception("Can't write Local Master Keys to file: " + fileName, e);
            }
        }

        private void GenerateLMK()
        {
            lmks.Clear();
            try
            {
                for (int i = 0; i <= LMK_PAIRS_NO; i++)
                {
                    ISecretKey lmkKey = (ISecretKey)JCEHandler.GenerateDESKey(LMK_KEY_LENGTH);
                    SpreadLMKVariants(lmkKey.GetEncoded(), i);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Can't generate Local Master Keys", e);
            }
        }

        private void SpreadLMKVariants(byte[] lmkData, int idx)
        {
            int i = 0;
            foreach (int v in variants)
            {
                int k = 0;
                byte[] variantData = ApplyVariant(lmkData, v);
                foreach (int sv in schemeVariants)
                {
                    byte[] svData = ApplySchemeVariant(variantData, sv);
                    int key = idx;
                    key += 0x100 * i;
                    key += 0x1000 * k++;

                    System.Diagnostics.Debug.WriteLine(String.Format("[Key={6}]:PairNo[LMK0x{0}]:Variant[{1}={4}]:Schema[{2}={5}]:Key[{3}]",
                            String.Format("{0:X}", idx).PadLeft(2, '0'),
                            String.Format("{0:X}", i).PadLeft(2, '0'),
                            String.Format("{0:X}", k - 1).PadLeft(2, '0'),
                            Formatting.ByteArrayToHexString(svData),
                            String.Format("{0:X}", v).PadLeft(2, '0'),
                            String.Format("{0:X}", sv).PadLeft(2, '0'),
                            String.Format("{0:X}", key).PadLeft(4, '0')));

                    //3rd key = 1st key, each key = 64 bits
                    svData = Formatting.ConcatArrays(svData, 0, JCEHandler.GetBytesLength(SMAdapter.LENGTH_DES3_2KEY), svData, 0, JCEHandler.GetBytesLength(SMAdapter.LENGTH_DES));
                    lmks.Add(key, (ISecretKey)JCEHandler.FormDESKey(SMAdapter.LENGTH_DES3_2KEY, svData));
                }
                i++;
            }
        }

        private byte[] ApplyVariant(byte[] lmkdata, int variant)
        {
            byte[] vardata = new byte[lmkdata.Length];
            Array.Copy(lmkdata, 0, vardata, 0, lmkdata.Length);
            //XOR first byte of first key with selected variant byte
            vardata[0] ^= (byte)variant;
            return vardata;
        }

        private byte[] ApplySchemeVariant(byte[] lmkdata, int variant)
        {
            byte[] vardata = new byte[lmkdata.Length];
            Array.Copy(lmkdata, 0, vardata, 0, lmkdata.Length);
            //XOR first byte of second key with selected variant byte
            vardata[8] ^= (byte)variant;
            return vardata;
        }

        private IKey DecryptFromLMK(SecureDESKey secureDESKey)
        {
            IKey left, medium, right;
            byte[] keyBytes = secureDESKey.GetKeyBytes();
            byte[] bl = new byte[SMAdapter.LENGTH_DES >> 3];
            byte[] bm = new byte[SMAdapter.LENGTH_DES >> 3];
            byte[] br = new byte[SMAdapter.LENGTH_DES >> 3];
            byte[] clearKeyBytes = null;
            int? lmkIndex = GetKeyTypeIndex(secureDESKey.GetKeyLength(), secureDESKey.GetKeyType());
            if (lmkIndex == null)
                throw new Exception("Unsupported key type: " + secureDESKey.GetKeyType());
            lmkIndex |= secureDESKey.GetVariant() << 8;//same as lmkIndex += (variant*0x100) : see readfile method
            switch (secureDESKey.GetScheme())
            {
                case KeyScheme.Z:
                case KeyScheme.X:
                case KeyScheme.Y:
                    clearKeyBytes = JCEHandler.DecryptData(keyBytes, GetLMK(lmkIndex.Value));
                    break;
                case KeyScheme.U:
                    left = GetLMK(KEY_U_LEFT << 12 | lmkIndex & 0xfff);
                    right = GetLMK(KEY_U_RIGHT << 12 | lmkIndex & 0xfff);
                    Array.Copy(keyBytes, 0, bl, 0, bl.Length);
                    Array.Copy(keyBytes, bl.Length, br, 0, br.Length);
                    bl = JCEHandler.DecryptData(bl, left);
                    br = JCEHandler.DecryptData(br, right);
                    clearKeyBytes = Arrays.Concatenate(bl, br);
                    clearKeyBytes = Formatting.ConcatArrays(clearKeyBytes, 0, clearKeyBytes.Length, clearKeyBytes, 0, br.Length);
                    break;
                case KeyScheme.T:
                    left = GetLMK(KEY_T_LEFT << 12 | lmkIndex & 0xfff);
                    medium = GetLMK(KEY_T_MEDIUM << 12 | lmkIndex & 0xfff);
                    right = GetLMK(KEY_T_RIGHT << 12 | lmkIndex & 0xfff);
                    Array.Copy(keyBytes, 0, bl, 0, bl.Length);
                    Array.Copy(keyBytes, bl.Length, bm, 0, bm.Length);
                    Array.Copy(keyBytes, bl.Length + bm.Length, br, 0, br.Length);
                    bl = JCEHandler.DecryptData(bl, left);
                    bm = JCEHandler.DecryptData(bm, medium);
                    br = JCEHandler.DecryptData(br, right);
                    clearKeyBytes = Arrays.Concatenate(bl, bm);
                    clearKeyBytes = Arrays.Concatenate(clearKeyBytes, br);
                    break;
            }
            if (!Util.IsDESParityAdjusted(clearKeyBytes))
                throw new Exception("Parity not adjusted");
            return JCEHandler.FormDESKey(secureDESKey.GetKeyLength(), clearKeyBytes);
        }

        private SecureDESKey EncryptToLMK(short keyLength, String keyType, IKey clearDESKey)
        {
            IKey novar, left, medium, right;
            byte[] clearKeyBytes = JCEHandler.ExtractDESKeyMaterial(keyLength, clearDESKey);
            byte[] bl = new byte[SMAdapter.LENGTH_DES >> 3];
            byte[] bm = new byte[SMAdapter.LENGTH_DES >> 3];
            byte[] br = new byte[SMAdapter.LENGTH_DES >> 3];
            byte[] encrypted = null;
            // enforce correct (odd) parity before encrypting the key
            Util.AdjustDESParity(clearKeyBytes);
            int lmkIndex = GetKeyTypeIndex(keyLength, keyType);
            switch (GetScheme(keyLength, keyType))
            {
                case KeyScheme.Z:
                case KeyScheme.X:
                case KeyScheme.Y:
                    novar = GetLMK(lmkIndex);
                    encrypted = JCEHandler.EncryptData(clearKeyBytes, novar);
                    break;
                case KeyScheme.U:
                    left = GetLMK(KEY_U_LEFT << 12 | lmkIndex & 0xfff);
                    right = GetLMK(KEY_U_RIGHT << 12 | lmkIndex & 0xfff);
                    Array.Copy(clearKeyBytes, 0, bl, 0, bl.Length);
                    Array.Copy(clearKeyBytes, bl.Length, br, 0, br.Length);
                    bl = JCEHandler.EncryptData(bl, left);
                    br = JCEHandler.EncryptData(br, right);
                    encrypted = Arrays.Concatenate(bl, br);
                    break;
                case KeyScheme.T:
                    left = GetLMK(KEY_T_LEFT << 12 | lmkIndex & 0xfff);
                    medium = GetLMK(KEY_T_MEDIUM << 12 | lmkIndex & 0xfff);
                    right = GetLMK(KEY_T_RIGHT << 12 | lmkIndex & 0xfff);
                    Array.Copy(clearKeyBytes, 0, bl, 0, bl.Length);
                    Array.Copy(clearKeyBytes, bl.Length, bm, 0, bm.Length);
                    Array.Copy(clearKeyBytes, bl.Length + bm.Length, br, 0, br.Length);
                    bl = JCEHandler.EncryptData(bl, left);
                    bm = JCEHandler.EncryptData(bm, medium);
                    br = JCEHandler.EncryptData(br, right);
                    encrypted = Arrays.Concatenate(bl, bm);
                    encrypted = Arrays.Concatenate(encrypted, br);
                    break;
            }
            SecureDESKey secureDESKey = new SecureDESKey(keyLength, keyType, encrypted, CalculateKeyCheckValue(clearDESKey));
            return secureDESKey;
        }

        private ISecretKey GetLMK(int? lmkIndex)
        {
            if (lmkIndex == null)
                throw new Exception("Invalid key code");
            ISecretKey lmk = lmks[lmkIndex.Value];
            if (lmk == null)
                throw new Exception("Invalid key code: LMK0x" + "LMK0x" + String.Format("{0:X}", lmkIndex).PadLeft(2, '0').ToLower());
            return lmk;
        }

        public bool VerifyARQCImpl(MKDMethod mkdm, SKDMethod skdm, SecureDESKey imkac, String accountNo, String accntSeqNo, byte[] arqc, byte[] atc, byte[] upn, byte[] transData)
        {
            byte[] res = CalculateARQC(mkdm, skdm, imkac, accountNo, accntSeqNo, atc, upn, transData);
            return Arrays.AreEqual(arqc, res);
        }

        public byte[] CalculateARQC(MKDMethod mkdm, SKDMethod skdm, SecureDESKey imkac, String accountNo, String accntSeqNo, byte[] atc, byte[] upn, byte[] transData)
        {
            byte[] panpsn = FormatPANPSN(accountNo, accntSeqNo, mkdm);
            IKey mkac = DeriveICCMasterKey(DecryptFromLMK(imkac), panpsn);
            IKey skac = mkac;
            switch (skdm)
            {
                case SKDMethod.VSDC:
                    ConstraintMKDM(mkdm, skdm);
                    break;
                case SKDMethod.MCHIP:
                    ConstraintMKDM(mkdm, skdm);
                    skac = DeriveSK_MK(mkac, atc, upn);
                    break;
                case SKDMethod.EMV_CSKD:
                    skac = DeriveCommonSK_AC(mkac, atc);
                    break;
                default:
                    throw new Exception("Session Key Derivation " + skdm + " not supported");
            }

            return CalculateMACISO9797Alg3(skac, transData);
        }

        public byte[] VerifyARQCGenerateARPCImpl(MKDMethod? mkdm, SKDMethod skdm, SecureDESKey imkac, String accountNo, String accntSeqNo, byte[] arqc, byte[] atc, byte[] upn, byte[] transData, ARPCMethod arpcMethod, byte[] arc, byte[] propAuthData)
        {
            return VerifyARQCGenerateARPCImpl(mkdm, skdm, DecryptFromLMK(imkac), accountNo, accntSeqNo, arqc, atc, upn, transData, arpcMethod, arc, propAuthData);
        }

        public byte[] GenerateARPCImpl(MKDMethod? mkdm, SKDMethod skdm, SecureDESKey imkac, String accountNo, String accntSeqNo, byte[] arqc, byte[] atc, byte[] upn, ARPCMethod arpcMethod, byte[] arc, byte[] propAuthData)
        {
            if (mkdm == null)
                mkdm = MKDMethod.OPTION_A;

            byte[] panpsn = FormatPANPSN(accountNo, accntSeqNo, mkdm.Value);
            IKey mkac = DeriveICCMasterKey(DecryptFromLMK(imkac), panpsn);
            IKey skarpc = mkac;
            switch (skdm)
            {
                case SKDMethod.VSDC:
                    ConstraintMKDM(mkdm.Value, skdm);
                    ConstraintARPCM(skdm, arpcMethod);
                    break;
                case SKDMethod.MCHIP:
                    ConstraintMKDM(mkdm.Value, skdm);
                    ConstraintARPCM(skdm, arpcMethod);
                    break;
                case SKDMethod.EMV_CSKD:
                    skarpc = DeriveCommonSK_AC(mkac, atc);
                    break;
                default:
                    throw new Exception("Session Key Derivation " + skdm + " not supported");
            }

            return CalculateARPC(skarpc, arqc, arpcMethod, arc, propAuthData);
        }

        public IKey DeriveICCMasterKey(SecureDESKey imkac, string pan, string pansn, MKDMethod method)
        {
            return DeriveICCMasterKey(DecryptFromLMK(imkac), FormatPANPSN(pan, pansn, method));
        }

        public SecureDESKey FormKEYfromThreeClearComponents(short keyLength, String keyType, String clearComponent1HexString, String clearComponent2HexString, String clearComponent3HexString)
        {
            SecureDESKey secureDESKey;
            try
            {
                byte[] clearComponent1 = Formatting.HexStringToByteArray(clearComponent1HexString);
                byte[] clearComponent2 = Formatting.HexStringToByteArray(clearComponent2HexString);
                byte[] clearComponent3 = Formatting.HexStringToByteArray(clearComponent3HexString);
                byte[] clearKeyBytes = Formatting.Xor(Formatting.Xor(clearComponent1, clearComponent2), clearComponent3);
                IKey clearKey = JCEHandler.FormDESKey(keyLength, clearKeyBytes);
                secureDESKey = EncryptToLMK(keyLength, keyType, clearKey);
            }
            catch (Exception e)
            {
                throw e;
            }
            return secureDESKey;
        }


        #region Static Methods

        public static byte[] VerifyARQCGenerateARPCImpl(MKDMethod? mkdm, SKDMethod skdm, IKey imkac, String accountNo, String accntSeqNo, byte[] arqc, byte[] atc, byte[] upn, byte[] transData, ARPCMethod arpcMethod, byte[] arc, byte[] propAuthData)
        {
            if (mkdm == null)
                mkdm = MKDMethod.OPTION_A;

            byte[] panpsn = FormatPANPSN(accountNo, accntSeqNo, mkdm.Value);
            IKey mkac = DeriveICCMasterKey(imkac, panpsn);
            IKey skac = mkac;
            IKey skarpc = mkac;
            switch (skdm)
            {
                case SKDMethod.VSDC:
                    ConstraintMKDM(mkdm.Value, skdm);
                    ConstraintARPCM(skdm, arpcMethod);
                    break;
                case SKDMethod.MCHIP:
                    ConstraintMKDM(mkdm.Value, skdm);
                    ConstraintARPCM(skdm, arpcMethod);
                    skac = DeriveSK_MK(mkac, atc, upn);
                    break;
                case SKDMethod.EMV_CSKD:
                    skac = DeriveSK_MK(mkac, atc, new byte[4]);
                    skarpc = skac;
                    break;
                default:
                    throw new Exception("Session Key Derivation " + skdm + " not supported");
            }

            if (!Arrays.AreEqual(arqc, CalculateMACISO9797Alg3(skac, transData)))
                return null;

            return CalculateARPC(skarpc, arqc, arpcMethod, arc, propAuthData);
        }

        private static byte[] CalculateARPC(IKey skarpc, byte[] arqc, ARPCMethod? arpcMethod, byte[] arc, byte[] propAuthData)
        {
            if (arpcMethod == null)
                arpcMethod = ARPCMethod.METHOD_1;

            byte[] b = new byte[8];
            switch (arpcMethod)
            {
                case ARPCMethod.METHOD_1:
                    Array.Copy(arc, arc.Length - 2, b, 0, 2);
                    b = Formatting.Xor(arqc, b);
                    return JCEHandler.EncryptData(b, skarpc);
                case ARPCMethod.METHOD_2:
                    b = Arrays.Concatenate(arqc, arc);
                    if (propAuthData != null)
                        b = Arrays.Concatenate(b, propAuthData);
                    b = PaddingISO9797Method2(b);
                    b = CalculateMACISO9797Alg3(skarpc, b);
                    return Arrays.CopyOf(b, 4);
                default:
                    throw new Exception("ARPC Method " + arpcMethod + " not supported");
            }
        }

        private static IKey CalculateMACSessionKey(SKDMethod method, IKey imkMAC, string accountNoA, string accountNoA_CSN, byte[] atc, byte[] arqc)
        {
            IKey udkMAC = DeriveICCMasterKey(imkMAC, accountNoA, accountNoA_CSN, MKDMethod.OPTION_A);
            IKey macSK = DeriveCommonSK_MAC_DEA(method, udkMAC, arqc, atc);
            return macSK;
        }

        private static IKey CalculateDEASessionKey(SKDMethod method, IKey imkDEA, string accountNoA, string accountNoA_CSN, byte[] atc, byte[] arqc)
        {
            IKey udkDEA = DeriveICCMasterKey(imkDEA, accountNoA, accountNoA_CSN, MKDMethod.OPTION_A);
            IKey deaSK = DeriveCommonSK_MAC_DEA(method, udkDEA, arqc, atc);
            return deaSK;
        }

        private static IKey CalculateACSessionKey(MKDMethod? mkdm, SKDMethod skdm, IKey imkac, String accountNo, String accntSeqNo, byte[] atc, byte[] upn)
        {
            if (mkdm == null)
                mkdm = MKDMethod.OPTION_A;

            byte[] panpsn = FormatPANPSN(accountNo, accntSeqNo, mkdm.Value);
            IKey mkac = DeriveICCMasterKey(imkac, panpsn);
            IKey skac = mkac;
            switch (skdm)
            {
                case SKDMethod.VSDC:
                    ConstraintMKDM(mkdm.Value, skdm);
                    break;
                case SKDMethod.MCHIP:
                    ConstraintMKDM(mkdm.Value, skdm);
                    skac = DeriveSK_MK(mkac, atc, upn);
                    break;
                case SKDMethod.EMV_CSKD:
                    skac = DeriveCommonSK_AC(mkac, atc);
                    break;
                default:
                    throw new Exception("Session Key Derivation " + skdm + " not supported");
            }
            return skac;
        }

        public static byte[] CalculatePinChangeCommand(IKey skDEA, IKey skMAC, byte[] atc, byte[] arqc, byte[] pinBlock)
        {
            IKey kl = JCEHandler.FormDESKey(SMAdapter.LENGTH_DES, Formatting.copyOfRange(skDEA.GetEncoded(), 0, 8));
            IKey kr = JCEHandler.FormDESKey(SMAdapter.LENGTH_DES, Formatting.copyOfRange(skDEA.GetEncoded(), 8, 16));

            byte[] adpuDataEnc = JCEHandler.EncryptData(pinBlock, kl);
            adpuDataEnc = JCEHandler.DecryptData(adpuDataEnc, kr);
            adpuDataEnc = JCEHandler.EncryptData(adpuDataEnc, kl);

            byte length84 = (byte)(adpuDataEnc.Length + 8);//mac is 8 long
            byte[] adpuHeader = Arrays.Concatenate(Formatting.HexStringToByteArray("84240002"), new byte[] { length84 });

            byte[] adpuMacData = Formatting.ConcatArrays(
                adpuHeader,
                atc,
                arqc,
                adpuDataEnc
                );

            adpuMacData = PaddingISO9797Method2(adpuMacData);
            byte[] adpuMac = CalculateMACISO9797Alg3(skMAC, adpuMacData);

            byte length86 = (byte)(adpuHeader.Length + adpuDataEnc.Length + adpuMac.Length);

            byte[] result = Formatting.ConcatArrays(
                new byte[] { 0x86, length86 },
                adpuHeader,
                adpuDataEnc,
                adpuMac
                );

            return result;
        }

        public static IKey DeriveICCMasterKey(IKey imkac, string pan, string pansn, MKDMethod method)
        {
            return DeriveICCMasterKey(imkac, FormatPANPSN(pan, pansn, method));
        }

        public static IKey GenerateDESKey(short keyLength)
        {
            return JCEHandler.GenerateDESKey(keyLength);
        }
        
        private static void ConstraintARPCM(SKDMethod skdm, ARPCMethod arpcMethod)
        {
            if (arpcMethod == ARPCMethod.METHOD_2)
                throw new Exception("ARPC generation method 2 is not used in practice with scheme " + skdm);
        }

        public static byte[] PaddingISO9797Method2(byte[] d)
        {
            //Padding - first byte 0x80 rest 0x00
            byte[] t = new byte[d.Length - d.Length % 8 + 8];
            Array.Copy(d, 0, t, 0, d.Length);
            for (int i = d.Length; i < t.Length; i++)
                t[i] = (byte)(i == d.Length ? 0x80 : 0x00);
            d = t;
            return d;
        }

        private static byte[] FormatPANPSN(String pan, String psn, MKDMethod mkdm)
        {
            switch (mkdm)
            {
                case MKDMethod.OPTION_A:
                    return FormatPANPSNOptionA(pan, psn);
                case MKDMethod.OPTION_B:
                    if (pan.Length <= 16)
                        //use OPTION_A
                        return FormatPANPSNOptionA(pan, psn);
                    return FormatPANPSNOptionB(pan, psn);
                default:
                    throw new Exception("Unsupported ICC Master Key derivation method");
            }
        }

        private static byte[] FormatPANPSNOptionA(String pan, String psn)
        {
            if (pan.Length < 14)
                pan = pan.PadLeft(14, '0');

            byte[] b = PreparePANPSN(pan, psn);

            return Arrays.CopyOfRange(b, b.Length - 8, b.Length);
        }

        private static byte[] FormatPANPSNOptionB(String pan, String psn)
        {
            byte[] b = PreparePANPSN(pan, psn);
            //20-bytes sha-1 digest
            byte[] r = SHA1_MESSAGE_DIGEST.Digest(b);
            //decimalized HEX string of digest
            String rs = DecimalizeVisa(r);
            //return 16-bytes decimalizd digest
            return Formatting.HexStringToByteArray(rs.Substring(0, 16));
        }

        private static byte[] PreparePANPSN(String pan, String psn)
        {
            if (psn == null || String.IsNullOrEmpty(psn))
                psn = "00";
            String ret = pan + psn;
            //convert digits to bytes and pad with "0"
            //to left for ensure even number of digits
            return Formatting.HexStringToByteArray(ret);
        }

        private static String DecimalizeVisa(byte[] b)
        {
            char[] bec = Formatting.ByteArrayToHexString(b).ToUpper().ToCharArray();
            char[] bhc = new char[bec.Length];
            int k = 0;
            //Select 0-9 chars
            foreach (char c in bec)
                if (c < 'A')
                    bhc[k++] = c;
            //Select A-F chars and map them to 0-5
            char adjust = (char)('A' - '0');
            foreach (char c in bec)
                if (c >= 'A')
                    bhc[k++] = (char)(c - adjust);
            return new String(bhc);
        }

        public static byte[] CalculateMACISO9797Alg3(IKey key, byte[] d)
        {
            IKey kl = JCEHandler.FormDESKey(SMAdapter.LENGTH_DES, Arrays.CopyOfRange(key.GetEncoded(), 0, 8));
            IKey kr = JCEHandler.FormDESKey(SMAdapter.LENGTH_DES, Arrays.CopyOfRange(key.GetEncoded(), 8, 16));
            if (d.Length % 8 != 0)
            {
                //Padding with 0x00 bytes
                byte[] t = new byte[d.Length - d.Length % 8 + 8];
                Array.Copy(d, 0, t, 0, d.Length);
                d = t;
            }
            //MAC_CBC alg 3
            byte[] y_i = Formatting.HexStringToByteArray("0000000000000000");
            byte[] yi = new byte[8];
            for (int i = 0; i < d.Length; i += 8)
            {
                Array.Copy(d, i, yi, 0, yi.Length);
                y_i = JCEHandler.EncryptData(Formatting.Xor(yi, y_i), kl);
            }
            y_i = JCEHandler.DecryptData(y_i, kr);
            y_i = JCEHandler.EncryptData(y_i, kl);
            return y_i;
        }

        private static void ConstraintMKDM(MKDMethod mkdm, SKDMethod skdm)
        {
            if (mkdm == MKDMethod.OPTION_B)
                throw new Exception("Master Key Derivation Option B is not used in practice with scheme " + skdm);
        }

        public static IKey DeriveCommonSK_MAC_DEA(SKDMethod skdm, IKey mkac, byte[] arqc, byte[] atc)
        {
            switch (skdm)
            {
                case SKDMethod.VSDC:
                    byte[] l = Arrays.CopyOfRange(mkac.GetEncoded(), 0, 8);
                    byte[] lOr = Formatting.Xor(l, Formatting.PadArray(atc, 8, true, 0x00));
                    byte[] r = Arrays.CopyOfRange(mkac.GetEncoded(), 8, 16);
                    byte[] buff = new byte[] { 0xFF, 0xFF };
                    byte[] atcMod = Formatting.PadArray(Formatting.Xor(atc, buff), 8, true, 0x00);
                    byte[] rOr = Formatting.Xor(r, atcMod);
                    return JCEHandler.FormDESKey(SMAdapter.LENGTH_DES3_2KEY, Arrays.Concatenate(lOr, rOr));
                case SKDMethod.MCHIP:
                    return DeriveCommonSK_SM(mkac, arqc, false);
                case SKDMethod.EMV_CSKD:
                    return DeriveCommonSK_SM(mkac, arqc, false);
                default:
                    throw new Exception("Session Key Derivation " + skdm + " not supported");
            }
        }

        private static IKey DeriveCommonSK_AC(IKey mkac, byte[] atc)
        {

            byte[] r = new byte[8];
            Array.Copy(atc, atc.Length - 2, r, 0, 2);

            return DeriveCommonSK_SM(mkac, r);
        }

        public static IKey DeriveSK_MK(IKey mkac, byte[] atc, byte[] upn)
        {
            byte[] r = new byte[8];
            Array.Copy(atc, atc.Length - 2, r, 0, 2);
            Array.Copy(upn, upn.Length - 4, r, 4, 4);

            return DeriveCommonSK_SM(mkac, r);
        }

        private static IKey DeriveCommonSK_SM(IKey mksm, byte[] rand, bool adjustParity = true)
        {
            byte[] rl = Arrays.CopyOf(rand, 8);
            rl[2] = (byte)0xf0;
            byte[] skl = JCEHandler.EncryptData(rl, mksm);
            byte[] rr = Arrays.CopyOf(rand, 8);
            rr[2] = (byte)0x0f;
            byte[] skr = JCEHandler.EncryptData(rr, mksm);
            if (adjustParity)
            {
                Util.AdjustDESParity(skl);
                Util.AdjustDESParity(skr);
            }
            return JCEHandler.FormDESKey(SMAdapter.LENGTH_DES3_2KEY, Arrays.Concatenate(skl, skr));
        }

        private static IKey DeriveICCMasterKey(IKey imk, byte[] panpsn)
        {
            byte[] l = Arrays.CopyOfRange(panpsn, 0, 8);
            //left part of derived key
            l = JCEHandler.EncryptData(l, imk);
            byte[] r = Arrays.CopyOfRange(panpsn, 0, 8);
            //inverse clear right part of key
            r = Formatting.Xor(r, EMVDESSecurity.fPaddingBlock);
            //right part of derived key
            r = JCEHandler.EncryptData(r, imk);
            //derived key
            byte[] mk = Arrays.Concatenate(l, r);
            //fix DES parity of key
            Util.AdjustDESParity(mk);
            //form JCE Tripple-DES Key
            return JCEHandler.FormDESKey(SMAdapter.LENGTH_DES3_2KEY, mk);
        }

        private static byte[] CalculateKeyCheckValue(IKey key)
        {
            byte[] encryptedZeroBlock = JCEHandler.EncryptData(zeroBlock, key);
            return Util.Trim(encryptedZeroBlock, 3);
        }

        private static int GetKeyTypeIndex(short keyLength, String keyType)
        {
            int index;
            if (keyType == null)
                return 0;
            String majorType = GetMajorType(keyType);
            if (!keyTypeToLMKIndex.ContainsKey(majorType))
                throw new Exception("Unsupported key type: " + majorType);
            index = keyTypeToLMKIndex[majorType];
            index |= GetVariant(keyType) << 8;
            return index;
        }

        private static KeyScheme GetScheme(int keyLength, String keyType)
        {
            KeyScheme scheme = KeyScheme.Z;
            switch (keyLength)
            {
                case SMAdapter.LENGTH_DES:
                    scheme = KeyScheme.Z; break;
                case SMAdapter.LENGTH_DES3_2KEY:
                    scheme = KeyScheme.X; break;
                case SMAdapter.LENGTH_DES3_3KEY:
                    scheme = KeyScheme.Y; break;
            }
            if (keyType == null)
                return scheme;
            Regex rx = new Regex(KEY_TYPE_PATTERN);
            Match m = rx.Match(keyType);
            if (m.Groups[4] != null)
                try
                {
                    scheme = (KeyScheme)Enum.Parse(typeof(KeyScheme), m.Groups[4].Value);
                }
                catch (Exception ex)
                {
                    throw new Exception("Value " + m.Groups[4].Value + " is not valid key scheme", ex);
                }
            return scheme;
        }

        private static int GetVariant(String keyType)
        {
            int variant = 0;
            Regex rx = new Regex(KEY_TYPE_PATTERN);
            Match m = rx.Match(keyType);
            if (m.Groups[3] != null)
                try
                {
                    variant = Int32.Parse(m.Groups[3].Value);
                }
                catch (FormatException ex)
                {
                    throw new FormatException("Value " + m.Groups[4].Value + " is not valid key variant", ex);
                }
            return variant;
        }

        private static String GetMajorType(String keyType)
        {
            Regex rx = new Regex(KEY_TYPE_PATTERN);
            Match m = rx.Match(keyType);
            if (m.Groups.Count > 0 && m.Groups[1] != null)
                return m.Groups[1].Value;
            throw new Exception("Missing key type");
        }
        
        public static string PrintDESKey(string message, IKey key)
        {
            return Formatting.ByteArrayToHexString(key.GetEncoded());
        }

        public static string PrintDESKey(string message, SecureDESKey key)
        {
            return Formatting.ByteArrayToHexString(key.GetKeyBytes());
        }

        public static CryptoMetaData DetermineCryptoMeta(TLV emvData)
        {
            TLV _84 = emvData.Children.Get(EMVTagsEnum.DEDICATED_FILE_DF_NAME_84_KRN.Tag);
            if (_84 == null)
                throw new Exception("No AID found");

            TLV _9F10 = emvData.Children.Get(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag);
            if (_9F10 == null)
                throw new Exception("No IAD found");

            TLV _9F26 = emvData.Children.Get(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag);
            if (_9F26 == null)
                throw new Exception("No Cryptogram found");

            TLV _9F37 = emvData.Children.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN.Tag);
            if (_9F37 == null)
                throw new Exception("No UPN found");

            TLV _9F36 = emvData.Children.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag);
            if (_9F36 == null)
                throw new Exception("No ATC found");

            TLV _9F02 = emvData.Children.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag);
            if (_9F02 == null)
                throw new Exception("No Amount found");

            byte[] panBCD = GetPanBCD(emvData);
            
            //older cards may not return this
            TLV _5F34 = emvData.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN.Tag);
            if (_5F34 == null)
                _5F34 = TLV.Create(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN.Tag, new byte[1]);

            SKDMethod method = DetermineCryptoMethod(Formatting.ByteArrayToHexString(_84.Value));
            byte kdi;
            byte cryptoVersionNumber;
            byte[] cvr;

            switch (method)
            {
                case SKDMethod.VSDC:
                    byte format = _9F10.Value[0];
                    if(format == 0x06) //format = version 0/1/3
                    {
                        kdi = _9F10.Value[1];
                        byte cvn = _9F10.Value[2];
                        switch (cvn)
                        {
                            case 0x0A:
                                cryptoVersionNumber = 0x10;
                                break;
                            case 0x11:
                                cryptoVersionNumber = 0x17;
                                break;
                            case 0x12:
                            case 0x22:
                                cryptoVersionNumber = 0x18;
                                break;

                            default:
                                throw new Exception("DetermineCryptoType: cryptoVersionNumber not supported:" + _9F10.Value[2]);
                        }
                        //byte 4 to 7 = cvr
                        cvr = Formatting.copyOfRange(_9F10.Value, 3, 7);

                    }
                    else if (format == 0x1F) 
                    {
                        byte cvn = _9F10.Value[1];
                        format = (byte)(cvn >> 4);//left nibble is version, can only be 2 for vis verion 1.6
                        if(format != 0x02)
                            throw new Exception("DetermineCryptoType: unknown iad version: " + format);
                        kdi = _9F10.Value[2]; //version 2
                        switch (cvn)
                        {
                            case 0x0A:
                                cryptoVersionNumber = 0x10;
                                break;
                            case 0x11:
                                cryptoVersionNumber = 0x17;
                                break;
                            case 0x12:
                            case 0x22:
                                cryptoVersionNumber = 0x18;
                                break;

                            default:
                                throw new Exception("DetermineCryptoType: cryptoVersionNumber not supported:" + _9F10.Value[2]);
                        }
                        //byte 4 to 8 = cvr
                        cvr = Formatting.copyOfRange(_9F10.Value, 3, 8);
                    }
                    else
                    {
                        throw new Exception("DetermineCryptoType: unknown iad version: " + format);
                    }
                    break;

                case SKDMethod.MCHIP:
                    kdi = _9F10.Value[0];
                    cryptoVersionNumber = _9F10.Value[1];
                    //byte 1 = kdi
                    //byte 2 =  cryptogram version number
                    //byte 3 to 8 = cvr
                    cvr = Formatting.copyOfRange(_9F10.Value, 2, 8);
                    break;

                default:
                    throw new Exception("DetermineCryptoType: SKDMethod not supported:" + method);
            }

            CryptoMetaData meta = new CryptoMetaData()
            {
                CVR = cvr,
                KDI = kdi,
                SKDMethod = method,
                Cryptogram = _9F26.Value,
                PAN = Formatting.BcdToString(panBCD),
                PSN = Formatting.ByteArrayToHexString(_5F34.Value),
                ATC = _9F36.Value,
                UPN = _9F37.Value,
                IAD = _9F10.Value,
                Amount = _9F02.Value,
            };

            switch (cryptoVersionNumber)
            {
                case 0x10:
                    meta.CryptoVersion = CrptoVersionEnum._10;
                    break;

                case 0x17:
                    meta.CryptoVersion = CrptoVersionEnum._17;
                    break;

                case 0x18:
                    if (meta.SKDMethod == SKDMethod.VSDC)//for 18 vsdc uses the std emv ccd as per 8.1.2 book 2
                        meta.SKDMethod = SKDMethod.EMV_CSKD;

                    meta.CryptoVersion = CrptoVersionEnum._18;
                    break;

                default:
                    throw new Exception("CrptoVersion not supported:" + cryptoVersionNumber);
            }
            return meta;
        }

        private static SKDMethod DetermineCryptoMethod(String aid)
        {
            if (aid.StartsWith(AIDEnum.A0000000031010.ToString()))
            {
                return SKDMethod.VSDC;
            }
            if (aid.StartsWith(AIDEnum.A0000000041010.ToString()))
            {
                return SKDMethod.MCHIP;
            }
            throw new Exception("DetermineCryptoMethod: AID not supported:" + aid);
        }

        private static byte[] GetPanBCD(TLV emvData)
        {
            byte[] panBCD;
            TLV _5A = emvData.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag);
            if (_5A != null)
                panBCD = _5A.Value;
            else
            {
                TLV _57 = emvData.Children.Get(EMVTagsEnum.TRACK_2_EQUIVALENT_DATA_57_KRN.Tag);
                if (_57 == null)
                    throw new Exception("No PAN found");
                String panString = Formatting.ByteArrayToHexString(_57.Value);
                panBCD = Formatting.StringToBcd(panString.Split('D')[0], false);
            }

            //unpad pan, badly formatted pans may have more than 1 pad char
            string stripped = Formatting.ByteArrayToHexString(panBCD).Replace("F", "");
            return Formatting.StringToBcd(stripped,false);
        }
        
        private static byte[] VerifyCryptogram10ARQCGenerateARPCImpl(TLV emvData, CryptoMetaData cryptoMetaData, byte[] arc)
        {
            //int depth = 0;
            //Logger.Log(emvData.ToPrintString(ref depth));

            TLV _9F03 = emvData.Children.Get(EMVTagsEnum.AMOUNT_OTHER_NUMERIC_9F03_KRN.Tag);
            if (_9F03 == null)
                throw new Exception("No Amount Other found");

            TLV _9F1A = emvData.Children.Get(EMVTagsEnum.TERMINAL_COUNTRY_CODE_9F1A_KRN.Tag);
            if (_9F1A == null)
                throw new Exception("No Terminal Country code found");

            TLV _95 = emvData.Children.Get(EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN.Tag);
            if (_95 == null)
                throw new Exception("No TVR code found");

            TLV _5F2A = emvData.Children.Get(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN.Tag);
            if (_5F2A == null)
                throw new Exception("No Transaction Currency code found");

            TLV _9A = emvData.Children.Get(EMVTagsEnum.TRANSACTION_DATE_9A_KRN.Tag);
            if (_9A == null)
                throw new Exception("No Transaction Date found");

            TLV _9C = emvData.Children.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN.Tag);
            if (_9C == null)
                throw new Exception("No Transaction Type found");

            TLV _82 = emvData.Children.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag);
            if (_82 == null)
                throw new Exception("No AIP found");


            byte[] data = new byte[0];
            data = Formatting.ConcatArrays(data, cryptoMetaData.Amount);
            data = Formatting.ConcatArrays(data, _9F03.Value);
            data = Formatting.ConcatArrays(data, _9F1A.Value);
            data = Formatting.ConcatArrays(data, _95.Value);
            data = Formatting.ConcatArrays(data, _5F2A.Value);
            data = Formatting.ConcatArrays(data, _9A.Value);
            data = Formatting.ConcatArrays(data, _9C.Value);
            data = Formatting.ConcatArrays(data, cryptoMetaData.UPN);
            data = Formatting.ConcatArrays(data, _82.Value);
            data = Formatting.ConcatArrays(data, cryptoMetaData.ATC);
            data = Formatting.ConcatArrays(data, cryptoMetaData.CVR);

            switch (cryptoMetaData.SKDMethod)
            {
                case SKDMethod.VSDC:
                    break;
                case SKDMethod.MCHIP:
                    data = PaddingISO9797Method2(data);
                    break;
                default:
                    throw new Exception("Unknown SKDMethod");
            }

            byte[] propAuthData = null;

            //propAuthData used for method 2
            //upn needed for mchip
            byte[] arpc = VerifyARQCGenerateARPCImpl(
                MKDMethod.OPTION_A,
                cryptoMetaData.SKDMethod,
                cryptoMetaData.IMKACUnEncrypted,
                cryptoMetaData.PAN,
                cryptoMetaData.PSN,
                cryptoMetaData.Cryptogram,
                cryptoMetaData.ATC,
                cryptoMetaData.UPN,
                data,
                ARPCMethod.METHOD_1,
                arc,
                propAuthData);

            return arpc;
        }

        //same as Common Core Definitions with a Cryptogram Version of 5
        private static byte[] VerifyCryptogram18ARQCGenerateARPCImpl(TLV emvData, CryptoMetaData cryptoMetaData, byte[] arc)
        {
            //int depth = 0;
            //Logger.Log(emvData.ToPrintString(ref depth));

            TLV _9F03 = emvData.Children.Get(EMVTagsEnum.AMOUNT_OTHER_NUMERIC_9F03_KRN.Tag);
            if (_9F03 == null)
                throw new Exception("No Amount Other found");

            TLV _9F1A = emvData.Children.Get(EMVTagsEnum.TERMINAL_COUNTRY_CODE_9F1A_KRN.Tag);
            if (_9F1A == null)
                throw new Exception("No Terminal Country code found");

            TLV _95 = emvData.Children.Get(EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN.Tag);
            if (_95 == null)
                throw new Exception("No TVR code found");

            TLV _5F2A = emvData.Children.Get(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN.Tag);
            if (_5F2A == null)
                throw new Exception("No Transaction Currency code found");

            TLV _9A = emvData.Children.Get(EMVTagsEnum.TRANSACTION_DATE_9A_KRN.Tag);
            if (_9A == null)
                throw new Exception("No Transaction Date found");

            TLV _9C = emvData.Children.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN.Tag);
            if (_9C == null)
                throw new Exception("No Transaction Type found");

            TLV _82 = emvData.Children.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag);
            if (_82 == null)
                throw new Exception("No AIP found");

            byte[] data = new byte[0];
            data = Formatting.ConcatArrays(data, cryptoMetaData.Amount);
            data = Formatting.ConcatArrays(data, _9F03.Value);
            data = Formatting.ConcatArrays(data, _9F1A.Value);
            data = Formatting.ConcatArrays(data, _95.Value);
            data = Formatting.ConcatArrays(data, _5F2A.Value);
            data = Formatting.ConcatArrays(data, _9A.Value);
            data = Formatting.ConcatArrays(data, _9C.Value);
            data = Formatting.ConcatArrays(data, cryptoMetaData.UPN);
            data = Formatting.ConcatArrays(data, _82.Value);
            data = Formatting.ConcatArrays(data, cryptoMetaData.ATC);
            data = Formatting.ConcatArrays(data, cryptoMetaData.IAD);
            
            switch (cryptoMetaData.SKDMethod)
            {
                case SKDMethod.VSDC:
                    data = PaddingISO9797Method2(data);
                    break;
                case SKDMethod.MCHIP:
                    data = PaddingISO9797Method2(data);
                    break;
                case SKDMethod.EMV_CSKD:
                    data = PaddingISO9797Method2(data);
                    break;
                default:
                    throw new Exception("Unknown SKDMethod");
            }

            byte[] propAuthData = null;

            //propAuthData used for method 2
            //upn, atc needed for mchip
            //option b used with EMV_CSKD to properly format pan > 16, if pan <= 16 option a is used
            byte[] arpc = VerifyARQCGenerateARPCImpl(
                cryptoMetaData.SKDMethod != SKDMethod.EMV_CSKD ? MKDMethod.OPTION_A : MKDMethod.OPTION_B,
                cryptoMetaData.SKDMethod,
                cryptoMetaData.IMKACUnEncrypted,
                cryptoMetaData.PAN,
                cryptoMetaData.PSN,
                cryptoMetaData.Cryptogram,
                cryptoMetaData.ATC,
                cryptoMetaData.UPN,
                data,
                cryptoMetaData.SKDMethod!=SKDMethod.EMV_CSKD?ARPCMethod.METHOD_1:ARPCMethod.METHOD_2,
                arc,
                propAuthData);

            return arpc;
        }

        private static byte[] VerifyCryptogram17ARQCGenerateARPCImpl(TLV emvData, CryptoMetaData cryptoMetaData, byte[] arc)
        {
            //byte[] data = Formatting.ConcatArrays(_9F02.Value, _9F37.Value, _9F36.Value, new byte[] { _9F10.Value[4] });
            byte[] data = new byte[0];
            data = Formatting.ConcatArrays(data, cryptoMetaData.Amount);
            data = Formatting.ConcatArrays(data, cryptoMetaData.UPN);
            data = Formatting.ConcatArrays(data, cryptoMetaData.ATC);
            data = Formatting.ConcatArrays(data, new byte[] { cryptoMetaData.CVR[1] });

            switch (cryptoMetaData.SKDMethod)
            {
                case SKDMethod.VSDC:
                    break;
                case SKDMethod.MCHIP:
                    data = PaddingISO9797Method2(data);
                    break;
                default:
                    throw new Exception("Unknown SKDMethod");
            }

            byte[] propAuthData = null;

            //propAuthData used for method 2
            //upn needed for mchip
            byte[] arpc = VerifyARQCGenerateARPCImpl(
                MKDMethod.OPTION_A,
                cryptoMetaData.SKDMethod,
                cryptoMetaData.IMKACUnEncrypted,
                cryptoMetaData.PAN,
                cryptoMetaData.PSN,
                cryptoMetaData.Cryptogram,
                cryptoMetaData.ATC,
                cryptoMetaData.UPN,
                data,
                ARPCMethod.METHOD_1,
                arc,
                propAuthData);

            return arpc;
        }

        public static byte[] VerifyCryptogramGenARPC(TLV emvData, CryptoMetaData cryptoMetaData, byte[] arc)
        {
            byte[] arpc;
            switch (cryptoMetaData.CryptoVersion)
            {
                case CrptoVersionEnum._10:
                    arpc = VerifyCryptogram10ARQCGenerateARPCImpl(emvData, cryptoMetaData, arc);
                    break;

                case CrptoVersionEnum._17:
                    arpc = VerifyCryptogram17ARQCGenerateARPCImpl(emvData, cryptoMetaData, arc);
                    break;

                case CrptoVersionEnum._18:
                    arpc = VerifyCryptogram18ARQCGenerateARPCImpl(emvData, cryptoMetaData, arc);
                    break;

                default:
                    throw new Exception("CryptoVersion not supported:" + cryptoMetaData.CryptoVersion);
            }
            return arpc;
        }

        public static byte[] CalculatePinChangeScript(TLV emvData, CryptoMetaData cryptoMetaData, String newPin, byte[] arqc)
        {
            TLV _84 = emvData.Children.Get(EMVTagsEnum.DEDICATED_FILE_DF_NAME_84_KRN.Tag);
            if (_84 == null)
                throw new Exception("No AID found");

            TLV _9F36 = emvData.Children.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag);
            if (_9F36 == null)
                throw new Exception("No ATC found");

            TLV _9F37 = emvData.Children.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN.Tag);
            if (_9F37 == null)
                throw new Exception("No UPN found");
            
            byte[] panBCD = GetPanBCD(emvData);

            TLV _5F34 = emvData.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN.Tag);
            if (_5F34 == null)
                throw new Exception("No pan sequence number found");
            
            IKey skDEA = CalculateDEASessionKey(cryptoMetaData.SKDMethod, cryptoMetaData.IMKDEAUnEncrypted, Formatting.BcdToString(panBCD), Formatting.ByteArrayToHexString(_5F34.Value), _9F36.Value, arqc);
            IKey skMAC = CalculateMACSessionKey(cryptoMetaData.SKDMethod, cryptoMetaData.IMKMACUnEncrypted, Formatting.BcdToString(panBCD), Formatting.ByteArrayToHexString(_5F34.Value), _9F36.Value, arqc);
            IKey skAC =  CalculateACSessionKey(MKDMethod.OPTION_A, cryptoMetaData.SKDMethod, cryptoMetaData.IMKACUnEncrypted, Formatting.BcdToString(panBCD), Formatting.ByteArrayToHexString(_5F34.Value), _9F36.Value, _9F37.Value);

            byte[] pinBlock;
            switch (cryptoMetaData.CryptoVersion)
            {
                case CrptoVersionEnum._10:
                    switch (cryptoMetaData.SKDMethod)
                    {
                        case SKDMethod.VSDC:
                            pinBlock = PinFormatter.CalculateVISLegacyPinBlockCVN_10_18(newPin, skAC);
                            break;

                        case SKDMethod.MCHIP:
                            pinBlock = PinFormatter.CalculateMChipPinBlockCVN_10(newPin);
                            break;

                        default:
                            throw new Exception("CalculatePinChangeScript: SKDMethod not supported:" + cryptoMetaData.SKDMethod + " for CVN:" + cryptoMetaData.CryptoVersion);
                    }
                    
                    break;

                case CrptoVersionEnum._17:
                    throw new Exception("CalculatePinChangeScript: CryptoVersion not supported:" + cryptoMetaData.CryptoVersion);

                case CrptoVersionEnum._18:
                    switch (cryptoMetaData.SKDMethod)
                    {
                        case SKDMethod.VSDC:
                            pinBlock = PinFormatter.CalculateVISLegacyPinBlockCVN_10_18(newPin, skAC);
                            break;

                        case SKDMethod.MCHIP:
                            pinBlock = PinFormatter.CalculateMChipPinBlockCVN_10(newPin);
                            break;

                        default:
                            throw new Exception("CalculatePinChangeScript: SKDMethod not supported:" + cryptoMetaData.SKDMethod + " for CVN:" + cryptoMetaData.CryptoVersion);
                    }
                    break;

                default:
                    throw new Exception("CalculatePinChangeScript: CryptoVersion not supported:" + cryptoMetaData.CryptoVersion);
            }
            return CalculatePinChangeCommand(skDEA, skMAC, _9F36.Value, arqc, pinBlock);
        }

        #endregion
    }
}

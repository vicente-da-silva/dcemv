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
using DCEMV.Shared;
using DCEMV.FormattingUtils;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{
    public abstract class PublicKeyCertificate
    {
        public byte HashAlgorithmIndicator { get; set; } //0x01 = SHA-1
        public byte PublicKeyAlgorithmIndicator { get; set; } //0x01 = RSA
        [XmlElement(DataType = "hexBinary")]
        public byte[] Modulus { get; set; }
        [XmlElement(DataType = "hexBinary")]
        public byte[] Exponent { get; set; }
        [XmlElement(DataType = "hexBinary")]
        public byte[] ExpiryDate { get; set; }

        public static byte[] DecryptRSA(byte[] bytesToDecrypt, byte[] publicKeyModulus, byte[] publicKeyExponent)
        {
            //Prepend 0x00 to unsigned data to avoid that the most significant bit is interpreted as the "signed" bit
            byte[] modAppend;
            byte[] expAppend;
            byte[] dataAppend;

            if (publicKeyModulus[0] >= 0x80)
                modAppend = Formatting.ConcatArrays(new byte[] { 0x00 }, publicKeyModulus);
            else
                modAppend = publicKeyModulus;

            if (publicKeyExponent[0] >= 0x80)
                expAppend = Formatting.ConcatArrays(new byte[] { 0x00 }, publicKeyExponent);
            else
                expAppend = publicKeyExponent.Reverse().ToArray();

            if (bytesToDecrypt[0] >= 0x80)
                dataAppend = Formatting.ConcatArrays(new byte[] { 0x00 }, bytesToDecrypt);
            else
                dataAppend = bytesToDecrypt;

            //Bouncy Castle BigInteger expects array to be in big endian order, as per EMV data, if this were a .NET BigInteger is would expect it in little endian
            BigInteger biMod = new BigInteger(1, modAppend);
            BigInteger biExp = new BigInteger(1, expAppend);
            BigInteger biDta = new BigInteger(1, dataAppend);

            byte[] result = biDta.ModPow(biExp, biMod).ToByteArray();

            //these seems to be a bug in the bouncy castle RsaCoreEngine ConvertInput method, we add 0x00 to avoid misinterpretation of our numbers as -, 
            //since they are unsigned, and BigInteger expects a signed number, this adding of the byte makes the length of the input longer and
            //the following lines of code => int maxLength = (bitSizeOfModulas + 7) / 8; if (inLen > maxLength) <= compares the length of the Modulus after 
            //BigInteger conversion to the length of the input array before BigInteger confusion, resulting in different lengths

            //RsaKeyParameters keyParameter = new RsaKeyParameters(false, biMod, biExp);
            //Pkcs1Encoding encryptEngine = new Pkcs1Encoding(new RsaEngine());
            //encryptEngine.Init(false, keyParameter);
            //byte[] decryptedBytes = encryptEngine.ProcessBlock(dataAppend, 0, dataAppend.Length);

            if (result.Length == (bytesToDecrypt.Length + 1) && result[0] == (byte)0x00)
            {
                //Remove 0x00 from beginning of array
                byte[] tmp = new byte[bytesToDecrypt.Length];
                Array.Copy(result, 1, tmp, 0, bytesToDecrypt.Length);
                result = tmp;
            }

            return result;
        }
    }
    public class CAPublicKeyCertificate : PublicKeyCertificate
    {
        public byte Index { get; set; }
        [XmlElement(DataType = "hexBinary")]
        public byte[] ActivationDate { get; set; }
        [XmlElement(DataType = "hexBinary")]
        public byte[] Checksum { get; set; }

        public RIDEnum RID { get; set; }

        public CAPublicKeyCertificate()
        {
        }

        public CAPublicKeyCertificate(RIDEnum RID, byte Index, byte HashAlgorithmIndicator, byte PublicKeyAlgorithmIndicator, byte[] Modulus, byte[] Exponent, byte[] ActivationDate, byte[] ExpiryDate, byte[] Checksum)
        {
            this.RID = RID;
            this.Index = Index;
            this.HashAlgorithmIndicator = HashAlgorithmIndicator;
            this.PublicKeyAlgorithmIndicator = PublicKeyAlgorithmIndicator;
            this.Modulus = Modulus;
            this.Exponent = Exponent;
            this.ActivationDate = ActivationDate;
            this.ExpiryDate = ExpiryDate;
            this.Checksum = Checksum;
        }
    }
    public class RevokedCAPublicKeyCertificate
    {
        public byte Index { get; set; }
        [XmlElement(DataType = "hexBinary")]
        public byte[] ExpiryDate { get; set; }
        public RIDEnum RID { get; set; }

        public RevokedCAPublicKeyCertificate()
        {
        }

        public RevokedCAPublicKeyCertificate(RIDEnum RID, byte Index, byte[] ExpiryDate)
        {
            this.Index = Index;
            this.ExpiryDate = ExpiryDate;
            this.RID = RID;
        }
    }
    public class IccPinKeyCertificate : PublicKeyCertificate
    {
        public byte RecoveredDataHeader;
        public byte CertificateFormat;
        public byte[] ApplicationPAN;
        public byte[] CertificateSerialNumber;
        public byte ICCPinKeyLength;
        public byte ICCPinKeyExponentLength;
        public byte[] ICCPinKeyorLeftmostDigitsofIssuerPublicKey;
        public byte[] UnpaddedICCPinKeyorLeftmostDigitsofIssuerPublicKey;
        public byte[] HashResult;
        public byte RecoveredDataTrailer;

        private byte[] IccPinKeyRemainder;
        private int issuerPublicKeyModulusLength;

        public IccPinKeyCertificate(byte[] iccCertData, int issuerPublicKeyModulusLength, byte[] iccPinKeyRemainder, byte[] iccPinKeyExponent)
        {
            this.issuerPublicKeyModulusLength = issuerPublicKeyModulusLength;
            this.IccPinKeyRemainder = iccPinKeyRemainder;
            this.Exponent = iccPinKeyExponent;
            
            Deserialize(iccCertData, 0);
        }

        public int Deserialize(byte[] raw, int pos)
        {
            RecoveredDataHeader = raw[pos];
            pos++;
            CertificateFormat = raw[pos];
            pos++;
            ApplicationPAN = new byte[10];
            Array.Copy(raw, pos, ApplicationPAN, 0, ApplicationPAN.Length);
            pos = pos + ApplicationPAN.Length;
            ExpiryDate = new byte[2];
            Array.Copy(raw, pos, ExpiryDate, 0, ExpiryDate.Length);
            pos = pos + ExpiryDate.Length;
            CertificateSerialNumber = new byte[3];
            Array.Copy(raw, pos, CertificateSerialNumber, 0, CertificateSerialNumber.Length);
            pos = pos + CertificateSerialNumber.Length;
            HashAlgorithmIndicator = raw[pos];
            pos++;
            PublicKeyAlgorithmIndicator = raw[pos];
            pos++;
            ICCPinKeyLength = raw[pos];
            pos++;
            ICCPinKeyExponentLength = raw[pos];
            pos++;

            ICCPinKeyorLeftmostDigitsofIssuerPublicKey = new byte[issuerPublicKeyModulusLength - 42];
            Array.Copy(raw, pos, ICCPinKeyorLeftmostDigitsofIssuerPublicKey, 0, ICCPinKeyorLeftmostDigitsofIssuerPublicKey.Length);
            pos = pos + ICCPinKeyorLeftmostDigitsofIssuerPublicKey.Length;

            if (ICCPinKeyLength <= (issuerPublicKeyModulusLength - 42))
            {
                int padLength = issuerPublicKeyModulusLength - 42 - ICCPinKeyLength;
                byte[] newLeftMost = new byte[ICCPinKeyorLeftmostDigitsofIssuerPublicKey.Length - padLength];
                Array.Copy(ICCPinKeyorLeftmostDigitsofIssuerPublicKey, 0, newLeftMost, 0, newLeftMost.Length);
                UnpaddedICCPinKeyorLeftmostDigitsofIssuerPublicKey = newLeftMost;
            }
            else
                UnpaddedICCPinKeyorLeftmostDigitsofIssuerPublicKey = ICCPinKeyorLeftmostDigitsofIssuerPublicKey;

            HashResult = new byte[20];
            Array.Copy(raw, pos, HashResult, 0, HashResult.Length);
            pos = pos + HashResult.Length;

            RecoveredDataTrailer = raw[pos];
            pos++;

            return pos;
        }

        internal bool ValidateHash()
        {
            byte[] concatenated = Formatting.ConcatArrays(
                new byte[] { CertificateFormat },
                ApplicationPAN,
                ExpiryDate,
                CertificateSerialNumber,
                new byte[] { HashAlgorithmIndicator },
                new byte[] { PublicKeyAlgorithmIndicator },
                new byte[] { ICCPinKeyLength },
                new byte[] { ICCPinKeyExponentLength },
                ICCPinKeyorLeftmostDigitsofIssuerPublicKey,
                IccPinKeyRemainder,
                Exponent
                );

            byte[] hash = SHA1.Create().ComputeHash(concatenated);

            if (Formatting.ByteArrayToHexString(HashResult) != Formatting.ByteArrayToHexString(hash))
                return false;

            return true;
        }

        internal static IccPinKeyCertificate BuildAndValidatePublicKey(KernelDatabaseBase database, byte[] issuerPublicKeyModulus, byte[] issuerPublicKeyExponent)
        {
            TLV iccPinKeyCertificate = database.Get(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PIN_ENCIPHERMENT_PUBLIC_KEY_CERTIFICATE_9F2D_KRN);
            if (iccPinKeyCertificate == null) return null;
            TLV iccPinKeyExponent = database.Get(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PIN_ENCIPHERMENT_PUBLIC_KEY_EXPONENT_9F2E_KRN);
            if (iccPinKeyExponent == null) return null;
            TLV iccPinKeyRemainder = database.Get(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PIN_ENCIPHERMENT_PUBLIC_KEY_REMAINDER_9F2F_KRN);
            if (iccPinKeyRemainder == null) return null;

            if (iccPinKeyCertificate.Value.Length != issuerPublicKeyModulus.Length)
                return null;

            byte[] decrypt = DecryptRSA(iccPinKeyCertificate.Value, issuerPublicKeyModulus, issuerPublicKeyExponent);

            IccPinKeyCertificate iccCertData = new IccPinKeyCertificate(decrypt, issuerPublicKeyModulus.Length,
                iccPinKeyRemainder == null ? new byte[0] : iccPinKeyRemainder.Value, iccPinKeyExponent.Value);

            if (iccCertData.RecoveredDataTrailer != 0xBC)
                return null;

            if (iccCertData.RecoveredDataHeader != 0x6A)
                return null;

            if (iccCertData.CertificateFormat != 0x04)
                return null;

            if (!iccCertData.ValidateHash())
                return null;

            string pan = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN).Value);
            string panToCompare = Formatting.ByteArrayToHexString(iccCertData.ApplicationPAN).Replace("FF", "");

            if (!pan.StartsWith(panToCompare))
                return null;

            DateTime expiry = DateTime.ParseExact(Formatting.BcdToString(iccCertData.ExpiryDate), "MMyy", System.Globalization.CultureInfo.InvariantCulture);
            if (expiry <= DateTime.Now)
                return null;

            if (iccCertData.PublicKeyAlgorithmIndicator != 0x01)
                return null;

            if (iccPinKeyRemainder != null)
                iccCertData.Modulus = Formatting.ConcatArrays(iccCertData.UnpaddedICCPinKeyorLeftmostDigitsofIssuerPublicKey, iccPinKeyRemainder.Value);
            else
                iccCertData.Modulus = iccCertData.UnpaddedICCPinKeyorLeftmostDigitsofIssuerPublicKey;

            return iccCertData;
        }
    }
    public class IccPublicKeyCertificate : PublicKeyCertificate
    {
        public byte RecoveredDataHeader;
        public byte CertificateFormat;
        public byte[] ApplicationPAN;
        public byte[] CertificateSerialNumber;
        public byte ICCPublicKeyLength;
        public byte ICCPublicKeyExponentLength;
        public byte[] ICCPublicKeyorLeftmostDigitsofIssuerPublicKey;
        public byte[] UnpaddedICCPublicKeyorLeftmostDigitsofIssuerPublicKey;
        public byte[] HashResult;
        public byte RecoveredDataTrailer;

        private byte[] IccuPulicKeyRemainder;
        private int issuerPublicKeyModulusLength;
        private byte[] staticDataToBeAuthenticated;

        public IccPublicKeyCertificate(byte[] iccCertData, int issuerPublicKeyModulusLength, byte[] iccPublicKeyRemainder, byte[] iccPublicKeyExponent, byte[] staticDataToBeAuthenticated)
        {
            this.issuerPublicKeyModulusLength = issuerPublicKeyModulusLength;
            this.IccuPulicKeyRemainder = iccPublicKeyRemainder;
            this.Exponent = iccPublicKeyExponent;
            this.staticDataToBeAuthenticated = staticDataToBeAuthenticated;
            Deserialize(iccCertData, 0);
        }

        public int Deserialize(byte[] raw, int pos)
        {
            RecoveredDataHeader = raw[pos];
            pos++;
            CertificateFormat = raw[pos];
            pos++;
            ApplicationPAN = new byte[10];
            Array.Copy(raw, pos, ApplicationPAN, 0, ApplicationPAN.Length);
            pos = pos + ApplicationPAN.Length;
            ExpiryDate = new byte[2];
            Array.Copy(raw, pos, ExpiryDate, 0, ExpiryDate.Length);
            pos = pos + ExpiryDate.Length;
            CertificateSerialNumber = new byte[3];
            Array.Copy(raw, pos, CertificateSerialNumber, 0, CertificateSerialNumber.Length);
            pos = pos + CertificateSerialNumber.Length;
            HashAlgorithmIndicator = raw[pos];
            pos++;
            PublicKeyAlgorithmIndicator = raw[pos];
            pos++;
            ICCPublicKeyLength = raw[pos];
            pos++;
            ICCPublicKeyExponentLength = raw[pos];
            pos++;

            ICCPublicKeyorLeftmostDigitsofIssuerPublicKey = new byte[issuerPublicKeyModulusLength - 42];
            Array.Copy(raw, pos, ICCPublicKeyorLeftmostDigitsofIssuerPublicKey, 0, ICCPublicKeyorLeftmostDigitsofIssuerPublicKey.Length);
            pos = pos + ICCPublicKeyorLeftmostDigitsofIssuerPublicKey.Length;

            if (ICCPublicKeyLength <= (issuerPublicKeyModulusLength - 42))
            {
                int padLength = issuerPublicKeyModulusLength - 42 - ICCPublicKeyLength;
                byte[] newLeftMost = new byte[ICCPublicKeyorLeftmostDigitsofIssuerPublicKey.Length - padLength];
                Array.Copy(ICCPublicKeyorLeftmostDigitsofIssuerPublicKey, 0, newLeftMost, 0, newLeftMost.Length);
                UnpaddedICCPublicKeyorLeftmostDigitsofIssuerPublicKey = newLeftMost;
            }
            else
                UnpaddedICCPublicKeyorLeftmostDigitsofIssuerPublicKey = ICCPublicKeyorLeftmostDigitsofIssuerPublicKey;

            HashResult = new byte[20];
            Array.Copy(raw, pos, HashResult, 0, HashResult.Length);
            pos = pos + HashResult.Length;

            RecoveredDataTrailer = raw[pos];
            pos++;

            return pos;
        }

        internal bool ValidateHash()
        {
            byte[] concatenated = Formatting.ConcatArrays(
                new byte[] { CertificateFormat },
                ApplicationPAN,
                ExpiryDate,
                CertificateSerialNumber,
                new byte[] { HashAlgorithmIndicator },
                new byte[] { PublicKeyAlgorithmIndicator },
                new byte[] { ICCPublicKeyLength },
                new byte[] { ICCPublicKeyExponentLength },
                ICCPublicKeyorLeftmostDigitsofIssuerPublicKey,
                IccuPulicKeyRemainder,
                Exponent,
                staticDataToBeAuthenticated
                );

            byte[] hash = SHA1.Create().ComputeHash(concatenated);

            if (Formatting.ByteArrayToHexString(HashResult) != Formatting.ByteArrayToHexString(hash))
                return false;

            return true;
        }

        internal static IccPublicKeyCertificate BuildAndValidatePublicKey(KernelDatabaseBase database, StaticDataToBeAuthenticatedList staticDataToBeAuthenticated, byte[] issuerPublicKeyModulus, byte[] issuerPublicKeyExponent)
        {
            //section 6.4 EMV 4.3 Book 2

            TLV iccPublicKeyCertificate = database.Get(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PUBLIC_KEY_CERTIFICATE_9F46_KRN);
            TLV iccPublicKeyExponent = database.Get(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PUBLIC_KEY_EXPONENT_9F47_KRN);
            TLV iccPublicKeyRemainder = database.Get(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PUBLIC_KEY_REMAINDER_9F48_KRN);

            if (iccPublicKeyCertificate.Value.Length != issuerPublicKeyModulus.Length)
                return null;

            byte[] decrypt = DecryptRSA(iccPublicKeyCertificate.Value, issuerPublicKeyModulus, issuerPublicKeyExponent);

            IccPublicKeyCertificate iccCertData = new IccPublicKeyCertificate(decrypt, issuerPublicKeyModulus.Length,
                iccPublicKeyRemainder == null ? new byte[0] : iccPublicKeyRemainder.Value, iccPublicKeyExponent.Value, database.StaticDataToBeAuthenticated.BuildStaticDataToBeAuthenticated());

            if (iccCertData.RecoveredDataTrailer != 0xBC)
                return null;

            if (iccCertData.RecoveredDataHeader != 0x6A)
                return null;

            if (iccCertData.CertificateFormat != 0x04)
                return null;

            if (!iccCertData.ValidateHash())
                return null;

            string pan = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN).Value);
            string panToCompare = Formatting.ByteArrayToHexString(iccCertData.ApplicationPAN).Replace("FF", "");

            if (!pan.StartsWith(panToCompare))
                return null;

            DateTime expiry = DateTime.ParseExact(Formatting.BcdToString(iccCertData.ExpiryDate), "MMyy", System.Globalization.CultureInfo.InvariantCulture);
            //TODO: if you have a test tool trying to use an expired cert then comment this test out or update your test tool
            //if (expiry <= DateTime.Now)
            //{
            //    Logger.Log("Error: Trying to use an expired issuer public key");
            //    return null;
            //}

            if (iccCertData.PublicKeyAlgorithmIndicator != 0x01)
                return null;

            if (iccPublicKeyRemainder != null)
                iccCertData.Modulus = Formatting.ConcatArrays(iccCertData.UnpaddedICCPublicKeyorLeftmostDigitsofIssuerPublicKey, iccPublicKeyRemainder.Value);
            else
                iccCertData.Modulus = iccCertData.UnpaddedICCPublicKeyorLeftmostDigitsofIssuerPublicKey;

            return iccCertData;
        }
    }
    public class IssuerPublicKeyCertificate : PublicKeyCertificate
    {
        public byte RecoveredDataHeader;
        public byte CertificateFormat;
        public byte[] IssuerIdentifier;
        public byte[] CertificateSerialNumber;
        public byte IssuerPublicKeyLength;
        public byte IssuerPublicKeyExponentLength;
        public byte[] IssuerPublicKeyorLeftmostDigitsofIssuerPublicKey;
        public byte[] UnpaddedIssuerPublicKeyorLeftmostDigitsofIssuerPublicKey;
        public byte[] HashResult;
        public byte RecoveredDataTrailer;

        private byte[] IssuerPublicKeyRemainder;
        private int certAuthPublicKeyModulusLength;

        public static Logger Logger = new Logger(typeof(IssuerPublicKeyCertificate));

        public IssuerPublicKeyCertificate(byte[] issuerCertData, int certAuthPublicKeyModulusLength, byte[] issuerPublicKeyRemainder, byte[] issuerPublicKeyExponent)
        {
            this.certAuthPublicKeyModulusLength = certAuthPublicKeyModulusLength;
            this.IssuerPublicKeyRemainder = issuerPublicKeyRemainder;
            this.Exponent = issuerPublicKeyExponent;
            Deserialize(issuerCertData, 0);
        }

        public int Deserialize(byte[] raw, int pos)
        {
            RecoveredDataHeader = raw[pos];
            pos++;
            CertificateFormat = raw[pos];
            pos++;
            IssuerIdentifier = new byte[4];
            Array.Copy(raw, pos, IssuerIdentifier, 0, IssuerIdentifier.Length);
            pos = pos + IssuerIdentifier.Length;
            ExpiryDate = new byte[2];
            Array.Copy(raw, pos, ExpiryDate, 0, ExpiryDate.Length);
            pos = pos + ExpiryDate.Length;
            CertificateSerialNumber = new byte[3];
            Array.Copy(raw, pos, CertificateSerialNumber, 0, CertificateSerialNumber.Length);
            pos = pos + CertificateSerialNumber.Length;
            HashAlgorithmIndicator = raw[pos];
            pos++;
            PublicKeyAlgorithmIndicator = raw[pos];
            pos++;
            IssuerPublicKeyLength = raw[pos];
            pos++;
            IssuerPublicKeyExponentLength = raw[pos];
            pos++;

            IssuerPublicKeyorLeftmostDigitsofIssuerPublicKey = new byte[certAuthPublicKeyModulusLength - 36];
            Array.Copy(raw, pos, IssuerPublicKeyorLeftmostDigitsofIssuerPublicKey, 0, IssuerPublicKeyorLeftmostDigitsofIssuerPublicKey.Length);
            pos = pos + IssuerPublicKeyorLeftmostDigitsofIssuerPublicKey.Length;

            if (IssuerPublicKeyLength <= (certAuthPublicKeyModulusLength - 36))
            {
                int padLength = certAuthPublicKeyModulusLength - 36 - IssuerPublicKeyLength;
                byte[] newLeftMost = new byte[IssuerPublicKeyorLeftmostDigitsofIssuerPublicKey.Length - padLength];
                Array.Copy(IssuerPublicKeyorLeftmostDigitsofIssuerPublicKey, 0, newLeftMost, 0, newLeftMost.Length);
                UnpaddedIssuerPublicKeyorLeftmostDigitsofIssuerPublicKey = newLeftMost;
            }
            else
                UnpaddedIssuerPublicKeyorLeftmostDigitsofIssuerPublicKey = IssuerPublicKeyorLeftmostDigitsofIssuerPublicKey;

            HashResult = new byte[20];
            Array.Copy(raw, pos, HashResult, 0, HashResult.Length);
            pos = pos + HashResult.Length;

            RecoveredDataTrailer = raw[pos];
            pos++;

            return pos;
        }

        internal bool ValidateHash()
        {
            byte[] concatenated = Formatting.ConcatArrays(
                new byte[] { CertificateFormat },
                IssuerIdentifier,
                ExpiryDate,
                CertificateSerialNumber,
                new byte[] { HashAlgorithmIndicator },
                new byte[] { PublicKeyAlgorithmIndicator },
                new byte[] { IssuerPublicKeyLength },
                new byte[] { IssuerPublicKeyExponentLength },
                IssuerPublicKeyorLeftmostDigitsofIssuerPublicKey,
                IssuerPublicKeyRemainder,
                Exponent
                );

            byte[] hash = SHA1.Create().ComputeHash(concatenated);

            if (Formatting.ByteArrayToHexString(HashResult) != Formatting.ByteArrayToHexString(hash))
                return false;

            return true;
        }



        internal static IssuerPublicKeyCertificate BuildAndValidatePublicKey(KernelDatabaseBase database, byte[] caPublicKeyModulus, byte[] caPublicKeyExponent)
        {
            //section 6.3 EMV 4.3 Book 2

            TLV issuerPublicKeyCertificate = database.Get(EMVTagsEnum.ISSUER_PUBLIC_KEY_CERTIFICATE_90_KRN);
            TLV issuerPublicKeyExponent = database.Get(EMVTagsEnum.ISSUER_PUBLIC_KEY_EXPONENT_9F32_KRN);
            TLV issuerPublicKeyRemainder = database.Get(EMVTagsEnum.ISSUER_PUBLIC_KEY_REMAINDER_92_KRN);

            if (issuerPublicKeyCertificate.Value.Length != caPublicKeyModulus.Length)
                return null;

            byte[] decrypt = DecryptRSA(issuerPublicKeyCertificate.Value, caPublicKeyModulus, caPublicKeyExponent);

            IssuerPublicKeyCertificate issuerCertData = new IssuerPublicKeyCertificate(decrypt, caPublicKeyModulus.Length,
                issuerPublicKeyRemainder == null ? new byte[0] : issuerPublicKeyRemainder.Value, issuerPublicKeyExponent.Value);

            if (issuerCertData.RecoveredDataTrailer != 0xBC)
                return null;

            if (issuerCertData.RecoveredDataHeader != 0x6A)
                return null;

            if (issuerCertData.CertificateFormat != 0x02)
                return null;

            if (!issuerCertData.ValidateHash())
                return null;

            string pan = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN).Value);
            string issuerIdentifier = Formatting.ByteArrayToHexString(issuerCertData.IssuerIdentifier).Replace("FF", "");

            if (!pan.StartsWith(issuerIdentifier))
                return null;

            DateTime expiry = DateTime.ParseExact(Formatting.BcdToString(issuerCertData.ExpiryDate), "MMyy", System.Globalization.CultureInfo.InvariantCulture);
            //TODO: if you have a test tool trying to use an expired cert then comment this test out or update your test tool
            //if (expiry <= DateTime.Now)
            //{
            //    Logger.Log("Error: Trying to use an expired issuer public key");
            //    return null;
            //}

            //step 10 optional

            if (issuerCertData.PublicKeyAlgorithmIndicator != 0x01)
                return null;

            if (issuerPublicKeyRemainder != null)
                issuerCertData.Modulus = Formatting.ConcatArrays(issuerCertData.UnpaddedIssuerPublicKeyorLeftmostDigitsofIssuerPublicKey, issuerPublicKeyRemainder.Value);
            else
                issuerCertData.Modulus = issuerCertData.UnpaddedIssuerPublicKeyorLeftmostDigitsofIssuerPublicKey;

            return issuerCertData;
        }
    }
    public class PublicKeyCertificateManager
    {
        public static Logger Logger = new Logger(typeof(PublicKeyCertificateManager));

        private List<CAPublicKeyCertificate> CAPublicKeyCertificates { get; set; }
        private List<RevokedCAPublicKeyCertificate> RevokedCAPublicKeyCertificates { get; set; }

        public PublicKeyCertificateManager(IConfigurationProvider configProvider)
        {
            LoadCAPublicKeyCertificates(configProvider);
            LoadRevokedCAPublicKeyCertificates(configProvider);
        }
        private bool CheckIfCertInCertRevocationList(CAPublicKeyCertificate capk)
        {
            int found = RevokedCAPublicKeyCertificates.Count(x =>
            {
                DateTime dt = DateTime.ParseExact(Formatting.BcdToString(x.ExpiryDate), "MMyy", System.Globalization.CultureInfo.InvariantCulture);
                if (x.RID == capk.RID && x.Index == capk.Index && dt < DateTime.Now)
                    return true;
                else
                    return false;
            });
            if (found > 0)
            {
                Logger.Log("Certificate is revoked, index:" + Formatting.ByteArrayToHexString(new byte[] { capk.Index }));
                return true;
            }
            else
                return false;
        }

        public void LoadRevokedCAPublicKeyCertificates(IConfigurationProvider configProvider)
        {
            Logger.Log("Revoked CA Public Key Certificates:");
            RevokedCAPublicKeyCertificates = XMLUtil<List<RevokedCAPublicKeyCertificate>>.Deserialize(configProvider.GetRevokedPublicKeyCertificatesXML());
        }
        public void LoadCAPublicKeyCertificates(IConfigurationProvider configProvider)
        {
            Logger.Log("CA Public Key Certificates:");
            CAPublicKeyCertificates = XMLUtil<List<CAPublicKeyCertificate>>.Deserialize(configProvider.GetPublicKeyCertificatesXML());
        }

        internal CAPublicKeyCertificate GetCAPK(RIDEnum ridEnum, byte authIndex)
        {
            CAPublicKeyCertificate capk = CAPublicKeyCertificates.Find(x => x.Index == authIndex && x.RID == ridEnum);
            if (capk != null)
            {
                if (CheckIfCertInCertRevocationList(capk))
                    return null;

                Logger.Log("Certificate found for index: " + Formatting.ByteArrayToHexString(new byte[] { authIndex }));
            }
            else
                Logger.Log("Certificate not found for index: " + Formatting.ByteArrayToHexString(new byte[] { authIndex }));

            return capk;
        }
    }
}

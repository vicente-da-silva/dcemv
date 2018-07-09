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
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using System;
using System.IO;
using System.Text;

namespace DCEMV.EMVSecurity
{
    public class EMVRSASecurity
    {
        public static AsymmetricCipherKeyPair GenerateCAKey()
        {
            return CreateRSAKeyPair(2048);
        }
        public static X509Certificate GenerateCACert(string commonName, DateTime now, AsymmetricCipherKeyPair caKeys)
        {
            return CreateCertificate("CA Cert", now, now.AddYears(20), new BigInteger("1"), caKeys.Private, null, caKeys); //CA signed with its own private key
        }
        public static AsymmetricCipherKeyPair GenerateIssuerKey()
        {
            return CreateRSAKeyPair(2048);
        }
        public static X509Certificate GenerateIssuerCert(string commonName, DateTime now, AsymmetricCipherKeyPair caKeys, X509Certificate caCert, AsymmetricCipherKeyPair issuerKeys)
        {
            return CreateCertificate("Payloola", now, now.AddYears(10), new BigInteger("1"), caKeys.Private, caCert, issuerKeys);
        }
        public static AsymmetricCipherKeyPair GenerateCardKey()
        {
            return CreateRSAKeyPair(2048);
        }
        public static X509Certificate GenerateCardCert(string commonName, DateTime now, AsymmetricCipherKeyPair issuerKeys, X509Certificate issuerCert, AsymmetricCipherKeyPair iccKeys)
        {
            return CreateCertificate("CardXYZ", now, now.AddYears(5), new BigInteger("1"), issuerKeys.Private, issuerCert, iccKeys);
        }

        public static string PrintPublicKey(AsymmetricCipherKeyPair keys)
        {
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keys.Public);
            byte[] serializedBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            string serialized = Convert.ToBase64String(serializedBytes);
            return "Key==>" + serialized;
        }
        public static string PrintPrivateKey(AsymmetricCipherKeyPair keys)
        {
            StringBuilder sb = new StringBuilder();
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keys.Private);
            RsaPrivateKeyStructure rsa = RsaPrivateKeyStructure.GetInstance(privateKeyInfo.ParsePrivateKey());

            byte[] serializedBytes = privateKeyInfo.ToAsn1Object().GetDerEncoded();
            string serialized = Convert.ToBase64String(serializedBytes);
            sb.AppendLine("Key==>" + serialized);
            sb.AppendLine("Prime 1 ==>" + rsa.Prime1.ToString());
            sb.AppendLine("Prime 2 ==>" + rsa.Prime2.ToString());
            sb.AppendLine("Modulus ==>" + rsa.Modulus.ToString());
            sb.AppendLine("Coefficient ==>" + rsa.Coefficient.ToString());
            sb.AppendLine("Exponent1 ==>" + rsa.Exponent1.ToString());
            sb.AppendLine("Exponent2 ==>" + rsa.Exponent2.ToString());
            sb.AppendLine("PrivateExponent ==>" + rsa.PrivateExponent.ToString());
            sb.AppendLine("PublicExponent ==>" + rsa.PublicExponent.ToString());
            return sb.ToString();
        }
        public static string PrintPEMString(X509Certificate cert)
        {
            StringBuilder sbPEM = new StringBuilder();
            PemWriter CSRPemWriter = new PemWriter(new StringWriter(sbPEM));
            CSRPemWriter.WriteObject(cert);
            CSRPemWriter.Writer.Flush();
            return sbPEM.ToString();
        }

        private static AsymmetricCipherKeyPair CreateRSAKeyPair(int strength)
        {
            RsaKeyPairGenerator caKeyGenerator = new RsaKeyPairGenerator();
            caKeyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), strength));
            return caKeyGenerator.GenerateKeyPair();
        }
        private static X509Certificate CreateCertificate(string subjectName, DateTime startDate, DateTime expiryDate, BigInteger serialNumber, AsymmetricKeyParameter parentPrivateKey, X509Certificate parentCert, AsymmetricCipherKeyPair keysToSign)
        {
            X509V3CertificateGenerator certGen = new X509V3CertificateGenerator();
            certGen.SetSerialNumber(serialNumber);
            if (parentCert != null)
                certGen.SetIssuerDN(parentCert.SubjectDN);
            else//is CA cert
                certGen.SetIssuerDN(new X509Name("CN=" + subjectName));
            certGen.SetNotBefore(startDate);
            certGen.SetNotAfter(expiryDate);
            certGen.SetSubjectDN(new X509Name("CN=" + subjectName));
            certGen.SetPublicKey(keysToSign.Public);

            if (parentCert == null) //is CA cert
                certGen.AddExtension(X509Extensions.BasicConstraints, false, new BasicConstraints(true));
            else
                certGen.AddExtension(X509Extensions.AuthorityKeyIdentifier, false, new AuthorityKeyIdentifierStructure(parentCert));

            certGen.AddExtension(X509Extensions.SubjectKeyIdentifier, false, new SubjectKeyIdentifierStructure(keysToSign.Public));

            X509Certificate cert = certGen.Generate(new Asn1SignatureFactory("SHA512WITHRSA", parentPrivateKey));

            return cert;
        }
    }
}

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
using Tpm2Lib;

namespace DCEMV.TPMDriver
{
    public class DeviceProvioningResult
    {
        byte[] DevicePublicEncryptionKey { get; }
        byte[] DevicePublicSignatureVerificationKey { get; }
        string SerialNumber { get; }
        
        public DeviceProvioningResult(string serialNumber, byte[] devicePublicEncryptionKey, byte[] devicePublicSignatureVerificationKey)
        { 
            this.DevicePublicEncryptionKey = devicePublicEncryptionKey;
            this.DevicePublicSignatureVerificationKey = devicePublicSignatureVerificationKey;
            this.SerialNumber = serialNumber;
        }
    }
    public class TPMDriver
    {
        private Tpm2 tpm;

        public TPMDriver(Tpm2 tpm)
        {
            this.tpm = tpm;
        }

        public DeviceProvioningResult ProvisionTPM(string provisioningScript, byte[] authValue, byte[] serverPublicEncryptionKey, byte[] serverPublicSignatureVerificationKey)
        {
            /*
             * A terminal will have an admin screen, on which there will be a provision button, 
             * the user presses the button, the terminal calls the web service, and the 
             * provioning process begins
             * 
             * This provisioning process can only be done in a secure location, as the provisioning
             * web service will only be available on the network in the secure location
             * 
             * First we run a powershell script, recived from the web service, that will take ownership of the TPM and 
             * set its owner auth password to a randomly generated password
             * 
             * The web service will store this password against the serial number of this device
             * 
             * Tell the TPM to generate a RSA key pair to be used for decrypting server sent data
             * 
             * Tell the TPM to generate a RSA key pair to be used for signing this terminals to be sent data
             * 
             * The web service will store the public keys for each of these pairs against the serial number of this device
             * 
             * The web service will pass the 2 server public keys (encryption and signature verification keys) to the terminal for storage
             * 
             * Terminal is now provisioned
             */

            return new DeviceProvioningResult("",null,null);
        }

        public SignatureRsassa SignMessageToSend(TpmHandle keyHandle, byte[] message)
        {
            TpmHash dataToSign = TpmHash.FromData(TpmAlgId.Sha1, message);
            return tpm.Sign(keyHandle,dataToSign.HashData, new NullSigScheme(), TpmHashCheck.NullHashCheck()) as SignatureRsassa;
        }

        public byte[] EcryptMessageToSendOffTPM(TpmPublic keyPublic, byte[] message)
        {
            return keyPublic.EncryptOaep(message, new byte[0]);
        }

        public byte[] EcryptMessageToSend(TpmHandle keyHandle, byte[] message)
        {
            IAsymSchemeUnion decScheme = new SchemeOaep(TpmAlgId.Sha1); //the hash algorithm used to digest the message
            return tpm.RsaEncrypt(keyHandle, message, decScheme, new byte[0]);
        }

        public byte[] DecryptReceivedMessage(TpmHandle keyHandle, byte[] encrypted)
        {
            //TODO: how to auth the command?
            IAsymSchemeUnion decScheme = new SchemeOaep(TpmAlgId.Sha1); //the hash algorithm used to digest the message
            return tpm.RsaDecrypt(keyHandle, encrypted, decScheme, new byte[0]);
        }

        public TkVerified VerifySignatureOfRecivedMessage(TpmPublic keyPublic, byte[] message, ISignatureUnion signature)
        {
            TpmHash dataToSign = TpmHash.FromData(TpmAlgId.Sha256, message);
            TpmHandle pubHandle = tpm.LoadExternal(null, keyPublic, TpmHandle.RhOwner);
            return tpm.VerifySignature(pubHandle, dataToSign.HashData, signature);
        }

        public bool VerifySignatureOfRecivedMessageOffTPM(TpmPublic keyPublic, byte[] message, ISignatureUnion signature)
        {
            return keyPublic.VerifySignatureOverData(message, signature);
        }

        public TpmHandle GenerateRSASigningKeyPair(AuthValue ownerAuth, out TpmPublic keyPublic, byte[] keyAuth, TpmHandle persistantHandle)
        {
            var keyTemplate = new TpmPublic(TpmAlgId.Sha1,                                      // Name algorithm
                                               ObjectAttr.UserWithAuth | ObjectAttr.Sign |      // Signing key
                                               ObjectAttr.FixedParent | ObjectAttr.FixedTPM |   // Non-migratable 
                                               ObjectAttr.SensitiveDataOrigin,
                                               new byte[0],                                     // no policy
                                               new RsaParms(new SymDefObject(),                 // not a restricted decryption key
                                                            new SchemeRsassa(TpmAlgId.Sha1),    // an unrestricted signing key
                                                            2048, 
                                                            0),
                                               new Tpm2bPublicKeyRsa());

            var sensCreate = new SensitiveCreate(keyAuth, new byte[0]);

            //TpmPublic keyPublic;

            byte[] outsideInfo = Globs.GetRandomBytes(8);
            var creationPcrArray = new PcrSelection[0];

            TpmHandle h = tpm[ownerAuth].CreatePrimary(
                TpmHandle.RhOwner,                          // In the owner-hierarchy
                sensCreate,                                 // With this auth-value
                keyTemplate,                                // Describes key
                outsideInfo,                                // For creation ticket
                creationPcrArray,                           // For creation ticket
                out keyPublic,                              // Out pubKey and attributes
                out CreationData creationData,                           // Not used here
                out byte[] creationHash,                           // Not used here
                out TkCreation creationTicket);

            tpm.EvictControl(TpmHandle.RhOwner, h, persistantHandle);

            return h;
        }

        public TpmHandle GenerateRsaEncryptionKeyPair(AuthValue ownerAuth, out TpmPublic keyPublic, byte[] keyAuth, TpmHandle persistantHandle)
        {
            var sensCreate = new SensitiveCreate(keyAuth, new byte[0]);

            TpmPublic keyTemplate = new TpmPublic(
                TpmAlgId.Sha1,    
                    ObjectAttr.UserWithAuth | ObjectAttr.Decrypt  |  
                    ObjectAttr.FixedParent | ObjectAttr.FixedTPM | 
                    ObjectAttr.SensitiveDataOrigin,
                new byte[0],                                            
                new RsaParms(
                    new SymDefObject(),                                 //a unrestricted decryption key
                    new NullAsymScheme(),                               //not a signing key
                    2048,
                    0),
                new Tpm2bPublicKeyRsa());

            byte[] outsideInfo = Globs.GetRandomBytes(8);
            var creationPcrArray = new PcrSelection[0];


            TpmHandle h = tpm[ownerAuth].CreatePrimary(
                TpmRh.Owner,
                sensCreate,
                keyTemplate,
                outsideInfo,
                creationPcrArray,
                out keyPublic,
                out CreationData creationData,
                out byte[] creationHash,
                out TkCreation creationTicket);

            tpm.EvictControl(TpmHandle.RhOwner, h, persistantHandle);

            return h;
        }
    }
}

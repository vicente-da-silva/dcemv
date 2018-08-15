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
using DCEMV.ISO7816Protocol;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{
    public static class VerifySAD
    {
        public static byte[] VerifySSAD(ICCDynamicDataType iccDDType, KernelDatabaseBase database, CAPublicKeyCertificate caPublicKey, byte[] sdadRaw)
        {
            //section 5.4 of EMV book 2 - SDA
            //1.If the Signed Static Application Data has a length different from the
            //length of the ICC Public Key Modulus, SDA has failed
            IssuerPublicKeyCertificate ipk = IssuerPublicKeyCertificate.BuildAndValidatePublicKey(database, caPublicKey.Modulus, caPublicKey.Exponent);
            if (ipk == null) return null;

            if (sdadRaw.Length != ipk.Modulus.Length)
                return null;

            //2.In order to obtain the Recovered Data specified in Table 7, apply the
            //recovery function specified in Annex A2.1 on the Signed Static Application
            //Data using the Issuer Public Key in conjunction with the corresponding
            //algorithm.If the Recovered Data Trailer is not equal to 'BC', SDA has
            //failed.
            byte[] decrypted = PublicKeyCertificate.DecryptRSA(sdadRaw, ipk.Modulus, ipk.Exponent );
            SSAD ssad = new SSAD(decrypted);

            //3.Check the Recovered Data Header. If it is not '6A', SDA has failed.
            if (ssad.DataHeader != 0x6A)
                return null;
            //4.Check the Signed Data Format. If it is not '03', SDA has failed. 
            //For Visa specialpurpose readers it may return 0x93 if offline data authentication is supported for online transactions (in the TTQ)
            if (ssad.SignedDataFormat != 0x03 && ssad.SignedDataFormat != 0x05 && ssad.SignedDataFormat != 0x95)
                return null;

            //5.Concatenate from left to right the second to the fifth data elements in
            //Table 7(that is, Signed Data Format through Pad Pattern), followed by
            //the static data to be authenticated as specified in section 10.3 of Book 3.If
            //the Static Data Authentication Tag List is present and contains tags other
            //than '82', then SDA has failed
            byte[] dataForHash = ssad.Concat(database.StaticDataToBeAuthenticated.BuildStaticDataToBeAuthenticated());

            //6.Apply the indicated hash algorithm(derived from the Hash Algorithm
            //Indicator) to the result of the concatenation of the previous step to
            //produce the hash result.
            byte[] hash = SHA1.Create().ComputeHash(dataForHash);

            //7.Compare the calculated hash result from the previous step with the
            //recovered Hash Result.If they are not the same, SDA has failed.
            if (Formatting.ByteArrayToHexString(ssad.HashResult) != Formatting.ByteArrayToHexString(hash))
                return null;

            return ssad.DataAuthenticationCode;

        }
        public static ICCDynamicData VerifySDAD(ICCDynamicDataType iccDDType, KernelDatabaseBase database, CAPublicKeyCertificate caPublicKey, byte[] sdadRaw)
        {
            //section 6.5.2 of EMV book 2 - DDA

            //1.If the Signed Dynamic Application Data has a length different from the
            //length of the ICC Public Key Modulus, CDA has failed
            IssuerPublicKeyCertificate ipk = IssuerPublicKeyCertificate.BuildAndValidatePublicKey(database, caPublicKey.Modulus, caPublicKey.Exponent);
            if (ipk == null) return null;
            IccPublicKeyCertificate iccpk = IccPublicKeyCertificate.BuildAndValidatePublicKey(database, database.StaticDataToBeAuthenticated, ipk.Modulus, ipk.Exponent);
            if (iccpk == null) return null;

            if (sdadRaw.Length != iccpk.Modulus.Length)
                return null;

            //2.To obtain the recovered data specified in Table 22, apply the recovery
            //function as specified in Annex A2.1 on the Signed Dynamic Application
            //Data using the ICC Public Key in conjunction with the corresponding
            //algorithm.If the Recovered Data Trailer is not equal to 'BC', CDA has
            //failed 
            byte[] decrypted = PublicKeyCertificate.DecryptRSA(sdadRaw, iccpk.Modulus, iccpk.Exponent);
            SDAD sdad = new SDAD(decrypted);

            //3.Check the Recovered Data Header. If it is not '6A', CDA has failed.
            if (sdad.DataHeader != 0x6A)
                return null;
            //4.Check the Signed Data Format. If it is not '05', CDA has failed. 
            //For Visa specialpurpose readers it may return 0x95 iff offline data authentication is supported for online transactions (in the TTQ)
            if (sdad.SignedDataFormat != 0x05 && sdad.SignedDataFormat != 0x95)
                return null;

            ICCDynamicData iccDD = new ICCDynamicData(database, sdad.ICCDynamicData, iccDDType);

            //5.Concatenate from left to right the second to the sixth data elements in
            //Table 17(that is, Signed Data Format through Pad Pattern), followed by
            //the data elements specified by the DDOL.
            TLV ddol = database.Get(EMVTagsEnum.DYNAMIC_DATA_AUTHENTICATION_DATA_OBJECT_LIST_DDOL_9F49_KRN);
            byte[] ddolRelatedData;
            if (ddol == null)
            {
                TLV unpred = database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN);
                unpred.Val.PackValue(unpred.Val.GetLength());
                ddolRelatedData = unpred.Value;
            }
            else
            {
                ddolRelatedData = CommonRoutines.PackRelatedDataTag(database, ddol);
            }

            byte[] dataForHash = sdad.Concat(ddolRelatedData);

            //6.Apply the indicated hash algorithm(derived from the Hash Algorithm
            //Indicator) to the result of the concatenation of the previous step to
            //produce the hash result.
            byte[] hash = SHA1.Create().ComputeHash(dataForHash);


            //7.Compare the calculated hash result from the previous step with the
            //recovered Hash Result.If they are not the same, DDA has failed.
            if (Formatting.ByteArrayToHexString(sdad.HashResult) != Formatting.ByteArrayToHexString(hash))
                return null;

            return iccDD;
        }
        public static ICCDynamicData VerifySDAD_K3(ICCDynamicDataType iccDDType, KernelDatabaseBase database, CAPublicKeyCertificate caPublicKey, byte[] sdadRaw)
        {
            //section 6.5.2 of EMV book 2 - DDA

            //1.If the Signed Dynamic Application Data has a length different from the
            //length of the ICC Public Key Modulus, CDA has failed
            IssuerPublicKeyCertificate ipk = IssuerPublicKeyCertificate.BuildAndValidatePublicKey(database, caPublicKey.Modulus, caPublicKey.Exponent);
            if (ipk == null) return null;
            IccPublicKeyCertificate iccpk = IccPublicKeyCertificate.BuildAndValidatePublicKey(database, database.StaticDataToBeAuthenticated, ipk.Modulus, ipk.Exponent);
            if (iccpk == null) return null;

            if (sdadRaw.Length != iccpk.Modulus.Length)
                return null;

            //2.To obtain the recovered data specified in Table 22, apply the recovery
            //function as specified in Annex A2.1 on the Signed Dynamic Application
            //Data using the ICC Public Key in conjunction with the corresponding
            //algorithm.If the Recovered Data Trailer is not equal to 'BC', CDA has
            //failed 
            byte[] decrypted = PublicKeyCertificate.DecryptRSA(sdadRaw, iccpk.Modulus, iccpk.Exponent );
            SDAD sdad = new SDAD(decrypted);

            //3.Check the Recovered Data Header. If it is not '6A', CDA has failed.
            if (sdad.DataHeader != 0x6A)
                return null;
            //4.Check the Signed Data Format. If it is not '05', CDA has failed. 
            //For Visa specialpurpose readers it may return 0x95 iff offline data authentication is supported for online transactions (in the TTQ)
            if (sdad.SignedDataFormat != 0x05 && sdad.SignedDataFormat != 0x95)
                return null;

            ICCDynamicData iccDD = new ICCDynamicData(database, sdad.ICCDynamicData, iccDDType);

            //5.Concatenate from left to right the second to the sixth data elements in
            //Table 17(that is, Signed Data Format through Pad Pattern), followed by
            //the data elements specified by the DDOL.

            //C.1 K3 Kernel wants it done this way rather than they way sepecified in step 5
            //The Terminal Dynamic Data elements input to the hash algorithm shall be as
            //specified in Table C-1 instead of being specified in the DDOL(as the DDOL is
            //not a recognized data element for Kernel 3).The kernel may treat the tags
            //specified in Table C-1 as default DDOLs for fDDA version '01'
            List<byte[]> hashList = new List<byte[]>
            {
                database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN).Value,
                database.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN).Value,
                database.Get(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN).Value,
                database.Get(EMVTagsEnum.CARD_AUTHENTICATION_RELATED_DATA_9F69_KRN3).Value
            };

            //6.Apply the indicated hash algorithm(derived from the Hash Algorithm
            //Indicator) to the result of the concatenation of the previous step to
            //produce the hash result.
            byte[] dataForHash = sdad.Concat(hashList.SelectMany(x => x).ToArray());
            byte[] hash = SHA1.Create().ComputeHash(dataForHash);


            //7.Compare the calculated hash result from the previous step with the
            //recovered Hash Result.If they are not the same, DDA has failed.
            if (Formatting.ByteArrayToHexString(sdad.HashResult) != Formatting.ByteArrayToHexString(hash))
                return null;

            return iccDD;
        }
        public static ICCDynamicData VerifySDAD(ICCDynamicDataType iccDDType ,bool isFirstGenAC, KernelDatabaseBase database, StaticDataToBeAuthenticatedList staticDataToBeAuthenticated, CAPublicKeyCertificate caPublicKey, CardResponse genACCardResponse)
        {
            EMVGenerateACResponse genAcResponse = (genACCardResponse.ApduResponse as EMVGenerateACResponse);

            //section 6.6.2 of EMV book 2 - CDA

            //1.If the Signed Dynamic Application Data has a length different from the
            //length of the ICC Public Key Modulus, CDA has failed
            byte[] sdadRaw = genAcResponse.SignedDynamicApplicationData.Value;
            IssuerPublicKeyCertificate ipk = IssuerPublicKeyCertificate.BuildAndValidatePublicKey(database, caPublicKey.Modulus, caPublicKey.Exponent);
            if (ipk == null) return null;
            IccPublicKeyCertificate iccpk = IccPublicKeyCertificate.BuildAndValidatePublicKey(database, staticDataToBeAuthenticated, ipk.Modulus, ipk.Exponent);
            if (iccpk == null) return null;

            if (sdadRaw.Length != iccpk.Modulus.Length)
                return null;

            //2.To obtain the recovered data specified in Table 22, apply the recovery
            //function as specified in Annex A2.1 on the Signed Dynamic Application
            //Data using the ICC Public Key in conjunction with the corresponding
            //algorithm.If the Recovered Data Trailer is not equal to 'BC', CDA has
            //failed 
            byte[] decrypted = PublicKeyCertificate.DecryptRSA(sdadRaw, iccpk.Modulus,  iccpk.Exponent );
            SDAD sdad = new SDAD(decrypted);

            //3.Check the Recovered Data Header. If it is not '6A', CDA has failed.
            if (sdad.DataHeader != 0x6A)
                return null;
            //4.Check the Signed Data Format. If it is not '05', CDA has failed.
            if (sdad.SignedDataFormat != 0x05)
                return null;

            //5. Retrieve from the ICC Dynamic Data the data specified in Table 19
            ICCDynamicData iccDD = new ICCDynamicData(database, sdad.ICCDynamicData, iccDDType);

            //6.Check that the Cryptogram Information Data retrieved from the ICC
            //Dynamic Data is equal to the Cryptogram Information Data obtained from
            //the response to the GENERATE AC command. If this is not the case, CDA
            //has failed.
            if (genAcResponse.CryptogramInformationData.Value[0] != iccDD.CryptogramInformationData)
                return null;

            //7.Concatenate from left to right the second to the sixth data elements in
            //Table 22(that is, Signed Data Format through Pad Pattern), followed by
            //the Unpredictable Number.
            byte[] unpredicatbleNumber = database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN).Value;
            byte[] dataForHash = sdad.Concat(unpredicatbleNumber);

            //8.Apply the indicated hash algorithm (derived from the Hash Algorithm
            //Indicator) to the result of the concatenation of the previous step to
            //produce the hash result.
            byte[] hash = SHA1.Create().ComputeHash(dataForHash);

            //9.Compare the calculated hash result from the previous step with the
            //recovered Hash Result.If they are not the same, CDA has failed.
            if (Formatting.ByteArrayToHexString(sdad.HashResult) != Formatting.ByteArrayToHexString(hash))
                return null;

            //10. Concatenate from left to right the values of the following data elements:
            List<byte[]> result = new List<byte[]>();
            if (isFirstGenAC)
            {
                //-The values of the data elements specified by, and in the order they
                //appear in the PDOL, and sent by the terminal in the GET
                //PROCESSING OPTIONS command.
                result.Add(database.Get(EMVTagsEnum.PDOL_RELATED_DATA_DF8111_KRN2).Value);
                //-The values of the data elements specified by, and in the order they
                //appear in the CDOL1, and sent by the terminal in the first
                //GENERATE AC command.
                result.Add(database.Get(EMVTagsEnum.CDOL1_RELATED_DATA_DF8107_KRN2).Value);
                //-The tags, lengths, and values of the data elements returned by the ICC
                //in the response to the GENERATE AC command in the order they are
                //returned, with the exception of the Signed Dynamic Application Data.
                foreach(TLV tlv in genAcResponse.GetResponseTags())
                    if (tlv.Tag.TagLable != EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag)
                        result.Add(tlv.Serialize());
            }
            else
            {
                //-The values of the data elements specified by, and in the order they
                //appear in the PDOL, and sent by the terminal in the GET
                //PROCESSING OPTIONS command.
                result.Add(database.Get(EMVTagsEnum.PDOL_RELATED_DATA_DF8111_KRN2).Value);
                //-The values of the data elements specified by, and in the order they
                //appear in the CDOL1, and sent by the terminal in the first
                //GENERATE AC command.
                result.Add(database.Get(EMVTagsEnum.CDOL1_RELATED_DATA_DF8107_KRN2).Value);
                //-The values of the data elements specified by, and in the order they
                //appear in the CDOL2, and sent by the terminal in the second
                //GENERATE AC command.
                TLV cdol2 = database.Get(EMVTagsEnum.CARD_RISK_MANAGEMENT_DATA_OBJECT_LIST_2_CDOL2_8D_KRN);
                if(cdol2 != null)
                    result.Add(CommonRoutines.PackRelatedDataTag(database, cdol2));
                //-The tags, lengths, and values of the data elements returned by the ICC
                //in the response to the GENERATE AC command in the order they are
                //returned, with the exception of the Signed Dynamic Application Data.
                foreach (TLV tlv in genAcResponse.GetResponseTags())
                    if (tlv.Tag.TagLable != EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag)
                        result.Add(tlv.Serialize());
            }
            byte[] transactionHashData = result.SelectMany(a => a).ToArray();

            //11.Apply the indicated hash algorithm (derived from the Hash Algorithm
            //Indicator) to the result of the concatenation of the previous step to
            //produce the Transaction Data Hash Code.
            byte[] transactionHash = SHA1.Create().ComputeHash(transactionHashData);

            //12.Compare the calculated Transaction Data Hash Code from the previous
            //step with the Transaction Data Hash Code retrieved from the ICC
            //Dynamic Data in Step 5.If they are not the same, CDA has failed.
            if (Formatting.ByteArrayToHexString(iccDD.TransactionDataHashCode) != Formatting.ByteArrayToHexString(transactionHash))
                return null;

            return iccDD;
        }
        public static void AddSDADDataToDatabase(KernelDatabaseBase database, ICCDynamicData iccdd)
        {
            if(iccdd.ApplicationCryptogram != null)
            {
                TLV ac = database.Get(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN);
                if (ac == null)
                    ac = TLV.Create(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag);
                ac.Value = iccdd.ApplicationCryptogram;
                database.AddToList(ac);
            }

            TLV iccdn = database.Get(EMVTagsEnum.ICC_DYNAMIC_NUMBER_9F4C_KRN);
            if (iccdn == null)
                iccdn = TLV.Create(EMVTagsEnum.ICC_DYNAMIC_NUMBER_9F4C_KRN.Tag);
            iccdn.Value = iccdd.ICCDynamicNumber;
            database.AddToList(iccdn);
        }
    }
}

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
using DCEMV.ISO7816Protocol;
using System.Collections.Generic;
using System.Linq;
using DCEMV.TLVProtocol;
using System;

namespace DCEMV.EMVProtocol.Kernels
{
    public static class CommonRoutines
    {
        public static Logger Logger = new Logger(typeof(CommonRoutines));

        public static TLV InitializeDiscretionaryData(KernelDatabaseBase database)
        {
            TLV disc = database.Get(EMVTagsEnum.DISCRETIONARY_DATA_FF8106_KRN2);
            if (disc == null)
            {
                disc = TLV.Create(EMVTagsEnum.DISCRETIONARY_DATA_FF8106_KRN2.Tag);
                database.AddToList(disc);
            }
            return disc;
        }

        public static void PostDEK(KernelDatabaseBase database, KernelQ qManager)
        {
            qManager.EnqueueToOutput(new KernelDEKResponse(new DATA_TO_SEND_FF8104_KRN2(database), new DATA_NEEDED_DF8106_KRN2(database)));
        }

        public static void PostUIOnly(KernelDatabaseBase database, KernelQ qManager,
            KernelMessageidentifierEnum uiMessage,
            KernelStatusEnum uiStatus, bool updateDB)
        {
            USER_INTERFACE_REQUEST_DATA_DF8116_KRN2 uird = new USER_INTERFACE_REQUEST_DATA_DF8116_KRN2(database);
            uird.Value.KernelMessageidentifierEnum = uiMessage;
            uird.Value.KernelStatusEnum = uiStatus;
            if (updateDB)
                uird.UpdateDB();

            qManager.EnqueueToOutput(new KernelUIResponse(uird));
        }

        public static void PostUIOnly(KernelDatabaseBase database, KernelQ qManager,
            KernelMessageidentifierEnum uiMessage,
            KernelStatusEnum uiStatus, bool updateDB, byte[] holdTime)
        {
            USER_INTERFACE_REQUEST_DATA_DF8116_KRN2 uird = new USER_INTERFACE_REQUEST_DATA_DF8116_KRN2(database);
            uird.Value.KernelMessageidentifierEnum = uiMessage;
            uird.Value.KernelStatusEnum = uiStatus;
            uird.Value.HoldTime = holdTime;
            if (updateDB)
                uird.UpdateDB();

            qManager.EnqueueToOutput(new KernelUIResponse(uird));
        }



        internal static SignalsEnum PostOutcomeOnly(KernelDatabaseBase database, KernelQ qManager,
           Kernel2OutcomeStatusEnum k2OutcomeStatus,
           KernelCVMEnum cvmStatus,
           bool receipt)
        {
            return PostOutcome(database, qManager,
                KernelMessageidentifierEnum.N_A,
                KernelStatusEnum.N_A,
                null,
                k2OutcomeStatus,
                Kernel2StartEnum.N_A,
                true,
                KernelMessageidentifierEnum.N_A,
                L1Enum.NOT_SET,
                null,
                L2Enum.NOT_SET,
                L3Enum.NOT_SET,
                ValueQualifierEnum.NONE,
                null,
                null,
                receipt,
                cvmStatus);
        }

        //may be a response to a clean request in which case it will contain the torn record, if any, removed
        public static SignalsEnum PostOutcomeOnly(KernelDatabaseBase database, KernelQ qManager,
            Kernel2OutcomeStatusEnum k2OutcomeStatus,
            Kernel2StartEnum start)
        {
            return PostOutcome(database, qManager,
                KernelMessageidentifierEnum.N_A,
                KernelStatusEnum.N_A,
                null,
                k2OutcomeStatus,
                start,
                null,
                KernelMessageidentifierEnum.N_A,
                L1Enum.NOT_SET,
                null,
                L2Enum.NOT_SET,
                L3Enum.NOT_SET,
                ValueQualifierEnum.NONE,
                null,
                null,
                false,
                KernelCVMEnum.N_A);
        }

        public static SignalsEnum PostOutcomeWithError(KernelDatabaseBase database, KernelQ qManager,
           Kernel2OutcomeStatusEnum status,
           Kernel2StartEnum start,
           L1Enum l1Enum,
           L2Enum l2Enum,
           L3Enum l3Enum)
        {
            if (l1Enum == L1Enum.NOT_SET && l2Enum == L2Enum.NOT_SET && l3Enum == L3Enum.NOT_SET)
                throw new EMVProtocolException("Cannot post with all enum error not set");

            return PostOutcome(database, qManager,
                KernelMessageidentifierEnum.N_A,
                KernelStatusEnum.N_A,
                null,
                status,
                start,
                null,
                KernelMessageidentifierEnum.N_A,
                l1Enum,
                null,
                l2Enum,
                l3Enum,
                ValueQualifierEnum.NONE,
                null,
                null,
                false,
                KernelCVMEnum.N_A);
        }


        internal static SignalsEnum PostOutcome(KernelDatabaseBase database, KernelQ qManager,
            KernelMessageidentifierEnum uiMessage,
            KernelStatusEnum uiStatus,
            byte[] holdTime,
            Kernel2OutcomeStatusEnum status,
            Kernel2StartEnum start,
            bool? isUIRequestOnOutcomePresent,
            KernelMessageidentifierEnum messageOnError,
            L1Enum l1Enum,
            byte[] SW12,
            L2Enum l2Enum,
            L3Enum l3Enum)
        {
            return PostOutcome(database, qManager,
                uiMessage,
                uiStatus,
                holdTime,
                status,
                start,
                isUIRequestOnOutcomePresent,
                messageOnError,
                l1Enum,
                SW12,
                l2Enum,
                l3Enum,
                ValueQualifierEnum.NONE,
                null,
                null,
                false,
                KernelCVMEnum.N_A);
        }

        internal static SignalsEnum PostOutcome(KernelDatabaseBase database, KernelQ qManager,
            KernelMessageidentifierEnum uiMessage,
            KernelStatusEnum uiStatus,
            byte[] holdTime,
            Kernel2OutcomeStatusEnum status,
            Kernel2StartEnum start,
            bool? isUIRequestOnOutcomePresent,
            KernelMessageidentifierEnum messageOnError,
            L1Enum l1Enum,
            byte[] SW12,
            L2Enum l2Enum,
            L3Enum l3Enum,
            ValueQualifierEnum valueQualifierEnum,
            byte[] valueQualifier,
            byte[] currencyCode,
            bool receipt,
            KernelCVMEnum cvmStatus)
        {
            if (messageOnError != KernelMessageidentifierEnum.N_A || l1Enum != L1Enum.NOT_SET || l2Enum != L2Enum.NOT_SET || l3Enum != L3Enum.NOT_SET)
            {
                TLV disc = InitializeDiscretionaryData(database);

                ERROR_INDICATION_DF8115_KRN2 kei = new ERROR_INDICATION_DF8115_KRN2(database);
                kei.Value.MsgOnError = messageOnError;
                kei.Value.L1Enum = l1Enum;
                kei.Value.L2Enum = l2Enum;
                kei.Value.L3Enum = l3Enum;
                if (SW12 != null) kei.Value.SW12 = SW12;
                kei.UpdateDB();

                disc.Children.AddToList(kei);
                disc.Serialize();
            }

            if (uiMessage != KernelMessageidentifierEnum.N_A || uiStatus != KernelStatusEnum.N_A)
            {
                USER_INTERFACE_REQUEST_DATA_DF8116_KRN2 uird = new USER_INTERFACE_REQUEST_DATA_DF8116_KRN2(database);
                uird.Value.KernelMessageidentifierEnum = uiMessage;
                uird.Value.KernelStatusEnum = uiStatus;
                if (holdTime == null)
                {
                    TLV holdTimeTLV = database.GetDefault(EMVTagsEnum.MESSAGE_HOLD_TIME_DF812D_KRN2);
                    if (holdTimeTLV != null)
                        holdTime = holdTimeTLV.Value;
                    else
                        holdTime = new byte[] { 0x00, 0x00, 0x00 };
                }
                uird.Value.HoldTime = holdTime;
                if (valueQualifier != null) uird.Value.ValueQualifier = valueQualifier;
                uird.Value.ValueQualifierEnum = valueQualifierEnum;
                if (currencyCode != null) uird.Value.CurrencyCode = currencyCode;
                uird.UpdateDB();
            }

            OUTCOME_PARAMETER_SET_DF8129_KRN2 kops = new OUTCOME_PARAMETER_SET_DF8129_KRN2(database);
            kops.Value.Status = status;
            kops.Value.Start = start;
            if (isUIRequestOnOutcomePresent == null)
            {
                kops.Value.UIRequestOnOutcomePresent = isUIRequestOnOutcomePresent == false;
                kops.Value.UIRequestOnRestartPresent = isUIRequestOnOutcomePresent == false;
            }
            else
            {
                kops.Value.UIRequestOnOutcomePresent = isUIRequestOnOutcomePresent == true ? true : false;
                kops.Value.UIRequestOnRestartPresent = isUIRequestOnOutcomePresent == true ? false : true;
            }
            kops.Value.DataRecordPresent = database.Get(EMVTagsEnum.DATA_RECORD_FF8105_KRN2) != null ? true : false;
            kops.Value.DiscretionaryDataPresent = database.Get(EMVTagsEnum.DISCRETIONARY_DATA_FF8106_KRN2) != null ? true : false;
            //kops.Value.Receipt = receipt;
            kops.Value.CVM = cvmStatus;
            kops.UpdateDB();

            qManager.EnqueueToOutput(new KernelOUTResponse(
                           database.Get(EMVTagsEnum.OUTCOME_PARAMETER_SET_DF8129_KRN2),
                           database.Get(EMVTagsEnum.ERROR_INDICATION_DF8115_KRN2),
                           database.Get(EMVTagsEnum.DATA_RECORD_FF8105_KRN2),
                           database.Get(EMVTagsEnum.DISCRETIONARY_DATA_FF8106_KRN2),
                           database.Get(EMVTagsEnum.USER_INTERFACE_REQUEST_DATA_DF8116_KRN2)));

            return SignalsEnum.STOP;
        }

        internal static void UpdateErrorIndication(KernelDatabaseBase database, CardResponse cardResponse, L1Enum l1Enum, L2Enum l2Enum, L3Enum l3Enum)
        {
            ERROR_INDICATION_DF8115_KRN2 kei = new ERROR_INDICATION_DF8115_KRN2(database);
            kei.Value.L1Enum = l1Enum;
            kei.Value.L2Enum = l2Enum;
            kei.Value.L3Enum = l3Enum;
            kei.Value.SW12 = cardResponse.ApduResponse.SW12;
            kei.UpdateDB();
        }
        internal static void UpdateErrorIndication(KernelDatabaseBase database, CardResponse cardResponse, KernelMessageidentifierEnum messageId, L1Enum l1Enum, L2Enum l2Enum, L3Enum l3Enum)
        {
            ERROR_INDICATION_DF8115_KRN2 kei = new ERROR_INDICATION_DF8115_KRN2(database);
            kei.Value.L1Enum = l1Enum;
            kei.Value.L2Enum = l2Enum;
            kei.Value.L3Enum = l3Enum;
            kei.Value.MsgOnError = messageId;
            kei.Value.SW12 = cardResponse.ApduResponse.SW12;
            kei.UpdateDB();
        }
        internal static void UpdateUserInterfaceRequestData(KernelDatabaseBase database, byte[] languagePref)
        {
            USER_INTERFACE_REQUEST_DATA_DF8116_KRN2 kuir = new USER_INTERFACE_REQUEST_DATA_DF8116_KRN2(database);
            kuir.Value.LanguagePreference = languagePref;
            kuir.UpdateDB();
        }
        internal static void UpdateUserInterfaceRequestData(KernelDatabaseBase database, KernelMessageidentifierEnum messageId, KernelStatusEnum status)
        {
            USER_INTERFACE_REQUEST_DATA_DF8116_KRN2 uird = new USER_INTERFACE_REQUEST_DATA_DF8116_KRN2(database);
            uird.Value.KernelMessageidentifierEnum = messageId;
            uird.Value.KernelStatusEnum = status;
            uird.Value.HoldTime = database.GetDefault(EMVTagsEnum.MESSAGE_HOLD_TIME_DF812D_KRN2).Value;
            uird.UpdateDB();
        }
        internal static void UpdateOutcomeParameterSet(KernelDatabaseBase database, KernelCVMEnum cvmEnum)
        {
            OUTCOME_PARAMETER_SET_DF8129_KRN2 kops = new OUTCOME_PARAMETER_SET_DF8129_KRN2(database);
            kops.Value.CVM = cvmEnum;
            kops.UpdateDB();
        }
        internal static void UpdateOutcomeParameterSet(KernelDatabaseBase database, bool receipt)
        {
            OUTCOME_PARAMETER_SET_DF8129_KRN2 kops = new OUTCOME_PARAMETER_SET_DF8129_KRN2(database);
            kops.Value.Receipt = receipt;
            kops.UpdateDB();
        }
        
        internal static void UpdateOutcomeParameterSet(KernelDatabaseBase database, byte fieldOffRequest)
        {
            OUTCOME_PARAMETER_SET_DF8129_KRN2 kops = new OUTCOME_PARAMETER_SET_DF8129_KRN2(database);
            kops.Value.FieldOffRequest = fieldOffRequest;
            kops.UpdateDB();
        }

        public static void PackDSDOLRelatedDataTag(KernelDatabaseBase database)
        {
            TLV tlvRelatedData = database.Get(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2);
            if (tlvRelatedData == null)
            {
                database.AddToList(TLV.Create(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2.Tag));
                tlvRelatedData = database.Get(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2);
            }
            List<byte[]> tlvRelatedDataBytes = new List<byte[]>();
            TLVList tags = TLV.DeserializeChildrenWithNoV(database.Get(EMVTagsEnum.DSDOL_9F5B_KRN2).Value, 0);
            
            int count = 1;
            foreach (TLV tlv in tags)
            {
                TLV valForPack = database.Get(tlv.Tag.TagLable);
                if (valForPack == null)
                    valForPack = TLV.Create(tlv.Tag.TagLable);

                if (count == tags.Count && tlv.Val.GetLength() > valForPack.Value.Length)
                {
                    tlvRelatedDataBytes.Add(valForPack.Value);
                }
                else
                {
                    valForPack.Val.PackValue(tlv.Val.GetLength());
                    tlvRelatedDataBytes.Add(valForPack.Value);
                    count++;
                }
                Logger.Log("Packing DSDOL, Adding tag: " + valForPack.ToString());
            }
           
            tlvRelatedData.Value = tlvRelatedDataBytes.SelectMany(a => a).ToArray();
        }
        public static void PackRelatedDataTag(KernelDatabaseBase database, EMVTagMeta tagToPack, TLV tagListForPack)
        {
            TLV tlvRelatedData = database.Get(tagToPack);
            if (tlvRelatedData == null)
            {
                database.AddToList(TLV.Create(tagToPack.Tag));
                tlvRelatedData = database.Get(tagToPack);
            }
            List<byte[]> tlvRelatedDataBytes = new List<byte[]>();
            TLVList tags = TLV.DeserializeChildrenWithNoV(tagListForPack.Value, 0);
            Logger.Log("Packing tag: " + tagListForPack.ToString());
            foreach (TLV tlv in tags)
            {
                TLV valForPackDb = database.Get(tlv.Tag.TagLable);
                TLV valForPack;
                if (valForPackDb == null)
                    valForPack = TLV.Create(tlv.Tag.TagLable);
                else
                {
                    byte[] copyVal = new byte[valForPackDb.Value.Length];
                    Array.Copy(valForPackDb.Value, copyVal, valForPackDb.Value.Length);
                    valForPack = TLV.Create(tlv.Tag.TagLable, copyVal);
                }
                valForPack.Val.PackValue(tlv.Val.GetLength());
                Logger.Log("Adding tag: " + valForPack.ToString());
                tlvRelatedDataBytes.Add(valForPack.Value);
            }
            tlvRelatedData.Value = tlvRelatedDataBytes.SelectMany(a => a).ToArray();
        }
        public static byte[] PackRelatedDataTag(KernelDatabaseBase database, TLV tagListForPack)
        {
            List<byte[]> tlvRelatedDataBytes = new List<byte[]>();
            TLVList tags = TLV.DeserializeChildrenWithNoV(tagListForPack.Value, 0);
            Logger.Log("Packing tag: " + tagListForPack.ToString());
            foreach (TLV tlv in tags)
            {
                TLV valForPackDb = database.Get(tlv.Tag.TagLable);
                TLV valForPack;
                if (valForPackDb == null)
                    valForPack = TLV.Create(tlv.Tag.TagLable);
                else
                {
                    byte[] copyVal = new byte[valForPackDb.Value.Length];
                    Array.Copy(valForPackDb.Value, copyVal, valForPackDb.Value.Length);
                    valForPack = TLV.Create(tlv.Tag.TagLable, copyVal);
                }
                valForPack.Val.PackValue(tlv.Val.GetLength());
                Logger.Log("Adding tag: " + valForPack.ToString());
                tlvRelatedDataBytes.Add(valForPack.Value);
            }
            return tlvRelatedDataBytes.SelectMany(a => a).ToArray();
        }
        public static void PackRelatedDataTag(KernelDatabaseBase database, EMVTagMeta tagToPack, TLVList tagListForPack)
        {
            TLV tlvRelatedData = database.Get(tagToPack);
            List<byte[]> tlvRelatedDataBytes = new List<byte[]>();
            Logger.Log("Packing tag: " + tagListForPack.ToString());
            foreach (TLV tlv in tagListForPack)
            {
                TLV valForPackDb = database.Get(tlv.Tag.TagLable);
                TLV valForPack;
                if (valForPackDb == null)
                    valForPack = TLV.Create(tlv.Tag.TagLable);
                else
                {
                    byte[] copyVal = new byte[valForPackDb.Value.Length];
                    Array.Copy(valForPackDb.Value, copyVal, valForPackDb.Value.Length);
                    valForPack = TLV.Create(tlv.Tag.TagLable, copyVal);
                }
                valForPack.Val.PackValue(tlv.Val.GetLength());
                Logger.Log("Adding tag: " + valForPack.ToString());
                tlvRelatedDataBytes.Add(valForPack.Value);
            }
            tlvRelatedData.Value = tlvRelatedDataBytes.SelectMany(a => a).ToArray();
        }
        
        public static byte[] PackUdolRelatedDataTag(KernelDatabaseBase database)
        {
            TLV udol = database.Get(EMVTagsEnum.UDOL_9F69_KRN2);
            byte[] udolRelatedData;
            if (udol == null)
                udol = database.Get(EMVTagsEnum.DEFAULT_UDOL_DF811A_KRN2);

            udolRelatedData = PackRelatedDataTag(database, udol);
            return udolRelatedData;
        }
        
        public static void CreateEMVDataRecord(KernelDatabaseBase database)
        {
            //Table 4.7 
            if (database.Get(EMVTagsEnum.DATA_RECORD_FF8105_KRN2) == null)
                database.AddToList(TLV.Create(EMVTagsEnum.DATA_RECORD_FF8105_KRN2.Tag));

            CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN cvr = new CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN(database);
            if (cvr.Value.GetCVMPerformed() == CVMCode.EncipheredPINVerifiedOnline && cvr.Value.GetCVMResult() != CVMResult.Failed)
                AddDREntry(EMVTagsEnum.TRANSACTION_PERSONAL_IDENTIFICATION_NUMBER_PIN_DATA_99_KRN, database);

            AddDREntry(EMVTagsEnum.POINTOFSERVICE_POS_ENTRY_MODE_9F39_KRN, database);
            AddDREntry(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN, database);
            AddDREntry(EMVTagsEnum.AMOUNT_OTHER_NUMERIC_9F03_KRN, database);
            AddDREntry(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN, database);
            AddDREntry(EMVTagsEnum.APPLICATION_EXPIRATION_DATE_5F24_KRN, database);
            AddDREntry(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN, database);
            AddDREntry(EMVTagsEnum.APPLICATION_LABEL_50_KRN, database);
            AddDREntry(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN, database);
            AddDREntry(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN, database);
            AddDREntry(EMVTagsEnum.APPLICATION_PREFERRED_NAME_9F12_KRN, database);
            AddDREntry(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN, database);
            AddDREntry(EMVTagsEnum.APPLICATION_USAGE_CONTROL_9F07_KRN, database);
            AddDREntry(EMVTagsEnum.APPLICATION_VERSION_NUMBER_TERMINAL_9F09_KRN, database);
            AddDREntry(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN, database);
            AddDREntry(EMVTagsEnum.CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN, database);
            AddDREntry(EMVTagsEnum.DEDICATED_FILE_DF_NAME_84_KRN, database);
            AddDREntry(EMVTagsEnum.INTERFACE_DEVICE_IFD_SERIAL_NUMBER_9F1E_KRN, database);
            AddDREntry(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN, database);
            AddDREntry(EMVTagsEnum.ISSUER_CODE_TABLE_INDEX_9F11_KRN, database);
            AddDREntry(EMVTagsEnum.TERMINAL_CAPABILITIES_9F33_KRN, database);
            AddDREntry(EMVTagsEnum.TERMINAL_COUNTRY_CODE_9F1A_KRN, database);
            AddDREntry(EMVTagsEnum.TERMINAL_TYPE_9F35_KRN, database);
            AddDREntry(EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN, database);
            AddDREntry(EMVTagsEnum.TRACK_2_EQUIVALENT_DATA_57_KRN, database);
            AddDREntry(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN, database);
            AddDREntry(EMVTagsEnum.TRANSACTION_DATE_9A_KRN, database);
            AddDREntry(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN, database);
            AddDREntry(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN, database);
            AddDREntry(EMVTagsEnum.TRANSACTION_STATUS_INFORMATION_9B_KRN, database);

            #region c-3 kernel 3 req 3.2.1.3
            AddDREntry(EMVTagsEnum.PAYMENT_ACCOUNT_REFERENCE_9F24_KRN, database);
            #endregion

            //Kernel 2
            AddDREntry(EMVTagsEnum.TRANSACTION_CATEGORY_CODE_9F53_KRN2, database);

            //Kernel 3
            #region c-3 kernel 3 req 3.2.1.2
            AddDREntry(EMVTagsEnum.FORM_FACTOR_INDICATOR_FFI_9F6E_KRN3, database);
            AddDREntry(EMVTagsEnum.CUSTOMER_EXCLUSIVE_DATA_CED_9F7C_KRN3, database);
            #endregion

            #region c-3 kernel 3 req 4.1.1.1
            TLV ffi = database.Get(EMVTagsEnum.FORM_FACTOR_INDICATOR_FFI_9F6E_KRN3);
            if (ffi != null)
                ffi.Value[3] = (byte)(ffi.Value[3] & 0xF0);//indicating that the transaction was conducted using [ISO 14443]
        #endregion
    }
        public static void CreateEMVDiscretionaryData(KernelDatabaseBase database)
        {
            //Table 4.9
            if (database.Get(EMVTagsEnum.DISCRETIONARY_DATA_FF8106_KRN2) == null)
                database.AddToList(TLV.Create(EMVTagsEnum.DISCRETIONARY_DATA_FF8106_KRN2.Tag));

            AddDDEntry(EMVTagsEnum.APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2, database);
            AddDDEntry(EMVTagsEnum.APPLICATION_CURRENCY_CODE_9F42_KRN, database);
            AddDDEntry(EMVTagsEnum.BALANCE_READ_AFTER_GEN_AC_DF8105_KRN2, database);
            AddDDEntry(EMVTagsEnum.BALANCE_READ_BEFORE_GEN_AC_DF8104_KRN2, database);
            AddDDEntry(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2, database);
            AddDDEntry(EMVTagsEnum.DS_SUMMARY_STATUS_DF810B_KRN2, database);
            AddDDEntry(EMVTagsEnum.ERROR_INDICATION_DF8115_KRN2, database);
            AddDDEntry(EMVTagsEnum.POSTGEN_AC_PUT_DATA_STATUS_DF810E_KRN2, database);
            AddDDEntry(EMVTagsEnum.PREGEN_AC_PUT_DATA_STATUS_DF810F_KRN2, database);
            AddDDEntry(EMVTagsEnum.THIRD_PARTY_DATA_9F6E_KRN2, database);
            AddDDEntry(EMVTagsEnum.TORN_RECORD_FF8101_KRN2, database);
        }
        public static void CreateMSDataRecord(KernelDatabaseBase database)
        {
            //Table 4.10
            if (database.Get(EMVTagsEnum.DATA_RECORD_FF8105_KRN2) == null)
                database.AddToList(TLV.Create(EMVTagsEnum.DATA_RECORD_FF8105_KRN2.Tag));

            AddDREntry(EMVTagsEnum.POINTOFSERVICE_POS_ENTRY_MODE_9F39_KRN, database);
            AddDREntry(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN, database);

            AddDREntry(EMVTagsEnum.APPLICATION_LABEL_50_KRN, database);
            AddDREntry(EMVTagsEnum.APPLICATION_PREFERRED_NAME_9F12_KRN, database);
            AddDREntry(EMVTagsEnum.DEDICATED_FILE_DF_NAME_84_KRN, database);
            AddDREntry(EMVTagsEnum.ISSUER_CODE_TABLE_INDEX_9F11_KRN, database);
            AddDREntry(EMVTagsEnum.MAGSTRIPE_APPLICATION_VERSION_NUMBER_READER_9F6D_KRN2, database);
            AddDREntry(EMVTagsEnum.TRACK_1_DATA_56_KRN2, database);
            AddDREntry(EMVTagsEnum.TRACK_2_DATA_9F6B_KRN2, database);
        }
        public static void CreateMSDiscretionaryDataRecord(KernelDatabaseBase database)
        {
            //Table 4.8
            if (database.Get(EMVTagsEnum.DISCRETIONARY_DATA_FF8106_KRN2) == null)
                database.AddToList(TLV.Create(EMVTagsEnum.DISCRETIONARY_DATA_FF8106_KRN2.Tag));

            AddDDEntry(EMVTagsEnum.APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2, database);
            AddDDEntry(EMVTagsEnum.DD_CARD_TRACK1_DF812A_KRN2, database);
            AddDDEntry(EMVTagsEnum.DD_CARD_TRACK2_DF812B_KRN2, database);
            AddDDEntry(EMVTagsEnum.ERROR_INDICATION_DF8115_KRN2, database);
            AddDDEntry(EMVTagsEnum.THIRD_PARTY_DATA_9F6E_KRN2, database);
        }

        private static void AddDREntry(EMVTagMeta tag, KernelDatabaseBase database)
        {
            if (database.IsPresent(tag.Tag))
                database.Get(EMVTagsEnum.DATA_RECORD_FF8105_KRN2).Children.AddToList(TLV.Create(tag.Tag, database.Get(tag).Value));
        }
        private static void AddDDEntry(EMVTagMeta tag, KernelDatabaseBase database)
        {
            if (database.IsPresent(tag.Tag))
                database.Get(EMVTagsEnum.DISCRETIONARY_DATA_FF8106_KRN2).Children.AddToList(TLV.Create(tag.Tag, database.Get(tag).Value));
        }
    }
}

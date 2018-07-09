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
using System;
using System.Diagnostics;
using DCEMV.TLVProtocol;


namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_4_5_6_CommonProcessing
    {
        public static SignalsEnum DoCommonProcessing(string source,Kernel2Database database, KernelQ qManager, CardQ cardQManager, Stopwatch sw, TornTransactionLogManager tornTransactionLogManager)
        {

            #region 456.1
            if(database.NextCommandEnum == NextCommandEnum.READ_RECORD)
            #endregion
            {
                #region 456.2
                TLVList toRemove = new TLVList();
                foreach (TLV tlv in database.TagsToReadYet)
                {
                    if (database.IsNotEmpty(tlv.Tag.TagLable))
                    {
                        database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(tlv);
                        toRemove.AddToList(tlv);
                    }
                }
                foreach (TLV tlv in toRemove)
                    database.TagsToReadYet.RemoveFromList(tlv);
                #endregion

                #region 456.3
                if (database.IsNotEmptyList(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag) && database.TagsToReadYet.Count == 0)
                #endregion
                {
                    #region 456.4
                    CommonRoutines.PostDEK(database, qManager);
                    database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                    database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
                    #endregion
                }
                return SignalsEnum.WAITING_FOR_EMV_READ_RECORD_RESPONSE;
            }
            else
            {
                if(database.NextCommandEnum == NextCommandEnum.GET_DATA)
                {
                    return SignalsEnum.WAITING_FOR_GET_DATA_RESPONSE;
                }
                else
                {
                    #region 456.5
                    if (database.IsEmpty(EMVTagsEnum.PROCEED_TO_FIRST_WRITE_FLAG_DF8110_KRN2.Tag))
                    #endregion
                    {

                        DATA_NEEDED_DF8106_KRN2 dataNeeded = new DATA_NEEDED_DF8106_KRN2(database);
                        #region 456.6
                        dataNeeded.Value.Tags.Add(EMVTagsEnum.PROCEED_TO_FIRST_WRITE_FLAG_DF8110_KRN2.Tag);
                        #endregion
                        dataNeeded.UpdateDB();

                        return Do456_7_To_456_10(source, database, qManager, cardQManager, sw);
                    }
                    else
                    {
                        #region 456.11
                        if(database.IsPresent(EMVTagsEnum.PROCEED_TO_FIRST_WRITE_FLAG_DF8110_KRN2.Tag) && database.Get(EMVTagsEnum.PROCEED_TO_FIRST_WRITE_FLAG_DF8110_KRN2).Value[0] == 0x00)
                        #endregion
                        {
                            #region 456.7
                            return Do456_7_To_456_10(source, database, qManager, cardQManager, sw);
                            #endregion
                        }
                    }
                }
            }

            #region 456.12
            if(database.IsEmpty(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag))
            #endregion
            {
                #region 456.13
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.AMOUNT_NOT_PRESENT);
                #endregion
            }

            #region 456.14
            long aa = Formatting.BcdToLong(database.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN).Value);
            long rctl = database.ReaderContactlessTransactionLismit;
            if (aa > rctl)
            #endregion
            {
                #region 456.15
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.SELECT_NEXT, Kernel2StartEnum.C, L1Enum.NOT_SET, L2Enum.MAX_LIMIT_EXCEEDED, L3Enum.NOT_SET);
                #endregion
            }
            
            #region 456.16
            if(!(database.IsNotEmpty(EMVTagsEnum.APPLICATION_EXPIRATION_DATE_5F24_KRN.Tag) &&
                database.IsNotEmpty(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag) &&
                database.IsNotEmpty(EMVTagsEnum.CARD_RISK_MANAGEMENT_DATA_OBJECT_LIST_1_CDOL1_8C_KRN.Tag)))
            #endregion
            {
                #region 456.17
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcome(database, qManager,
                    KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                    KernelStatusEnum.NOT_READY,
                    null,
                    Kernel2OutcomeStatusEnum.END_APPLICATION,
                    Kernel2StartEnum.N_A,
                    true,
                    KernelMessageidentifierEnum.N_A,
                    L1Enum.NOT_SET,
                    null,
                    L2Enum.CARD_DATA_MISSING,
                    L3Enum.NOT_SET);
                #endregion
            }

            #region 456.18
            IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);
            if (ids.Value.IsRead)
            #endregion
            {
                #region 456.19
                string dsid = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.DS_ID_9F5E_KRN2).Value);
                string pan = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN).Value);
                string seqNumber = "00";
                if (database.IsNotEmpty(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN.Tag))
                    seqNumber = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN).Value);
                string concat = pan + seqNumber;
                if(concat.Length % 2 != 0)
                {
                    concat = "0" + concat;
                }
                if (concat.Length < 16)
                {
                    concat.PadLeft(16, '0');
                }
                if(dsid != concat)
                #endregion
                {
                    #region 456.20.1,456.20.2
                    CommonRoutines.CreateEMVDiscretionaryData(database);
                    return CommonRoutines.PostOutcome(database, qManager,
                        KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                        KernelStatusEnum.NOT_READY,
                        null,
                        Kernel2OutcomeStatusEnum.END_APPLICATION,
                        Kernel2StartEnum.N_A,
                        true,
                        KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                        L1Enum.NOT_SET,
                        null,
                        L2Enum.CARD_DATA_ERROR,
                        L3Enum.NOT_SET);
                    #endregion
                }
            }

            #region 456.21
            TLVList toRemove2 = new TLVList();
            foreach (TLV tlv in database.TagsToReadYet)
            {
                if (database.IsPresent(tlv.Tag.TagLable))
                {
                    database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(tlv);
                }
                else
                {
                    if (database.IsKnown(tlv.Tag.TagLable))
                    {
                        database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(TLV.Create(tlv.Tag.TagLable));
                    }
                }
                toRemove2.AddToList(tlv);
            }
            
            foreach (TLV tlv in toRemove2)
                database.TagsToReadYet.RemoveFromList(tlv);
            #endregion

            #region 456.22
            if(database.IsNotEmptyList(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag))
            #endregion
            {
                #region 456.23
                CommonRoutines.PostDEK(database, qManager);
                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
                #endregion
            }

            #region 456.24
            if(database.ODAStatus == 0x80)
            #endregion
            {
                TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                #region 456.25
                if (!(
                    database.IsNotEmpty(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN.Tag) &&
                    database.IsNotEmpty(EMVTagsEnum.ISSUER_PUBLIC_KEY_CERTIFICATE_90_KRN.Tag) &&
                    database.IsNotEmpty(EMVTagsEnum.ISSUER_PUBLIC_KEY_EXPONENT_9F32_KRN.Tag) &&
                    database.IsNotEmpty(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PUBLIC_KEY_CERTIFICATE_9F46_KRN.Tag) &&
                    database.IsNotEmpty(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PUBLIC_KEY_EXPONENT_9F47_KRN.Tag) &&
                    database.IsNotEmpty(EMVTagsEnum.STATIC_DATA_AUTHENTICATION_TAG_LIST_9F4A_KRN.Tag)
                    ))
                {
                    tvr.Value.ICCDataMissing = true;
                    tvr.Value.CDAFailed = true;
                }

                if(database.PublicKeyCertificateManager.GetCAPK(RIDEnum.A000000004, database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN).Value[0]) == null)
                {
                    tvr.Value.CDAFailed = true;
                }

                
                #endregion

                #region 456.26
                bool test = false;
                TLV aip = null;
                if (database.IsNotEmpty(EMVTagsEnum.STATIC_DATA_AUTHENTICATION_TAG_LIST_9F4A_KRN.Tag))
                {
                    TLV sdal = database.Get(EMVTagsEnum.STATIC_DATA_AUTHENTICATION_TAG_LIST_9F4A_KRN);
                    TLVList list = TLV.DeserializeChildrenWithNoLV(sdal.Value,0);
                    if(list.Count == 1)
                    {
                        aip = list.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag);
                        if(aip != null)
                        {
                            test = true;
                        }
                    }
                }

                if(test == false)
                {
                    #region 456.27.1 - 456.27.2
                    CommonRoutines.CreateEMVDiscretionaryData(database);
                    return CommonRoutines.PostOutcome(database, qManager,
                        KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                        KernelStatusEnum.NOT_READY,
                        null,
                        Kernel2OutcomeStatusEnum.END_APPLICATION,
                        Kernel2StartEnum.N_A,
                        true,
                        KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                        L1Enum.NOT_SET,
                        null,
                        L2Enum.CARD_DATA_ERROR,
                        L3Enum.NOT_SET);
                    #endregion
                }
                #endregion

                #region 456.28
                int length = database.StaticDataToBeAuthenticated.Serialize().Length;
                if(2048 - length >= aip.Value.Length)
                {
                    database.StaticDataToBeAuthenticated.AddToList(database.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN));
                }
                else
                {
                    tvr.Value.CDAFailed = true;
                }
                #endregion
                tvr.UpdateDB();
            }

            #region 456.30
            long cvmrl = Formatting.BcdToLong(database.Get(EMVTagsEnum.READER_CVM_REQUIRED_LIMIT_DF8126_KRN2).Value);
            if(aa > cvmrl)
            #endregion
            {
                #region 456.31
                CommonRoutines.UpdateOutcomeParameterSet(database, true);
                #endregion

                #region 456.32
                database.Get(EMVTagsEnum.TERMINAL_CAPABILITIES_9F33_KRN).Value[1] = database.Get(EMVTagsEnum.CVM_CAPABILITY_CVM_REQUIRED_DF8118_KRN2).Value[0];
                #endregion
            }
            else
            {
                #region 456.33
                database.Get(EMVTagsEnum.TERMINAL_CAPABILITIES_9F33_KRN).Value[1] = database.Get(EMVTagsEnum.CVM_CAPABILITY_NO_CVM_REQUIRED_DF8119_KRN2).Value[0];
                #endregion
            }

            #region 456.34
            SignalsEnum se = PreGenACBalanceReading_7_1.PreGenACBalanceReading(database, qManager, cardQManager);
            if (se != SignalsEnum.NONE)
                return se;
            #endregion
            
            #region 456.35
            ProcessingRestrictions_7_7.ProcessingRestrictions(database);
            #endregion

            #region 456.35
            CVMSelection_7_5.CVMSelection(database,
                new Func<bool>(() =>
                {
                    return new KERNEL_CONFIGURATION_DF811B_KRN2(database).Value.OnDeviceCardholderVerificationSupported;
                }));
            #endregion

            #region 456.36
            long rcfl = Formatting.BcdToLong(database.Get(EMVTagsEnum.READER_CONTACTLESS_FLOOR_LIMIT_DF8123_KRN2).Value);
            if (aa > rcfl)
            #endregion
            {
                #region 456.38
                TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                tvr.Value.TransactionExceedsFloorLimit = true;
                tvr.UpdateDB();
                #endregion
            }

            #region 456.39
            database.ACType.Value.DSACTypeEnum = TerminalActionAnalysis_7_8.TerminalActionAnalysis(database);
            #endregion

            //#region support for Refunds pg 177
            //byte transactionType = database.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN).Value[0];
            //if (transactionType == (byte)TransactionTypeEnum.Refund)
            //{
            //    database.ACType.Value.DSACTypeEnum = ACTypeEnum.AAC;
            //    database.ODAStatus = 0x00; //dont request CDA in first gen ac
            //}
            //#endregion


            #region 456.42
            if (database.IsNotEmptyList(EMVTagsEnum.TAGS_TO_WRITE_BEFORE_GEN_AC_FF8102_KRN2.Tag))
            #endregion
            {
                #region 456.50
                TLV tlvRemoved = database.TagsToWriteBeforeGenACYet.GetFirstAndRemoveFromList();
                EMVPutDataRequest request = new EMVPutDataRequest(tlvRemoved);
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                return SignalsEnum.WAITING_FOR_PUT_DATA_RESPONSE_BEFORE_GEN_AC;
                #endregion
            }
            else
            {
                TORN_RECORD_FF8101_KRN2 foundTTL = null;
                #region 456.43
                uint mnttl = Formatting.ConvertToInt32(database.GetDefault(EMVTagsEnum.MAX_NUMBER_OF_TORN_TRANSACTION_LOG_RECORDS_DF811D_KRN2).Value);
                if (database.IsNotEmpty(EMVTagsEnum.DRDOL_9F51_KRN2.Tag) &&  mnttl != 0)
                #endregion
                {
                    #region 456.44
                    foreach (TORN_RECORD_FF8101_KRN2 tlv in tornTransactionLogManager.TornTransactionLogs)
                    {
                        if (database.IsNotEmpty(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN.Tag))
                        {
                            if((Formatting.ByteArrayToHexString(tlv.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag).Value) == Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN).Value)) &&
                                (Formatting.ByteArrayToHexString(tlv.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN.Tag).Value) == Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN).Value)))
                            {
                                foundTTL = tlv;
                            }
                        }
                        else
                        {
                            if ((Formatting.ByteArrayToHexString(tlv.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag).Value) == Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN).Value)) &&
                                (tlv.Children.IsNotPresent(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN.Tag)))
                            {
                                foundTTL = tlv;
                            }
                        }
                    }
                    #endregion
                    if(foundTTL == null)
                    {
                        #region 456.45
                        EMVGenerateACRequest request = PrepareGenACCommandProcedure_7_6.PrepareGenACCommand(database, qManager, cardQManager);
                        cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                        return SignalsEnum.WAITING_FOR_GEN_AC_1;
                        #endregion
                    }
                    else
                    {
                        #region 456.47
                        database.TornTempRecord = foundTTL;
                        #endregion

                        #region 456.48
                        database.Get(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2).Value = database.TornTempRecord.Children.Get(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2.Tag).Value;
                        EMVRecoverACRequest requestRecover = new EMVRecoverACRequest(database.Get(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2));
                        cardQManager.EnqueueToInput(new CardRequest(requestRecover, CardinterfaceServiceRequestEnum.ADPU));
                        return SignalsEnum.WAITING_FOR_RECOVER_AC;
                        #endregion
                    }
                }
                else
                {
                    #region 456.45
                    EMVGenerateACRequest request = PrepareGenACCommandProcedure_7_6.PrepareGenACCommand(database, qManager, cardQManager);
                    cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                    return SignalsEnum.WAITING_FOR_GEN_AC_1;
                    #endregion
                }
            }
        }

        private static SignalsEnum Do456_7_To_456_10(string source, Kernel2Database database, KernelQ qManager, CardQ cardQManager, Stopwatch sw)
        {
            #region 456.7
            TLVList toRemove = new TLVList();
            foreach (TLV tlv in database.TagsToReadYet)
            {
                if (database.IsNotEmpty(tlv.Tag.TagLable))
                {
                    database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(tlv);
                    toRemove.AddToList(tlv);
                }
            }
            foreach (TLV tlv in toRemove)
                database.TagsToReadYet.RemoveFromList(tlv);
            #endregion

            #region 456.8
            if (database.IsNotEmptyList(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2.Tag) || (database.IsNotEmptyList(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag) && database.TagsToReadYet.Count == 0))
            #endregion
            {
                #region 456.9
                CommonRoutines.PostDEK(database, qManager);
                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
                #endregion
            }

            #region 456.10
            //database.Get(EMVTagsEnum.TIME_OUT_VALUE_DF8127_KRN2);
            sw.Start();
            #endregion

            return SignalsEnum.WAITING_FOR_EMV_MODE_FIRST_WRITE_FLAG;
        }
    }
}

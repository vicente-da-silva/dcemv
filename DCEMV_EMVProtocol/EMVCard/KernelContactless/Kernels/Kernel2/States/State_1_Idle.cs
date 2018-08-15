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
using System;
using System.Diagnostics;
using DCEMV.TLVProtocol;
using DCEMV.ISO7816Protocol;


namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_1_Idle
    {
        public static SignalsEnum Execute(
            Kernel2Database database, 
            KernelQ qManager, 
            CardQ cardQManager, 
            TornTransactionLogManager tornTransactionLogManager, 
            Stopwatch sw)
        {
            if (qManager.GetInputQCount() == 0)
                throw new EMVProtocolException("Execute_1_Idle: Kernel Input Q empty");

            KernelRequest kernel2Request = qManager.DequeueFromInput(false);

            switch (kernel2Request.KernelTerminalReaderServiceRequestEnum)
            {
                case KernelTerminalReaderServiceRequestEnum.STOP:
                    return EntryPointSTOP(database, qManager);

                case KernelTerminalReaderServiceRequestEnum.CLEAN:
                    return EntryPointCLEAN(database, kernel2Request, qManager, tornTransactionLogManager);

                case KernelTerminalReaderServiceRequestEnum.ACT:
                    return EntryPointACT(database, kernel2Request, qManager, cardQManager, sw);

                default:
                    throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in Execute_1_Idle:" + Enum.GetName(typeof(KernelTerminalReaderServiceRequestEnum), kernel2Request.KernelTerminalReaderServiceRequestEnum));
            }
        }

        /*
         * S1.1, S1.7 - S1.23
         */
        private static SignalsEnum EntryPointACT(Kernel2Database database, KernelRequest kernel1Request, KernelQ qManager, CardQ cardQManager, Stopwatch sw)
        {
            #region S1.7
            foreach (TLV tlv in kernel1Request.InputData)
            {
                if (tlv.Tag.TagLable == EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)
                {
                    if (!database.ParseAndStoreCardResponse(tlv))
                    {
                        return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.SELECT_NEXT, Kernel2StartEnum.C, L1Enum.NOT_SET, L2Enum.PARSING_ERROR, L3Enum.NOT_SET);
                    }
                }
                else
                {
                    if ((database.IsKnown(tlv.Tag.TagLable) || database.IsPresent(tlv.Tag.TagLable)) && EMVTagsEnum.DoesTagIncludesPermission(tlv.Tag.TagLable, UpdatePermissionEnum.ACT))
                        database.AddToList(tlv);
                }
            }

            if (database.IsNotEmpty(EMVTagsEnum.LANGUAGE_PREFERENCE_5F2D_KRN.Tag))
            {
                byte[] languagePrefFromCard = database.Get(EMVTagsEnum.LANGUAGE_PREFERENCE_5F2D_KRN).Value;
                byte[] languagePrefFromCardPadded = new byte[8]; //will be padded with trailing 0's
                Array.Copy(languagePrefFromCard, languagePrefFromCardPadded, languagePrefFromCard.Length);
                CommonRoutines.UpdateUserInterfaceRequestData(database, languagePrefFromCardPadded);
            }
            
            #region S1.8
            if (database.IsNotPresent(EMVTagsEnum.DEDICATED_FILE_DF_NAME_84_KRN.Tag) || database.IsEmpty(EMVTagsEnum.DEDICATED_FILE_DF_NAME_84_KRN.Tag))
            {
                return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.SELECT_NEXT, Kernel2StartEnum.C, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
            }
            #endregion

            if (database.IsNotEmpty(EMVTagsEnum.APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2.Tag))
            {
                APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2 aciVal = new APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2(database);
                if (aciVal.Value.SupportForFieldOffDetection)
                {
                    byte[] holdTimeValue = database.GetDefault(EMVTagsEnum.HOLD_TIME_VALUE_DF8130_KRN2).Value;
                    CommonRoutines.UpdateOutcomeParameterSet(database, holdTimeValue[0]);
                }
            }
            #endregion

            #region S1.9
            CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN cvmr = new CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN(database);
            cvmr.UpdateDB();

            database.ACType = new DS_AC_TYPE_DF8108_KRN2(database);
            database.ACType.Value.DSACTypeEnum = ACTypeEnum.TC;

            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
            tvr.UpdateDB();

            database.ODAStatus = 0x00;

            database.AddToList(TLV.Create(EMVTagsEnum.RRP_COUNTER_DF8307_KRN2.Tag, new byte[] { 0x00 }));

            //CARD_DATA_INPUT_CAPABILITY_DF8117_KRN2 df8117 = new CARD_DATA_INPUT_CAPABILITY_DF8117_KRN2(database);
            //SECURITY_CAPABILITY_DF811F_KRN2 df811f = new SECURITY_CAPABILITY_DF811F_KRN2(database);

            TERMINAL_CAPABILITIES_9F33_KRN _9f33 = new TERMINAL_CAPABILITIES_9F33_KRN(database);
            //_9f33.Value.SetCardDataInputCapabilityValue(df8117);
            //_9f33.Value.SetSecurityCapabilityValue(df811f);
            _9f33.UpdateDB();

            database.StaticDataToBeAuthenticated.Initialize();

            database.AddToList(TLV.Create(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN.Tag, new byte[] {0x00, 0x00, 0x00, 0x00 }));
            database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN).Value = Formatting.GetRandomNumber();

            //REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcpv = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2();
            //rcpv.Value.ACTypeEnum = ACTypeEnum.TC;
            //rcpv.UpdateDB();
            #endregion

            #region S1.10
            database.Initialize(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2.Tag);
            database.Initialize(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag);

            DATA_NEEDED_DF8106_KRN2 dataNeeded = new DATA_NEEDED_DF8106_KRN2(database);
            DATA_TO_SEND_FF8104_KRN2 dataToSend = new DATA_TO_SEND_FF8104_KRN2(database);

            database.TagsToReadYet.Initialize();

            if (database.IsNotEmptyList(EMVTagsEnum.TAGS_TO_READ_DF8112_KRN2.Tag))
                database.TagsToReadYet.AddListToList(database.Get(EMVTagsEnum.TAGS_TO_READ_DF8112_KRN2).Children);
            else
            {
                dataNeeded.Value.AddTag(EMVTagsEnum.TAGS_TO_READ_DF8112_KRN2);
                dataNeeded.UpdateDB();
            }
            #endregion

            #region S1.11
            bool MissingPDOLDataFlag = false;
            #endregion

            #region S1.12
            TLV _9f38 = database.Get(EMVTagsEnum.PROCESSING_OPTIONS_DATA_OBJECT_LIST_PDOL_9F38_KRN);
            TLVList pdolList = TLV.DeserializeChildrenWithNoV(_9f38.Value, 0);
            foreach (TLV tlv in pdolList)
            {
                if (database.IsEmpty(tlv.Tag.TagLable))
                {
                    MissingPDOLDataFlag = true;
                    dataNeeded.Value.AddTag(tlv.Tag.TagLable);
                }
            }
            dataNeeded.UpdateDB();

            #region S1.13 and S1.14
            if (!MissingPDOLDataFlag)
            {
                database.Initialize(EMVTagsEnum.PDOL_RELATED_DATA_DF8111_KRN2.Tag);
                CommonRoutines.PackRelatedDataTag(database, EMVTagsEnum.PDOL_RELATED_DATA_DF8111_KRN2, pdolList);
                EMVGetProcessingOptionsRequest request = new EMVGetProcessingOptionsRequest(database.Get(EMVTagsEnum.PDOL_RELATED_DATA_DF8111_KRN2));
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
            }
            #endregion

            #endregion

            #region S1.15
            TLVList toRemove = new TLVList();
            foreach (TLV tlv in database.TagsToReadYet)
            {
                if (database.IsNotEmpty(tlv.Tag.TagLable))
                {
                    database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(tlv);
                    toRemove.AddToList(tlv);
                }
            }

            foreach(TLV tlv in toRemove)
                database.TagsToReadYet.RemoveFromList(tlv);
            #endregion

            #region S1.16
            database.Initialize(EMVTagsEnum.IDS_STATUS_DF8128_KRN2.Tag);
            database.Initialize(EMVTagsEnum.DS_SUMMARY_STATUS_DF810B_KRN2.Tag);
            database.Initialize(EMVTagsEnum.POSTGEN_AC_PUT_DATA_STATUS_DF810E_KRN2.Tag);
            database.Initialize(EMVTagsEnum.PREGEN_AC_PUT_DATA_STATUS_DF810F_KRN2.Tag);
            database.Initialize(EMVTagsEnum.DS_DIGEST_H_DF61_KRN2.Tag);

            database.Get(EMVTagsEnum.IDS_STATUS_DF8128_KRN2).Value = new byte[] { 0x00 };
            database.Get(EMVTagsEnum.DS_SUMMARY_STATUS_DF810B_KRN2).Value = new byte[] { 0x00 };
            database.Get(EMVTagsEnum.POSTGEN_AC_PUT_DATA_STATUS_DF810E_KRN2).Value = new byte[] { 0x00 };
            database.Get(EMVTagsEnum.PREGEN_AC_PUT_DATA_STATUS_DF810F_KRN2).Value = new byte[] { 0x00 };
            database.Get(EMVTagsEnum.DS_DIGEST_H_DF61_KRN2).Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };

            database.TagsToWriteAfterGenACYet.Initialize();
            database.TagsToWriteBeforeGenACYet.Initialize();

            if (database.IsNotEmptyList(EMVTagsEnum.TAGS_TO_WRITE_BEFORE_GEN_AC_FF8102_KRN2.Tag))
                database.TagsToWriteBeforeGenACYet.AddListToList(database.Get(EMVTagsEnum.TAGS_TO_WRITE_BEFORE_GEN_AC_FF8102_KRN2).Children);
            if (database.IsNotEmptyList(EMVTagsEnum.TAGS_TO_WRITE_AFTER_GEN_AC_FF8103_KRN2.Tag))
                database.TagsToWriteAfterGenACYet.AddListToList(database.Get(EMVTagsEnum.TAGS_TO_WRITE_AFTER_GEN_AC_FF8103_KRN2).Children);

            if (database.IsEmptyList(EMVTagsEnum.TAGS_TO_WRITE_BEFORE_GEN_AC_FF8102_KRN2.Tag))
                dataNeeded.Value.AddTag(EMVTagsEnum.TAGS_TO_WRITE_BEFORE_GEN_AC_FF8102_KRN2);
            if (database.IsEmptyList(EMVTagsEnum.TAGS_TO_WRITE_AFTER_GEN_AC_FF8103_KRN2.Tag))
                dataNeeded.Value.AddTag(EMVTagsEnum.TAGS_TO_WRITE_AFTER_GEN_AC_FF8103_KRN2);

            dataNeeded.UpdateDB();
            #endregion

            #region S1.17 
            if (database.IsNotEmpty(EMVTagsEnum.DSVN_TERM_DF810D_KRN2.Tag) && database.IsPresent(EMVTagsEnum.DS_REQUESTED_OPERATOR_ID_9F5C_KRN2.Tag))
            {
                #region S1.18
                if (database.IsPresent(EMVTagsEnum.DS_ID_9F5E_KRN2.Tag))
                    database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(database.Get(EMVTagsEnum.DS_ID_9F5E_KRN2));
                else
                {
                    TLV dsid = TLV.Create(EMVTagsEnum.DS_ID_9F5E_KRN2.Tag);
                    dsid.Val.PackValue(EMVTagsEnum.DS_ID_9F5E_KRN2.DataFormatter.GetMaxLength());
                    database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(dsid);
                }

                if (database.IsPresent(EMVTagsEnum.APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2.Tag))
                    database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(database.Get(EMVTagsEnum.APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2));
                else
                {
                    TLV aci = TLV.Create(EMVTagsEnum.APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2.Tag);
                    aci.Val.PackValue(EMVTagsEnum.APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2.DataFormatter.GetMaxLength());
                    database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(aci);
                }
                #endregion

                #region S1.19
                if(database.IsNotEmpty(EMVTagsEnum.APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2.Tag))
                {
                    APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2 aci = new APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2(database);
                    if ((aci.Value.DataStorageVersionNumberEnum == DataStorageVersionNumberEnum.VERSION_1 || aci.Value.DataStorageVersionNumberEnum == DataStorageVersionNumberEnum.VERSION_2) &&
                        database.IsNotEmpty(EMVTagsEnum.DS_ID_9F5E_KRN2.Tag))
                    {
                        #region S1.20
                        IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);
                        ids.Value.IsRead = true;
                        ids.UpdateDB();
                        #endregion
                    }
                }
                #endregion
            }
            #endregion

            #region S1.21
            if (MissingPDOLDataFlag)
            {
                #region S1.22
                CommonRoutines.PostDEK(database, qManager);
                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
                #endregion

                #region S1.23
                sw.Restart();
                #endregion
                
                return SignalsEnum.WAITING_FOR_PDOL_DATA;
            }
            #endregion

            return SignalsEnum.WAITING_FOR_GPO_REPONSE;
        }

        

        /*
         * S1.2 and S1.3
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager)
        {
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
        }

        /*
         * S1.4, S1.5 and S1.6
         */
        private static SignalsEnum EntryPointCLEAN(Kernel2Database database, KernelRequest kernel1Request, KernelQ qManager, TornTransactionLogManager tornTransactionLogManager)
        {
            foreach (TLV tlv in kernel1Request.InputData)
            {
                bool updateConditionsOfTIncludeACTSignal = EMVTagsEnum.DoesTagIncludesPermission(tlv.Tag.TagLable, UpdatePermissionEnum.ACT);
                if ((database.IsKnown(tlv.Tag.TagLable) || database.IsPresent(tlv.Tag.TagLable)) && updateConditionsOfTIncludeACTSignal)
                    database.AddToList(tlv);
            }
            TLV discretionaryData = CommonRoutines.InitializeDiscretionaryData(database);

            foreach (TORN_RECORD_FF8101_KRN2 ttl in tornTransactionLogManager.TornTransactionLogs)
            {
                DateTime ttlTransactionDate = EMVTagsEnum.TRANSACTION_DATE_9A_KRN.FormatAsDateTime(ttl.Children.Get(EMVTagsEnum.TRANSACTION_DATE_9A_KRN.Tag).Value);
                DateTime ttlTransactionTime = EMVTagsEnum.TRANSACTION_TIME_9F21_KRN.FormatAsDateTime(ttl.Children.Get(EMVTagsEnum.TRANSACTION_TIME_9F21_KRN.Tag).Value);
                DateTime configTransactionDate = EMVTagsEnum.TRANSACTION_DATE_9A_KRN.FormatAsDateTime(database.Get(EMVTagsEnum.TRANSACTION_DATE_9A_KRN.Tag).Value);
                DateTime configTransactionTime = EMVTagsEnum.TRANSACTION_TIME_9F21_KRN.FormatAsDateTime(database.Get(EMVTagsEnum.TRANSACTION_TIME_9F21_KRN.Tag).Value);

                TimeSpan tsDate = ttlTransactionDate - configTransactionDate;
                TimeSpan tsTime = ttlTransactionTime - configTransactionTime;

                int totalSeconds = tsDate.Seconds + tsTime.Seconds;
                int defaultToCheck = (int)Formatting.ConvertToInt32(database.GetDefault(EMVTagsEnum.MAX_LIFETIME_OF_TORN_TRANSACTION_LOG_RECORD_DF811C_KRN2).Value);
                if (totalSeconds > defaultToCheck)
                {
                    discretionaryData.Children.AddToList(ttl);
                    tornTransactionLogManager.TornTransactionLogs.RemoveFromList(ttl);
                }
            }

            return CommonRoutines.PostOutcomeOnly(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A);
        }
    }
}

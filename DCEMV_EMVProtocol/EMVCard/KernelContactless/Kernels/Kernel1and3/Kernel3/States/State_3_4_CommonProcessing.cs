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

namespace DCEMV.EMVProtocol.Kernels.K3
{
    public static class State_3_4_CommonProcessing
    {
        public static SignalsEnum DoCommonProcessing(string source, KernelDatabaseBase databaseIn, KernelQ qManager, CardQ cardQManager, Stopwatch sw, PublicKeyCertificateManager pkcm, CardExceptionManager cardExceptionManager)
        {
            Kernel3Database database = (Kernel3Database)databaseIn;
            if (database.NextCommandEnum == NextCommandEnum.READ_RECORD)
            {
                DoDEKIfNeeded(database, qManager);
                return SignalsEnum.WAITING_FOR_EMV_READ_RECORD_RESPONSE;
            }

            DoDEKIfNeeded(database, qManager);

            //ReaderContactlessTransactionLimit exceeded
            if (database.ProcessingIndicatorsForSelected.ContactlessApplicationNotAllowed)
            {
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.SELECT_NEXT, Kernel2StartEnum.C, L1Enum.NOT_SET, L2Enum.MAX_LIMIT_EXCEEDED, L3Enum.NOT_SET);
            }

            #region 5.4.3.1
            TLV cidTLV = database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN);
            if (cidTLV == null)
            {
                cidTLV = TLV.Create(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag);
                cidTLV.Val.PackValue(cidTLV.Val.DataFormatter.GetMaxLength());
                byte iad = database.Get(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN).Value[4];
                Formatting.SetBitPosition(ref cidTLV.Value[0], Formatting.GetBitPosition(iad, 6), 8);
                Formatting.SetBitPosition(ref cidTLV.Value[0], Formatting.GetBitPosition(iad, 5), 7);
                database.AddToList(cidTLV);
            }
            #endregion

            #region 5.4.3.2
            byte cid = cidTLV.Value[0];
            cid = (byte)(cid >> 6);
            TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN ttq = new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN(database);
            if (cid == (byte)ACTypeEnum.AAC)
                database.DeclineRequiredByReaderIndicator = true;
            if (cid == (byte)ACTypeEnum.ARQC || ttq.Value.OnlineCryptogramRequired)
                database.OnlineRequiredByReaderIndicator = true;
            if (cid != (byte)ACTypeEnum.AAC && cid != (byte)ACTypeEnum.ARQC && cid != (byte)ACTypeEnum.TC)
                database.DeclineRequiredByReaderIndicator = true;
            #endregion

            #region 5.4.2.1
            if (!database.DeclineRequiredByReaderIndicator)
            {
                if (!CheckMandatoryFields(database, pkcm))
                {
                    return CommonRoutines.PostOutcome(database, qManager,
                            KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                            KernelStatusEnum.PROCESSING_ERROR,
                            null,
                            Kernel2OutcomeStatusEnum.END_APPLICATION,
                            Kernel2StartEnum.N_A,
                            true,
                            KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                            L1Enum.NOT_SET,
                            null,
                            L2Enum.CARD_DATA_ERROR,
                            L3Enum.NOT_SET);
                }
            }
            #endregion

            if (!database.DeclineRequiredByReaderIndicator && !database.OnlineRequiredByReaderIndicator)
            {
                #region 5.5.1.1 to 5.5.1.5
                SignalsEnum result = DoProcessingRestrictions(database, qManager, cardExceptionManager);
                if (result != SignalsEnum.NONE)
                    return result;
                #endregion

                #region  5.6.1.1, 5.6.1.2 and 5.6.2.1, 5.6.2.1
                if (!DoOfflineAuth(database, qManager, pkcm))
                #endregion
                {
                    CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3 ctq = new CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3(database);
                    if (ctq.Value.GoOnlineIfOfflineDataAuthenticationFailsAndReaderIsOnlineCapable && !database.ProcessingIndicatorsForSelected.TTQ.Value.OfflineOnlyReader)
                    {
                        database.OnlineRequiredByReaderIndicator = true;
                    }
                    else if (ctq.Value.SwitchInterfaceIfOfflineDataAuthenticationFailsAndReaderSupportsVIS && database.ProcessingIndicatorsForSelected.TTQ.Value.EMVContactChipSupported)
                    {
                        return CommonRoutines.PostOutcome(database, qManager,
                             KernelMessageidentifierEnum.INSERT_CARD,
                             KernelStatusEnum.PROCESSING_ERROR,
                             null,
                             Kernel2OutcomeStatusEnum.TRY_ANOTHER_INTERFACE,
                             Kernel2StartEnum.N_A,
                             true,
                             KernelMessageidentifierEnum.INSERT_CARD,
                             L1Enum.NOT_SET,
                             null,
                             L2Enum.TERMINAL_DATA_ERROR,
                             L3Enum.NOT_SET);
                    }
                    else
                        database.DeclineRequiredByReaderIndicator = true;
                }
            }


            #region 5.7.1.1, 5.7.1.2, 5.7.1.3
            KernelCVMEnum cvm = KernelCVMEnum.N_A;
            if (!database.DeclineRequiredByReaderIndicator)
            {
                cvm = DoCVMProcessing(database, (ACTypeEnum)Formatting.GetEnum(typeof(ACTypeEnum), cid));
                if (cvm == KernelCVMEnum.NO_CVM && ttq.Value.CVMRequired)
                    database.DeclineRequiredByReaderIndicator = true;
            }
            #endregion

            #region 4.3.1.1
            byte[] currencyCode = null;
            byte[] balance = null;
            ValueQualifierEnum vq = ValueQualifierEnum.NONE;
            if (database.Kernel3Configuration.DisplayAvailableSpendingAmount)
            {
                TLV balanceTLV = database.Get(EMVTagsEnum.AVAILABLE_OFFLINE_SPENDING_AMOUNT_AOSA_9F5D_KRN3);
                if (balanceTLV != null)
                {
                    vq = ValueQualifierEnum.BALANCE;
                    balance = balanceTLV.Value;
                    currencyCode = database.Get(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN).Value;
                }
            }
            #endregion

            //#region support for Refunds 
            //byte transactionType = database.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN).Value[0];
            //if (transactionType == (byte)TransactionTypeEnum.Refund)
            //    database.DeclineRequiredByReaderIndicator = true;
            //#endregion

            #region 5.8.1.1
            if (database.OnlineRequiredByReaderIndicator && !database.DeclineRequiredByReaderIndicator)
            {
                CommonRoutines.CreateEMVDataRecord(database);
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcome(database, qManager,
                                 KernelMessageidentifierEnum.AUTHORISING_PLEASE_WAIT,
                                 KernelStatusEnum.NOT_READY,
                                 null,
                                 Kernel2OutcomeStatusEnum.ONLINE_REQUEST,
                                 Kernel2StartEnum.N_A,
                                 true,
                                 KernelMessageidentifierEnum.N_A,
                                 L1Enum.NOT_SET,
                                 null,
                                 L2Enum.NOT_SET,
                                 L3Enum.NOT_SET,
                                 vq,
                                 balance,
                                 currencyCode,
                                 false,
                                 cvm);
            }
            #endregion

            #region 5.9.1.1
            if (!database.OnlineRequiredByReaderIndicator && !database.DeclineRequiredByReaderIndicator)
            {
                CommonRoutines.CreateEMVDataRecord(database);
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcome(database, qManager,
                                 cvm == KernelCVMEnum.OBTAIN_SIGNATURE ? KernelMessageidentifierEnum.APPROVED_SIGN : KernelMessageidentifierEnum.APPROVED,
                                 KernelStatusEnum.READY_TO_READ,
                                 null,
                                 Kernel2OutcomeStatusEnum.APPROVED,
                                 Kernel2StartEnum.N_A,
                                 true,
                                 KernelMessageidentifierEnum.N_A,
                                 L1Enum.NOT_SET,
                                 null,
                                 L2Enum.NOT_SET,
                                 L3Enum.NOT_SET,
                                 vq,
                                 balance,
                                 currencyCode,
                                 false,
                                 cvm);
            }
            #endregion

            CommonRoutines.CreateEMVDiscretionaryData(database);
            //#region support for Refunds 
            //if (transactionType == (byte)TransactionTypeEnum.Refund)
            //#endregion
            //{
            //    //CommonRoutines.PostUIOnly(database, qManager, Kernel1MessageidentifierEnum.CLEAR_DISPLAY, Kernel1StatusEnum.N_A, true, new byte[] { 0x00, 0x00, 0x00 });

            //    CommonRoutines.CreateEMVDataRecord(database);
            //    return CommonRoutines.PostOutcome(database, qManager,
            //        KernelMessageidentifierEnum.N_A,
            //        KernelStatusEnum.N_A,
            //        null,
            //        Kernel2OutcomeStatusEnum.END_APPLICATION,
            //        Kernel2StartEnum.N_A,
            //        true,
            //        KernelMessageidentifierEnum.N_A,
            //        L1Enum.NOT_SET,
            //        null,
            //        L2Enum.NOT_SET,
            //        L3Enum.NOT_SET,
            //        ValueQualifierEnum.NONE,
            //        null,
            //        null,
            //        false,
            //        cvm);
            //}
            //else
            //{
            #region 5.9.1.2
            return CommonRoutines.PostOutcome(database, qManager,
                                KernelMessageidentifierEnum.DECLINED,
                                KernelStatusEnum.READY_TO_READ,
                                null,
                                Kernel2OutcomeStatusEnum.DECLINED,
                                Kernel2StartEnum.N_A,
                                true,
                                KernelMessageidentifierEnum.N_A,
                                L1Enum.NOT_SET,
                                null,
                                L2Enum.NOT_SET,
                                L3Enum.NOT_SET,
                                vq,
                                balance,
                                currencyCode,
                                false,
                                cvm);
            #endregion
            //}
        }

        public static KernelCVMEnum DoCVMProcessing(Kernel3Database database, ACTypeEnum acType)
        {
            KernelCVMEnum cvm = KernelCVMEnum.N_A;
            TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN ttq = new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN(database);

            if (!database.DeclineRequiredByReaderIndicator)
            {
                #region 5.7.1.1
                if (database.IsEmpty(EMVTagsEnum.CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3.Tag) && ttq.Value.CVMRequired)
                #endregion
                {
                    if (ttq.Value.SignatureSupported)
                    {
                        cvm = KernelCVMEnum.OBTAIN_SIGNATURE;
                    }
                    else if (ttq.Value.ConsumerDeviceCVMSupported && ttq.Value.OnlinePINSupported)
                    {
                        cvm = KernelCVMEnum.ONLINE_PIN;
                    }
                    else// only ttq.Value.ConsumerDeviceCVMSupported 
                    {
                        database.DeclineRequiredByReaderIndicator = true;
                    }
                }
                if (database.IsNotEmpty(EMVTagsEnum.CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3.Tag))
                {
                    #region 5.7.1.2
                    CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3 ctq = new CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3(database);
                    if (ctq.Value.OnlinePINRequired && ttq.Value.OnlinePINSupported)
                    {
                        cvm = KernelCVMEnum.ONLINE_PIN;
                        database.OnlineRequiredByReaderIndicator = true;
                    }
                    else
                    {
                        if (ctq.Value.ConsumerDeviceCVMPerformed)
                        {
                            TLV card = database.Get(EMVTagsEnum.CARD_AUTHENTICATION_RELATED_DATA_9F69_KRN3);
                            if (card != null)
                            {
                                byte[] card67 = new byte[2];
                                byte[] ctq12 = new byte[2];
                                Array.Copy(card.Value, 6, card67, 0, 2);
                                Array.Copy(ctq.Value.Value, 0, ctq12, 0, 2);
                                if (Formatting.ByteArrayToHexString(card67) == Formatting.ByteArrayToHexString(ctq12))
                                {
                                    cvm = KernelCVMEnum.CONFIRMATION_CODE_VERIFIED;
                                }
                                else
                                {
                                    database.DeclineRequiredByReaderIndicator = true;
                                }
                            }
                            else
                            {
                                if (acType == ACTypeEnum.ARQC)
                                {
                                    cvm = KernelCVMEnum.CONFIRMATION_CODE_VERIFIED;
                                }
                                else
                                {
                                    database.DeclineRequiredByReaderIndicator = true;
                                }
                            }
                        }

                    }

                    if (!ctq.Value.OnlinePINRequired && !ctq.Value.ConsumerDeviceCVMPerformed)
                    {
                        if (ttq.Value.SignatureSupported && ctq.Value.SignatureRequired)
                        {
                            cvm = KernelCVMEnum.OBTAIN_SIGNATURE;
                        }
                    }

                    #endregion
                }
            }
            if (cvm == KernelCVMEnum.N_A && ttq.Value.CVMRequired)
            {
                cvm = KernelCVMEnum.NO_CVM;
                database.DeclineRequiredByReaderIndicator = true;
            }
            if (cvm == KernelCVMEnum.N_A)
                cvm = KernelCVMEnum.NO_CVM;

            return cvm;
        }

        public static bool DoOfflineAuth(KernelDatabaseBase database, KernelQ qManager, PublicKeyCertificateManager pkcm)
        {
            try
            {
                TLV sdadTLV = database.Get(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN);
                TLV ssadTLV = database.Get(EMVTagsEnum.SIGNED_STATIC_APPLICATION_DATA_93_KRN);
                if (sdadTLV == null && ssadTLV == null)
                    return false;

                if (database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN) == null)
                    return false;

                CAPublicKeyCertificate capk = pkcm.GetCAPK(RIDEnum.A000000003, database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN).Value[0]);
                if (capk == null)
                    return false;

                TLV aip = database.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag);
                int length = database.StaticDataToBeAuthenticated.Serialize().Length;
                if (aip != null && database.IsNotEmpty(EMVTagsEnum.STATIC_DATA_AUTHENTICATION_SDA_TAG_LIST_9F4A_KRN3.Tag))
                    if (2048 - length >= aip.Value.Length)
                        database.StaticDataToBeAuthenticated.AddToList(database.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN));
                    else
                        return false;

                if (sdadTLV != null)
                {
                    TLV card = database.Get(EMVTagsEnum.CARD_AUTHENTICATION_RELATED_DATA_9F69_KRN3);
                    if (card == null || card.Value[0] != 0x01) //check version number of fdda
                        return false;

                    byte[] sdadRaw = database.Get(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN).Value;
                    ICCDynamicData iccdd = VerifySAD.VerifySDAD_K3(ICCDynamicDataType.DYNAMIC_NUMBER_ONLY, database, capk, sdadRaw);
                    if (iccdd == null) return false;

                    VerifySAD.AddSDADDataToDatabase(database, iccdd);
                    return true;
                }

                if (ssadTLV != null)
                {
                    byte[] sdadRaw = database.Get(EMVTagsEnum.SIGNED_STATIC_APPLICATION_DATA_93_KRN).Value;
                    byte[] authCode = VerifySAD.VerifySSAD(ICCDynamicDataType.DYNAMIC_NUMBER_ONLY, database, capk, sdadRaw);
                    if (authCode == null) return false;
                    TLV dataAuthenticationCode = database.Get(EMVTagsEnum.DATA_AUTHENTICATION_CODE_9F45_KRN);
                    if (dataAuthenticationCode == null)
                        dataAuthenticationCode = TLV.Create(EMVTagsEnum.DATA_AUTHENTICATION_CODE_9F45_KRN.Tag, authCode);
                    database.AddToList(dataAuthenticationCode);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static SignalsEnum DoProcessingRestrictions(Kernel3Database database, KernelQ qManager, CardExceptionManager cardExceptionManager)
        {
            #region 5.5.1.1
            DateTime transactionDate = EMVTagsEnum.TRANSACTION_DATE_9A_KRN.FormatAsDateTime(database.Get(EMVTagsEnum.TRANSACTION_DATE_9A_KRN).Value);
            DateTime appExpiryDate = DateTime.Now;
            TLV appExiryDateTLV = database.Get(EMVTagsEnum.APPLICATION_EXPIRATION_DATE_5F24_KRN);
            if (appExiryDateTLV != null)
                appExpiryDate = EMVTagsEnum.APPLICATION_EXPIRATION_DATE_5F24_KRN.FormatAsDateTime(appExiryDateTLV.Value);
            if (appExiryDateTLV == null || (transactionDate > appExpiryDate))
            {
                CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3 ctq = new CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3(database);
                if (ctq.Value.GoOnlineIfApplicationExpired)
                {
                    database.OnlineRequiredByReaderIndicator = true;
                }
                else
                {
                    database.DeclineRequiredByReaderIndicator = true;
                    return SignalsEnum.NONE;
                }
            }
            #endregion

            #region 5.5.1.2
            string pan = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN).Value);
            if (database.Kernel3Configuration.ExceptionFileEnabled && cardExceptionManager.CheckForCardException(pan))
            {
                database.DeclineRequiredByReaderIndicator = true;
                return SignalsEnum.NONE;
            }
            #endregion

            TransactionTypeEnum tt = (TransactionTypeEnum)Formatting.GetEnum(typeof(TransactionTypeEnum), database.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN).Value[0]);

            #region 5.5.1.3
            if (tt == TransactionTypeEnum.CashWithdrawal || tt == TransactionTypeEnum.CashDisbursement && database.Kernel3Configuration.AUCManualCheckSupported)
            {
                TLV icc = database.Get(EMVTagsEnum.ISSUER_COUNTRY_CODE_5F28_KRN);
                if (icc == null)
                {
                    database.DeclineRequiredByReaderIndicator = true;
                    return SignalsEnum.NONE;
                }
                TLV tcc = database.Get(EMVTagsEnum.TERMINAL_COUNTRY_CODE_9F1A_KRN);
                APPLICATION_USAGE_CONTROL_9F07_KRN auc = new APPLICATION_USAGE_CONTROL_9F07_KRN(database);

                if (!((Formatting.ByteArrayToHexString(icc.Value) == Formatting.ByteArrayToHexString(tcc.Value) && auc.Value.IsValidForDomesticCashTransactions) ||
                       (Formatting.ByteArrayToHexString(icc.Value) != Formatting.ByteArrayToHexString(tcc.Value) && auc.Value.IsValidForInternationalCashTransactions)))
                {
                    CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3 ctq = new CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3(database);
                    if (database.IsEmpty(EMVTagsEnum.CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3.Tag) || ctq.Value.SwitchInterfaceForCashTransactions)
                    {
                        return CommonRoutines.PostOutcome(database, qManager,
                         KernelMessageidentifierEnum.N_A,
                         KernelStatusEnum.PROCESSING_ERROR,
                         null,
                         Kernel2OutcomeStatusEnum.TRY_ANOTHER_INTERFACE,
                         Kernel2StartEnum.N_A,
                         true,
                         KernelMessageidentifierEnum.N_A,
                         L1Enum.NOT_SET,
                         null,
                         L2Enum.STATUS_BYTES,
                         L3Enum.NOT_SET);
                    }
                    else
                    {
                        database.DeclineRequiredByReaderIndicator = true;
                        return SignalsEnum.NONE;
                    }
                }
            }
            #endregion

            #region 5.5.1.4
            if (tt == TransactionTypeEnum.PurchaseWithCashback && database.Kernel3Configuration.AUCCashbackCheckSupported)
            {
                TLV icc = database.Get(EMVTagsEnum.ISSUER_COUNTRY_CODE_5F28_KRN);
                if (icc == null)
                {
                    database.DeclineRequiredByReaderIndicator = true;
                    return SignalsEnum.NONE;
                }
                TLV tcc = database.Get(EMVTagsEnum.TERMINAL_COUNTRY_CODE_9F1A_KRN);

                APPLICATION_USAGE_CONTROL_9F07_KRN auc = new APPLICATION_USAGE_CONTROL_9F07_KRN(database);

                if (!((Formatting.ByteArrayToHexString(icc.Value) == Formatting.ByteArrayToHexString(tcc.Value) && auc.Value.IsValidForDomesticCashTransactions) ||
                        (Formatting.ByteArrayToHexString(icc.Value) != Formatting.ByteArrayToHexString(tcc.Value) && auc.Value.IsValidForInternationalCashTransactions)))
                {
                    CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3 ctq = new CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3(database);
                    if (database.IsEmpty(EMVTagsEnum.CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3.Tag) || ctq.Value.SwitchInterfaceForCashBackTransactions)
                    {
                        return CommonRoutines.PostOutcome(database, qManager,
                        KernelMessageidentifierEnum.N_A,
                        KernelStatusEnum.PROCESSING_ERROR,
                        null,
                        Kernel2OutcomeStatusEnum.TRY_ANOTHER_INTERFACE,
                        Kernel2StartEnum.N_A,
                        true,
                        KernelMessageidentifierEnum.N_A,
                        L1Enum.NOT_SET,
                        null,
                        L2Enum.STATUS_BYTES,
                        L3Enum.NOT_SET);
                    }
                    else
                    {
                        database.DeclineRequiredByReaderIndicator = true;
                        return SignalsEnum.NONE;
                    }
                }
            }
            #endregion

            #region 5.5.1.5
            if (database.Kernel3Configuration.ATMOfflineCheck)
            {
                TERMINAL_TYPE_9F35_KRN termType = new TERMINAL_TYPE_9F35_KRN(database);
                if (termType.Value.TerminalType.Code == 0x14 || termType.Value.TerminalType.Code == 0x15 || termType.Value.TerminalType.Code == 0x16)
                {
                    if (database.IsEmpty(EMVTagsEnum.CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3.Tag))
                    {
                        database.DeclineRequiredByReaderIndicator = true;
                        return SignalsEnum.NONE;
                    }
                    else
                    {
                        CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3 ctq = new CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3(database);
                        if (!ctq.Value.ValidForContactlessATMTransactions)
                        {
                            database.OnlineRequiredByReaderIndicator = true;
                        }
                        else
                        {
                            APPLICATION_USAGE_CONTROL_9F07_KRN auc = new APPLICATION_USAGE_CONTROL_9F07_KRN(database);
                            if (auc == null)
                            {
                                database.OnlineRequiredByReaderIndicator = true;
                            }
                            else if (!auc.Value.IsValidAtATMs)
                            {
                                database.DeclineRequiredByReaderIndicator = true;
                                return SignalsEnum.NONE;
                            }
                            else
                            {
                                return CommonRoutines.PostOutcome(database, qManager,
                                   KernelMessageidentifierEnum.N_A,
                                   KernelStatusEnum.PROCESSING_ERROR,
                                   null,
                                   Kernel2OutcomeStatusEnum.TRY_ANOTHER_INTERFACE,
                                   Kernel2StartEnum.N_A,
                                   true,
                                   KernelMessageidentifierEnum.N_A,
                                   L1Enum.NOT_SET,
                                   null,
                                   L2Enum.STATUS_BYTES,
                                   L3Enum.NOT_SET);
                            }
                        }
                    }
                }
            }
            #endregion

            return SignalsEnum.NONE;
        }



        private static bool CheckMandatoryFields(Kernel3Database database, PublicKeyCertificateManager pkcm)
        {
            TLV ttqCheck = database.Get(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN);
            if (ttqCheck == null) return false;

            TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN ttq = new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN(database);

            //Reader
            if (database.IsNotPresent(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag)) return false;
            if (database.IsNotPresent(EMVTagsEnum.AMOUNT_OTHER_NUMERIC_9F03_KRN.Tag)) return false;
            //if (database.IsNotPresent(EMVTagsEnum.APPLICATION_IDENTIFIER_AID_TERMINAL_9F06_KRN.Tag)) return false;
            //if (database.IsNotPresent(EMVTagsEnum.MERCHANT_NAME_AND_LOCATION_9F4E_KRN.Tag)) return false;
            if (database.IsNotPresent(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN.Tag)) return false;
            if (database.IsNotPresent(EMVTagsEnum.TRANSACTION_DATE_9A_KRN.Tag)) return false;
            if (database.IsNotPresent(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN.Tag)) return false;
            if (database.IsNotPresent(EMVTagsEnum.TERMINAL_COUNTRY_CODE_9F1A_KRN.Tag)) return false;
            if (database.IsNotPresent(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN.Tag)) return false;

            //Card
            //if (database.IsNotPresent(EMVTagsEnum.CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3.Tag)) return false;
            //CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3 ctq = new CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3(database);

            //if (database.IsNotPresent(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN.Tag)) return false;
            //if (database.IsNotPresent(EMVTagsEnum.APPLICATION_DEDICATED_FILE_ADF_NAME_4F_KRN.Tag)) return false;
            if (database.IsNotPresent(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag)) return false;
            if (database.IsNotPresent(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag)) return false;

            if (database.IsNotPresent(EMVTagsEnum.DEDICATED_FILE_DF_NAME_84_KRN.Tag)) return false;
            //if (database.IsNotPresent(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_PROPRIETARY_TEMPLATE_A5_KRN.Tag)) return false;
            //if (database.IsNotPresent(EMVTagsEnum.FILE_CONTROL_INFORMATION_FCI_TEMPLATE_6F_KRN.Tag)) return false;
            //if (database.IsNotPresent(EMVTagsEnum.FORM_FACTOR_INDICATOR_FFI_9F6E_KRN3.Tag)) return false;
            if (database.IsNotPresent(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag)) return false;
            if (database.IsNotPresent(EMVTagsEnum.PROCESSING_OPTIONS_DATA_OBJECT_LIST_PDOL_9F38_KRN.Tag)) return false;
            if (database.IsNotPresent(EMVTagsEnum.TRACK_2_EQUIVALENT_DATA_57_KRN.Tag)) return false;

            bool odaCardSupported = true;
            if (database.IsPresent(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag))
            {
                APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
                APPLICATION_INTERCHANGE_PROFILE_82_KRN.APPLICATION_INTERCHANGE_PROFILE_82_KRN_VALUE aipST = aip.Value;
                if (!aipST.DDAsupported)
                    odaCardSupported = false;
                else
                    odaCardSupported = true;

                if (database.IsPresent(EMVTagsEnum.CARD_ADDITIONAL_PROCESSES_9F68_KRN.Tag))
                {
                    if (Formatting.IsBitSet(database.Get(EMVTagsEnum.CARD_ADDITIONAL_PROCESSES_9F68_KRN).Value[1], 5))
                    {
                        odaCardSupported = true;
                    }
                    else
                        odaCardSupported = false;
                }
            }

            //Card fdda
            if (odaCardSupported && ttq.Value.OfflineDataAuthenticationForOnlineAuthorizationsSupported && database.Kernel3Configuration.FDDAForOnlineSupported)
            {
                if (database.IsNotPresent(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag) &&
                        database.IsNotPresent(EMVTagsEnum.SIGNED_STATIC_APPLICATION_DATA_93_KRN.Tag) &&
                        database.IsNotPresent(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag))
                    return false;

                if (database.IsPresent(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag))
                {
                    if (database.IsNotPresent(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN.Tag)) return false;
                    if (pkcm.GetCAPK(RIDEnum.A000000003, database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN).Value[0]) == null) return false;

                    if (database.IsNotPresent(EMVTagsEnum.ISSUER_PUBLIC_KEY_CERTIFICATE_90_KRN.Tag)) return false;
                    if (database.IsNotPresent(EMVTagsEnum.ISSUER_PUBLIC_KEY_EXPONENT_9F32_KRN.Tag)) return false;
                    if (database.IsNotPresent(EMVTagsEnum.ISSUER_PUBLIC_KEY_REMAINDER_92_KRN.Tag)) return false;

                    if (database.IsNotPresent(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PUBLIC_KEY_CERTIFICATE_9F46_KRN.Tag)) return false;
                    if (database.IsNotPresent(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PUBLIC_KEY_EXPONENT_9F47_KRN.Tag)) return false;
                    //if (database.IsNotPresent(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PUBLIC_KEY_REMAINDER_9F48_KRN.Tag)) return false;

                    if (database.IsNotPresent(EMVTagsEnum.CARD_AUTHENTICATION_RELATED_DATA_9F69_KRN3.Tag)) return false;

                    //if (database.IsNotPresent(EMVTagsEnum.APPLICATION_EXPIRATION_DATE_5F24_KRN.Tag)) return false;
                    if (database.IsNotPresent(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag)) return false;
                }

                if (database.IsPresent(EMVTagsEnum.SIGNED_STATIC_APPLICATION_DATA_93_KRN.Tag))
                {
                    if (database.IsNotPresent(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN.Tag)) return false;
                    if (pkcm.GetCAPK(RIDEnum.A000000003, database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN).Value[0]) == null) return false;

                    if (database.IsNotPresent(EMVTagsEnum.ISSUER_PUBLIC_KEY_CERTIFICATE_90_KRN.Tag)) return false;
                    if (database.IsNotPresent(EMVTagsEnum.ISSUER_PUBLIC_KEY_EXPONENT_9F32_KRN.Tag)) return false;
                    if (database.IsNotPresent(EMVTagsEnum.ISSUER_PUBLIC_KEY_REMAINDER_92_KRN.Tag)) return false;

                    //if (database.IsNotPresent(EMVTagsEnum.APPLICATION_EXPIRATION_DATE_5F24_KRN.Tag)) return false;
                    if (database.IsNotPresent(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag)) return false;
                }
            }

            //Application Usage Control
            //if (database.IsNotPresent(EMVTagsEnum.ISSUER_COUNTRY_CODE_5F28_KRN.Tag)) return false;

            return true;
        }

        private static bool DoDEKIfNeeded(KernelDatabaseBase database, KernelQ qManager)
        {
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

            if (database.IsNotEmptyList(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag) && database.TagsToReadYet.Count == 0)
            {
                CommonRoutines.PostDEK(database, qManager);
                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
                return true;
            }
            return false;
        }
    }
}

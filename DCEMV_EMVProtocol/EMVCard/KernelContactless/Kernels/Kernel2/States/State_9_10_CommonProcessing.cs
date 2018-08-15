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
using DCEMV.TLVProtocol;
using DCEMV.ISO7816Protocol;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_9_10_CommonProcessing
    {
        public static SignalsEnum DoCDA9_10_1(Kernel2Database database, KernelQ qManager, PublicKeyCertificateManager publicKeyCertificateManager, CardQ cardQManager, CardResponse cardResponse)
        {
            CAPublicKeyCertificate capk = publicKeyCertificateManager.GetCAPK(RIDEnum.A000000004,database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN).Value[0]);

            if (capk == null)
            {
                #region 9_10.1
                return DoPart_F(database, qManager, cardResponse);
                #endregion
            }
            else
            {
                #region 9_10.2
                IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);
                TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                if (ids.Value.IsRead)
                #endregion
                {
                    bool successIDSRead = false;
                    #region 9_10.2.2
                    if (tvr.Value.RelayResistancePerformedEnum == RelayResistancePerformedEnum.RRP_PERFORMED)
                    #endregion
                    {
                        #region 9_10.3.1
                        successIDSRead = VerifySDAD_Summaries_CheckRelayData_9_10__3_1(database, capk, cardResponse);
                        #endregion
                    }
                    else
                    {
                        #region 9_10.3
                        successIDSRead = VerifySDAD_Summaries_9_10__3(database, capk, cardResponse);
                        #endregion
                    }
                    #region 9_10.3.5
                    if (!successIDSRead)
                    #endregion
                    {
                        return DoPart_F(database, qManager, cardResponse);
                    }
                    else
                    {
                        #region 9_10.8
                        string dbds1 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.DS_SUMMARY_1_9F7D_KRN2).Value);
                        if (!database.IsNotEmpty(EMVTagsEnum.DS_SUMMARY_2_DF8101_KRN2.Tag))
                        #endregion
                        {
                            #region 9_10.9
                            return DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                            #endregion
                        }

                        #region 9_10.10
                        string dbds2 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.DS_SUMMARY_2_DF8101_KRN2).Value);
                        if (dbds1 != dbds2)
                        #endregion
                        {
                            #region 9_10.11
                            return DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.IDS_READ_ERROR, L3Enum.NOT_SET);
                            #endregion
                        }

                        #region 9_10.12
                        byte dsss = database.Get(EMVTagsEnum.DS_SUMMARY_STATUS_DF810B_KRN2).Value[0];
                        dsss = (byte)(dsss | 0x80); //set succesful read
                        database.Get(EMVTagsEnum.DS_SUMMARY_STATUS_DF810B_KRN2).Value[0] = dsss;
                        #endregion

                        #region 9_10.13
                        if (!ids.Value.IsWrite)
                        #endregion
                        {
                            #region 9_10.70
                            return DoPart_E(database, qManager, cardQManager);
                            #endregion
                        }
                        else
                        {
                            #region 9_10.14
                            if (!database.IsPresent(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2.Tag))
                            #endregion
                            {
                                #region 9_10.15
                                return DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                                #endregion
                            }
                            else
                            {
                                #region 9_10.16
                                string dbds3 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2).Value);
                                if (dbds2 == dbds3)
                                #endregion
                                {
                                    #region 9_10.18
                                    byte dsodsifr = database.Get(EMVTagsEnum.DS_ODS_INFO_FOR_READER_DF810A_KRN2).Value[0];
                                    if ((dsodsifr & 0x02) == 0x02) //stop if write set
                                    #endregion
                                    {
                                        #region 9_10.19
                                        return DoInvalidResponsePart_D(database, qManager, L1Enum.NOT_SET, L2Enum.IDS_WRITE_ERROR, L3Enum.NOT_SET);
                                        #endregion
                                    }
                                    else
                                    {
                                        return DoPart_E(database, qManager, cardQManager);
                                    }
                                }
                                else
                                {
                                    #region 9_10.17
                                    dsss = (byte)(dsss | 0x40); //set succesful write
                                    database.Get(EMVTagsEnum.DS_SUMMARY_STATUS_DF810B_KRN2).Value[0] = dsss;
                                    return DoPart_E(database, qManager, cardQManager);
                                    #endregion
                                }
                            }
                        }
                    }
                }
                else
                {
                    bool successIDSNotRead = false;
                    #region 9_10.2.1
                    if (tvr.Value.RelayResistancePerformedEnum == RelayResistancePerformedEnum.RRP_PERFORMED)
                    #endregion
                    {
                        #region 9_10.4.1
                        successIDSNotRead = VerifySDAD_CheckRelayData_9_10__4_1(database, capk, cardResponse);
                        #endregion
                    }
                    else
                    {
                        #region 9_10.4
                        successIDSNotRead = VerifySDAD_9_10__4(database, capk, cardResponse);
                        #endregion
                    }
                    #region 9_10.6
                    if (!successIDSNotRead)
                    #endregion
                    {
                        return DoPart_F(database, qManager, cardResponse);
                    }
                    else
                    {
                        return DoPart_E(database, qManager, cardQManager);
                    }
                }
            }
        }
        public static SignalsEnum DoNOCDA9_10_30(Kernel2Database database, KernelQ qManager, PublicKeyCertificateManager publicKeyCertificateManager, CardQ cardQManager, CardResponse cardResponse)
        {
            #region 9_10.30
            if (!database.IsPresent(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag))
            #endregion
            {
                #region 9_10.31
                return DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                #endregion
            }
            else
            {
                #region 9_10.32
                REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcp = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
                if ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x00)
                #endregion
                {
                    #region 9_10.33
                    IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);
                    if (ids.Value.IsRead)
                    #endregion
                    {
                        #region 9_10.37
                        return DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_ERROR, L3Enum.NOT_SET);
                        #endregion
                    }
                    else
                    {
                        #region 9_10.35
                        if (rcp.Value.ACTypeEnum == ACTypeEnum.AAC)
                        #endregion
                        {
                            #region 9_10.36
                            if (rcp.Value.CDASignatureRequested)
                            #endregion
                            {
                                #region 9_10.37
                                return DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_ERROR, L3Enum.NOT_SET);
                                #endregion
                            }
                            else
                            {
                                return DoPart_E(database, qManager, cardQManager);
                            }
                        }
                        else
                        {
                            return DoPart_E(database, qManager, cardQManager);
                        }
                    }
                }
                else
                {
                    #region 9_10.34
                    if (rcp.Value.CDASignatureRequested)
                    #endregion
                    {
                        #region 9_10.37
                        return DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_ERROR, L3Enum.NOT_SET);
                        #endregion
                    }
                    else
                    {
                        #region 9_10.38
                        TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                        if (tvr.Value.RelayResistancePerformedEnum == RelayResistancePerformedEnum.RRP_PERFORMED)
                        #endregion
                        {
                            #region 9_10.39
                            Do9_10__39(database);
                            #endregion
                        }
                        return DoPart_E(database, qManager, cardQManager);
                    }
                }
            }
        }
        private static void Do9_10__39(Kernel2Database database)
        {
            if (database.IsNotEmpty(EMVTagsEnum.TRACK_2_EQUIVALENT_DATA_57_KRN.Tag))
            {
                TRACK_2_EQUIVALENT_DATA_57_KRN t2ed = new TRACK_2_EQUIVALENT_DATA_57_KRN(database);
                if (database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN).Value.Length <= 8)
                {
                    t2ed.Value.DiscretionaryData = "0000000000000";
                }
                else
                {
                    t2ed.Value.DiscretionaryData = "0000000000";
                }
                t2ed.UpdateDB();

                if (database.IsNotEmpty(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN.Tag) &&
                    database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN).Value[0] < 0x0A)
                {
                    byte capki = database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN).Value[0];
                    t2ed.Value.DiscretionaryData = capki + t2ed.Value.DiscretionaryData.Substring(1);
                }

                byte rrpc = database.Get(EMVTagsEnum.RRP_COUNTER_DF8307_KRN2).Value[0];
                t2ed.Value.DiscretionaryData = t2ed.Value.DiscretionaryData[0] + rrpc + t2ed.Value.DiscretionaryData.Substring(2);

                byte[] drre = new byte[2];
                Array.Copy(database.Get(EMVTagsEnum.DEVICE_RELAY_RESISTANCE_ENTROPY_DF8302_KRN2).Value, 0,
                    drre, 0, drre.Length);
                ushort ddreC = Formatting.ConvertToInt16(drre);
                string ddreS = Convert.ToString(ddreC);
                ddreS = ddreS.PadLeft(5, '0');
                t2ed.Value.DiscretionaryData = t2ed.Value.DiscretionaryData.Substring(0, 2) +
                    ddreS +
                    t2ed.Value.DiscretionaryData.Substring(7);

                if (database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN).Value.Length <= 8)
                {
                    byte drre3 = (database.Get(EMVTagsEnum.DEVICE_RELAY_RESISTANCE_ENTROPY_DF8302_KRN2).Value[2]);
                    ushort ddre3C = Formatting.ConvertToInt16(new byte[] { drre3 });
                    string ddre3S = Convert.ToString(ddre3C);
                    ddre3S = ddre3S.PadLeft(3, '0');
                    t2ed.Value.DiscretionaryData = t2ed.Value.DiscretionaryData.Substring(0, 7) +
                        ddre3S +
                        t2ed.Value.DiscretionaryData.Substring(10);
                }

                ushort mrrpt = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.MEASURED_RELAY_RESISTANCE_PROCESSING_TIME_DF8306_KRN2).Value);
                mrrpt = (ushort)(mrrpt / 10);
                if (mrrpt > 999)
                {
                    mrrpt = 999;
                }

                string mrrptS = Convert.ToString(mrrpt);
                mrrptS = mrrptS.PadLeft(3, '0');

                t2ed.Value.DiscretionaryData = t2ed.Value.DiscretionaryData.Substring(0, t2ed.Value.DiscretionaryData.Length - 3) +
                        mrrptS;

                t2ed.UpdateDB();
            }
        }
        public static SignalsEnum DoInvalidResponsePart_C(Kernel2Database database, KernelQ qManager, L1Enum l1Enum, L2Enum l2Enum, L3Enum l3Enum)
        {
            #region 9_10.50
            CommonRoutines.UpdateUserInterfaceRequestData(database, KernelMessageidentifierEnum.ERROR_OTHER_CARD, KernelStatusEnum.NOT_READY);
            #endregion

            #region 9_10.51
            IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);

            CommonRoutines.CreateEMVDiscretionaryData(database);
            if (ids.Value.IsWrite)
            #endregion
            {
                #region 9_10.52
                CommonRoutines.CreateEMVDataRecord(database);
                #endregion
            }

            #region 9_10.53
            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.N_A,
                KernelStatusEnum.N_A,
                null,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                l1Enum,
                null,
                l2Enum,
                l3Enum);
            #endregion
        }
        private static SignalsEnum DoInvalidResponsePart_D(Kernel2Database database, KernelQ qManager, L1Enum l1Enum, L2Enum l2Enum, L3Enum l3Enum)
        {
            #region 9_10.61 and 9_10.62
            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                KernelStatusEnum.NOT_READY,
                database.GetDefault(EMVTagsEnum.MESSAGE_HOLD_TIME_DF812D_KRN2).Value,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                l1Enum,
                null,
                l2Enum,
                l3Enum);
            #endregion
        }
        private static SignalsEnum DoPart_F(Kernel2Database database, KernelQ qManager, CardResponse cardResponse)
        {
            #region 9_10.46
            //see below
            #endregion

            #region 9_10.46.1
            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
            tvr.Value.CDAFailed = true;
            tvr.UpdateDB();
            #endregion

            return DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CAM_FAILED, L3Enum.NOT_SET);
        }
        private static SignalsEnum DoPart_E(Kernel2Database database, KernelQ qManager, CardQ cardQManager)
        {
            #region 9_10.70
            CommonRoutines.CreateEMVDataRecord(database);

            Kernel2OutcomeStatusEnum k2OutcomeStatus = Kernel2OutcomeStatusEnum.N_A;
            Kernel2StartEnum k2StartStatus = Kernel2StartEnum.N_A;
            KernelStatusEnum k2Status = KernelStatusEnum.N_A;
            KernelMessageidentifierEnum k2MessageIdentifier = KernelMessageidentifierEnum.N_A;
            byte[] holdTime = new byte[] { 0x00, 0x00, 0x00 };
            ValueQualifierEnum valueQualifierEnum = ValueQualifierEnum.NONE;
            KernelCVMEnum cvmEnum = new OUTCOME_PARAMETER_SET_DF8129_KRN2(database).Value.CVM;
            byte[] currencyCode = new byte[2];
            byte[] valueQualifier = new byte[6];
            bool uiRequestOnOutcomePresent;

            #endregion

            #region 9_10.71
            if (database.IsNotEmpty(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2.Tag) &&
                ((int)Formatting.ConvertToInt32(database.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2).Value) & 0x0000030F) != 0x00000000)
            #endregion
            {
                #region 9_10.72
                k2OutcomeStatus = Kernel2OutcomeStatusEnum.END_APPLICATION;
                k2StartStatus = Kernel2StartEnum.B;
                #endregion

                #region 9_10.73
                PHONE_MESSAGE_TABLE_DF8131_KRN2 pmt = new PHONE_MESSAGE_TABLE_DF8131_KRN2(database.GetDefault(EMVTagsEnum.PHONE_MESSAGE_TABLE_DF8131_KRN2));
                foreach (PhoneMessageTableEntry_DF8131 entry in pmt.Value.Entries)
                {
                    int pcii = (int)Formatting.ConvertToInt32(database.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2).Value);
                    int pciMask = (int)Formatting.ConvertToInt32(entry.PCIIMask);
                    int pciValue = (int)Formatting.ConvertToInt32(entry.PCIIValue);
                    if ((pciMask & pcii) == pciValue)
                    {
                        k2MessageIdentifier = entry.MessageIdentifier;
                        k2Status = entry.Status;
                        holdTime = database.GetDefault(EMVTagsEnum.MESSAGE_HOLD_TIME_DF812D_KRN2).Value;
                        break;
                    }
                }
                #endregion
            }
            else
            {
                string tt = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN).Value);
                #region 9_10.74
                if ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x40)
                #endregion
                {
                    k2OutcomeStatus = Kernel2OutcomeStatusEnum.APPROVED;
                }
                else
                {
                    if ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x80)
                    {
                        k2OutcomeStatus = Kernel2OutcomeStatusEnum.ONLINE_REQUEST;
                    }
                    else
                    {
                        /*
                         * Check if Transaction Type indicates a cash transaction (cash
                         * withdrawal or cash disbursement) or a purchase transaction (purchase
                         * or purchase with cashback).
                         */
                        if (tt == "01" || tt == "17" || tt == "00" || tt == "09")
                        {
                            if (database.IsNotEmpty(EMVTagsEnum.THIRD_PARTY_DATA_9F6E_KRN2.Tag))
                            {
                                THIRD_PARTY_DATA_9F6E_KRN tpd = new THIRD_PARTY_DATA_9F6E_KRN(database);
                                TERMINAL_CAPABILITIES_9F33_KRN tc = new TERMINAL_CAPABILITIES_9F33_KRN(database);
                                ushort uid = Formatting.ConvertToInt16(tpd.Value.UniqueIdentifier);
                                ushort dt = Formatting.ConvertToInt16(tpd.Value.DeviceType);
                                if ((uid & 0x8000) == 0x0000 && dt != 0x3030 || tc.Value.ICWithContactsCapable)
                                {
                                    k2OutcomeStatus = Kernel2OutcomeStatusEnum.DECLINED;
                                }
                                else
                                {
                                    k2OutcomeStatus = Kernel2OutcomeStatusEnum.TRY_ANOTHER_INTERFACE;
                                }
                            }
                            else
                            {
                                k2OutcomeStatus = Kernel2OutcomeStatusEnum.TRY_ANOTHER_INTERFACE;
                            }
                        }
                        else
                        {
                            k2OutcomeStatus = Kernel2OutcomeStatusEnum.END_APPLICATION;
                        }
                    }
                }

                #region 9_10.75
                k2Status = KernelStatusEnum.NOT_READY;

                if ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x40)
                {
                    holdTime = database.GetDefault(EMVTagsEnum.MESSAGE_HOLD_TIME_DF812D_KRN2).Value;

                    if (database.IsNotEmpty(EMVTagsEnum.BALANCE_READ_AFTER_GEN_AC_DF8105_KRN2.Tag))
                    {
                        valueQualifierEnum = ValueQualifierEnum.BALANCE;
                        valueQualifier = database.Get(EMVTagsEnum.BALANCE_READ_AFTER_GEN_AC_DF8105_KRN2).Value;
                        if (database.IsNotEmpty(EMVTagsEnum.APPLICATION_CURRENCY_CODE_9F42_KRN.Tag))
                        {
                            currencyCode = database.Get(EMVTagsEnum.APPLICATION_CURRENCY_CODE_9F42_KRN).Value;
                        }
                    }

                    if (cvmEnum == KernelCVMEnum.OBTAIN_SIGNATURE)
                    {
                        k2MessageIdentifier = KernelMessageidentifierEnum.APPROVED_SIGN;
                    }
                    else
                    {
                        k2MessageIdentifier = KernelMessageidentifierEnum.APPROVED;
                    }
                }
                else
                {
                    if ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x80)
                    {
                        holdTime = new byte[] { 0x00, 0x00, 0x00 };
                        k2MessageIdentifier = KernelMessageidentifierEnum.AUTHORISING_PLEASE_WAIT;
                    }
                    else
                    {
                        if (tt == "01" || tt == "17" || tt == "00" || tt == "09")
                        {
                            holdTime = database.GetDefault(EMVTagsEnum.MESSAGE_HOLD_TIME_DF812D_KRN2).Value;


                            if (database.IsNotEmpty(EMVTagsEnum.THIRD_PARTY_DATA_9F6E_KRN2.Tag))
                            {
                                THIRD_PARTY_DATA_9F6E_KRN tpd = new THIRD_PARTY_DATA_9F6E_KRN(database);
                                TERMINAL_CAPABILITIES_9F33_KRN tc = new TERMINAL_CAPABILITIES_9F33_KRN(database);
                                ushort uid = Formatting.ConvertToInt16(tpd.Value.UniqueIdentifier);
                                ushort dt = Formatting.ConvertToInt16(tpd.Value.DeviceType);
                                if ((uid & 0x8000) == 0x0000 && dt != 0x3030 || tc.Value.ICWithContactsCapable)
                                {
                                    k2MessageIdentifier = KernelMessageidentifierEnum.DECLINED;
                                }
                                else
                                {
                                    k2MessageIdentifier = KernelMessageidentifierEnum.INSERT_CARD;
                                }
                            }
                            else
                            {
                                k2MessageIdentifier = KernelMessageidentifierEnum.INSERT_CARD;
                            }
                        }
                        else
                        {
                            holdTime = new byte[] { 0x00, 0x00, 0x00 };
                            k2MessageIdentifier = KernelMessageidentifierEnum.CLEAR_DISPLAY;
                        }
                    }
                }

                #endregion
            }

            #region 9_10.76
            if (database.IsNotEmpty(EMVTagsEnum.TAGS_TO_WRITE_AFTER_GEN_AC_FF8103_KRN2.Tag))
            #endregion
            {
                #region 9_10.77
                TLV tagToPut = database.TagsToWriteAfterGenACYet.GetFirstAndRemoveFromList();
                EMVPutDataRequest request = new EMVPutDataRequest(tagToPut);
                #endregion
                #region 9_10.78
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                return SignalsEnum.WAITING_FOR_PUT_DATA_RESPONSE_AFTER_GEN_AC;
                #endregion
            }

            CommonRoutines.CreateEMVDiscretionaryData(database);
            
            #region 9_10.78.1
            if (database.IsNotEmpty(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2.Tag) &&
                ((int)Formatting.ConvertToInt32(database.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2).Value) & 0x0000030F) != 0x00000000)
            #endregion
            {
                #region 9_10.79
                CommonRoutines.PostUIOnly(database, qManager, k2MessageIdentifier, k2Status, true, holdTime);
                #endregion

                #region 9_10.80
                uiRequestOnOutcomePresent = false;
                k2Status = KernelStatusEnum.READY_TO_READ;
                holdTime = new byte[] { 0x00, 0x00, 0x00 };
                #endregion
            }
            else
            {
                #region 9_10.81
                uiRequestOnOutcomePresent = true;
                #endregion
            }
            return CommonRoutines.PostOutcome(database, qManager,
                k2MessageIdentifier, 
                k2Status, 
                holdTime, 
                k2OutcomeStatus, 
                k2StartStatus, 
                uiRequestOnOutcomePresent,
                KernelMessageidentifierEnum.N_A, 
                L1Enum.NOT_SET, 
                null, 
                L2Enum.NOT_SET, 
                L3Enum.NOT_SET, 
                valueQualifierEnum, 
                valueQualifier, 
                currencyCode, 
                false,
                cvmEnum);
        }
        
        
        private static bool VerifySDAD_9_10__4(Kernel2Database database, CAPublicKeyCertificate capk, CardResponse cardResponse)
        {
            ICCDynamicData iccdd = VerifySAD.VerifySDAD(ICCDynamicDataType.NO_IDS_OR_RRP, true, database, database.StaticDataToBeAuthenticated, capk, cardResponse);
            if (iccdd == null) return false;

            VerifySAD.AddSDADDataToDatabase(database, iccdd);
            return true;
        }
        private static bool VerifySDAD_CheckRelayData_9_10__4_1(Kernel2Database database, CAPublicKeyCertificate capk, CardResponse cardResponse)
        {
            ICCDynamicData iccdd = VerifySAD.VerifySDAD(ICCDynamicDataType.RRP, true, database, database.StaticDataToBeAuthenticated, capk, cardResponse);
            if (iccdd == null) return false;

            VerifySAD.AddSDADDataToDatabase(database, iccdd);

            string s1 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.TERMINAL_RELAY_RESISTANCE_ENTROPY_DF8301_KRN2).Value);
            string s2 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.DEVICE_RELAY_RESISTANCE_ENTROPY_DF8302_KRN2).Value);
            string s3 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.MIN_TIME_FOR_PROCESSING_RELAY_RESISTANCE_APDU_DF8303_KRN2).Value);
            string s4 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.MAX_TIME_FOR_PROCESSING_RELAY_RESISTANCE_APDU_DF8304_KRN2).Value);
            string s5 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.DEVICE_ESTIMATED_TRANSMISSION_TIME_FOR_RELAY_RESISTANCE_RAPDU_DF8305_KRN2).Value);

            if (s1 != Formatting.ByteArrayToHexString(iccdd.Terminal_Relay_Resistance_Entropy)) return false;
            if (s2 != Formatting.ByteArrayToHexString(iccdd.Device_Relay_Resistance_Entropy)) return false;
            if (s3 != Formatting.ByteArrayToHexString(iccdd.Min_Time_For_Processing_Relay_Resistance_APDU)) return false;
            if (s4 != Formatting.ByteArrayToHexString(iccdd.Max_Time_For_Processing_Relay_Resistance_APDU)) return false;
            if (s5 != Formatting.ByteArrayToHexString(iccdd.Device_Estimated_Transmission_Time_For_Relay_Resistance_R_APDU)) return false;

            return true;
        }
        private static bool VerifySDAD_Summaries_9_10__3(Kernel2Database database, CAPublicKeyCertificate capk, CardResponse cardResponse)
        {
            ICCDynamicData iccdd = VerifySAD.VerifySDAD(ICCDynamicDataType.IDS, true, database, database.StaticDataToBeAuthenticated, capk, cardResponse);
            if (iccdd == null) return false;

            VerifySAD.AddSDADDataToDatabase(database, iccdd);

            TLV ds2 = database.Get(EMVTagsEnum.DS_SUMMARY_2_DF8101_KRN2);
            TLV ds3 = database.Get(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2);

            if (ds2 == null && iccdd.DSSummary2 != null)
            {
                ds2 = TLV.Create(EMVTagsEnum.DS_SUMMARY_2_DF8101_KRN2.Tag, iccdd.DSSummary2);
                database.AddToList(ds2);
            }

            if (ds3 == null && iccdd.DSSummary3 != null)
            {
                ds3 = TLV.Create(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2.Tag, iccdd.DSSummary3);
                database.AddToList(ds3);
            }

            if (iccdd.DSSummary2 != null) ds2.Value = iccdd.DSSummary2;
            if (iccdd.DSSummary3 != null) ds3.Value = iccdd.DSSummary3;

            return true;
        }
        private static bool VerifySDAD_Summaries_CheckRelayData_9_10__3_1(Kernel2Database database, CAPublicKeyCertificate capk, CardResponse cardResponse)
        {
            ICCDynamicData iccdd = VerifySAD.VerifySDAD(ICCDynamicDataType.IDS_AND_RRP, true, database, database.StaticDataToBeAuthenticated, capk, cardResponse);
            if (iccdd == null) return false;

            VerifySAD.AddSDADDataToDatabase(database, iccdd);

            TLV ds2 = database.Get(EMVTagsEnum.DS_SUMMARY_2_DF8101_KRN2);
            TLV ds3 = database.Get(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2);

            if (ds2 == null && iccdd.DSSummary2 != null)
            {
                ds2 = TLV.Create(EMVTagsEnum.DS_SUMMARY_2_DF8101_KRN2.Tag, iccdd.DSSummary2);
                database.AddToList(ds2);
            }

            if (ds3 == null && iccdd.DSSummary3 != null)
            {
                ds3 = TLV.Create(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2.Tag, iccdd.DSSummary3);
                database.AddToList(ds3);
            }

            if (iccdd.DSSummary2 != null) ds2.Value = iccdd.DSSummary2;
            if (iccdd.DSSummary3 != null) ds3.Value = iccdd.DSSummary3;

            string s1 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.TERMINAL_RELAY_RESISTANCE_ENTROPY_DF8301_KRN2).Value);
            string s2 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.DEVICE_RELAY_RESISTANCE_ENTROPY_DF8302_KRN2).Value);
            string s3 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.MIN_TIME_FOR_PROCESSING_RELAY_RESISTANCE_APDU_DF8303_KRN2).Value);
            string s4 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.MAX_TIME_FOR_PROCESSING_RELAY_RESISTANCE_APDU_DF8304_KRN2).Value);
            string s5 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.DEVICE_ESTIMATED_TRANSMISSION_TIME_FOR_RELAY_RESISTANCE_RAPDU_DF8305_KRN2).Value);

            if (s1 != Formatting.ByteArrayToHexString(iccdd.Terminal_Relay_Resistance_Entropy)) return false;
            if (s2 != Formatting.ByteArrayToHexString(iccdd.Device_Relay_Resistance_Entropy)) return false;
            if (s3 != Formatting.ByteArrayToHexString(iccdd.Min_Time_For_Processing_Relay_Resistance_APDU)) return false;
            if (s4 != Formatting.ByteArrayToHexString(iccdd.Max_Time_For_Processing_Relay_Resistance_APDU)) return false;
            if (s5 != Formatting.ByteArrayToHexString(iccdd.Device_Estimated_Transmission_Time_For_Relay_Resistance_R_APDU)) return false;

            return true;
        }
    }
}

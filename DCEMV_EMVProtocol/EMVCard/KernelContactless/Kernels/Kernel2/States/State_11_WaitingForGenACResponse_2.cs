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
    public static class State_11_WaitingForGenACResponse_2
    {
        public static SignalsEnum Execute(
            Kernel2Database database,
            KernelQ qManager,
            CardQ cardQManager,
            TornTransactionLogManager tornTransactionLogManager,
            PublicKeyCertificateManager publicKeyCertificateManager,
            Stopwatch sw)
        {
            if (qManager.GetOutputQCount() > 0) //there is a pending request to the terminal
            {
                KernelRequest kernel1Request = qManager.DequeueFromInput(false);
                switch (kernel1Request.KernelTerminalReaderServiceRequestEnum)
                {
                    case KernelTerminalReaderServiceRequestEnum.STOP:
                        return EntryPointSTOP(database, qManager);

                    case KernelTerminalReaderServiceRequestEnum.DET:
                        return EntryPointDET(database, kernel1Request);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_11_WaitingForGenACResponse_2:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse, qManager, cardQManager, tornTransactionLogManager, sw, publicKeyCertificateManager);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager, tornTransactionLogManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_11_WaitingForGenACResponse_2:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }


        /*
        * 11.2
        */
        private static SignalsEnum EntryPointRA(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, TornTransactionLogManager tornTransactionLogManager, Stopwatch sw, PublicKeyCertificateManager publicKeyCertificateManager)
        {
            tornTransactionLogManager.TornTransactionLogs.RemoveFromList(database.Get(EMVTagsEnum.TORN_RECORD_FF8101_KRN2));

            #region 11.6
            if (!cardResponse.ApduResponse.Succeeded)
            #endregion
            {
                #region 11.7
                return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.TRY_AGAIN, L1Enum.RETURN_CODE, L2Enum.NOT_SET, L3Enum.NOT_SET);
                #endregion
            }

            #region 11.8
            bool parsingResult = false;
            if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x77)
            #endregion
            {
                EMVGetProcessingOptionsResponse response = cardResponse.ApduResponse as EMVGetProcessingOptionsResponse;
                parsingResult = database.ParseAndStoreCardResponse(response.ResponseData);
            }
            else
            {
                if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x80)
                {
                    if (cardResponse.ApduResponse.ResponseData.Length < 11 ||
                        cardResponse.ApduResponse.ResponseData.Length > 43 ||
                        database.IsNotEmpty(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag) ||
                        database.IsNotEmpty(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag) ||
                        database.IsNotEmpty(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag) ||
                        (cardResponse.ApduResponse.ResponseData.Length > 11 &&
                        database.IsNotEmpty(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag))
                        )
                    {
                        parsingResult = false;
                    }
                    else
                    {
                        byte[] responseBuffer = new byte[cardResponse.ApduResponse.ResponseData.Length - 2];
                        Array.Copy(cardResponse.ApduResponse.ResponseData, 2, responseBuffer, 0, responseBuffer.Length);
                        database.AddToList(TLV.Create(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag, new byte[] { responseBuffer[0] }));
                        database.AddToList(TLV.Create(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag, new byte[] { responseBuffer[1], responseBuffer[2] }));

                        byte[] ac = new byte[8];
                        Array.Copy(responseBuffer, 3, ac, 0, 8);
                        database.AddToList(TLV.Create(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag, ac));
                        if (responseBuffer.Length > 11)
                        {
                            byte[] iad = new byte[responseBuffer.Length - 11];
                            Array.Copy(responseBuffer, 11, iad, 0, iad.Length);
                            database.AddToList(TLV.Create(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag, iad));
                        }
                        parsingResult = true;
                    }
                }
            }

            #region 11.9
            if (!parsingResult)
            #endregion
            {
                #region 11.10
                return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, L2Enum.PARSING_ERROR, L3Enum.NOT_SET);
                #endregion
            }

            #region 11.18
            if (!(database.IsNotEmpty(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag) &&
                database.IsNotEmpty(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag)))
            #endregion
            {
                #region 11.19
                return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                #endregion

            }

            #region 11.20
            REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcp = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
            if (
                ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x40 && rcp.Value.ACTypeEnum == ACTypeEnum.TC) &&
                (((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x80) &&
                (rcp.Value.ACTypeEnum == ACTypeEnum.TC || rcp.Value.ACTypeEnum == ACTypeEnum.ARQC)) ||
                (database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x00
                )
            #endregion
            {
                #region 11.22
                SignalsEnum result = PostGenACBalanceReading_7_3.PostGenACBalanceReading(database, qManager, cardQManager);
                if (result != SignalsEnum.NONE)
                    return result;
                #endregion

                #region 11.23
                if (!database.IsNotEmptyList(EMVTagsEnum.TAGS_TO_WRITE_AFTER_GEN_AC_FF8103_KRN2.Tag))
                #endregion
                {
                    #region 11.24
                    CommonRoutines.PostUIOnly(database, qManager, KernelMessageidentifierEnum.CLEAR_DISPLAY, KernelStatusEnum.CARD_READ_SUCCESSFULLY, true);
                    #endregion
                }

                #region 11.25
                if (database.IsNotEmpty(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag))
                #endregion
                {
                    return DoCDAPart_A(database, qManager, publicKeyCertificateManager, cardQManager, cardResponse);
                }
                else
                {
                    return DoNOCDAPart_B(database, qManager, publicKeyCertificateManager, cardQManager, cardResponse);
                }
            }
            else
            {
                #region 11.21
                return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, L2Enum.CARD_DATA_ERROR, L3Enum.NOT_SET);
                #endregion
            }
        }
        private static SignalsEnum DoCDAPart_A(Kernel2Database database, KernelQ qManager, PublicKeyCertificateManager publicKeyCertificateManager, CardQ cardQManager, CardResponse cardResponse)
        {
            #region 11.40
            CAPublicKeyCertificate capk = publicKeyCertificateManager.GetCAPK(RIDEnum.A000000004, database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN).Value[0]);
            #endregion

            if (capk == null)
            {
                #region 11.46
                return DoPart_F(database, qManager, cardResponse);
                #endregion
            }
            else
            {
                #region 11.41
                IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);
                TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                if (ids.Value.IsRead)
                #endregion
                {
                    bool successIDSRead = false;
                    #region 11.41.2
                    if (tvr.Value.RelayResistancePerformedEnum == RelayResistancePerformedEnum.RRP_PERFORMED)
                    #endregion
                    {
                        #region 11.42.1
                        successIDSRead = VerifySDAD_Summaries_CheckRelayData_42_1(database,capk,cardResponse);
                        #endregion
                    }
                    else
                    {
                        #region 11.42
                        successIDSRead = VerifySDAD_Summaries_42(database, capk, cardResponse);
                        #endregion
                    }
                    if (!successIDSRead)
                    {
                        #region 11.46
                        return DoPart_F(database, qManager, cardResponse);
                        #endregion
                    }
                    else
                    {
                        #region
                        return DoPart_E(database,qManager, cardQManager);
                        #endregion
                    }
                }
                else
                {
                    bool successIDSNotRead = false;
                    #region 11.41.1
                    if (tvr.Value.RelayResistancePerformedEnum == RelayResistancePerformedEnum.RRP_PERFORMED)
                    #endregion
                    {
                        #region 11.43.1
                        successIDSNotRead = VerifySDAD_CheckRelayData_43_1(database, capk, cardResponse);
                        #endregion
                    }
                    else
                    {
                        #region 11.43
                        successIDSNotRead = VerifySDAD_43(database, capk, cardResponse);
                        #endregion
                    }

                    if (!successIDSNotRead)
                    {
                        #region 11.46
                        return DoPart_F(database, qManager, cardResponse);
                        #endregion
                    }
                    else
                    {
                        #region 11.47
                        IDS_STATUS_DF8128_KRN2.IDS_STATUS_DF8128_KRN2_VALUE idsV = new IDS_STATUS_DF8128_KRN2.IDS_STATUS_DF8128_KRN2_VALUE(EMVTagsEnum.IDS_STATUS_DF8128_KRN2.DataFormatter);
                        idsV.Deserialize(database.TornTempRecord.Children.Get(EMVTagsEnum.IDS_STATUS_DF8128_KRN2.Tag).Value, 0);
                        string dbds1 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.DS_SUMMARY_1_9F7D_KRN2).Value);
                        if (idsV.IsWrite)
                        #endregion
                        {
                            #region 11.48
                            string ttrds1 = Formatting.ByteArrayToHexString(database.TornTempRecord.Children.Get(EMVTagsEnum.DS_SUMMARY_1_9F7D_KRN2.Tag).Value);
                            if (dbds1 != ttrds1)
                            #endregion
                            {
                                #region 11.49
                                return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, L2Enum.IDS_READ_ERROR, L3Enum.NOT_SET);
                                #endregion
                            }
                        }
                        #region 11.50
                        if (!database.IsPresent(EMVTagsEnum.DS_SUMMARY_2_DF8101_KRN2.Tag))
                        #endregion
                        {
                            #region 11.51
                            return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                            #endregion
                        }

                        #region 11.52
                        string dbds2 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.DS_SUMMARY_2_DF8101_KRN2).Value);
                        if (dbds1 != dbds2)
                        #endregion
                        {
                            #region 11.53
                            return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, L2Enum.IDS_READ_ERROR, L3Enum.NOT_SET);
                            #endregion
                        }

                        #region 11.54
                        byte dsss = database.Get(EMVTagsEnum.DS_SUMMARY_STATUS_DF810B_KRN2).Value[0];
                        dsss = (byte)(dsss | 0x80); //set succesful read
                        database.Get(EMVTagsEnum.DS_SUMMARY_STATUS_DF810B_KRN2).Value[0] = dsss;
                        #endregion

                        #region 11.55
                        if (!ids.Value.IsWrite)
                        #endregion
                        {
                            #region 11.110
                            return DoPart_E(database, qManager, cardQManager);
                            #endregion
                        }
                        else
                        {
                            #region 11.56
                            if (!database.IsPresent(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2.Tag))
                            #endregion
                            {
                                #region 11.57
                                return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                                #endregion
                            }
                            else
                            {
                                #region 11.58
                                string dbds3 = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2).Value);
                                if (dbds2 == dbds3)
                                #endregion
                                {
                                    #region 11.60
                                    byte dsodsifr = database.Get(EMVTagsEnum.DS_ODS_INFO_FOR_READER_DF810A_KRN2).Value[0];
                                    if ((dsodsifr & 0x02) == 0x02) //stop if write set
                                    #endregion
                                    {
                                        #region 11.61
                                        CommonRoutines.UpdateErrorIndication(database, cardResponse, L1Enum.NOT_SET, L2Enum.IDS_WRITE_ERROR, L3Enum.NOT_SET);
                                        #endregion
                                        return DoInvalidResponsePart_D(database, qManager);
                                    }
                                    else
                                    {
                                        return DoPart_E(database, qManager, cardQManager);
                                    }
                                }
                                else
                                {
                                    #region 11.59
                                    dsss = (byte)(dsss | 0x40); //set succesful write
                                    database.Get(EMVTagsEnum.DS_SUMMARY_STATUS_DF810B_KRN2).Value[0] = dsss;
                                    return DoPart_E(database, qManager, cardQManager);
                                    #endregion
                                }
                            }
                        }
                    }
                }
            }
        }
        private static SignalsEnum DoNOCDAPart_B(Kernel2Database database, KernelQ qManager, PublicKeyCertificateManager publicKeyCertificateManager, CardQ cardQManager, CardResponse cardResponse)
        {
            #region 11.70
            if (!database.IsPresent(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag))
            #endregion
            {
                #region 11.71
                return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                #endregion
            }
            else
            {
                #region 11.72
                REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcp = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
                if ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x00)
                #endregion
                {
                    #region 11.73
                    IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);
                    if (ids.Value.IsRead)
                    #endregion
                    {
                        #region 11.77
                        return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, L2Enum.CARD_DATA_ERROR, L3Enum.NOT_SET);
                        #endregion
                    }
                    else
                    {
                        #region 11.75
                        if (rcp.Value.ACTypeEnum == ACTypeEnum.AAC)
                        #endregion
                        {
                            #region 11.76
                            if (rcp.Value.CDASignatureRequested)
                            #endregion
                            {
                                #region 11.77
                                return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, L2Enum.CARD_DATA_ERROR, L3Enum.NOT_SET);
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
                    #region 11.74
                    if (rcp.Value.CDASignatureRequested)
                    #endregion
                    {
                        #region 11.77
                        return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, L2Enum.CARD_DATA_ERROR, L3Enum.NOT_SET);
                        #endregion
                    }
                    else
                    {
                        #region 11.78
                        TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                        if (tvr.Value.RelayResistancePerformedEnum == RelayResistancePerformedEnum.RRP_PERFORMED)
                        #endregion
                        {
                            #region 11.79
                            Do11_79(database);
                            #endregion
                        }
                        return DoPart_E(database, qManager, cardQManager);
                    }
                }
            }
        }
        private static void Do11_79(Kernel2Database database)
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
        private static SignalsEnum DoInvalidResponsePart_C(Kernel2Database database, KernelQ qManager, CardResponse cardResponse, KernelMessageidentifierEnum message, L1Enum l1Enum, L2Enum l2Enum, L3Enum l3Enum)
        {
            #region 11.90
            //done in 11.95
            #endregion

            #region 11.91
            IDS_STATUS_DF8128_KRN2.IDS_STATUS_DF8128_KRN2_VALUE idsV = new IDS_STATUS_DF8128_KRN2.IDS_STATUS_DF8128_KRN2_VALUE(EMVTagsEnum.IDS_STATUS_DF8128_KRN2.DataFormatter);
            idsV.Deserialize(database.TornTempRecord.Children.Get(EMVTagsEnum.IDS_STATUS_DF8128_KRN2.Tag).Value, 0);
            if (!idsV.IsWrite)
            #endregion
            {
                #region 11.92
                database.Get(EMVTagsEnum.TORN_RECORD_FF8101_KRN2).Value = database.TornTempRecord.Value;
                #endregion
            }

            #region 11.93
            IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);

            CommonRoutines.CreateEMVDiscretionaryData(database);
            if (ids.Value.IsWrite)
            #endregion
            {
                #region 11.94
                CommonRoutines.CreateEMVDataRecord(database);
                #endregion
            }
           
            #region 11.95
            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                KernelStatusEnum.NOT_READY,
                null,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true,
                message,
                l1Enum,
                cardResponse.ApduResponse.SW12,
                l2Enum,
                l3Enum);
            #endregion
          
        }
        private static SignalsEnum DoInvalidResponsePart_D(Kernel2Database database, KernelQ qManager)
        {
            #region 11.101 11.102
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
                L2Enum.NOT_SET,
                L3Enum.NOT_SET);
            #endregion
        }
        private static SignalsEnum DoPart_E(Kernel2Database database, KernelQ qManager,CardQ cardQManager)
        {
            #region 11.110
            CommonRoutines.CreateEMVDataRecord(database);
            #endregion

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

            #region 11.111
            if (database.IsNotEmpty(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2.Tag) &&
                ((int)Formatting.ConvertToInt32(database.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2).Value) & 0x00030F) != 0x000000)
            #endregion
            {
                #region 11.112
                k2OutcomeStatus = Kernel2OutcomeStatusEnum.END_APPLICATION;
                k2StartStatus = Kernel2StartEnum.B;
                #endregion

                #region 11.113
                PHONE_MESSAGE_TABLE_DF8131_KRN2 pmt = (PHONE_MESSAGE_TABLE_DF8131_KRN2)database.GetDefault(EMVTagsEnum.PHONE_MESSAGE_TABLE_DF8131_KRN2);
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
                #region 11.114
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

                #region 11.115
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

            #region 11.116
            if (database.IsNotEmpty(EMVTagsEnum.TAGS_TO_WRITE_AFTER_GEN_AC_FF8103_KRN2.Tag))
            #endregion
            {
                #region 11.117
                TLV tagToPut = database.TagsToWriteAfterGenACYet.GetFirstAndRemoveFromList();
                EMVPutDataRequest request = new EMVPutDataRequest(tagToPut);
                #endregion
                #region 11.118
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                return SignalsEnum.WAITING_FOR_PUT_DATA_RESPONSE_AFTER_GEN_AC;
                #endregion
            }

            CommonRoutines.CreateEMVDiscretionaryData(database);
            #region 11.118.1
            if (database.IsNotEmpty(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2.Tag) &&
                ((int)Formatting.ConvertToInt32(database.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2).Value) & 0x00030F) != 0x000000)
            #endregion
            {
                #region 11.119
                CommonRoutines.PostUIOnly(database, qManager, k2MessageIdentifier, k2Status, true);
                #endregion

                #region 11.120
                uiRequestOnOutcomePresent = false;
                k2Status = KernelStatusEnum.READY_TO_READ;
                holdTime = new byte[] { 0x00, 0x00, 0x00 };
                #endregion
            }
            else
            {
                #region 11.121
                uiRequestOnOutcomePresent = true;
                #endregion
            }

            return CommonRoutines.PostOutcome(database, qManager,
                k2MessageIdentifier, k2Status, holdTime, k2OutcomeStatus, k2StartStatus, uiRequestOnOutcomePresent,
                KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, null, L2Enum.NOT_SET, L3Enum.NOT_SET, valueQualifierEnum, valueQualifier, currencyCode, false, KernelCVMEnum.N_A);
        }
        private static SignalsEnum DoPart_F(Kernel2Database database, KernelQ qManager, CardResponse cardResponse)
        {
            #region 11.46
            //see below
            #endregion

            #region 11.46.1
            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
            tvr.Value.CDAFailed = true;
            tvr.UpdateDB();
            #endregion

            return DoInvalidResponsePart_C(database, qManager, cardResponse, KernelMessageidentifierEnum.N_A, L1Enum.NOT_SET, L2Enum.CAM_FAILED, L3Enum.NOT_SET);
        }
        
        /*
        * 11.1
        */
        private static SignalsEnum EntryPointL1RSP(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, TornTransactionLogManager tornTransactionLogManager)
        {
            #region 11.11
            IDS_STATUS_DF8128_KRN2.IDS_STATUS_DF8128_KRN2_VALUE idsV = new IDS_STATUS_DF8128_KRN2.IDS_STATUS_DF8128_KRN2_VALUE(EMVTagsEnum.IDS_STATUS_DF8128_KRN2.DataFormatter);
            idsV.Deserialize(database.TornTempRecord.Children.Get(EMVTagsEnum.IDS_STATUS_DF8128_KRN2.Tag).Value, 0);
            if (!idsV.IsWrite)
            #endregion
            {
                #region 11.12
                tornTransactionLogManager.TornTransactionLogs.RemoveFromList(database.Get(EMVTagsEnum.TORN_RECORD_FF8101_KRN2));
                #endregion
            }

            #region 11.13
            database.TornTempRecord = new TORN_RECORD_FF8101_KRN2(database);
            database.TornTempRecord.Initialize();
            database.TornTempRecord.AddTornTransactionLog(database);
            #endregion


            #region 11.15
            tornTransactionLogManager.AddTornTransactionLog(database);
            #endregion

            #region 11.16 - 11.17
            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.TRY_AGAIN,
                KernelStatusEnum.READY_TO_READ,
                new byte[] { 0x00, 0x00, 0x00 },
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.B,
                false,
                KernelMessageidentifierEnum.TRY_AGAIN,
                L1Enum.NOT_SET,
                cardResponse.ApduResponse.SW12,
                L2Enum.STATUS_BYTES,
                L3Enum.NOT_SET);
            #endregion
        }
        /*
        * 11.4
        */
        private static SignalsEnum EntryPointDET(Kernel2Database database, KernelRequest kernel1Request)
        {
            return SignalsEnum.WAITING_FOR_GEN_AC_2;
        }
        /*
         * 11.3
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager)
        {
            return SignalsEnum.WAITING_FOR_GEN_AC_2;
        }

        private static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }

        private static void AddSDADDataToDatabase(Kernel2Database database, ICCDynamicData iccdd)
        {
            TLV iccdn = database.Get(EMVTagsEnum.ICC_DYNAMIC_NUMBER_9F4C_KRN);
            TLV ac = database.Get(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN);

            if (iccdn == null)
                iccdn = TLV.Create(EMVTagsEnum.ICC_DYNAMIC_NUMBER_9F4C_KRN.Tag);

            if (ac == null)
                ac = TLV.Create(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag);

            iccdn.Value = iccdd.ICCDynamicNumber;
            ac.Value = iccdd.ApplicationCryptogram;

            database.AddToList(iccdn);
            database.AddToList(ac);
        }

        private static bool VerifySDAD_43(Kernel2Database database, CAPublicKeyCertificate capk, CardResponse cardResponse)
        {
            ICCDynamicData iccdd = VerifySAD.VerifySDAD(ICCDynamicDataType.NO_IDS_OR_RRP, true, database, database.StaticDataToBeAuthenticated, capk, cardResponse);
            if (iccdd == null) return false;

            AddSDADDataToDatabase(database, iccdd);
            return true;
        }
        private static bool VerifySDAD_CheckRelayData_43_1(Kernel2Database database, CAPublicKeyCertificate capk, CardResponse cardResponse)
        {
            ICCDynamicData iccdd = VerifySAD.VerifySDAD(ICCDynamicDataType.RRP, true, database, database.StaticDataToBeAuthenticated, capk, cardResponse);
            if (iccdd == null) return false;

            AddSDADDataToDatabase(database, iccdd);

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
        private static bool VerifySDAD_Summaries_42(Kernel2Database database, CAPublicKeyCertificate capk, CardResponse cardResponse)
        {
            ICCDynamicData iccdd = VerifySAD.VerifySDAD(ICCDynamicDataType.IDS, true, database, database.StaticDataToBeAuthenticated, capk, cardResponse);
            if (iccdd == null) return false;

            AddSDADDataToDatabase(database, iccdd);

            TLV ds2 = database.Get(EMVTagsEnum.DS_SUMMARY_2_DF8101_KRN2);
            TLV ds3 = database.Get(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2);

            if (ds2 == null && iccdd.DSSummary2 != null)
                ds2 = TLV.Create(EMVTagsEnum.DS_SUMMARY_2_DF8101_KRN2.Tag);

            if (ds3 == null && iccdd.DSSummary3 != null)
                ds3 = TLV.Create(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2.Tag);

            if (iccdd.DSSummary2 != null) ds2.Value = iccdd.DSSummary2;
            if (iccdd.DSSummary3 != null) ds3.Value = iccdd.DSSummary3;

            return true;
        }
        private static bool VerifySDAD_Summaries_CheckRelayData_42_1(Kernel2Database database, CAPublicKeyCertificate capk, CardResponse cardResponse)
        {
            ICCDynamicData iccdd = VerifySAD.VerifySDAD(ICCDynamicDataType.IDS_AND_RRP, true, database, database.StaticDataToBeAuthenticated, capk, cardResponse);
            if (iccdd == null) return false;

            AddSDADDataToDatabase(database, iccdd);

            TLV ds2 = database.Get(EMVTagsEnum.DS_SUMMARY_2_DF8101_KRN2);
            TLV ds3 = database.Get(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2);

            if (ds2 == null && iccdd.DSSummary2 != null)
                ds2 = TLV.Create(EMVTagsEnum.DS_SUMMARY_2_DF8101_KRN2.Tag);

            if (ds3 == null && iccdd.DSSummary3 != null)
                ds3 = TLV.Create(EMVTagsEnum.DS_SUMMARY_3_DF8102_KRN2.Tag);

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

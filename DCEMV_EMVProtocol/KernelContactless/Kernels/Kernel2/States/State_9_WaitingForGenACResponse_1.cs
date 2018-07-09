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
using DCEMV.TLVProtocol;
using System;
using System.Diagnostics;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_9_WaitingForGenACResponse_1
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_9_WaitingForGenACResponse_1:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
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
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_9_WaitingForGenACResponse_1:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }
        /*
        * S9.2
        */
        private static SignalsEnum EntryPointRA(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, TornTransactionLogManager tornTransactionLogManager, Stopwatch sw, PublicKeyCertificateManager publicKeyCertificateManager)
        {
            if (!cardResponse.ApduResponse.Succeeded)
            {
                #region 9.17
                return State_9_10_CommonProcessing.DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.STATUS_BYTES, L3Enum.NOT_SET);
                #endregion
            }

            #region 9.18
            bool parsingResult = false;
            if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x77)
            #endregion
            {
                EMVGenerateACResponse response = cardResponse.ApduResponse as EMVGenerateACResponse;
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
            

            #region 9.20
            if (!parsingResult)
            #endregion
            {
                return State_9_10_CommonProcessing.DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.PARSING_ERROR, L3Enum.NOT_SET);
            }

            #region 9.21
            if (!(database.IsNotEmpty(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag) &&
                database.IsNotEmpty(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag)))
            #endregion
            {
                #region 9.22
                return State_9_10_CommonProcessing.DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                #endregion

            }

            #region 9.23
            REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcp = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
            if (
                ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x40 && rcp.Value.ACTypeEnum == ACTypeEnum.TC) || 
                ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x80 && (rcp.Value.ACTypeEnum == ACTypeEnum.TC || rcp.Value.ACTypeEnum == ACTypeEnum.ARQC)) ||
                ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x00)
                )
            #endregion
            {
                #region 9.25
                SignalsEnum result = PostGenACBalanceReading_7_3.PostGenACBalanceReading(database, qManager, cardQManager);
                if (result != SignalsEnum.NONE)
                    return result;
                #endregion

                #region 9.26
                if (!database.IsNotEmptyList(EMVTagsEnum.TAGS_TO_WRITE_AFTER_GEN_AC_FF8103_KRN2.Tag))
                #endregion
                {
                    #region 9.27
                    CommonRoutines.PostUIOnly(database, qManager, KernelMessageidentifierEnum.CLEAR_DISPLAY, KernelStatusEnum.CARD_READ_SUCCESSFULLY, true);
                    #endregion
                }

                #region 9.28
                if (database.IsNotEmpty(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag))
                #endregion
                {
                    return State_9_10_CommonProcessing.DoCDA9_10_1(database, qManager, publicKeyCertificateManager, cardQManager, cardResponse);
                }
                else
                {
                    return State_9_10_CommonProcessing.DoNOCDA9_10_30(database, qManager, publicKeyCertificateManager, cardQManager, cardResponse);
                }
            }
            else
            {
                #region 9.24
                return State_9_10_CommonProcessing.DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                #endregion
            }
        }
        /*
         * S9.1
         */
        private static SignalsEnum EntryPointL1RSP(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, TornTransactionLogManager tornTransactionLogManager)
        {
            int mnttl = (int)Formatting.ConvertToInt32(database.GetDefault(EMVTagsEnum.MAX_NUMBER_OF_TORN_TRANSACTION_LOG_RECORDS_DF811D_KRN2).Value);
            #region 9.5
            if(!(mnttl > 0 && database.IsNotEmpty(EMVTagsEnum.DRDOL_9F51_KRN2.Tag)))
            #endregion
            {
                #region 9.6
                IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);
                if (ids.Value.IsWrite)
                #endregion
                {
                    #region 9.7 - 9.8
                    CommonRoutines.CreateEMVDiscretionaryData(database);
                    CommonRoutines.CreateEMVDataRecord(database);
                    return CommonRoutines.PostOutcome(database, qManager,
                        KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                        KernelStatusEnum.NOT_READY,
                        null,
                        Kernel2OutcomeStatusEnum.END_APPLICATION,
                        Kernel2StartEnum.N_A,
                        true,
                        KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                        cardResponse.L1Enum,
                        null,
                        L2Enum.NOT_SET,
                        L3Enum.NOT_SET);
                    #endregion
                }
                else
                {
                    #region 9.9 - 9.10
                    CommonRoutines.CreateEMVDiscretionaryData(database);
                    return CommonRoutines.PostOutcome(database, qManager,
                        KernelMessageidentifierEnum.TRY_AGAIN,
                        KernelStatusEnum.READY_TO_READ,
                        new byte[] { 0x00, 0x00, 0x00 },
                        Kernel2OutcomeStatusEnum.END_APPLICATION,
                        Kernel2StartEnum.B,
                        false,
                        KernelMessageidentifierEnum.TRY_AGAIN,
                        cardResponse.L1Enum,
                        null,
                        L2Enum.NOT_SET,
                        L3Enum.NOT_SET);
                    #endregion
                }
                
            }

            #region 9.11
            database.TornTempRecord = new TORN_RECORD_FF8101_KRN2(database);
            database.TornTempRecord.Initialize();
            database.TornTempRecord.AddTornTransactionLog(database);
            #endregion

            #region 9.13
            tornTransactionLogManager.AddTornTransactionLog(database);
            #endregion

            {
                #region 9.14 - 9.15
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcome(database, qManager,
                    KernelMessageidentifierEnum.TRY_AGAIN,
                    KernelStatusEnum.READY_TO_READ,
                    new byte[] { 0x00, 0x00, 0x00 },
                    Kernel2OutcomeStatusEnum.END_APPLICATION,
                    Kernel2StartEnum.B,
                    false,
                    KernelMessageidentifierEnum.TRY_AGAIN,
                    cardResponse.L1Enum,
                    null,
                    L2Enum.STATUS_BYTES,
                    L3Enum.NOT_SET);
                #endregion
            }

        }

        
        
        /*
        * S9.4
        */
        private static SignalsEnum EntryPointDET(Kernel2Database database, KernelRequest kernel1Request)
        {
            return SignalsEnum.WAITING_FOR_GEN_AC_1;
        }
        /*
         * S9.3
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager)
        {
            return SignalsEnum.WAITING_FOR_GEN_AC_1;
        }

        private static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }
}

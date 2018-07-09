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
using DCEMV.ISO7816Protocol;
using System;
using System.Diagnostics;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_10_WaitingForRecoverACResponse
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_10_WaitingForRecoverACResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
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
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_10_WaitingForRecoverACResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }
        /*
        * S9.2
        */
        private static SignalsEnum EntryPointRA(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, TornTransactionLogManager tornTransactionLogManager, Stopwatch sw, PublicKeyCertificateManager publicKeyCertificateManager)
        {
            #region 10.7
            if (!cardResponse.ApduResponse.Succeeded)
            #endregion
            {
                #region 10.8
                EMVGenerateACRequest request = PrepareGenACCommandProcedure_7_6.PrepareGenACCommand(database, qManager, cardQManager);
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                return SignalsEnum.WAITING_FOR_GEN_AC_2;
                #endregion
            }

            #region 10.10
            tornTransactionLogManager.TornTransactionLogs.RemoveFromList(database.Get(EMVTagsEnum.TORN_RECORD_FF8101_KRN2));
            #endregion

            #region 10.11
            foreach(TLV tlv in database.TornTempRecord.Children)
                database.AddToList(tlv);
            #endregion

            #region 10.12
            bool parsingResult = false;
            if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x77)
            #endregion
            {
                EMVGetProcessingOptionsResponse response = cardResponse.ApduResponse as EMVGetProcessingOptionsResponse;
                parsingResult = database.ParseAndStoreCardResponse(response.ResponseData);
            }

            if (!parsingResult)
            {
                #region 10.14
                return State_9_10_CommonProcessing.DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.PARSING_ERROR, L3Enum.NOT_SET);
                #endregion
            }
            else
            {
                #region 10.13
                if (!(database.IsNotEmpty(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag) &&
                    database.IsNotEmpty(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag)))
                #endregion
                {
                    #region 10.16
                    return State_9_10_CommonProcessing.DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                    #endregion
                }
                else
                {
                    #region 10.17
                    REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcp = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
                    if (
                        ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x40 && rcp.Value.ACTypeEnum == ACTypeEnum.TC) &&
                        (((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x80) &&
                        (rcp.Value.ACTypeEnum == ACTypeEnum.TC || rcp.Value.ACTypeEnum == ACTypeEnum.ARQC)) ||
                        (database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x00
                        )
                    #endregion
                    {
                        #region 10.18
                        return State_9_10_CommonProcessing.DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_ERROR, L3Enum.NOT_SET);
                        #endregion
                    }
                    else
                    {
                        #region 10.19
                        SignalsEnum result = PostGenACBalanceReading_7_3.PostGenACBalanceReading(database, qManager, cardQManager);
                        if (result != SignalsEnum.NONE)
                            return result;
                        #endregion

                        #region 10.20
                        if (!database.IsNotEmptyList(EMVTagsEnum.TAGS_TO_WRITE_AFTER_GEN_AC_FF8103_KRN2.Tag))
                        #endregion
                        {
                            #region 10.21
                            CommonRoutines.PostUIOnly(database,qManager, KernelMessageidentifierEnum.CLEAR_DISPLAY, KernelStatusEnum.CARD_READ_SUCCESSFULLY, true);
                            #endregion
                            
                        }
                        #region 10.22
                        if (database.IsNotEmpty(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag))
                        #endregion
                        {
                            return State_9_10_CommonProcessing.DoCDA9_10_1(database,qManager,publicKeyCertificateManager,cardQManager,cardResponse);
                        }
                        else
                        {
                            return State_9_10_CommonProcessing.DoNOCDA9_10_30(database, qManager, publicKeyCertificateManager, cardQManager, cardResponse);
                        }
                    }
                }
            }
        }
        /*
        * 10.1, 10.6
        */
        private static SignalsEnum EntryPointL1RSP(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, TornTransactionLogManager tornTransactionLogManager)
        {
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
        }
        /*
        * 10.4
        */
        private static SignalsEnum EntryPointDET(Kernel2Database database, KernelRequest kernel1Request)
        {
            return SignalsEnum.WAITING_FOR_RECOVER_AC;
        }
        /*
         * 10.3
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager)
        {
            return SignalsEnum.WAITING_FOR_RECOVER_AC;
        }

        private static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }
}

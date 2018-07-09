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

namespace DCEMV.EMVProtocol.Kernels.K1
{
    public static class State_101_WaitingForInternalAuthenticate
    {
        public static SignalsEnum Execute(
            Kernel1Database database, 
            KernelQ qManager, 
            CardQ cardQManager,
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_101_WaitingForInternalAuthenticate:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse, qManager, cardQManager, sw, publicKeyCertificateManager);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_101_WaitingForInternalAuthenticate:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }

        private static SignalsEnum EntryPointRA(Kernel1Database database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, Stopwatch sw, PublicKeyCertificateManager publicKeyCertificateManager)
        {
            if (!cardResponse.ApduResponse.Succeeded)
            {
                return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.N_A,
                KernelStatusEnum.N_A,
                null,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                L1Enum.NOT_SET,
                cardResponse.ApduResponse.SW12,
                L2Enum.STATUS_BYTES,
                L3Enum.NOT_SET);
            }

            bool parsingResult = false;
            EMVInternalAuthenticateResponse response = cardResponse.ApduResponse as EMVInternalAuthenticateResponse;
            if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x77)
            {
                parsingResult = database.ParseAndStoreCardResponse(response.ResponseData);
            }
            else
            {
                database.AddToList(response.GetTLVSignedApplicationData());
            }

            if (!parsingResult)
            {
                return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.N_A,
                KernelStatusEnum.N_A,
                null,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                L1Enum.NOT_SET,
                null,
                L2Enum.PARSING_ERROR,
                L3Enum.NOT_SET);
            }

            if (!(database.IsNotEmpty(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag) &&
                database.IsNotEmpty(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag)))
            {
                return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.N_A,
                KernelStatusEnum.N_A,
                null,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                L1Enum.NOT_SET,
                null,
                L2Enum.CARD_DATA_MISSING,
                L3Enum.NOT_SET);
            }
            
            TLV cidTLV = database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN);
            byte cid = cidTLV.Value[0];
            cid = (byte)(cid >> 6);
            if (cid == (byte)ACTypeEnum.ARQC)
            {
                #region 3.10.3.1
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
                #endregion
            }

            #region 3.6.1.1
            CommonRoutines.PostUIOnly(database, qManager, KernelMessageidentifierEnum.CLEAR_DISPLAY, KernelStatusEnum.CARD_READ_SUCCESSFULLY, true);
            #endregion


            #region 3.8.1.1
            bool ddaPassed = K3.State_3_4_CommonProcessing.DoOfflineAuth(database, qManager, publicKeyCertificateManager);
            #endregion
            if (!ddaPassed)
            {
                #region 3.10.3.1
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
                #endregion
            }

            if (cid == (byte)ACTypeEnum.TC)
            {
                #region 3.9.2
                CommonRoutines.CreateEMVDataRecord(database);
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcome(database, qManager,
                                 KernelMessageidentifierEnum.APPROVED,
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
                                 ValueQualifierEnum.NONE,
                                 null,
                                 null,
                                 false,
                                 KernelCVMEnum.N_A);
                #endregion
            }

            //declined
            CommonRoutines.CreateEMVDiscretionaryData(database);
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
                                ValueQualifierEnum.NONE,
                                null,
                                null,
                                false,
                                KernelCVMEnum.N_A);
        }
        

        private static SignalsEnum EntryPointL1RSP(Kernel1Database database, CardResponse cardResponse, KernelQ qManager)
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
                L2Enum.STATUS_BYTES,
                L3Enum.NOT_SET);
        }
        
        private static SignalsEnum EntryPointDET(Kernel1Database database, KernelRequest kernel1Request)
        {
            return SignalsEnum.WAITING_FOR_INTERNAL_AUTHENTICATE;
        }
        private static SignalsEnum EntryPointSTOP(Kernel1Database database, KernelQ qManager)
        {
            return SignalsEnum.WAITING_FOR_INTERNAL_AUTHENTICATE;
        }

        private static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }
}

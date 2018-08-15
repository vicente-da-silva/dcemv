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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K3
{
    public static class State_3_WaitingForGPOResponse
    {
        public static SignalsEnum Execute(
            Kernel3Database database, 
            KernelQ qManager, 
            CardQ cardQManager,
            Stopwatch sw,
            PublicKeyCertificateManager publicKeyCertificateManager,
            CardExceptionManager cardExceptionManager)
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_3_WaitingForGPOResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse, qManager, cardQManager, sw, publicKeyCertificateManager, cardExceptionManager);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_3_WaitingForGPOResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }
        private static SignalsEnum EntryPointRA(Kernel3Database database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, Stopwatch sw, PublicKeyCertificateManager publicKeyCertificateManager, CardExceptionManager cardExceptionManager)
        {
            #region 5.2.2.2
            if (!cardResponse.ApduResponse.Succeeded)
            {
                TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN ttq = new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN(database);
                if (cardResponse.ApduResponse.SW1 == 0x69 && cardResponse.ApduResponse.SW2 == 0x84 && ttq.Value.EMVModeSupported)
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
                         cardResponse.ApduResponse.SW12,
                         L2Enum.STATUS_BYTES,
                         L3Enum.NOT_SET);
                }
                if (cardResponse.ApduResponse.SW1 == 0x69 && cardResponse.ApduResponse.SW2 == 0x85)
                {
                    return CommonRoutines.PostOutcome(database, qManager,
                         KernelMessageidentifierEnum.N_A,
                         KernelStatusEnum.N_A,
                         null,
                         Kernel2OutcomeStatusEnum.SELECT_NEXT,
                         Kernel2StartEnum.C,
                         null,
                         KernelMessageidentifierEnum.N_A,
                         L1Enum.NOT_SET, 
                         null,
                         L2Enum.NOT_SET,
                         L3Enum.NOT_SET);
                }
                if (cardResponse.ApduResponse.SW1 == 0x69 && cardResponse.ApduResponse.SW2 == 0x86 && ttq.Value.EMVModeSupported)
                {
                    return CommonRoutines.PostOutcome(database, qManager,
                         KernelMessageidentifierEnum.SEE_PHONE,
                         KernelStatusEnum.READY_TO_READ,
                         new byte[] { 0x00, 0x00, 0x0D },
                         Kernel2OutcomeStatusEnum.END_APPLICATION,
                         Kernel2StartEnum.B,
                         true,
                         KernelMessageidentifierEnum.SEE_PHONE,
                         L1Enum.NOT_SET,
                         cardResponse.ApduResponse.SW12,
                         L2Enum.STATUS_BYTES,
                         L3Enum.NOT_SET);
                }

                return CommonRoutines.PostOutcome(database, qManager,
                        KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                        KernelStatusEnum.PROCESSING_ERROR,
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
            #endregion

            bool parsingResult = false;
            if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x77)
            {
                EMVGetProcessingOptionsResponse response = cardResponse.ApduResponse as EMVGetProcessingOptionsResponse;
                parsingResult = database.ParseAndStoreCardResponse(response.ResponseData);
            }
            else
            {
                if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x80)
                {
                    EMVGetProcessingOptionsResponse response = cardResponse.ApduResponse as EMVGetProcessingOptionsResponse;
                    if (cardResponse.ApduResponse.ResponseData.Length < 6 || 
                        ((cardResponse.ApduResponse.ResponseData.Length - 2) % 4 != 0) ||
                            database.IsNotEmpty(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag) ||
                            database.IsNotEmpty(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN.Tag))
                    {
                        parsingResult = false;
                    }
                    else
                    {
                        database.AddToList(TLV.Create(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag, response.GetResponseTags().Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag).Value));
                        database.AddToList(TLV.Create(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN.Tag, response.GetResponseTags().Get(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN.Tag).Value));
                        parsingResult = true;
                    }
                }
            }

            if (!parsingResult)
            {
                return K1K3.State_3_R1_CommonProcessing.DoInvalidReponse(database, qManager, L1Enum.NOT_SET, L2Enum.PARSING_ERROR, L3Enum.NOT_SET);
            }
            else
            {
                #region 5.4.1.1
                CommonRoutines.PostUIOnly(database, qManager, KernelMessageidentifierEnum.CLEAR_DISPLAY, KernelStatusEnum.CARD_READ_SUCCESSFULLY, true);
                #endregion

                if (!(database.IsNotEmpty(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag)) &&
                        database.IsNotEmpty(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN.Tag))
                {
                    return K1K3.State_3_R1_CommonProcessing.DoInvalidReponse(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                }
                else
                {
                    TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN ttq = new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN(database);
                    if (ttq.Value.MagStripeModeSupported && !ttq.Value.EMVModeSupported)
                    {
                        return K1K3.State_3_R1_CommonProcessing.DoInvalidReponse(database, qManager, L1Enum.NOT_SET, L2Enum.MAGSTRIPE_NOT_SUPPORTED, L3Enum.NOT_SET);
                    }
                    else
                    {
                        if (!ttq.Value.MagStripeModeSupported && ttq.Value.EMVModeSupported)
                        {
                            return DoEMVMode(database, qManager, cardQManager, cardResponse, sw, publicKeyCertificateManager, cardExceptionManager);
                        }
                        else
                        {
                            return K1K3.State_3_R1_CommonProcessing.DoInvalidReponse(database, qManager, L1Enum.NOT_SET, L2Enum.MAGSTRIPE_NOT_SUPPORTED, L3Enum.NOT_SET); 
                        }
                    }
                }
            }
        }

        private static SignalsEnum DoEMVMode(Kernel3Database database, KernelQ qManager, CardQ cardQManager, CardResponse cardResponse, Stopwatch sw, PublicKeyCertificateManager publicKeyCertificateManager, CardExceptionManager cardExceptionManager)
        {
            TLV aflRaw = database.Get(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN);
            if (aflRaw != null)
            {
                List<byte[]> bytes = new List<byte[]>
                {
                    new byte[] { (byte)aflRaw.Value.Length },
                    aflRaw.Value
                };
                database.ActiveAFL.Value.Deserialize(bytes.SelectMany(a => a).ToArray(), 0);
                return K1K3.State_3_R1_CommonProcessing.DoCommonProcessing("State_3_WaitingForGPOResponse", database, qManager, cardQManager, cardResponse);
            }
            else
            {
                database.NextCommandEnum = NextCommandEnum.NONE;
                return State_3_4_CommonProcessing.DoCommonProcessing("State_3_WaitingForGPOResponse", database, qManager, cardQManager, sw, publicKeyCertificateManager, cardExceptionManager);
            }

        }

        private static SignalsEnum EntryPointDET(Kernel3Database database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_GPO_REPONSE;
        }

        private static SignalsEnum EntryPointL1RSP(Kernel3Database database, CardResponse cardResponse, KernelQ qManager)
        {
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.TRY_AGAIN, Kernel2StartEnum.B, cardResponse.L1Enum, L2Enum.NOT_SET, L3Enum.NOT_SET);
        }

        private static SignalsEnum EntryPointSTOP(Kernel3Database database, KernelQ qManager)
        {
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
        }

        private static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }
}

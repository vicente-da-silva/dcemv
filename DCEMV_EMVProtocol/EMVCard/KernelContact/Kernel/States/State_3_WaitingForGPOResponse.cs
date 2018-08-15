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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K
{
    public static class State_3_WaitingForGPOResponse
    {
        public static SignalsEnum Execute(
            KernelDatabase database,
            KernelQ qManager,
            CardQ cardQManager,
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_3_WaitingForGPOResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse, qManager, cardQManager, sw);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_3_WaitingForGPOResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }
      
        private static SignalsEnum EntryPointRA(KernelDatabase database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, Stopwatch sw)
        {
            if (!cardResponse.ApduResponse.Succeeded)
            {
                return CommonRoutines.PostOutcome(database, qManager,
                    KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                    KernelStatusEnum.NOT_READY,
                    null,
                    Kernel2OutcomeStatusEnum.SELECT_NEXT,
                    Kernel2StartEnum.C,
                    null,
                    KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                    L1Enum.NOT_SET, cardResponse.ApduResponse.SW12,
                    L2Enum.STATUS_BYTES,
                    L3Enum.NOT_SET);
            }

            bool parsingResult = false;
            EMVGetProcessingOptionsResponse response = cardResponse.ApduResponse as EMVGetProcessingOptionsResponse;
            if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x77)
            {
                parsingResult = database.ParseAndStoreCardResponse(response.ResponseData);
            }
            else
            {
                if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x80)
                {
                    //if (cardResponse.ApduResponse.ResponseData.Length < 6 ||
                    //    ((cardResponse.ApduResponse.ResponseData.Length - 2) % 4 != 0) ||
                    //        database.IsNotEmpty(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag) ||
                    //        database.IsNotEmpty(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN.Tag))
                    //{
                    //    parsingResult = false;
                    //}
                    //else
                    //{
                    foreach (TLV tlv in response.GetResponseTags())
                    {
                        parsingResult = database.ParseAndStoreCardResponse(tlv);
                        if (!parsingResult) break;
                    }
                    //}
                }
            }
            
            if (!parsingResult)
            {
                return DoInvalidReponse(database, qManager, L1Enum.NOT_SET, L2Enum.PARSING_ERROR, L3Enum.NOT_SET);
            }
            else
            {
                if (!(database.IsNotEmpty(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag)) &&
                        database.IsNotEmpty(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN.Tag))
                {
                    return DoInvalidReponse(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                }
                else
                {
                    return DoEMVMode(database, qManager, cardQManager, cardResponse, sw);
                }
            }
        }
        private static SignalsEnum DoEMVMode(KernelDatabase database, KernelQ qManager, CardQ cardQManager, CardResponse cardResponse, Stopwatch sw)
        {
            TLV aflRaw = database.Get(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN);
            byte[] kcValCheck = new byte[4];
            Array.Copy(aflRaw.Value, 0, kcValCheck, 0, kcValCheck.Length);
            //if (aflRaw.Value.Length >= 4 && Formatting.ByteArrayToHexString(kcValCheck) == "08010100")
            //{
            //    byte[] activeAFLBytes = new byte[aflRaw.Value.Length - 4];
            //    Array.Copy(aflRaw.Value, 4, activeAFLBytes, 0, activeAFLBytes.Length);

            //    List<byte[]> bytes = new List<byte[]>
            //    {
            //        new byte[] { (byte)activeAFLBytes.Length },
            //        activeAFLBytes
            //    };
            //    database.ActiveAFL.Value.Deserialize(bytes.SelectMany(a => a).ToArray(), 0);
            //}
            //else
            //{
                List<byte[]> bytes = new List<byte[]>
                {
                    new byte[] { (byte)aflRaw.Value.Length },
                    aflRaw.Value
                };
                database.ActiveAFL.Value.Deserialize(bytes.SelectMany(a => a).ToArray(), 0);
            //}
                
            return DoCommonProcessing("State_3_WaitingForGPOResponse", database, qManager, cardQManager, cardResponse);
        }
        
        private static SignalsEnum EntryPointDET(KernelDatabase database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_GPO_REPONSE;
        }
        private static SignalsEnum EntryPointL1RSP(KernelDatabase database, CardResponse cardResponse, KernelQ qManager)
        {
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.TRY_AGAIN, Kernel2StartEnum.B, cardResponse.L1Enum, L2Enum.NOT_SET, L3Enum.NOT_SET);
        }
        private static SignalsEnum EntryPointSTOP(KernelDatabase database, KernelQ qManager)
        {
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
        }

        private static SignalsEnum DoCommonProcessing(string source, KernelDatabase database, KernelQ qManager, CardQ cardQManager, CardResponse cardResponse)
        {
            TLV nextTLV = database.TagsToReadYet.GetNextGetDataTagFromList();
            if (nextTLV != null)
                database.ActiveTag = nextTLV.Tag.TagLable;
            else
                database.ActiveTag = null;

            if (database.ActiveTag != null)
            {
                EMVGetDataRequest request = new EMVGetDataRequest(Formatting.HexStringToByteArray(database.ActiveTag));
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                database.NextCommandEnum = NextCommandEnum.GET_DATA;
            }
            else
            {
                if (database.ActiveAFL == null)
                {
                    return DoInvalidReponse(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_ERROR, L3Enum.NOT_SET);
                }
                else
                {
                    EMVReadRecordRequest request = new EMVReadRecordRequest(database.ActiveAFL.Value.Entries[0].SFI, database.ActiveAFL.Value.Entries[0].FirstRecordNumber);
                    cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                    database.NextCommandEnum = NextCommandEnum.READ_RECORD;
                }
            }

            TLVList dataToSend = database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children;

            APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
            TERMINAL_CAPABILITIES_9F33_KRN tc = new TERMINAL_CAPABILITIES_9F33_KRN(database);

            return SignalsEnum.WAITING_FOR_EMV_READ_RECORD_RESPONSE;
        }

        private static SignalsEnum DoInvalidReponse(KernelDatabase database, KernelQ qManager, L1Enum l1Enum, L2Enum l2Enum, L3Enum l3Enum)
        {

            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                KernelStatusEnum.NOT_READY,
                null,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true, KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                l1Enum,
                null,
                l2Enum,
                l3Enum);

        }
    }
}

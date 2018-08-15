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
using DCEMV.EMVProtocol.Contact;
using DCEMV.ISO7816Protocol;
using System;
using System.Diagnostics;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K
{
    public static class State_10_WaitingForGenACResponse_2
    {
        public static SignalsEnum Execute(
            KernelDatabase database, 
            KernelQ qManager, 
            CardQ cardQManager,
            EMVSelectApplicationResponse emvSelectApplicationResponse,
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_10_WaitingForGenACResponse_2:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse, qManager, cardQManager,  sw, publicKeyCertificateManager, emvSelectApplicationResponse);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_10_WaitingForGenACResponse_2:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }
        private static SignalsEnum EntryPointRA(KernelDatabase database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, Stopwatch sw, PublicKeyCertificateManager publicKeyCertificateManager, EMVSelectApplicationResponse emvSelectApplicationResponse)
        {
            if (!cardResponse.ApduResponse.Succeeded)
            {
                return State_7_10_CommonProcessing.DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.STATUS_BYTES, L3Enum.NOT_SET);
            }

            SignalsEnum result = ProcessGenACCommand(database,cardResponse,qManager,cardQManager,sw,publicKeyCertificateManager,emvSelectApplicationResponse);

            TLV _72 = database.Get(EMVTagsEnum.ISSUER_SCRIPT_TEMPLATE_2_72_KRN);
            if (_72 != null)
            {
                database.ScriptsToRunAfterGenAC = CardActionAnalysis.BuildScriptList(_72);
                if (database.ScriptsToRunAfterGenAC.Count > 0)
                {
                    //post first script
                    TLV firstScript = database.ScriptsToRunAfterGenAC.GetFirstAndRemoveFromList();
                    EMVScriptCommandRequest scriptRequest = new EMVScriptCommandRequest();
                    scriptRequest.Deserialize(firstScript.Value);
                    cardQManager.EnqueueToInput(new CardRequest(scriptRequest, CardinterfaceServiceRequestEnum.ADPU));
                    return SignalsEnum.WAITING_FOR_SCRIPT_PROCESSING;
                }
            }

            return result;
        }

        private static SignalsEnum ProcessGenACCommand(KernelDatabase database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, Stopwatch sw, PublicKeyCertificateManager publicKeyCertificateManager, EMVSelectApplicationResponse emvSelectApplicationResponse)
        {
            //remove values stored during 1st gen ac
            database.RemoveFromList(database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag));
            database.RemoveFromList(database.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag));
            database.RemoveFromList(database.Get(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag));
            database.RemoveFromList(database.Get(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag));
            //will only exist if CDA was perfomred after 1st gen ac
            database.RemoveFromList(database.Get(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag));

            bool parsingResult = false;
            if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x77)
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

            if (!parsingResult)
            {
                return State_7_10_CommonProcessing.DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.PARSING_ERROR, L3Enum.NOT_SET);
            }

            if (!(database.IsNotEmpty(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag) &&
                database.IsNotEmpty(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag)))
            {
                return State_7_10_CommonProcessing.DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
            }

            APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
            TERMINAL_CAPABILITIES_9F33_KRN tc = new TERMINAL_CAPABILITIES_9F33_KRN(database);
            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);

            REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcp = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
            if (!
                (((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x40 && rcp.Value.ACTypeEnum == ACTypeEnum.TC) ||
                ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x80 && (rcp.Value.ACTypeEnum == ACTypeEnum.TC || rcp.Value.ACTypeEnum == ACTypeEnum.ARQC)) ||
                ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x00))
                )
            {
                return State_7_10_CommonProcessing.DoInvalidResponsePart_C(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
            }

            if (aip.Value.CDASupported && tc.Value.CDACapable)
            {
                string aid = emvSelectApplicationResponse.GetDFName();
                string rid = aid.Substring(0, 10);
                RIDEnum ridEnum = (RIDEnum)Enum.Parse(typeof(RIDEnum), rid);
                CAPublicKeyCertificate capk = database.PublicKeyCertificateManager.GetCAPK(ridEnum, database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN).Value[0]);

                if (capk == null)
                {
                    tvr.Value.CDAFailed = true;
                    tvr.UpdateDB();
                }

                if (database.IsNotEmpty(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag))
                {
                    State_7_10_CommonProcessing.DoCDA(database, qManager, capk, cardQManager, cardResponse, emvSelectApplicationResponse, false);
                }
                else
                {
                    tvr.Value.CDAFailed = true;
                    tvr.UpdateDB();
                }
            }
            return State_7_10_CommonProcessing.EndOnTCorAAC(database, qManager, cardQManager);
        }
        private static SignalsEnum EntryPointL1RSP(KernelDatabase database, CardResponse cardResponse, KernelQ qManager)
        {
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

        }
        private static SignalsEnum EntryPointDET(KernelDatabase database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_GEN_AC_2;
        }
        private static SignalsEnum EntryPointSTOP(KernelDatabase database, KernelQ qManager)
        {
            return SignalsEnum.WAITING_FOR_GEN_AC_2;
        }
    }
}

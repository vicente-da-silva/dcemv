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
    public static class State_11_WaitingForScriptProcessing
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_11_WaitingForScriptProcessing:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse, qManager, cardQManager, sw, publicKeyCertificateManager, emvSelectApplicationResponse);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_11_WaitingForScriptProcessing:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }
        private static SignalsEnum EntryPointRA(KernelDatabase database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, Stopwatch sw, PublicKeyCertificateManager publicKeyCertificateManager, EMVSelectApplicationResponse emvSelectApplicationResponse)
        {
            if(database.IsScriptProcessingBeforeGenACInProgress)
            {
                if (cardResponse.ApduResponse.SW1 != 0x90 && cardResponse.ApduResponse.SW1 != 0x62 && cardResponse.ApduResponse.SW1 != 0x63)
                {
                    TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                    tvr.Value.ScriptProcessingFailedBeforeFinalGENERATEAC = true;
                    tvr.UpdateDB();
                    database.IsScriptProcessingBeforeGenACInProgress = false;
                    return CardActionAnalysis.Initiate2ndCardActionAnalysis(database, qManager, cardQManager, emvSelectApplicationResponse);
                }

                if (database.ScriptsToRunBeforeGenAC.Count == 0)
                {
                    database.IsScriptProcessingBeforeGenACInProgress = false;
                    return CardActionAnalysis.Initiate2ndCardActionAnalysis(database, qManager, cardQManager, emvSelectApplicationResponse, true);
                }

                TLV firstScript = database.ScriptsToRunBeforeGenAC.GetFirstAndRemoveFromList();
                EMVScriptCommandRequest scriptRequest = new EMVScriptCommandRequest();
                scriptRequest.Deserialize(firstScript.Value);
                cardQManager.EnqueueToInput(new CardRequest(scriptRequest, CardinterfaceServiceRequestEnum.ADPU));
                return SignalsEnum.WAITING_FOR_SCRIPT_PROCESSING;
            }
            else
            {
                if (cardResponse.ApduResponse.SW1 != 0x90 && cardResponse.ApduResponse.SW1 != 0x62 && cardResponse.ApduResponse.SW1 != 0x63)
                {
                    TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                    tvr.Value.ScriptProcessingFailedAfterFinalGENERATEAC = true;
                    tvr.UpdateDB();
                    //update emv data to return with this updated value
                    if (database.Get(EMVTagsEnum.DATA_RECORD_FF8105_KRN2) != null)
                    {
                        database.Get(EMVTagsEnum.DATA_RECORD_FF8105_KRN2).Children.RemoveFromList(database.Get(EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN.Tag));
                        database.Get(EMVTagsEnum.DATA_RECORD_FF8105_KRN2).Children.AddToList(TLV.Create(EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN.Tag, database.Get(EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN).Value));
                    }
                    return SignalsEnum.STOP;
                }
                if (database.ScriptsToRunAfterGenAC.Count == 0)
                {
                    return SignalsEnum.STOP;
                }

                TLV firstScript = database.ScriptsToRunAfterGenAC.GetFirstAndRemoveFromList();
                EMVScriptCommandRequest scriptRequest = new EMVScriptCommandRequest();
                scriptRequest.Deserialize(firstScript.Value);
                cardQManager.EnqueueToInput(new CardRequest(scriptRequest, CardinterfaceServiceRequestEnum.ADPU));
                return SignalsEnum.WAITING_FOR_SCRIPT_PROCESSING;
            }
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
            return SignalsEnum.WAITING_FOR_SCRIPT_PROCESSING;
        }
        private static SignalsEnum EntryPointSTOP(KernelDatabase database, KernelQ qManager)
        {
            return SignalsEnum.WAITING_FOR_SCRIPT_PROCESSING;
        }
    }
}

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
using DCEMV.Shared;
using DCEMV.EMVProtocol.Contact;
using DCEMV.FormattingUtils;
using DCEMV.ISO7816Protocol;
using System;
using System.Diagnostics;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K
{
    public static class State_8_WaitingForOnlineAuth
    {
        public static Logger Logger = new Logger(typeof(State_8_WaitingForOnlineAuth));

        public static SignalsEnum Execute(
           KernelDatabase database,
           KernelQ qManager,
           CardQ cardQManager,
           EMVSelectApplicationResponse emvSelectApplicationResponse,
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

                    case KernelTerminalReaderServiceRequestEnum.ONLINE:
                        return EntryPointOnline(database, kernel1Request, qManager, cardQManager, emvSelectApplicationResponse);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_8_WaitingForOnlineAuth:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
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
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_8_WaitingForOnlineAuth:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }

       
        private static SignalsEnum EntryPointRA(KernelDatabase database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, Stopwatch sw)
        {
            //no signal should be received from card here 
            throw new EMVProtocolException("Invalid state in State_8_WaitingForOnlineAuth");
        }
        
        private static SignalsEnum EntryPointOnline(KernelDatabase database, KernelRequest kernel1Request, KernelQ qManager, CardQ cardQManager, EMVSelectApplicationResponse emvSelectApplicationResponse)
        {
            KernelOnlineRequest response = (KernelOnlineRequest)kernel1Request;
            //check if approved or not or unable to go online
            if (response.OnlineApprovalStatus == KernelOnlineResponseType.UnableToGoOnline)
            {
                //if unable to go online, check what action to take using TAC/IAC default, 
                TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                ulong tvrAsNumber = Formatting.ConvertToInt64(tvr.Value.Value);
                ulong tacAsNumber = Formatting.ConvertToInt64(database.Get(EMVTagsEnum.TERMINAL_ACTION_CODE_DEFAULT_DF8120_KRN2).Value);
                if (database.IsNotEmpty(EMVTagsEnum.ISSUER_ACTION_CODE_DEFAULT_9F0D_KRN.Tag))
                {
                    ulong iacAsNumber = Formatting.ConvertToInt64(database.Get(EMVTagsEnum.ISSUER_ACTION_CODE_DEFAULT_9F0D_KRN).Value);
                    if (!(
                        ((tacAsNumber | iacAsNumber) & tvrAsNumber) == 0)
                        )
                    {
                        database.ACType.Value.DSACTypeEnum = ACTypeEnum.AAC;
                    }
                    else
                        database.ACType.Value.DSACTypeEnum = ACTypeEnum.TC;
                }
                else
                {
                    if (!((tacAsNumber & tvrAsNumber) == 0))
                    {
                        database.ACType.Value.DSACTypeEnum = ACTypeEnum.AAC;
                    }
                    else
                        database.ACType.Value.DSACTypeEnum = ACTypeEnum.TC;
                }
                //what should these be set to in this case?
                database.AddToList(TLV.Create(EMVTagsEnum.ISSUER_AUTHENTICATION_DATA_91_KRN.Tag));
                database.AddToList(TLV.Create(EMVTagsEnum.AUTHORISATION_RESPONSE_CODE_8A_KRN.Tag));
            }
            else
            {
                if (response.OnlineApprovalStatus == KernelOnlineResponseType.Approve)
                {
                    database.ACType.Value.DSACTypeEnum = ACTypeEnum.TC;
                }
                else
                {
                    database.ACType.Value.DSACTypeEnum = ACTypeEnum.AAC;
                }
                database.AddToList(response.InputData.Get(EMVTagsEnum.ISSUER_AUTHENTICATION_DATA_91_KRN.Tag));
                database.AddToList(response.InputData.Get(EMVTagsEnum.AUTHORISATION_RESPONSE_CODE_8A_KRN.Tag));
                if(response.InputData.Get(EMVTagsEnum.ISSUER_SCRIPT_TEMPLATE_1_71_KRN.Tag)!=null)
                    database.AddToList(response.InputData.Get(EMVTagsEnum.ISSUER_SCRIPT_TEMPLATE_1_71_KRN.Tag));
                if (response.InputData.Get(EMVTagsEnum.ISSUER_SCRIPT_TEMPLATE_2_72_KRN.Tag) != null)
                    database.AddToList(response.InputData.Get(EMVTagsEnum.ISSUER_SCRIPT_TEMPLATE_2_72_KRN.Tag));

            }

            APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
            if (aip.Value.IssuerAuthenticationIsSupported && response.OnlineApprovalStatus != KernelOnlineResponseType.UnableToGoOnline)
            {
                TLV _91 = response.InputData.Get(EMVTagsEnum.ISSUER_AUTHENTICATION_DATA_91_KRN.Tag);
                EMVExternalAuthenticateRequest request = new EMVExternalAuthenticateRequest(_91.Value);
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                return SignalsEnum.WAITING_FOR_EXTERNAL_AUTHENTICATE;
            }
            else
            {
                //if scripts need to be run before gen ac, do now
                return CardActionAnalysis.Initiate2ndCardActionAnalysis(database, qManager, cardQManager, emvSelectApplicationResponse);
            }
        }

        private static SignalsEnum EntryPointDET(KernelDatabase database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_ONLINE_RESPONSE;
        }
       
        private static SignalsEnum EntryPointL1RSP(KernelDatabase database, CardResponse cardResponse, KernelQ qManager)
        {
            CommonRoutines.InitializeDiscretionaryData(database);
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
        }
       
        private static SignalsEnum EntryPointSTOP(KernelDatabase database, KernelQ qManager)
        {
            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
        }

       
    }
}

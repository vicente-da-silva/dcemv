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

namespace DCEMV.EMVProtocol.Kernels.K
{
    public static class State_9_WaitingForExternalAuthenticate
    {
        public static SignalsEnum Execute(
            KernelDatabase database, 
            KernelQ qManager, 
            CardQ cardQManager,
            PublicKeyCertificateManager publicKeyCertificateManager,
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

                    default:
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_9_WaitingForExternalAuthenticate:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
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
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_9_WaitingForExternalAuthenticate:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }

        private static SignalsEnum EntryPointRA(KernelDatabase database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, Stopwatch sw, PublicKeyCertificateManager publicKeyCertificateManager, EMVSelectApplicationResponse emvSelectApplicationResponse)
        {
            if (!cardResponse.ApduResponse.Succeeded)
            {
                TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                tvr.Value.IssuerAuthenticationFailed = true;
                tvr.UpdateDB();
            }
            else
            {
                TRANSACTION_STATUS_INFORMATION_9B_KRN tsi = new TRANSACTION_STATUS_INFORMATION_9B_KRN(database);
                tsi.Value.IssuerAuthenticationWasPerformed = true;
                tsi.UpdateDB();
            }

            //EMVExternalAuthenticateResponse response = cardResponse.ApduResponse as EMVExternalAuthenticateResponse;

            //if scripts need to be run before gen ac, do now
            return CardActionAnalysis.Initiate2ndCardActionAnalysis(database, qManager, cardQManager, emvSelectApplicationResponse);
        }
        

        private static SignalsEnum EntryPointL1RSP(KernelDatabase database, CardResponse cardResponse, KernelQ qManager)
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
        
        private static SignalsEnum EntryPointDET(KernelDatabase database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_EXTERNAL_AUTHENTICATE;
        }
        private static SignalsEnum EntryPointSTOP(KernelDatabase database, KernelQ qManager)
        {
            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
        }

        
    }
}

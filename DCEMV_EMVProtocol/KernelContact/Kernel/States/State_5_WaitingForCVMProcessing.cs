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

namespace DCEMV.EMVProtocol.Kernels.K
{
    public static class State_5_WaitingForCVMProcessing
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_5_WaitingForCVMProcessing:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                return ProcessNextCVM(database, qManager, cardQManager, sw);
            }
        }

        private static SignalsEnum ProcessNextCVM(KernelDatabase database, KernelQ qManager, CardQ cardQManager, Stopwatch sw)
        {
            CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN cvl = new CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN(database);
            CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN cvr = new CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN(database);

            bool goToNextCVM = false;
            if (cvr.Value.GetCVMPerformed() == CVMCode.EncipheredPINVerifiedOnline)
            {
                if (cvr.Value.GetCVMResult() == CVMResult.Failed)
                {
                    goToNextCVM = true;
                }
            }
            else
            {
                //if a previously run CVM failed then state returns here and we try the next CVM
                if (cvr.Value.GetCVMResult() != CVMResult.Success)
                {
                    goToNextCVM = true;
                }
            }

            if (goToNextCVM && database.CVMCurrentlySelectedCounter < cvl.Value.CardHolderVerificationRules.Count)
            { 
                //if previous cvm was offline pin, check pin try counter
                if (cvr.Value.GetCVMPerformed() == CVMCode.EncipheredPINVerificationPerformedByICC ||
                    cvr.Value.GetCVMPerformed() == CVMCode.EncipheredPINVerificationPerformedByICCAndSignature_Paper ||
                    cvr.Value.GetCVMPerformed() == CVMCode.PlaintextPINVerificationPerformedByICC ||
                    cvr.Value.GetCVMPerformed() == CVMCode.PlaintextPINVerificationPerformedByICCAndSignature_Paper)
                {
                    TLV pinTryCounterTLV = database.Get(EMVTagsEnum.PERSONAL_IDENTIFICATION_NUMBER_PIN_TRY_COUNTER_9F17_KRN);
                    if (pinTryCounterTLV != null)
                    {
                        ushort pinTryCounter = Formatting.ConvertToInt16(pinTryCounterTLV.Value);
                        if (pinTryCounter > 1) //at this point the pin try counter on the card is 0, we are always 1 behind
                        {
                            qManager.EnqueueToOutput(new KernelPinResponse());
                            return SignalsEnum.WAITING_FOR_PIN_RESPONSE;
                        }
                    }
                }

                #region Book 3 Section 10.5
                CVMSelection_7_5.CVMSelection(database, new Func<bool>(() => { return true; }));
                cvr = new CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN(database);//update after calling cvm selection
                #endregion

                if (cvr.Value.GetCVMPerformed() == CVMCode.EncipheredPINVerificationPerformedByICC ||
                    cvr.Value.GetCVMPerformed() == CVMCode.EncipheredPINVerificationPerformedByICCAndSignature_Paper ||
                    cvr.Value.GetCVMPerformed() == CVMCode.PlaintextPINVerificationPerformedByICC ||
                    cvr.Value.GetCVMPerformed() == CVMCode.PlaintextPINVerificationPerformedByICCAndSignature_Paper ||
                    cvr.Value.GetCVMPerformed() == CVMCode.EncipheredPINVerifiedOnline)
                {
                    qManager.EnqueueToOutput(new KernelPinResponse());
                    return SignalsEnum.WAITING_FOR_PIN_RESPONSE;
                }

                if (cvr.Value.GetCVMResult() == CVMResult.Failed)
                {
                    return SignalsEnum.WAITING_FOR_CVM_PROCESSING;
                }
            }

            if (cvr.Value.GetCVMPerformed() != CVMCode.NoCVMDone)
            {
                TRANSACTION_STATUS_INFORMATION_9B_KRN tsi = new TRANSACTION_STATUS_INFORMATION_9B_KRN(database);
                tsi.Value.CardholderVerificationWasPerformed = true;
                tsi.UpdateDB();
            }

            #region Book 3 Section 10.6
            //Terminal Risk Management
            TLV lcol = database.Get(EMVTagsEnum.LOWER_CONSECUTIVE_OFFLINE_LIMIT_9F14_KRN);
            TLV ucol = database.Get(EMVTagsEnum.UPPER_CONSECUTIVE_OFFLINE_LIMIT_9F23_KRN);
            if (lcol != null && ucol != null)
            {
                EMVGetDataRequest requestATC = new EMVGetDataRequest(Formatting.HexStringToByteArray(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag));
                EMVGetDataRequest requestLastOnlineATC = new EMVGetDataRequest(Formatting.HexStringToByteArray(EMVTagsEnum.LAST_ONLINE_APPLICATION_TRANSACTION_COUNTER_ATC_REGISTER_9F13_KRN.Tag));
                cardQManager.EnqueueToInput(new CardRequest(requestATC, CardinterfaceServiceRequestEnum.ADPU));
                cardQManager.EnqueueToInput(new CardRequest(requestLastOnlineATC, CardinterfaceServiceRequestEnum.ADPU));
            }

            //Get Floor Limit Data
            if (database.IsEmpty(EMVTagsEnum.TERMINAL_FLOOR_LIMIT_9F1B_KRN.Tag))
            {
                DATA_NEEDED_DF8106_KRN2 dataNeeded = new DATA_NEEDED_DF8106_KRN2(database);
                dataNeeded.Value.Tags.Add(EMVTagsEnum.TERMINAL_FLOOR_LIMIT_9F1B_KRN.Tag);
                dataNeeded.UpdateDB();
                CommonRoutines.PostDEK(database, qManager);
                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
            }

            qManager.EnqueueToOutput(new KernelTRMResponse());
            return SignalsEnum.WAITING_FOR_TERMINAL_RISK_MANAGEMENT;
            #endregion
        }

        private static SignalsEnum EntryPointDET(KernelDatabase database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_CVM_PROCESSING;
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

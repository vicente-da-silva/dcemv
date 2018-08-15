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
using DCEMV.ISO7816Protocol;
using System;

namespace DCEMV.EMVProtocol.Kernels.K
{
    internal class Kernel : KernelBase
    {
        private EMVSelectApplicationResponse emvSelectApplicationResponse;

        public Kernel(TransactionTypeEnum tt, CardQProcessor cardQProcessor, PublicKeyCertificateManager publicKeyCertificateManager, EntryPointPreProcessingIndicators processingIndicatorsForSelected, CardExceptionManager cardExceptionManager, IConfigurationProvider configProvider, EMVSelectApplicationResponse emvSelectApplicationResponse)
            : base(cardQProcessor, publicKeyCertificateManager, processingIndicatorsForSelected, cardExceptionManager, configProvider)
        {
            database = new KernelDatabase(publicKeyCertificateManager);
            database.InitializeDefaultDataObjects(tt, configProvider);
            this.emvSelectApplicationResponse = emvSelectApplicationResponse;
        }

        protected override void ExecuteAction(ActionsEnum action)
        {
            switch (action)
            {
                case ActionsEnum.Execute_Idle:
                    DoStateChange(State_1_Idle.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForPDOLData:
                    DoStateChange(State_2_WaitingForPDOLData.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForGPOResponse:
                    DoStateChange(State_3_WaitingForGPOResponse.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForEMVReadRecordResponse:
                    DoStateChange(State_4_WaitingForEMVReadRecord.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, emvSelectApplicationResponse, sw));
                    break;
                case ActionsEnum.Execute_WaitingForCVMProcessing:
                    DoStateChange(State_5_WaitingForCVMProcessing.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForGetPinReponse:
                    DoStateChange(State_5a_WaitingForGetPinReponse.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForGetPinTryCounter:
                    DoStateChange(State_5b_WaitingForGetPinTryCounter.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForGetChallenge:
                    DoStateChange(State_5c_WaitingForGetChallenge.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, publicKeyCertificateManager, emvSelectApplicationResponse, sw));
                    break;
                case ActionsEnum.Execute_WaitingForVerify:
                    DoStateChange(State_5d_WaitingForVerify.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForTerminalRiskManagement:
                    DoStateChange(State_6_WaitingForTerminalRiskManagement.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, emvSelectApplicationResponse, sw));
                    break;
                case ActionsEnum.Execute_WaitingForGenerateACResponse_1:
                    DoStateChange(State_7_WaitingForGenACResponse_1.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, emvSelectApplicationResponse, publicKeyCertificateManager, sw));
                    break;
                case ActionsEnum.Execute_WaitingForInternalAuthenticate:
                    DoStateChange(State_7_WaitingForInternalAuthenticate.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, publicKeyCertificateManager, emvSelectApplicationResponse, sw));
                    break;
                case ActionsEnum.Execute_WaitingForOnlineAuth:
                    DoStateChange(State_8_WaitingForOnlineAuth.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, emvSelectApplicationResponse, sw));
                    break;
                case ActionsEnum.Execute_WaitingForExternalAuthenticate:
                    DoStateChange(State_9_WaitingForExternalAuthenticate.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, publicKeyCertificateManager, emvSelectApplicationResponse, sw));
                    break;
                case ActionsEnum.Execute_WaitingForGenerateACResponse_2:
                    DoStateChange(State_10_WaitingForGenACResponse_2.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, emvSelectApplicationResponse, publicKeyCertificateManager, sw));
                    break;
                case ActionsEnum.Execute_WaitingForScriptProcessing:
                    DoStateChange(State_11_WaitingForScriptProcessing.Execute((KernelDatabase)database, KernelQ, cardQProcessor.CardQ, emvSelectApplicationResponse, publicKeyCertificateManager, sw));
                    break;

                case ActionsEnum.Execute_EXIT:
                    ;
                    break;
                default:
                    throw new Exception("ProcessEventChange: Invalid ActionsEnum value in EventStateActionDefinition");
            }
        }
    }
}
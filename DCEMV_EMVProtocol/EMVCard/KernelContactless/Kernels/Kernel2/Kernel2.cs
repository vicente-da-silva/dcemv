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
using System;
using DCEMV.ISO7816Protocol;
using DCEMV.Shared;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public class Kernel2 : KernelBase
    {
        protected TornTransactionLogManager tornTransactionLogManager;

        public Kernel2(TransactionTypeEnum tt, TornTransactionLogManager tornTransactionLogManager, CardQProcessor cardQProcessor, PublicKeyCertificateManager publicKeyCertificateManager, EntryPointPreProcessingIndicators processingIndicatorsForSelected, CardExceptionManager cardExceptionManager, IConfigurationProvider configProvider)
            :base(cardQProcessor, publicKeyCertificateManager, processingIndicatorsForSelected, cardExceptionManager, configProvider)
        {
            this.tornTransactionLogManager = tornTransactionLogManager;

            database = new Kernel2Database(publicKeyCertificateManager);
            database.InitializeDefaultDataObjects(tt, configProvider);
        }

        protected override void ExecuteAction(ActionsEnum action)
        {
            switch (action)
            {
                case ActionsEnum.Execute_Idle:
                    DoStateChange(State_1_Idle.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, tornTransactionLogManager, sw));
                    break;
                case ActionsEnum.Execute_WaitingForPDOLData:
                    DoStateChange(State_2_WaitingForPDOLData.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForGPOResponse:
                    DoStateChange(State_3_WaitingForGPOResponse.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForExchangeRelayResistanceDataResponse:
                    DoStateChange(State_R1_ExchangeRelayResistanceData.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForEMVReadRecordResponse:
                    DoStateChange(State_4_WaitingForEMVReadRecord.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, tornTransactionLogManager, sw));
                    break;
                case ActionsEnum.Execute_TerminateOnNextRA:
                    DoStateChange(State_4_TerminateOnNextRA.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ));
                    break;
                case ActionsEnum.Execute_WaitingForGetDataResponse:
                    DoStateChange(State_5_WaitingForGetDataResponse.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, tornTransactionLogManager, sw));
                    break;
                case ActionsEnum.Execute_WaitingForEMVModeFirstWriteFlag:
                    DoStateChange(State_6_WaitingForEMVModeFirstWriteFlag.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, tornTransactionLogManager, sw));
                    break;
                case ActionsEnum.Execute_WaitingForMagStripeReadRecordResponse:
                    DoStateChange(State_7_WaitingForMagStripeReadRecordResponse.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, tornTransactionLogManager, sw));
                    break;
                case ActionsEnum.Execute_WaitingForMagStripeModeFirstWriteFlag:
                    DoStateChange(State_8_WaitingForMagStripeFirstWriteFlag.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForGenerateACResponse_1:
                    DoStateChange(State_9_WaitingForGenACResponse_1.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, tornTransactionLogManager, publicKeyCertificateManager, sw));
                    break;
                case ActionsEnum.Execute_WaitingForRecoverACResponse:
                    DoStateChange(State_10_WaitingForRecoverACResponse.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, tornTransactionLogManager, publicKeyCertificateManager, sw));
                    break;
                case ActionsEnum.Execute_WaitingForGenerateACResponse_2:
                    DoStateChange(State_11_WaitingForGenACResponse_2.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, tornTransactionLogManager, publicKeyCertificateManager, sw));
                    break;
                case ActionsEnum.Execute_WaitingForPutDataResponseBeforeGenerateAC:
                    DoStateChange(State_12_WaitingForPutDataResponseBeforeGenerateAC.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, tornTransactionLogManager, sw));
                    break;
                case ActionsEnum.Execute_WaitingForCCCResponse_1:
                    DoStateChange(State_13_WaitingForCCCResponse1.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, tornTransactionLogManager, publicKeyCertificateManager, sw));
                    break;
                case ActionsEnum.Execute_WaitingForCCCResponse_2:
                    DoStateChange(State_14_WaitingForCCCResponse2.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, tornTransactionLogManager, publicKeyCertificateManager, sw));
                    break;
                case ActionsEnum.Execute_WaitingForPutDataResponseAfterGenerateAC:
                    DoStateChange(State_15_WaitingForPutDataResponseAfterGenerateAC.Execute((Kernel2Database)database, KernelQ, cardQProcessor.CardQ, tornTransactionLogManager, sw));
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

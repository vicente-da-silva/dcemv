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
using System.Diagnostics;
using DCEMV.ISO7816Protocol;
using DCEMV.Shared;
using DCEMV.EMVProtocol.Kernels.K1K3;

namespace DCEMV.EMVProtocol.Kernels.K3
{
    public class Kernel3 : KernelBase
    {
        public Kernel3(TransactionTypeEnum tt, CardQProcessor cardQProcessor, PublicKeyCertificateManager publicKeyCertificateManager, EntryPointPreProcessingIndicators processingIndicatorsForSelected, CardExceptionManager cardExceptionManager, IConfigurationProvider configProvider)
            : base(cardQProcessor, publicKeyCertificateManager, processingIndicatorsForSelected, cardExceptionManager, configProvider)
        {
            database = new Kernel3Database(processingIndicatorsForSelected, publicKeyCertificateManager);
            database.InitializeDefaultDataObjects(tt, configProvider);
        }
        
        protected override void ExecuteAction(ActionsEnum action)
        {
            switch (action)
            {
                case ActionsEnum.Execute_Idle:
                    DoStateChange(State_1_Idle.Execute((Kernel3Database)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForPDOLData:
                    DoStateChange(State_2_WaitingForPDOLData.Execute((Kernel3Database)database, KernelQ, cardQProcessor.CardQ, sw));
                    break;
                case ActionsEnum.Execute_WaitingForGPOResponse:
                    DoStateChange(State_3_WaitingForGPOResponse.Execute((Kernel3Database)database, KernelQ, cardQProcessor.CardQ, sw, publicKeyCertificateManager, cardExceptionManager));
                    break;
                case ActionsEnum.Execute_WaitingForEMVReadRecordResponse:
                    Func<string, KernelDatabaseBase, KernelQ, CardQ, Stopwatch, PublicKeyCertificateManager, CardExceptionManager, SignalsEnum> ReadRecordCompleteCallback =
                        new Func<string, KernelDatabaseBase, KernelQ, CardQ, Stopwatch, PublicKeyCertificateManager, CardExceptionManager ,SignalsEnum>(State_3_4_CommonProcessing.DoCommonProcessing);
                    DoStateChange(State_4_WaitingForEMVReadRecord.Execute((Kernel3Database)database, KernelQ, cardQProcessor.CardQ, sw, publicKeyCertificateManager, cardExceptionManager, ReadRecordCompleteCallback));
                    break;
                case ActionsEnum.Execute_TerminateOnNextRA:
                    DoStateChange(State_4_TerminateOnNextRA.Execute((Kernel3Database)database, KernelQ, cardQProcessor.CardQ));
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

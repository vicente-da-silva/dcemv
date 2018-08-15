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
using DCEMV.EMVProtocol.Kernels;
using DCEMV.EMVProtocol.Kernels.K1;
using DCEMV.EMVProtocol.Kernels.K2;
using DCEMV.EMVProtocol.Kernels.K3;
using DCEMV.ISO7816Protocol;

namespace DCEMV.EMVProtocol.Contactless
{
    public class EMVContactlessKernelActivation
    {
        public static KernelBase ActivateKernel(
            TransactionRequest tt,
            CardQProcessor cardInterface,
            TornTransactionLogManager tornTransactionLogManager,
            PublicKeyCertificateManager publicKeyCertificateManager,
            EMVSelectApplicationResponse response,
            TerminalSupportedKernelAidTransactionTypeCombination terminalCombinationForSelected,
            CardKernelAidCombination cardCombinationForSelected,
            EntryPointPreProcessingIndicators processingIndicatorsForSelected,
            CardExceptionManager cardExceptionManager,
            IConfigurationProvider configProvider
            ) //the response from the selected aid command
        {
            switch (((TerminalSupportedContactlessKernelAidTransactionTypeCombination)terminalCombinationForSelected).KernelEnum)
            {
                case KernelEnum.Kernel1:
                    EMVTagsEnum.DataKernelID = DataKernelID.K1;
                    return new Kernel1(tt.GetTransactionType_9C(), cardInterface, publicKeyCertificateManager, processingIndicatorsForSelected, cardExceptionManager, configProvider);

                case KernelEnum.Kernel2:
                    EMVTagsEnum.DataKernelID = DataKernelID.K2;
                    return new Kernel2(tt.GetTransactionType_9C(), tornTransactionLogManager, cardInterface, publicKeyCertificateManager, processingIndicatorsForSelected, cardExceptionManager, configProvider);

                case KernelEnum.Kernel3:
                    EMVTagsEnum.DataKernelID = DataKernelID.K3;
                    return new Kernel3(tt.GetTransactionType_9C(), cardInterface, publicKeyCertificateManager, processingIndicatorsForSelected, cardExceptionManager, configProvider);

                case KernelEnum.Kernel4:
                    break;

                case KernelEnum.Kernel5:
                    break;

                case KernelEnum.Kernel6:
                    break;

                case KernelEnum.Kernel7:
                    break;

                default:
                    throw new EMVProtocolException("Unsupported kernel: " + ((TerminalSupportedContactlessKernelAidTransactionTypeCombination)terminalCombinationForSelected).KernelEnum);
            }
            throw new EMVProtocolException("Unsupported kernel: " + ((TerminalSupportedContactlessKernelAidTransactionTypeCombination)terminalCombinationForSelected).KernelEnum);
        }
    }
}

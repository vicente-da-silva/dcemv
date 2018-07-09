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
using DCEMV.EMVProtocol.Kernels.K;
using DCEMV.ISO7816Protocol;

namespace DCEMV.EMVProtocol.Contact
{
    public class EMVContactKernelActivation
    {
        public static KernelBase ActivateKernel(
            TransactionRequest tt,
            CardQProcessor cardInterface,
            PublicKeyCertificateManager publicKeyCertificateManager,
            EMVSelectApplicationResponse response,
            TerminalSupportedKernelAidTransactionTypeCombination terminalCombinationForSelected,
            CardKernelAidCombination cardCombinationForSelected,
            EntryPointPreProcessingIndicators processingIndicatorsForSelected,
            CardExceptionManager cardExceptionManager,
            IConfigurationProvider configProvider
            ) //the response from the selected aid command
        {
            return new Kernel(tt.GetTransactionType_9C(), cardInterface, publicKeyCertificateManager, processingIndicatorsForSelected, cardExceptionManager, configProvider, response);
        }
    }
}

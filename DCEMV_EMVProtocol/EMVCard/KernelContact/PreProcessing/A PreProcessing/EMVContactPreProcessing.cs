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
using DCEMV.EMVProtocol.Kernels;
using System;
using System.Collections.Generic;

namespace DCEMV.EMVProtocol.Contact
{
    class EMVContactPreProcessing
    {
        public static List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators>> PreProcessing(TransactionRequest transactionRequest)
        {
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators>> EntryPointPreProcessingIndicators = new List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators>>();

            foreach (TerminalSupportedKernelAidTransactionTypeCombination kc in TerminalSupportedKernelAidTransactionTypeCombinations.SupportedContactCombinations)
            {
                EntryPointPreProcessingIndicators eppi = new EntryPointPreProcessingIndicators();
                EntryPointPreProcessingIndicators.Add(Tuple.Create(kc, eppi));
            }
            return EntryPointPreProcessingIndicators;
        }
    }
}

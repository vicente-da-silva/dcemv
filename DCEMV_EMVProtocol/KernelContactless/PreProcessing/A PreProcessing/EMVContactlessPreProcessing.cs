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

namespace DCEMV.EMVProtocol.Contactless
{
    public class EMVContactlessPreProcessing
    {
        public static List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators>> PreProcessing(TransactionRequest transactionRequest)
        {
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators>> EntryPointPreProcessingIndicators = new List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators>>();

            foreach (TerminalSupportedKernelAidTransactionTypeCombination kcA in TerminalSupportedKernelAidTransactionTypeCombinations.SupportedContactlessCombinations)
            {
                TerminalSupportedContactlessKernelAidTransactionTypeCombination kc = (TerminalSupportedContactlessKernelAidTransactionTypeCombination)kcA;

                if (transactionRequest.GetTransactionType_9C() != kc.TransactionTypeEnum)
                    continue;

                    //3.1.1.1
                EntryPointPreProcessingIndicators eppi = new EntryPointPreProcessingIndicators();
                EntryPointPreProcessingIndicators.Add(Tuple.Create(kcA, eppi));
                if (kc.TTQ != null)
                {
                    //3.1.1.2
                    eppi.TTQ = (TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN)kc.TTQ.Clone();
                    eppi.TTQ.Value.OnlineCryptogramRequired = false;
                    eppi.TTQ.Value.CVMRequired = false;
                    eppi.TTQ.Val.Serialize();
                }

                //3.1.1.3
                if (kc.StatusCheckSupportFlag != null && kc.StatusCheckSupportFlag == true && transactionRequest.IsSingleUnitOfCurrency)
                    eppi.StatusCheckRequested = true;

                //3.1.1.4
                if (transactionRequest.GetAmountAuthorized_9F02() == 0)
                    if (kc.ZeroAmountAllowedFlag != null && kc.ZeroAmountAllowedFlag == false)
                        eppi.ContactlessApplicationNotAllowed = true;
                    else
                        eppi.ZeroAmount = true;

                //3.1.1.5
                if(kc.ReaderContactlessTransactionLimit != null && transactionRequest.GetAmountAuthorized_9F02() > kc.ReaderContactlessTransactionLimit)
                    eppi.ContactlessApplicationNotAllowed = true;

                //3.1.1.6
                if (kc.ReaderContactlessFloorLimit != null && transactionRequest.GetAmountAuthorized_9F02() > kc.ReaderContactlessFloorLimit)
                    eppi.ReaderContactlessFloorLimitExceeded = true;

                //3.1.1.7
                if (kc.ReaderContactlessFloorLimit == null && kc.TerminalFloorLimit_9F1B != null && transactionRequest.GetAmountAuthorized_9F02() > kc.TerminalFloorLimit_9F1B)
                    eppi.ReaderContactlessFloorLimitExceeded = true;

                //3.1.1.8
                if (kc.ReaderCVMRequiredLimit != null && transactionRequest.GetAmountAuthorized_9F02() > kc.ReaderCVMRequiredLimit)
                    eppi.ReaderCVMRequiredLimitExceeded = true;
                
                if (eppi.TTQ != null)
                {
                    //3.1.1.9
                    if (eppi.ReaderContactlessFloorLimitExceeded)
                        eppi.TTQ.Value.OnlineCryptogramRequired = true;
                    
                    //3.1.1.10
                    if (eppi.StatusCheckRequested)
                        eppi.TTQ.Value.OnlineCryptogramRequired = true;

                    //3.1.1.11
                    if (eppi.ZeroAmount)
                        if(!eppi.TTQ.Value.OfflineOnlyReader)
                            eppi.TTQ.Value.OnlineCryptogramRequired = true;
                        else
                            eppi.ContactlessApplicationNotAllowed = true;

                    //3.1.1.12
                    if (eppi.ReaderCVMRequiredLimitExceeded)
                        eppi.TTQ.Value.CVMRequired = true;

                    eppi.TTQ.Serialize();
                }

                EntryPointPreProcessingIndicators.RemoveAll(x => x.Item2.ContactlessApplicationNotAllowed);
            }
            return EntryPointPreProcessingIndicators;
        }
    }
}

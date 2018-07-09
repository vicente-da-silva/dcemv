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
using System;
using System.Collections.Generic;

namespace DCEMV.EMVProtocol.Contactless
{
    public enum EntryPointEnum
    {
        StartA,
        StartB,
        StartC,
        StartD,
    }

    public class EMVContactlessProtocolActivation
    {
        public static UserInterfaceRequest ProtocolActivation(
            EntryPointEnum entryPoint,
            TransactionRequest transactionRequest,
            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators>> preProcessingIndicators, 
            EMVTerminalProcessingOutcome preProcessingOutcome, 
            bool restart)
        {
            #region 3.2.1.1
            if (!restart)
            #endregion
            {
                if (entryPoint == EntryPointEnum.StartB)
                {
                    List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators>> preProcessingIndicatorsNew = new List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators>>();
                    //3.2.1.1
                    foreach (Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators> kc in preProcessingIndicators)
                        preProcessingIndicatorsNew.Add(Tuple.Create(kc.Item1, new EntryPointPreProcessingIndicators()));

                    preProcessingIndicators = preProcessingIndicatorsNew;

                    foreach (Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators> kc in preProcessingIndicators)
                    {
                        if (((TerminalSupportedContactlessKernelAidTransactionTypeCombination)kc.Item1).TTQ != null)
                        {
                            kc.Item2.TTQ = (TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN)((TerminalSupportedContactlessKernelAidTransactionTypeCombination)kc.Item1).TTQ.Clone();
                            kc.Item2.TTQ.Value.OnlineCryptogramRequired = false;
                            kc.Item2.TTQ.Value.CVMRequired = false;
                        }
                    }
                }
                //clear the candidate list
            }
            #region 3.2.1.2
            else
            #endregion
            {
                if (preProcessingOutcome.UIRequestOnRestartPresent)
                    return preProcessingOutcome.UserInterfaceRequest;
            }
            #region 3.2.1.2
            if (!restart || (preProcessingOutcome != null && !preProcessingOutcome.UIRequestOnRestartPresent))
            #endregion
            {
                UserInterfaceRequest userInterfaceRequest = new UserInterfaceRequest()
                {
                    MessageIdentifier = MessageIdentifiersEnum.PresentCard,
                    Status = StatusEnum.ReadyToRead,
                };
                return userInterfaceRequest;
            }
            return null;
        }
    }
}

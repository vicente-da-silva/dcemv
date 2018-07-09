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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DCEMV.TLVProtocol;
using DCEMV.ISO7816Protocol;
using DCEMV.EMVProtocol.Contactless;
using DCEMV.EMVProtocol.Kernels.K2;
using DCEMV.EMVProtocol.Kernels;

namespace DCEMV.EMVProtocol
{
    public class EMVContactlessTerminalApplication : EMVTerminalApplicationBase
    {
        protected TornTransactionLogManager tornTransactionLogManager;
         
        public EMVContactlessTerminalApplication(CardQProcessor cardQProcessor, IConfigurationProvider configProvider)
            :base(cardQProcessor, configProvider)
        {
            tornTransactionLogManager = new TornTransactionLogManager();
        }

        public void StartTransactionRequest(TransactionRequest tr)
        {
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            TerminalSupportedKernelAidTransactionTypeCombinations.LoadContactlessSupportedCombination(configProvider, tr.GetTransactionType_9C());
            DoEntryPointA(tr);
        }

        protected override void DoEntryPointA(TransactionRequest tr)
        {
            this.tr = tr;
            #region 3.1.1.1 - 3.1.1.12 Preprocessing Start A
            preProcessingValues = EMVContactlessPreProcessing.PreProcessing(tr);
            #endregion
            #region 3.1.1.13 - Preprocessing Start A
            if (preProcessingValues.Count == 0)
            #endregion
            {
                EMVTerminalProcessingOutcome processingOutcome = new EMVTerminalProcessingOutcome()
                {
                    NextProcessState = EMVTerminalPreProcessingStateEnum.EndProcess,
                    UIRequestOnOutcomePresent = true,
                    UserInterfaceRequest = new UserInterfaceRequest() { MessageIdentifier = MessageIdentifiersEnum.PleaseInsertOrSwipeCard, Status = StatusEnum.ProcessingError },
                    UIRequestOnRestartPresent = false
                };
                OnProcessCompleted(processingOutcome);
                return;
            }
            DoEntryPointB(EMVTerminalPreProcessingStateEnum.Preprocessing_StartA);
        }

        protected override void DoEntryPointB(EMVTerminalPreProcessingStateEnum parent)
        {
            Task.Run(() =>
            {
                try
                {
                    ProtocolActivation_B().ContinueWith((parentTask) =>
                    {
                        try
                        {
                            if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                            {
                                cancellationTokenForPreProcessing.Dispose();
                                return;
                            }

                            switch (parent)
                            {
                                case EMVTerminalPreProcessingStateEnum.Preprocessing_StartA:
                                case EMVTerminalPreProcessingStateEnum.ProtocolActivation_StartB:
                                    DoEntryPointC(parent);
                                    break;

                                default:
                                    throw new EMVProtocolException("unimplemeted TerminalPreProcessingStateEnum in DoEntryPointB");
                            }
                        }
                        catch (Exception ex)
                        {
                            OnExceptionOccured(ex);
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                }
            });
        }

        protected async Task ProtocolActivation_B()
        {
            cardInField = false;
            UserInterfaceRequest request = EMVContactlessProtocolActivation.ProtocolActivation(EntryPointEnum.StartA, tr, preProcessingValues, null, false);
            if (request != null)
            {
                OnUserInterfaceRequest(new UIMessageEventArgs(request.MessageIdentifier, request.Status));
            }
            await Task.Run(() =>
            {
                while (!cardInField)
                {
                    if (cancellationTokenForPreProcessing.Token.IsCancellationRequested)
                    {
                        break;
                    }
                }
            });
            return;
        }

        protected override void DoEntryPointC(EMVTerminalPreProcessingStateEnum source)
        {
            Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination, EntryPointPreProcessingIndicators>
                indicators = CombinationSelection_C(source);

            switch (indicators.Item1.NextProcessState)
            {
                case EMVTerminalPreProcessingStateEnum.EndProcess:
                    OnProcessCompleted(indicators.Item1);
                    break;

                case EMVTerminalPreProcessingStateEnum.KernelActivation_StartD:
                    DoEntryPointD(indicators);
                    break;

                default:
                    throw new EMVProtocolException("unimplemeted TerminalPreProcessingStateEnum in DoEntryPointC");
            }
        }

        protected Tuple<EMVTerminalProcessingOutcome,
                    EMVSelectApplicationResponse,
                    TerminalSupportedKernelAidTransactionTypeCombination,
                    CardKernelAidCombination,
                    EntryPointPreProcessingIndicators>
          CombinationSelection_C(EMVTerminalPreProcessingStateEnum parent)
        {
            Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination, List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>>
                        result;
            switch (parent)
            {
                case EMVTerminalPreProcessingStateEnum.Preprocessing_StartA:
                    result = EMVContactlessCombinationSelection.CombinationSelection_FromA(cardQProcessor, null, null);
                    lastCandidateSelected = Tuple.Create(result.Item1, result.Item2, result.Item3, result.Item4);
                    break;

                case EMVTerminalPreProcessingStateEnum.ProtocolActivation_StartB:
                    result = EMVContactlessCombinationSelection.CombinationSelection_FromB(cardQProcessor, lastCandidateSelected, candidates, null, null);
                    break;

                case EMVTerminalPreProcessingStateEnum.CombinationSelection_StartC:
                    preProcessingValues.RemoveAll(x => ((TerminalSupportedContactlessKernelAidTransactionTypeCombination)x.Item1).RIDEnum == ((TerminalSupportedContactlessKernelAidTransactionTypeCombination)lastCandidateSelected.Item3).RIDEnum);
                    candidates.RemoveAll(x => ((TerminalSupportedContactlessKernelAidTransactionTypeCombination)x.Item1).RIDEnum == ((TerminalSupportedContactlessKernelAidTransactionTypeCombination)lastCandidateSelected.Item3).RIDEnum);
                    result = EMVContactlessCombinationSelection.CombinationSelection_FromC(cardQProcessor, candidates);
                    break;

                default:
                    throw new EMVProtocolException("unimplemeted TerminalPreProcessingStateEnum in CombinationSelection");
            }

            if (result.Item1.NextProcessState == EMVTerminalPreProcessingStateEnum.EndProcess)
                return Tuple.Create(result.Item1, (EMVSelectApplicationResponse)null, (TerminalSupportedKernelAidTransactionTypeCombination)null, (CardKernelAidCombination)null, (EntryPointPreProcessingIndicators)null);

            candidates = result.Item5;
            TerminalSupportedKernelAidTransactionTypeCombination terminalCombinationForSelected = result.Item3;

            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators>>
                filteredIndicators = preProcessingValues.Where(x => (((TerminalSupportedContactlessKernelAidTransactionTypeCombination)x.Item1).RIDEnum == ((TerminalSupportedContactlessKernelAidTransactionTypeCombination)terminalCombinationForSelected).RIDEnum) && (((TerminalSupportedContactlessKernelAidTransactionTypeCombination)x.Item1).KernelEnum == ((TerminalSupportedContactlessKernelAidTransactionTypeCombination)terminalCombinationForSelected).KernelEnum)).ToList();

            if (filteredIndicators.Count == 0)
            {
                result.Item1.NextProcessState = EMVTerminalPreProcessingStateEnum.EndProcess;
                return Tuple.Create(result.Item1, result.Item2, result.Item3, result.Item4, (EntryPointPreProcessingIndicators)null);
            }

            EntryPointPreProcessingIndicators processingIndicatorsForSelected = filteredIndicators.Select(x => x.Item2).ToList().First();

            return Tuple.Create(result.Item1, result.Item2, result.Item3, result.Item4, processingIndicatorsForSelected);
        }

        protected override void DoEntryPointD(Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination, EntryPointPreProcessingIndicators> indicators)
        {
            //3.4.1.1
            KernelBase kernel = EMVContactlessKernelActivation.ActivateKernel(tr, cardQProcessor, tornTransactionLogManager, publicKeyCertificateManager, indicators.Item2, indicators.Item3, indicators.Item4, indicators.Item5, cardExceptionManager, configProvider);
            kernel.ExceptionOccured += Kernel_ExceptionOccured;

            //TODO: change config to load only kernel specific tags
            terminalConfigurationData.LoadTerminalConfigurationDataObjects(((TerminalSupportedContactlessKernelAidTransactionTypeCombination)indicators.Item3).KernelEnum, configProvider);

            TLVList requestInput = new TLVList();
            requestInput.AddToList(indicators.Item2.GetFCITemplateTag());
            requestInput.AddListToList(tr.GetTxDataTLV());
            //TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN ttq = ((TerminalSupportedContactlessKernelAidTransactionTypeCombination)indicators.Item3).TTQ;
            TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN ttq = indicators.Item5.TTQ;
            if (ttq != null)
                requestInput.AddToList(TLV.Create(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN.Tag, ttq.Value.Value));

            KernelRequest request = new KernelRequest(KernelTerminalReaderServiceRequestEnum.ACT, requestInput);
            kernel.KernelQ.EnqueueToInput(request);
            Task.Run(() => kernel.StartNewTransaction(), cancellationTokenForTerminalApplication.Token);
            Task.Run(() => OnProcessCompleted(StartServiceQPRocess(kernel)));
        }

        //public TLV GetDefaultTLV(string tag)
        //{
        //    return terminalConfigurationData.TerminalConfigurationDataObjects.Get(tag);
        //}
    }
}

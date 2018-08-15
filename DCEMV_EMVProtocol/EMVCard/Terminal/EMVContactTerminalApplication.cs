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
using DCEMV.FormattingUtils;
using DCEMV.EMVProtocol.Contact;
using DCEMV.EMVProtocol.Kernels;

namespace DCEMV.EMVProtocol
{
    public interface IUICallbackProvider
    {
        string DisplayApplicationList(List<string> list);
    }
    
    public class EMVContactTerminalApplication : EMVTerminalApplicationBase
    {
        public new static Logger Logger = new Logger(typeof(EMVContactTerminalApplication));

        protected IUICallbackProvider uiProvider;
        protected KernelBase kernel;

        public EMVContactTerminalApplication(CardQProcessor cardQProcessor, IConfigurationProvider configProvider, IUICallbackProvider uiProvider)
            :base(cardQProcessor, configProvider)
        {
            this.uiProvider = uiProvider;
        }

        public void StartTransactionRequest(TransactionRequest tr)
        {
            Logger.Log(tr.ToString());
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            TerminalSupportedKernelAidTransactionTypeCombinations.LoadContactSupportedCombination(configProvider, tr.GetTransactionType_9C());
            DoEntryPointA(tr);
        }

        protected override void DoEntryPointA(TransactionRequest tr)
        {
            this.tr = tr;
            //get a list of supported aids for the transaction type
            preProcessingValues = EMVContactPreProcessing.PreProcessing(tr);
            if (preProcessingValues.Count == 0)
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
            UserInterfaceRequest request = EMVContactProtocolActivation.ProtocolActivation();
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
            Tuple<EMVTerminalProcessingOutcome, 
                EMVSelectApplicationResponse,
                TerminalSupportedKernelAidTransactionTypeCombination, 
                CardKernelAidCombination,
                EntryPointPreProcessingIndicators>
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
            Tuple<EMVTerminalProcessingOutcome, 
                EMVSelectApplicationResponse,
                TerminalSupportedKernelAidTransactionTypeCombination, 
                CardKernelAidCombination, 
                List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>>> result;
            switch (parent)
            {
                case EMVTerminalPreProcessingStateEnum.Preprocessing_StartA:
                    result = EMVContactCombinationSelection.CombinationSelection_FromA(cardQProcessor, uiProvider);
                    lastCandidateSelected = Tuple.Create(result.Item1, result.Item2, result.Item3, result.Item4);
                    break;

                case EMVTerminalPreProcessingStateEnum.ProtocolActivation_StartB:
                    result = EMVContactCombinationSelection.CombinationSelection_FromB(cardQProcessor, lastCandidateSelected, candidates, uiProvider);
                    break;

                case EMVTerminalPreProcessingStateEnum.CombinationSelection_StartC:
                    preProcessingValues.RemoveAll(x => ((TerminalSupportedContactKernelAidTransactionTypeCombination)x.Item1).AIDEnum == ((TerminalSupportedContactKernelAidTransactionTypeCombination)lastCandidateSelected.Item3).AIDEnum);
                    candidates.RemoveAll(x => ((TerminalSupportedContactKernelAidTransactionTypeCombination)x.Item1).AIDEnum == ((TerminalSupportedContactKernelAidTransactionTypeCombination)lastCandidateSelected.Item3).AIDEnum);
                    result = EMVContactCombinationSelection.CombinationSelection_FromC(cardQProcessor, candidates, uiProvider);
                    lastCandidateSelected = Tuple.Create(result.Item1, result.Item2, result.Item3, result.Item4);
                    break;

                default:
                    throw new EMVProtocolException("unimplemeted TerminalPreProcessingStateEnum in CombinationSelection");
            }

            if (result.Item1.NextProcessState == EMVTerminalPreProcessingStateEnum.EndProcess)
                return Tuple.Create(result.Item1, (EMVSelectApplicationResponse)null, (TerminalSupportedKernelAidTransactionTypeCombination)null, (CardKernelAidCombination)null, (EntryPointPreProcessingIndicators)null);

            candidates = result.Item5;
            TerminalSupportedKernelAidTransactionTypeCombination terminalCombinationForSelected = result.Item3;

            List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination,EntryPointPreProcessingIndicators>>
                filteredIndicators = preProcessingValues.Where(x => (((TerminalSupportedContactKernelAidTransactionTypeCombination)x.Item1).AIDEnum == ((TerminalSupportedContactKernelAidTransactionTypeCombination)terminalCombinationForSelected).AIDEnum)).ToList();

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
            kernel = EMVContactKernelActivation.ActivateKernel(tr, cardQProcessor, publicKeyCertificateManager, indicators.Item2, indicators.Item3, indicators.Item4, indicators.Item5, cardExceptionManager, configProvider);
            kernel.ExceptionOccured += Kernel_ExceptionOccured;

            //TODO: change config to load only kernel specific tags
            terminalConfigurationData.LoadTerminalConfigurationDataObjects(KernelEnum.Kernel, configProvider);

            TLVList requestInput = new TLVList();
            requestInput.AddToList(indicators.Item2.GetFCITemplateTag());
            requestInput.AddListToList(tr.GetTxDataTLV());
            if (indicators.Item5.TTQ != null)
                requestInput.AddToList(TLV.Create(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN.Tag, indicators.Item5.TTQ.Value.Value));

            KernelRequest request = new KernelRequest(KernelTerminalReaderServiceRequestEnum.ACT, requestInput);
            kernel.KernelQ.EnqueueToInput(request);
            Task.Run(() => kernel.StartNewTransaction(), cancellationTokenForTerminalApplication.Token);
            Task.Run(() => OnProcessCompleted(StartServiceQPRocess(kernel)));
        }

        public void DoPinReponse(string pin)
        {
            if(kernel==null)
                throw new EMVProtocolException("DoPinReponse called without kernel being activated");
            TLVList requestInput = new TLVList();
            requestInput.AddToList(TLV.Create(EMVTagsEnum.TRANSACTION_PERSONAL_IDENTIFICATION_NUMBER_PIN_DATA_99_KRN.Tag, Formatting.ASCIIStringToByteArray(pin)));
            KernelRequest request = new KernelRequest(KernelTerminalReaderServiceRequestEnum.PIN, requestInput);
            kernel.KernelQ.EnqueueToInput(request);
        }

        public void DoOnlineReponse(KernelOnlineResponseType responseType, TLV authCode_8A, TLV issuerAuthData_91, TLV scriptList_71, TLV scriptList_72)
        {
            if (kernel == null)
                throw new EMVProtocolException("DoOnlineReponse called without kernel being activated");

            TLVList returnVal = new TLVList();
            returnVal.AddToList(authCode_8A);
            returnVal.AddToList(issuerAuthData_91);
            if (scriptList_71 != null)
                returnVal.AddToList(scriptList_71);
            if (scriptList_72 != null)
                returnVal.AddToList(scriptList_72);
            KernelOnlineRequest request = new KernelOnlineRequest(returnVal, responseType);
            kernel.KernelQ.EnqueueToInput(request);
        }

        public void DoTRMReponse()
        {
            //random transaction selection

            if (kernel == null)
                throw new EMVProtocolException("DoTRMReponse called without kernel being activated");
            KernelTRMRequest request = new KernelTRMRequest(new TLVList()) { KernelTRMRequestType = KernelTRMRequestType.DontCare};
            kernel.KernelQ.EnqueueToInput(request);
        }
    }
}

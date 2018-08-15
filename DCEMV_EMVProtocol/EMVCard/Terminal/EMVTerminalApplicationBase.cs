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
using DCEMV.FormattingUtils;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DCEMV.TLVProtocol;
using DCEMV.ISO7816Protocol;
using DCEMV.EMVProtocol.Kernels;

namespace DCEMV.EMVProtocol
{
    public class OnlineEventArgs : EventArgs
    {
        public TLV data { get; }
        public TLV discretionaryData { get; }

        public OnlineEventArgs(TLV data, TLV discretionaryData)
        {
            this.data = data;
            this.discretionaryData = discretionaryData;
        }
    }
    public abstract class EMVTerminalApplicationBase
    {
        public static Logger Logger = new Logger(typeof(EMVTerminalApplicationBase));

        protected List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, EntryPointPreProcessingIndicators>> preProcessingValues;
        protected List<Tuple<TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination>> candidates;
        protected Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination> lastCandidateSelected;

        protected CardQProcessor cardQProcessor;
        protected PublicKeyCertificateManager publicKeyCertificateManager;
        protected TerminalConfigurationData terminalConfigurationData;
        protected CardExceptionManager cardExceptionManager;
        protected IConfigurationProvider configProvider;

        protected bool cardInField = false;
        public event EventHandler UserInterfaceRequest;
        public event EventHandler PinRequest;
        public event EventHandler TRMRequest;
        public event EventHandler OnlineRequest;
        public event EventHandler ProcessCompleted;
        public event EventHandler ExceptionOccured;

        protected CancellationTokenSource cancellationTokenForTerminalApplication;
        protected CancellationTokenSource cancellationTokenForPreProcessing;

        protected TransactionRequest tr;

        public EMVTerminalApplicationBase(CardQProcessor cardQProcessor, IConfigurationProvider configProvider)
        {
            this.configProvider = configProvider;

            cancellationTokenForTerminalApplication = new CancellationTokenSource();

            TLVMetaDataSourceSingleton.Instance.DataSource = new EMVTLVMetaDataSource();

            terminalConfigurationData = new TerminalConfigurationData();
            publicKeyCertificateManager = new PublicKeyCertificateManager(configProvider);
            cardExceptionManager = new CardExceptionManager(configProvider);

            this.cardQProcessor = cardQProcessor;
            this.cardQProcessor.ExceptionOccured += CardInterface_ExceptionOccured;
            this.cardQProcessor.CardInterfaceManger.CardPutInField += CardReader_CardPutInField;
            this.cardQProcessor.CardInterfaceManger.CardRemovedFromField += CardReader_CardRemovedFromField;
            cardQProcessor.StartServiceQProcess();
        }

        public TLV GetDefaultTLV(string tag)
        {
            return terminalConfigurationData.TerminalConfigurationDataObjects.Get(tag);
        }

        protected void OnExceptionOccured(Exception e)
        {
            cardQProcessor.StopServiceQProcess();
            StopServiceQProcess();
            ExceptionOccured?.Invoke(this, new ExceptionEventArgs() { Exception = e });
        }
        protected virtual void CardInterface_ExceptionOccured(object sender, EventArgs e)
        {
            cardQProcessor.StopServiceQProcess();
            StopServiceQProcess();
            ExceptionOccured?.Invoke(this, e);
        }
        protected virtual void Kernel_ExceptionOccured(object sender, EventArgs e)
        {
            cardQProcessor.StopServiceQProcess();
            StopServiceQProcess();
            ExceptionOccured?.Invoke(this, e);
        }
        protected void OnUserInterfaceRequest(UIMessageEventArgs e)
        {
            UserInterfaceRequest?.Invoke(this, e);
        }
        protected void OnPinRequest(EventArgs e)
        {
            PinRequest?.Invoke(this, e);
        }
        protected void OnTRMRequest(EventArgs e)
        {
            TRMRequest?.Invoke(this, e);
        }
        protected void OnOnlineRequest(OnlineEventArgs e)
        {
            OnlineRequest?.Invoke(this, e);
        }
        public void StopTerminalApplication()
        {
            cardQProcessor.StopServiceQProcess();
            StopServiceQProcess();
            if (cancellationTokenForPreProcessing != null)
                if (!cancellationTokenForPreProcessing.IsCancellationRequested)
                    cancellationTokenForPreProcessing.Cancel();
        }
        public void CancelTransactionRequest()
        {
            StopTerminalApplication();

            EMVTerminalProcessingOutcome processingOutcomeOUT = new EMVTerminalProcessingOutcome()
            {
                NextProcessState = EMVTerminalPreProcessingStateEnum.EndProcess,
                UIRequestOnOutcomePresent = true
            };
            UserInterfaceRequest uird = new UserInterfaceRequest()
            {
                MessageIdentifier = MessageIdentifiersEnum.ClearDisplay,
                Status = StatusEnum.ReadyToRead
            };
            processingOutcomeOUT.UserInterfaceRequest = uird;
            OnProcessCompleted(processingOutcomeOUT);
        }
        protected virtual void CardReader_CardRemovedFromField(object sender, EventArgs e)
        {
            cardInField = false;
            OnUserInterfaceRequest(new UIMessageEventArgs(MessageIdentifiersEnum.CardRemoved, StatusEnum.EndProcessing));
        }
        protected virtual void CardReader_CardPutInField(object sender, EventArgs e)
        {
            cardInField = true;
            OnUserInterfaceRequest(new UIMessageEventArgs(MessageIdentifiersEnum.CardInserted, StatusEnum.EndProcessing));
        }
        private void StopServiceQProcess()
        {
            try
            {
                if (cancellationTokenForPreProcessing != null)
                    if (!cancellationTokenForPreProcessing.IsCancellationRequested)
                        cancellationTokenForPreProcessing.Cancel();

                if (!cancellationTokenForTerminalApplication.IsCancellationRequested)
                    cancellationTokenForTerminalApplication.Cancel();
            }
            catch
            {
                //do nothing
            }
        }

        private MessageIdentifiersEnum MapIdentifierEnum(KernelMessageidentifierEnum kernelMessageidentifierEnum)
        {
            MessageIdentifiersEnum messageIdentifiersEnum;

            switch (kernelMessageidentifierEnum)
            {
                case KernelMessageidentifierEnum.CARD_READ_OK:
                    messageIdentifiersEnum = MessageIdentifiersEnum.CardReadOk;
                    break;
                case KernelMessageidentifierEnum.TRY_AGAIN:
                    messageIdentifiersEnum = MessageIdentifiersEnum.TryAgain;
                    break;
                case KernelMessageidentifierEnum.APPROVED:
                    messageIdentifiersEnum = MessageIdentifiersEnum.Approved;
                    break;
                case KernelMessageidentifierEnum.APPROVED_SIGN:
                    messageIdentifiersEnum = MessageIdentifiersEnum.ApprovedSign;
                    break;
                case KernelMessageidentifierEnum.DECLINED:
                    messageIdentifiersEnum = MessageIdentifiersEnum.Declined;
                    break;
                case KernelMessageidentifierEnum.ERROR_OTHER_CARD:
                    messageIdentifiersEnum = MessageIdentifiersEnum.InsertSwipeOrTryAnotherCard;
                    break;
                case KernelMessageidentifierEnum.INSERT_CARD:
                    messageIdentifiersEnum = MessageIdentifiersEnum.PleaseInsertOrSwipeCard;
                    break;
                case KernelMessageidentifierEnum.SEE_PHONE:
                    messageIdentifiersEnum = MessageIdentifiersEnum.SeePhone;
                    break;
                case KernelMessageidentifierEnum.AUTHORISING_PLEASE_WAIT:
                    messageIdentifiersEnum = MessageIdentifiersEnum.Authorizing;
                    break;
                case KernelMessageidentifierEnum.CLEAR_DISPLAY:
                    messageIdentifiersEnum = MessageIdentifiersEnum.ClearDisplay;
                    break;
                case KernelMessageidentifierEnum.N_A:
                    messageIdentifiersEnum = MessageIdentifiersEnum.NA;
                    break;

                default:
                    throw new EMVProtocolException("Unknown KernelMessageidentifierEnum");
            }

            return messageIdentifiersEnum;
        }
        private StatusEnum MapStatusEnum(KernelStatusEnum kernelStatusEnum)
        {
            switch (kernelStatusEnum)
            {
                case KernelStatusEnum.NOT_READY:
                    return StatusEnum.NotReady;
                case KernelStatusEnum.IDLE:
                    return StatusEnum.Idle;
                case KernelStatusEnum.READY_TO_READ:
                    return StatusEnum.ReadyToRead;
                case KernelStatusEnum.PROCESSING:
                    return StatusEnum.ProcessingError;
                case KernelStatusEnum.CARD_READ_SUCCESSFULLY:
                    return StatusEnum.CardReadOk;
                case KernelStatusEnum.PROCESSING_ERROR:
                    return StatusEnum.ProcessingError;
                case KernelStatusEnum.N_A:
                    return StatusEnum.NA;

                default:
                    throw new EMVProtocolException("Unknown KernelStatusEnum");
            }
        }

        private UIMessageEventArgs CreateUIMessageEventArgs(KernelMessageidentifierEnum kernelMessageidentifierEnum, KernelStatusEnum kernelStatusEnum, uint holdTime)
        {
            return new UIMessageEventArgs(MapIdentifierEnum(kernelMessageidentifierEnum), MapStatusEnum(kernelStatusEnum)) { HoldTime = holdTime };
        }
        private UIMessageEventArgs CreateUIMessageEventArgs(KernelMessageidentifierEnum kernelMessageidentifierEnum, KernelStatusEnum kernelStatusEnum, string additionalMessage)
        {
            return new UIMessageEventArgs(MapIdentifierEnum(kernelMessageidentifierEnum), MapStatusEnum(kernelStatusEnum)) { AdditionalMessage = additionalMessage };
        }
        protected virtual EMVTerminalProcessingOutcome StartServiceQPRocess(KernelBase kernel)
        {
            while (1 == 1)
            {
                try
                {
                    if (cancellationTokenForTerminalApplication.Token.IsCancellationRequested)
                    {
                        cancellationTokenForTerminalApplication.Dispose();
                        return null;
                    }

                    if (kernel.KernelQ.GetOutputQCount() == 0)
                    {
                        Task.Run(async () => await Task.Delay(1)).Wait();
                        continue;
                    }

                    KernelResponseBase k2Response = kernel.KernelQ.DequeueFromOutput(true);
                    if (k2Response == null)
                    {
                        Task.Run(async () => await Task.Delay(1)).Wait();
                        continue;
                    }

                    Logger.Log("Terminal received signal:" + k2Response.KernelReaderTerminalServiceResponseEnum);

                    switch (k2Response.KernelReaderTerminalServiceResponseEnum)
                    {
                        case KernelReaderTerminalServiceResponseEnum.DEK:
                            DATA_NEEDED_DF8106_KRN2 dataNeeded = ((KernelDEKResponse)k2Response).DataNeeded;
                            DATA_TO_SEND_FF8104_KRN2 dataToSend = ((KernelDEKResponse)k2Response).DataToSend;

                            TLVList requestInput = new TLVList();

                            //Logger.Log("------------------DEK Request Start-------------------");
                            foreach (string tag in dataNeeded.Value.Tags)
                            {
                                TLV found = terminalConfigurationData.TerminalConfigurationDataObjects.Get(tag);
                                if (found == null)
                                {
                                    Logger.Log("Tag Requested Of Terminal By Kernel Not Found:" + tag);
                                    throw new EMVTerminalException("cound not find tag for data needed: " + tag);
                                }

                                requestInput.AddToList(found);

                                //int depth = 0;
                                //Logger.Log("Tag Requested From Terminal:" + found.ToPrintString(ref depth));
                            }
                            //Logger.Log("------------------DEK Request End-------------------");
                            KernelRequest request = new KernelRequest(KernelTerminalReaderServiceRequestEnum.DET, requestInput);
                            kernel.KernelQ.EnqueueToInput(request);
                            break; //continue processing

                        case KernelReaderTerminalServiceResponseEnum.OUT:
                            KernelOUTResponse outResponse = (KernelOUTResponse)k2Response;
                            EMVTerminalProcessingOutcome processingOutcomeOUT = new EMVTerminalProcessingOutcome()
                            {
                                CVM = outResponse.OutcomeParameterSet_DF8129.Value.CVM
                            };
                            switch (outResponse.OutcomeParameterSet_DF8129.Value.Start)
                            {
                                case Kernel2StartEnum.A:
                                    throw new Exception("Kernel2StartEnum.A Not Implemented");

                                case Kernel2StartEnum.B:
                                    if (outResponse.UserInterfaceRequest_DF8116 != null)
                                    {
                                        OnUserInterfaceRequest(CreateUIMessageEventArgs(
                                            outResponse.UserInterfaceRequest_DF8116.Value.KernelMessageidentifierEnum,
                                            outResponse.UserInterfaceRequest_DF8116.Value.KernelStatusEnum,
                                            Formatting.ConvertToInt32(outResponse.UserInterfaceRequest_DF8116.Value.HoldTime)));
                                    }
                                    else
                                        OnUserInterfaceRequest(new UIMessageEventArgs(MessageIdentifiersEnum.TransmissionError, StatusEnum.EndProcessing));

                                    processingOutcomeOUT.NextProcessState = EMVTerminalPreProcessingStateEnum.ProtocolActivation_StartB;
                                    return processingOutcomeOUT;

                                case Kernel2StartEnum.C:
                                    processingOutcomeOUT.NextProcessState = EMVTerminalPreProcessingStateEnum.CombinationSelection_StartC;
                                    return processingOutcomeOUT;

                                case Kernel2StartEnum.D:
                                    throw new Exception("Kernel2StartEnum.D Not Implemented");

                                case Kernel2StartEnum.N_A:
                                    if (outResponse.UserInterfaceRequest_DF8116 != null)
                                    {
                                        string s1 = outResponse.UserInterfaceRequest_DF8116.Value.KernelMessageidentifierEnum.ToString();
                                        string s2 = outResponse.UserInterfaceRequest_DF8116.Value.KernelStatusEnum.ToString();
                                        string s3 = "";
                                        if (outResponse.UserInterfaceRequest_DF8116.Value.ValueQualifierEnum != ValueQualifierEnum.NONE)
                                        {
                                            s3 = outResponse.UserInterfaceRequest_DF8116.Value.ValueQualifierEnum + " : " + Formatting.BcdToString(outResponse.UserInterfaceRequest_DF8116.Value.ValueQualifier);
                                        }
                                        string s4 = "";
                                        if (outResponse.OutcomeParameterSet_DF8129.Value.CVM != KernelCVMEnum.NO_CVM)
                                        {
                                            s4 = outResponse.OutcomeParameterSet_DF8129.Value.CVM.ToString();
                                        }
                                        OnUserInterfaceRequest(CreateUIMessageEventArgs(
                                            outResponse.UserInterfaceRequest_DF8116.Value.KernelMessageidentifierEnum,
                                            outResponse.UserInterfaceRequest_DF8116.Value.KernelStatusEnum,
                                            string.Format("{0}:{1}", s3, s4)));
                                    }

                                    processingOutcomeOUT.DataRecord = outResponse.DataRecord_FF8105;
                                    processingOutcomeOUT.DiscretionaryData = outResponse.DiscretionaryData_FF8106;

                                    int depth = 0;
                                    if (outResponse.UserInterfaceRequest_DF8116 != null) Logger.Log(outResponse.UserInterfaceRequest_DF8116.ToPrintString(ref depth));
                                    depth = 0;
                                    Logger.Log(outResponse.OutcomeParameterSet_DF8129.ToPrintString(ref depth));
                                    depth = 0;
                                    if (outResponse.ErrorIndication_DF8115 != null) Logger.Log(outResponse.ErrorIndication_DF8115.ToPrintString(ref depth));
                                    depth = 0;
                                    if (outResponse.DataRecord_FF8105 != null) Logger.Log(outResponse.DataRecord_FF8105.ToPrintString(ref depth));
                                    depth = 0;
                                    if (outResponse.DiscretionaryData_FF8106 != null) Logger.Log(outResponse.DiscretionaryData_FF8106.ToPrintString(ref depth));

                                    processingOutcomeOUT.NextProcessState = EMVTerminalPreProcessingStateEnum.EndProcess;

                                    return processingOutcomeOUT;

                                default:
                                    throw new Exception("Unknown outResponse.OutcomeParameterSet_DF8129.Value.Start:" + outResponse.OutcomeParameterSet_DF8129.Value.Start.ToString());
                            }

                        case KernelReaderTerminalServiceResponseEnum.UI:
                            KernelUIResponse uiResponse = (KernelUIResponse)k2Response;

                            if (uiResponse.UserInterfaceRequest_DF8116 != null)
                            {
                                string s1 = uiResponse.UserInterfaceRequest_DF8116.Value.KernelMessageidentifierEnum.ToString();
                                string s2 = uiResponse.UserInterfaceRequest_DF8116.Value.KernelStatusEnum.ToString();
                                OnUserInterfaceRequest(CreateUIMessageEventArgs(
                                    uiResponse.UserInterfaceRequest_DF8116.Value.KernelMessageidentifierEnum,
                                    uiResponse.UserInterfaceRequest_DF8116.Value.KernelStatusEnum,
                                    Formatting.ConvertToInt32(uiResponse.UserInterfaceRequest_DF8116.Value.HoldTime)));
                            }

                            int depthUI = 0;
                            if (uiResponse.UserInterfaceRequest_DF8116 != null) Logger.Log(uiResponse.UserInterfaceRequest_DF8116.ToPrintString(ref depthUI));
                            break;


                        case KernelReaderTerminalServiceResponseEnum.PIN:
                            // display pin screen
                            OnPinRequest(new EventArgs());
                            break;

                        case KernelReaderTerminalServiceResponseEnum.TRM:
                            OnTRMRequest(new EventArgs());
                            break;

                        case KernelReaderTerminalServiceResponseEnum.ONLINE:
                            KernelOnlineResponse onlineResponse = (KernelOnlineResponse)k2Response;
                            OnOnlineRequest(new OnlineEventArgs(onlineResponse.data, onlineResponse.discretionaryData));
                            break;

                        default:
                            throw new Exception("Unknown Kernel1ReaderTerminalServiceResponseEnum:" + k2Response.KernelReaderTerminalServiceResponseEnum);

                    }

                    kernel.KernelQ.DequeueFromOutput(false); //only remove message when finished processing
                }
                catch (Exception ex)
                {
                    OnExceptionOccured(ex);
                    return null;
                }
            }
        }
        protected void OnProcessCompleted(EMVTerminalProcessingOutcome po)
        {
            try
            {
                if (po == null) //exception occurred
                    return;

                switch (po.NextProcessState)
                {
                    case EMVTerminalPreProcessingStateEnum.ProtocolActivation_StartB:
                        DoEntryPointB(EMVTerminalPreProcessingStateEnum.ProtocolActivation_StartB);
                        break;

                    case EMVTerminalPreProcessingStateEnum.CombinationSelection_StartC:
                        DoEntryPointC(EMVTerminalPreProcessingStateEnum.CombinationSelection_StartC);
                        break;

                    case EMVTerminalPreProcessingStateEnum.EndProcess:
                        //cardQProcessor.StopServiceQProcess();
                        TerminalProcessingOutcomeEventArgs tpo = new TerminalProcessingOutcomeEventArgs() { TerminalProcessingOutcome = po };
                        ProcessCompleted?.Invoke(this, tpo);
                        break;

                    default:
                        throw new EMVProtocolException("Unimplemeted TerminalPreProcessingStateEnum in OnProcessCompleted");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error in OnProcessCompleted:" + ex.Message);
                return;
            }
        }


        protected abstract void DoEntryPointA(TransactionRequest tr);
        protected abstract void DoEntryPointB(EMVTerminalPreProcessingStateEnum parent);
        protected abstract void DoEntryPointC(EMVTerminalPreProcessingStateEnum source);
        protected abstract void DoEntryPointD(Tuple<EMVTerminalProcessingOutcome, EMVSelectApplicationResponse, TerminalSupportedKernelAidTransactionTypeCombination, CardKernelAidCombination, EntryPointPreProcessingIndicators> indicators);
    }
}

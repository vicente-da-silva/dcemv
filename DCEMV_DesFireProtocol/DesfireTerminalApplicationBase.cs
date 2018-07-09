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
using DCEMV.ISO7816Protocol;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DCEMV.DesFireProtocol
{
    public enum DesFireTransactionTypeEnum
    {
        InstallApp,
        ProcessTransaction
    }
    public class DesfireTerminalApplicationBase
    {
        public static Logger Logger = new Logger(typeof(DesfireTerminalApplicationBase));

        protected CardQProcessor cardQProcessor;
        protected bool cardInField = false;

        public event EventHandler ExceptionOccured;
        public event EventHandler UserInterfaceRequest;
        public event EventHandler ProcessCompleted;

        protected CancellationTokenSource cancellationTokenForPreProcessing;

        public DesfireTerminalApplicationBase(CardQProcessor cardInterface)
        {
            try
            {
                this.cardQProcessor = cardInterface;
                this.cardQProcessor.ExceptionOccured += CardInterface_ExceptionOccured;
                this.cardQProcessor.CardInterfaceManger.CardPutInField += CardReader_CardPutInField;
                this.cardQProcessor.CardInterfaceManger.CardRemovedFromField += CardReader_CardRemovedFromField;
                cardInterface.StartServiceQProcess();
            }
            catch(Exception ex)
            {
                cardInterface.StopServiceQProcess();
                throw ex;
            }
        }

        private void OnExceptionOccured(Exception e)
        {
            cardQProcessor.StopServiceQProcess();
            ExceptionOccured?.Invoke(this, new ExceptionEventArgs() { Exception = e });
        }
        protected virtual void CardInterface_ExceptionOccured(object sender, EventArgs e)
        {
            //StopServiceQProcess();
            ExceptionOccured?.Invoke(this, e);
        }
       
        protected void OnProcessCompleted(TerminalProcessingOutcome po)
        {
            try
            {
                if (po == null) //exception occurred
                    return;

                switch (po.NextProcessState)
                {
                    case EMVTerminalPreProcessingStateEnum.EndProcess:
                        cardQProcessor.StopServiceQProcess();
                        ProcessCompleted?.Invoke(this, new TerminalProcessingOutcomeEventArgs() { TerminalProcessingOutcome = po });
                        break;

                    default:
                        throw new DesFireException("Unimplemeted TerminalPreProcessingStateEnum in OnProcessCompleted");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Error in OnProcessCompleted:" + ex.Message);
                return;
            }
        }
        protected void OnUserInterfaceRequest(UIMessageEventArgs e)
        {
            UserInterfaceRequest?.Invoke(this, e);
        }

        public void CancelTransactionRequest()
        {
            cardQProcessor.StopServiceQProcess();
            //StopServiceQProcess();
            if (cancellationTokenForPreProcessing != null)
                cancellationTokenForPreProcessing.Cancel();

            TerminalProcessingOutcome processingOutcomeOUT = new TerminalProcessingOutcome()
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
        
        public void StartTransactionRequest(DesFireTransactionTypeEnum desFireTransactionType)
        {
            cancellationTokenForPreProcessing = new CancellationTokenSource();
            DoEntryPointB(desFireTransactionType);
        }

        protected virtual void CardReader_CardRemovedFromField(object sender, EventArgs e)
        {
            cardInField = false;
            OnUserInterfaceRequest(new UIMessageEventArgs(MessageIdentifiersEnum.CardRemoved,StatusEnum.EndProcessing));
        }
        protected virtual void CardReader_CardPutInField(object sender, EventArgs e)
        {
            cardInField = true;
        }

        private void DoEntryPointB(DesFireTransactionTypeEnum desFireTransactionType)
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

                            DesfireAccessHandler desfireAccess;
                            byte[] sessionKey = new byte[0];
                            switch (desFireTransactionType)
                            {

                                case DesFireTransactionTypeEnum.ProcessTransaction:
                                    desfireAccess = new DesfireAccessHandler(cardQProcessor);
                                    CardDetails desfire = desfireAccess.ReadCardDetails();
                                    Logger.Log("DesFire Card Details:  " + Environment.NewLine + desfire.ToString());

                                    //desfireAccess.SelectApplication(new byte[] { 0x00, 0x00, 0x01 }); //select installed application

                                    //sessionKey = desfireAccess.AuthenticateAES();
                                    //Logger.Log("Session Key:  " + Formatting.ByteArrayToHexString(sessionKey));

                                    //byte[] dataWrittenAndReadBack = desfireAccess.WriteData(sessionKey, 0x01, 0x08);
                                    //Logger.Log("Data Written:  " + Formatting.ByteArrayToHexString(dataWrittenAndReadBack));

                                    OnProcessCompleted(new DesfireTerminalProcessingOutcome()
                                    {
                                        CardDetails = desfire,
                                        NextProcessState = EMVTerminalPreProcessingStateEnum.EndProcess,
                                        UIRequestOnOutcomePresent = true,
                                        UserInterfaceRequest = new UserInterfaceRequest()
                                        {
                                            MessageIdentifier = MessageIdentifiersEnum.ClearDisplay,
                                            Status = StatusEnum.ReadyToRead
                                        }
                                    });
                                    break;

                                case DesFireTransactionTypeEnum.InstallApp:
                                    desfireAccess = new DesfireAccessHandler(cardQProcessor);
                                    //auth to PICC application and get session key
                                    //PICC master key setting default does not require authentication to be done before creating an application
                                    //we will eventually change this
                                    desfireAccess.SelectApplication(new byte[] { 0x00, 0x00, 0x00 }); //select pic application

                                    //sessionKey = desfireAccess.AuthenticateAES();
                                    //Logger.Log("Session Key:  " + Formatting.ByteArrayToHexString(sessionKey));

                                    desfireAccess.GetKeyVersion(0x00);
                                    desfireAccess.GetKeySettings();
                                    desfireAccess.GetApplicationIDs();
                                    desfireAccess.GetFileIDs();

                                    desfireAccess.CreateApplication(new byte[] { 0x00, 0x00, 0x01 });
                                    desfireAccess.SelectApplication(new byte[] { 0x00, 0x00, 0x01 }); //select installed application
                                    desfireAccess.CreateFile(0x01);

                                    OnProcessCompleted(new TerminalProcessingOutcome()
                                    {
                                        NextProcessState = EMVTerminalPreProcessingStateEnum.EndProcess,
                                        UIRequestOnOutcomePresent = true,
                                        UserInterfaceRequest = new UserInterfaceRequest()
                                        {
                                            MessageIdentifier = MessageIdentifiersEnum.ClearDisplay,
                                            Status = StatusEnum.ReadyToRead
                                        }
                                    });
                                    break;

                                default:
                                    throw new DesFireException("Unrecognised desfire transaction type: " + desFireTransactionType);
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

            OnUserInterfaceRequest(new UIMessageEventArgs(MessageIdentifiersEnum.PresentCard, StatusEnum.ReadyToRead));

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
    }
}

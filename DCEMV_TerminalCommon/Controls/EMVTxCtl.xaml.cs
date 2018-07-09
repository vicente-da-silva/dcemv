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
using DCEMV.EMVProtocol;
using DCEMV.EMVProtocol.Kernels;
using DCEMV.ISO7816Protocol;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DCEMV.TLVProtocol;
using Xamarin.Forms;
using DCEMV.FormattingUtils;
using System.ComponentModel;

namespace DCEMV.TerminalCommon
{
    public class TotalAmountViewModel : INotifyPropertyChanged
    {
        private string total;
        public string Total
        {
            get { return total; }
            set
            {
                total = value;
                OnPropertyChanged("Total");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    public enum InterFaceType
    {
        Contact,
        Contactless,
        Cancelled
    }
    public enum TxResult
    {
        Approved,
        Declined,
        Error,
        Cancelled,
        ContactlessOnline,
        ContactlessMagOnline,
    }
    public class TxCompletedEventArgs : EventArgs
    {
        public TxCompletedEventArgs(TxResult txResult, InterFaceType interFaceType, Optional<TLV> emvData)
        {
            TxResult = txResult;
            InterFaceType = interFaceType;
            EMV_Data = emvData;
        }
        public TxResult TxResult { get; private set; }
        public InterFaceType InterFaceType { get; private set; }
        public Optional<TLV> EMV_Data { get; private set; }
    }
    public class UICallbackProvider : IUICallbackProvider
    {
        private Func<List<string>, string> displayApplicationListCb;

        public UICallbackProvider(Func<List<string>, string> displayApplicationListCb)
        {
            this.displayApplicationListCb = displayApplicationListCb;
        }

        public string DisplayApplicationList(List<string> list)
        {
            return displayApplicationListCb.Invoke(list);
        }
    }
    public class CardAppVM
    {
        public string AppName { get; set; }
        public string AID { get; set; }
    }

    public partial class EMVTxCtl : Grid
    {
        private static Logger Logger = new Logger(typeof(EMVTxCtl));

        private EMVContactlessTerminalApplication contactlessCardApp;
        private EMVContactTerminalApplication contactCardApp;
        private IUICallbackProvider uiProvider;

        private TaskCompletionSource<string> appSelectionTCS;
        private TaskCompletionSource<string> pinEntryTCS;

        private IConfigurationProvider configProvider;
        private ICardInterfaceManger contactCardInterfaceManger;
        private ICardInterfaceManger contactlessCardInterfaceManger;
        private IOnlineApprover onlineContactEMVApprover;
        private TCPClientStream tcpClientStream;

        private TotalAmountViewModel totalAmount;
        private Queue<UIMessageEventArgs> statusMessages = new Queue<UIMessageEventArgs>();
        private CancellationTokenSource cancellationTokenForStatusMessage;

        public event EventHandler TxCompleted;

        private TxResult txResult;

        public EMVTxCtl()
        {
            InitializeComponent();
        }

        #region Contactless App
        private void StartContactlessPaymentApp(TransactionRequest tr, string deviceID)
        {
            try
            {
                if (contactlessCardInterfaceManger == null)
                    return;

                contactlessCardApp = new EMVContactlessTerminalApplication(new CardQProcessor(contactlessCardInterfaceManger, deviceID), configProvider);
                contactlessCardApp.UserInterfaceRequest += ContactlessApp_UserInterfaceRequest;
                contactlessCardApp.ProcessCompleted += ContactlessApp_ProcessCompleted;
                contactlessCardApp.ExceptionOccured += ContactlessApp_ExceptionOccured;
                contactlessCardApp.StartTransactionRequest(tr);
            }
            catch (Exception ex)
            {
                SetStatusLabel(MakeUIMessage(ex), InterFaceType.Contact);
            }
        }
        private void StopContactlessPaymentApp()
        {
            if (contactlessCardApp != null)
                contactlessCardApp.StopTerminalApplication();
        }
        private void ContactlessApp_ExceptionOccured(object sender, EventArgs e)
        {
            try
            {
                SetStatusLabel(MakeUIMessage((e as ExceptionEventArgs).Exception), InterFaceType.Contactless);
            }
            catch
            {
                SetStatusLabel(new UIMessageEventArgs(MessageIdentifiersEnum.InsertSwipeOrTryAnotherCard, StatusEnum.EndProcessing), InterFaceType.Contactless);
            }
        }
        private void ContactlessApp_UserInterfaceRequest(object sender, EventArgs e)
        {
            SetStatusLabel(e as UIMessageEventArgs, InterFaceType.Contactless);
        }
        private void ContactlessApp_ProcessCompleted(object sender, EventArgs e)
        {
            ProcessCompletion(e as TerminalProcessingOutcomeEventArgs, InterFaceType.Contactless);
        }
        #endregion

        #region Contact App
        private void StartContactPaymentApp(TransactionRequest tr, string deviceID)
        {
            try
            {
                if (contactCardInterfaceManger == null)
                    return;

                contactCardApp = new EMVContactTerminalApplication(new CardQProcessor(contactCardInterfaceManger, deviceID), configProvider, uiProvider);
                contactCardApp.UserInterfaceRequest += ContactApp_UserInterfaceRequest;
                contactCardApp.ProcessCompleted += ContactApp_ProcessCompleted;
                contactCardApp.ExceptionOccured += ContactApp_ExceptionOccured;
                contactCardApp.PinRequest += ContactApp_PinRequest;
                contactCardApp.TRMRequest += ContactApp_TRMRequest;
                contactCardApp.OnlineRequest += ContactApp_OnlineRequest;
                contactCardApp.StartTransactionRequest(tr);
            }
            catch (Exception ex)
            {
                SetStatusLabel(MakeUIMessage(ex), InterFaceType.Contact);
            }
        }
        private void StopContactPaymentApp()
        {
            if (contactCardApp != null)
                contactCardApp.StopTerminalApplication();
        }
        private void ContactApp_OnlineRequest(object sender, EventArgs e)
        {
            ApproverResponse onlineResponse = GoOnlineForContactEMV(InterFaceType.Contact, ((OnlineEventArgs)e).data, ((OnlineEventArgs)e).discretionaryData);
            if (onlineResponse == null)
            {
                contactCardApp.DoOnlineReponse(KernelOnlineResponseType.UnableToGoOnline,
                    TLV.Create(EMVTagsEnum.AUTHORISATION_RESPONSE_CODE_8A_KRN.Tag),
                    TLV.Create(EMVTagsEnum.ISSUER_AUTHENTICATION_DATA_91_KRN.Tag),
                    TLV.Create(EMVTagsEnum.ISSUER_SCRIPT_TEMPLATE_1_71_KRN.Tag),
                    TLV.Create(EMVTagsEnum.ISSUER_SCRIPT_TEMPLATE_2_72_KRN.Tag));
            }
            else
            {
                if (onlineResponse.IsApproved)
                    contactCardApp.DoOnlineReponse(KernelOnlineResponseType.Approve, onlineResponse.AuthCode_8A, onlineResponse.IssuerAuthData_91, onlineResponse.IssuerScriptTemplate_71, onlineResponse.IssuerScriptTemplate_72);
                else
                    contactCardApp.DoOnlineReponse(KernelOnlineResponseType.Decline, onlineResponse.AuthCode_8A, onlineResponse.IssuerAuthData_91, onlineResponse.IssuerScriptTemplate_71, onlineResponse.IssuerScriptTemplate_72);
            }
        }
        private void ContactApp_TRMRequest(object sender, EventArgs e)
        {
            contactCardApp.DoTRMReponse();
        }
        private void CapturePin()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                gridAppList.IsVisible = false;
                gridTapInsertCard.IsVisible = false;
                gridPin.IsVisible = true;
            });

            pinEntryTCS = new TaskCompletionSource<string>();
            if (!pinEntryTCS.Task.Wait(60000))
            {
                gridAppList.IsVisible = false;
                gridTapInsertCard.IsVisible = true;
                gridPin.IsVisible = false;
            }
        }
        private void ContactlessApp_PinRequest(TLVList data)
        {
            CapturePin();
            //TODO: encrypt the pin
            TLV pin = TLV.Create(EMVTagsEnum.TRANSACTION_PERSONAL_IDENTIFICATION_NUMBER_PIN_DATA_99_KRN.Tag, Formatting.HexStringToByteArray(pinEntryTCS.Task.Result));
            data.AddToList(pin);
        }
        private void ContactApp_PinRequest(object sender, EventArgs e)
        {
            //if (string.IsNullOrEmpty(txtPin.Text))
            //{
            CapturePin();
            contactCardApp.DoPinReponse(pinEntryTCS.Task.Result);
            //}
            //else
            //{
            //    contactCardApp.DoPinReponse(txtPin.Text);
            //}
        }
        private void ContactApp_ProcessCompleted(object sender, EventArgs e)
        {
            ProcessCompletion(e as TerminalProcessingOutcomeEventArgs, InterFaceType.Contact);
        }
        private void ContactApp_UserInterfaceRequest(object sender, EventArgs e)
        {
            SetStatusLabel(e as UIMessageEventArgs, InterFaceType.Contact);
        }
        private void ContactApp_ExceptionOccured(object sender, EventArgs e)
        {
            try
            {
                SetStatusLabel(MakeUIMessage((e as ExceptionEventArgs).Exception), InterFaceType.Contact);
            }
            catch
            {
                SetStatusLabel(new UIMessageEventArgs(MessageIdentifiersEnum.TryAgain, StatusEnum.EndProcessing), InterFaceType.Contact);
            }
        }
        private string ContactApp_DisplayAppsOnCard(List<String> cardApps)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                gridAppList.IsVisible = true;
                gridTapInsertCard.IsVisible = false;
                gridPin.IsVisible = false;
                viewAppList.ItemsSource = null;
                List<CardAppVM> listView = new List<CardAppVM>();
                cardApps.ForEach(x =>
                {
                    string[] names = x.Split(':');
                    listView.Add(new CardAppVM() { AppName = names[0], AID = names[1] });
                    viewAppList.ItemsSource = listView;
                });
            });

            appSelectionTCS = new TaskCompletionSource<string>();
            if (!appSelectionTCS.Task.Wait(60000))
            {
                gridAppList.IsVisible = false;
                gridTapInsertCard.IsVisible = true;
                gridPin.IsVisible = false;
            }
            return appSelectionTCS.Task.Result;
        }
        private async void CmdOk_ApppList_Clicked(object sender, EventArgs e)
        {
            if (viewAppList.SelectedItem == null)
            {
                //await App.Current.MainPage.DisplayAlert("Error", "No app selected", "OK");
                return;
            }

            gridAppList.IsVisible = false;
            gridTapInsertCard.IsVisible = true;
            gridPin.IsVisible = false;

            string selectedApp = ((CardAppVM)viewAppList.SelectedItem).AID;
            appSelectionTCS.SetResult(selectedApp);
        }
        private void CmdCancel_ApppList_Clicked(object sender, EventArgs e)
        {
            string selectedApp = "";
            gridAppList.IsVisible = false;
            gridTapInsertCard.IsVisible = true;
            gridPin.IsVisible = false;
            appSelectionTCS.SetResult(selectedApp);
        }
        private void CmdOk_Pin_Clicked(object sender, EventArgs e)
        {
            gridAppList.IsVisible = false;
            gridTapInsertCard.IsVisible = true;
            gridPin.IsVisible = false;
            pinEntryTCS.SetResult(txtPin.Text);
        }
        private void CmdCancel_Pin_Clicked(object sender, EventArgs e)
        {
            gridAppList.IsVisible = false;
            gridTapInsertCard.IsVisible = true;
            gridPin.IsVisible = false;
            pinEntryTCS.SetResult("");
        }
        #endregion

        #region Shared
        private ApproverResponse GoOnlineForContactEMV(InterFaceType interfaceType, TLV data, TLV discretionary)
        {
            try
            {
                #region Merge EMV Lists
                if (discretionary != null)
                    data.Children.AddListToList(discretionary.Children);
                #endregion

                ApproverResponse onlineResponse = onlineContactEMVApprover.DoAuth(
                                        new ApproverRequest()
                                        {
                                            EMV_Data = data,
                                            TCPClientStream = tcpClientStream
                                        });
                return onlineResponse;
            }
            catch (Exception ex)
            {
                Logger.Log("Unable to go online:" + ex.Message);
                return null;
            }
        }
        private void ProcessCompletion(TerminalProcessingOutcomeEventArgs e, InterFaceType interFaceType)
        {
            TLV data = null;
            TLV discretionaryData = null;
            try
            {
                long? amount = Convert.ToInt64(totalAmount.Total);

                TerminalProcessingOutcome tpo = e.TerminalProcessingOutcome;
                if (tpo == null)//error occurred, error displayed via Ta_ExceptionOccured
                    return;

                if (tpo is EMVTerminalProcessingOutcome)
                {
                    data = ((EMVTerminalProcessingOutcome)tpo).DataRecord;
                    discretionaryData = ((EMVTerminalProcessingOutcome)tpo).DiscretionaryData;

                    if (data != null) //error
                    {
                        SetStatusLabel(new UIMessageEventArgs(MessageIdentifiersEnum.RemoveCard, StatusEnum.EndProcessing), interFaceType);

                        //may be a contactless magstripe transaction
                        if (data.Children.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag) == null)
                        {
                            txResult = TxResult.ContactlessMagOnline;
                        }
                        else
                        {
                            if (((data.Children.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag).Value[0] & 0xC0) >> 6) == (byte)ACTypeEnum.ARQC)
                            {
                                if (interFaceType == InterFaceType.Contact)
                                    throw new EMVProtocolException("Invalid state for contact, gen ac 2 returned arqc?");

                                CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN cvmr = new CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN(data.Children.Get(EMVTagsEnum.CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN.Tag));
                                if(cvmr.Value.GetCVMPerformed() == CVMCode.EncipheredPINVerifiedOnline)
                                    ContactlessApp_PinRequest(data.Children);

                                txResult = TxResult.ContactlessOnline;
                            }
                            if (((data.Children.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag).Value[0] & 0xC0) >> 6) == (byte)ACTypeEnum.TC)
                            {
                                txResult = TxResult.Approved;
                            }
                            if (((data.Children.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag).Value[0] & 0xC0) >> 6) == (byte)ACTypeEnum.AAC)
                            {
                                txResult = TxResult.Declined;
                            }
                        }
                    }
                    else
                    {
                        txResult = TxResult.Declined;
                    }

                    SetTxFinalResultLabel(txResult.ToString());
                }
                else
                {
                    SetStatusLabel(new UIMessageEventArgs(tpo.UserInterfaceRequest.MessageIdentifier, tpo.UserInterfaceRequest.Status), interFaceType);
                }
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    UIMessageEventArgs ui = new UIMessageEventArgs(MessageIdentifiersEnum.TryAgain, StatusEnum.ProcessingError) { AdditionalMessage = ex.Message };
                    SetStatusLabel(ui, interFaceType);
                });
            }
            finally
            {
                StopContactPaymentApp();
                StopContactlessPaymentApp();

                #region Merge EMV Lists
                if(discretionaryData!=null)
                    if(data!=null)
                        data.Children.AddListToList(discretionaryData.Children);
                #endregion

                TxCompleted?.Invoke(this, new TxCompletedEventArgs(txResult, interFaceType, Optional<TLV>.Create(data)));
            }
        }
        #endregion

        #region UI Code
        public void SetTxFinalResultLabel(String message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lblStatusFinalResult.Text = message;
            });
        }
        public void SetTxStartLabel(String message)
        {
            lblStatusTapPaymentCard.Text = message;
        }
        private void SetStatusLabel(UIMessageEventArgs message, InterFaceType interFaceType)
        {
            foreach (UIMessageEventArgs ui in statusMessages.ToArray())
            {
                if (ui.MessageIdentifiers == message.MessageIdentifiers)
                    return;
            }
            Logger.Log("SetStatusLabel: " + interFaceType + " - " + message.MessageIdentifiers);
            if (message.MessageIdentifiers == MessageIdentifiersEnum.SeePhone)
                SetTxFinalResultLabel("See Phone");
            statusMessages.Enqueue(message);
        }
        private void StartStatusMessageProcessor()
        {
            Task.Run(() =>
            {
                while (1 == 1)
                {
                    if (statusMessages.Count > 0)
                    {
                        UIMessageEventArgs e = statusMessages.Dequeue();
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            lblStatusPaymentApp.Text = MapMessageIdentifier(e.MessageIdentifiers);//.MakeMessage();
                        });

                        if (e.MessageIdentifiers != MessageIdentifiersEnum.ClearDisplay)
                        {
                            if (e.HoldTime > 0)
                            {
                                Task.Delay(checked((int)e.HoldTime)).Wait();
                                //Task.Delay(2000).Wait();
                            }
                            else
                            {
                                Task.Delay(500).Wait();
                            }
                        }
                    }
                    else
                    {
                        Task.Delay(500).Wait();

                        if (cancellationTokenForStatusMessage.Token.IsCancellationRequested)
                        {
                            break;
                        }
                    }

                    if (cancellationTokenForStatusMessage.Token.IsCancellationRequested)
                    {
                        break;
                    }
                }
            });
        }
        private string MapMessageIdentifier(MessageIdentifiersEnum messageIdentifiers)
        {
            switch (messageIdentifiers)
            {
                case MessageIdentifiersEnum.PleaseInsertOrSwipeCard: return "Please Insert Or Swipe Card";
                case MessageIdentifiersEnum.PresentCard: return "Present Card";
                case MessageIdentifiersEnum.PleasePresentOneCardOnly: return "Please Present One Card Only";
                case MessageIdentifiersEnum.InsertSwipeOrTryAnotherCard: return "Insert Swipe Or Try Another Card";
                case MessageIdentifiersEnum.ClearDisplay: return "";
                case MessageIdentifiersEnum.CardRemoved: return "Card Removed";
                case MessageIdentifiersEnum.CardInserted: return "Card Inserted";
                case MessageIdentifiersEnum.TransmissionError: return "Transmission Error";
                case MessageIdentifiersEnum.Approved: return "Approved";
                case MessageIdentifiersEnum.CardReadOk: return "Card Read OK";
                case MessageIdentifiersEnum.TryAgain: return "Try Again";
                case MessageIdentifiersEnum.ApprovedSign: return "Approved and Signature Required";
                case MessageIdentifiersEnum.Declined: return "Declined";
                case MessageIdentifiersEnum.SeePhone: return "See Phone";
                case MessageIdentifiersEnum.Authorizing: return "Authorizing";
                case MessageIdentifiersEnum.NA: return "";
                case MessageIdentifiersEnum.RemoveCard: return "Remove Card";

                default:
                    return "";
            }
        }
        private UIMessageEventArgs MakeUIMessage(Exception ex)
        {
            return new UIMessageEventArgs(MessageIdentifiersEnum.TryAgain, StatusEnum.EndProcessing) { AdditionalMessage = ex.Message };
        }
        #endregion

        #region App Start, Stop and Events
        public void Start(TransactionRequest tr, 
            ICardInterfaceManger contactCardInterfaceManger, string contactDeviceId, 
            ICardInterfaceManger contactlessCardInterfaceManger, string contactlessDeviceId,
            IConfigurationProvider configProvider, IOnlineApprover onlineContactEMVApprover, TCPClientStream tcpClientStream)
        {
            txResult = TxResult.Error;

            if (contactCardInterfaceManger == null && contactlessCardInterfaceManger == null)
                throw new EMVProtocolException("EMVTest: cannot have both CardInterfaceManger's as null");

            this.contactCardInterfaceManger = contactCardInterfaceManger;
            this.contactlessCardInterfaceManger = contactlessCardInterfaceManger;
            this.configProvider = configProvider;
            this.onlineContactEMVApprover = onlineContactEMVApprover;
            this.tcpClientStream = tcpClientStream;
            uiProvider = new UICallbackProvider(ContactApp_DisplayAppsOnCard);

            gridAppList.IsVisible = false;
            gridPin.IsVisible = false;
            gridProgress.IsVisible = false;
            totalAmount = new TotalAmountViewModel();
            txtPin.Text = "";
            lblTotal.BindingContext = totalAmount;
            totalAmount.Total = Convert.ToString(tr.GetAmountAuthorized_9F02());
            lblStatusTapPaymentCard.Text = "Please Tap or Insert the card you wish to make the payment with.";

            txtPin.Text = "4315";
            //txtPin.Text = "7320";

            cancellationTokenForStatusMessage = new CancellationTokenSource();
            StartStatusMessageProcessor();

            StartContactPaymentApp(tr, contactDeviceId);
            StartContactlessPaymentApp(tr, contactlessDeviceId);
        }
        public void Stop()
        {
            StopContactPaymentApp();
            StopContactlessPaymentApp();

            if (cancellationTokenForStatusMessage != null && !cancellationTokenForStatusMessage.IsCancellationRequested)
                cancellationTokenForStatusMessage.Cancel();
        }
        private void cmdCancelTx_Clicked(object sender, EventArgs e)
        {
            Stop();
            txResult = TxResult.Cancelled;
            SetTxFinalResultLabel("");
            SetStatusLabel(new UIMessageEventArgs(MessageIdentifiersEnum.ClearDisplay, StatusEnum.EndProcessing), InterFaceType.Cancelled);
            TxCompleted?.Invoke(this, new TxCompletedEventArgs(txResult, InterFaceType.Cancelled, Optional<TLV>.CreateEmpty()));
        }
        #endregion
    }
}

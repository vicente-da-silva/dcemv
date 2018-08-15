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
using ZXing.Net.Mobile.Forms;
using ZXing;
using DCEMV.EMVProtocol.EMVQRCode;
using DCEMV_QRDEProtocol;

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
        QRCodeToPoll,
        QRCodeScanned,
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
        QRCodeToPoll,
        QRCodeScanned,
    }
    public class TxCompletedEventArgs : EventArgs
    {
        public TxCompletedEventArgs(TxResult txResult, InterFaceType interFaceType, Optional<TLV> emvData, Optional<QRDEList> qr_Data)
        {
            TxResult = txResult;
            InterFaceType = interFaceType;
            EMV_Data = emvData;
            QR_Data = qr_Data;
        }
        public TxResult TxResult { get; private set; }
        public InterFaceType InterFaceType { get; private set; }
        public Optional<TLV> EMV_Data { get; private set; }
        public Optional<QRDEList> QR_Data { get; private set; }
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
    public enum QRCodeMode
    {
        PresentAndPoll,
        ScanAndProcess,
        None
    }
    public enum ViewState
    {
        Step1TransactDetails,
        Step2TapCard,
        AppList,
        Pin
    }

    public partial class EMVTxCtl : Grid
    {
        private static Logger Logger = new Logger(typeof(EMVTxCtl));

        private EMVContactlessTerminalApplication contactlessCardApp;
        private EMVContactTerminalApplication contactCardApp;

        private EMVTerminalQRCodePollApplication qrCodePollApp;
        private EMVTerminalQRCodeScanApplication qrCodeScanApp;
        private ZXingScannerPage barcodeScannerPage;
        private string barcodeValue;
        private QRCodeMode qrCodeMode;
        private bool qrCodeScanned;

        private IUICallbackProvider uiProvider;

        private TaskCompletionSource<string> appSelectionTCS;
        private TaskCompletionSource<string> pinEntryTCS;

        private IConfigurationProvider configProvider;
        private ICardInterfaceManger contactCardInterfaceManger;
        private ICardInterfaceManger contactlessCardInterfaceManger;
        private IOnlineApprover onlineContactEMVApprover;
        private TCPClientStream tcpClientStream;

        private string contactDeviceId;
        private string contactlessDeviceId;
        private string accountNumberInUse;

        private TotalAmountViewModel totalAmount;
        private Queue<UIMessageEventArgs> statusMessages = new Queue<UIMessageEventArgs>();
        private CancellationTokenSource cancellationTokenForStatusMessage;

        private TransactionRequest tr;
        public event EventHandler TxCompleted;
        private TxResult txResult;

        public EMVTxCtl()
        {
            InitializeComponent();
            QRMetaDataSourceSingleton.Instance.DataSource = new EMVQRMetaDataSource();
            TLVMetaDataSourceSingleton.Instance.DataSource = new EMVTLVMetaDataSource();

            uiProvider = new UICallbackProvider(ContactApp_DisplayAppsOnCard);

            totalAmount = new TotalAmountViewModel();
            totalAmount.Total = "";
            txtAmount.BindingContext = totalAmount;
            lblTotal.BindingContext = totalAmount;

            totalAmount.Total = "1000";

            //if(!String.IsNullOrEmpty(totalAmount.Total))
            //{
            //    CmdNextToPaymentApp_Clicked(null,null);
            //}
            //else
            //{
            lblStatusAskAmount.Text = "Enter the amount below";
            //}
        }

        #region QR Code Scan App
        private void StartQRCodeScanPaymentApp(string barcode)
        {
            try
            {
                qrCodeScanApp = new EMVTerminalQRCodeScanApplication();
                qrCodeScanApp.ExceptionOccured += QRCodeScanApp_ExceptionOccured;
                qrCodeScanApp.ProcessCompleted += QRCodeScanApp_ProcessCompleted;
                qrCodeScanApp.StartTransactionRequest(ref tr, barcode);
                totalAmount.Total = Convert.ToString(tr.GetAmountAuthorized_9F02());
                UpdateView(ViewState.Step2TapCard);
            }
            catch (Exception ex)
            {
                SetStatusLabel(MakeUIMessage(ex), InterFaceType.QRCodeScanned);
            }
        }
        private void QRCodeScanApp_ExceptionOccured(object sender, EventArgs e)
        {
            try
            {
                SetStatusLabel(MakeUIMessage((e as ExceptionEventArgs).Exception), InterFaceType.QRCodeScanned);
            }
            catch
            {
                SetStatusLabel(new UIMessageEventArgs(MessageIdentifiersEnum.TryAgain, StatusEnum.EndProcessing), InterFaceType.QRCodeScanned);
            }
        }
        private void QRCodeScanApp_ProcessCompleted(object sender, EventArgs e)
        {
            ProcessCompletion(e as TerminalProcessingOutcomeEventArgs, InterFaceType.QRCodeScanned);
        }
        #endregion

        #region QR Code Poll App
        private void StartQRCodePollPaymentApp()
        {
            try
            {
                qrCodePollApp = new EMVTerminalQRCodePollApplication();
                qrCodePollApp.ExceptionOccured += QRCodePollApp_ExceptionOccured;
                qrCodePollApp.ProcessCompleted += QRCodePollApp_ProcessCompleted;
                qrCodePollApp.StartTransactionRequest(tr, barcodeValue);
            }
            catch (Exception ex)
            {
                SetStatusLabel(MakeUIMessage(ex), InterFaceType.QRCodeToPoll);
            }
        }
        private void StopQRCodePollPaymentApp()
        {
            if (qrCodePollApp != null)
                qrCodePollApp.StopTerminalApplication();
        }
        private void QRCodePollApp_ExceptionOccured(object sender, EventArgs e)
        {
            try
            {
                SetStatusLabel(MakeUIMessage((e as ExceptionEventArgs).Exception), InterFaceType.QRCodeToPoll);
            }
            catch
            {
                SetStatusLabel(new UIMessageEventArgs(MessageIdentifiersEnum.TryAgain, StatusEnum.EndProcessing), InterFaceType.QRCodeToPoll);
            }
        }
        private void QRCodePollApp_ProcessCompleted(object sender, EventArgs e)
        {
            ProcessCompletion(e as TerminalProcessingOutcomeEventArgs, InterFaceType.QRCodeToPoll);
        }
        private string PresentBarcode(string accountToPresent)
        {
            if (tr != null)
            {
                barcodeValue = BuildQRCodeValue(tr.GetAmountAuthorized_9F02(), accountToPresent, GuidBuilder.Create());
                if (!String.IsNullOrEmpty(barcodeValue))
                {
                    zxingBIV.BarcodeOptions.Hints.Clear();
                    zxingBIV.BarcodeOptions.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
                    zxingBIV.BarcodeValue = barcodeValue;
                    zxingBIV.BarcodeOptions.Width = 600;
                    zxingBIV.BarcodeOptions.Height = 600;
                }
            }
            return barcodeValue;
        }
        public static string BuildQRCodeValue(long amount, string accountToPresent,string trackingId)
        {
            try
            {
                QRDEList listIn = new QRDEList();

                listIn.AddToList(QRDE.Create(EMVQRTagsEnum.PAYLOAD_FORMAT_INDICATOR_00, "01"));
                listIn.AddToList(QRDE.Create(EMVQRTagsEnum.POINT_OF_INITIATION_METHOD_01, "12"));

                QRDE _26 = QRDE.Create(EMVQRTagsEnum.MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_26);
                QRDE.Create(EMVQRTagsEnum.GLOBALLY_UNIQUE_IDENTIFIER_00, accountToPresent, _26);
                EMVQRTagsEnum.CreateUnknown(TagId._05, trackingId, _26); //tracking id
                listIn.AddToList(_26);

                //listIn.AddToList(QRDE.Create(EMVQRTagsEnum.MERCHANT_CATEGORY_CODE_52, "4111"));
                //listIn.AddToList(QRDE.Create(EMVQRTagsEnum.COUNTRY_CODE_58, "CN"));
                listIn.AddToList(QRDE.Create(EMVQRTagsEnum.TRANSACTION_AMOUNT_54, Convert.ToString(amount)));
                //listIn.AddToList(QRDE.Create(EMVQRTagsEnum.TRANSACTION_CURRENCY_53, "156"));

                listIn.AddToList(QRDE.Create(EMVQRTagsEnum.CRC_63, "0000"));

                return listIn.Serialize();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                return string.Empty;
            }
        }
        #endregion

        #region Contactless App
        private void StartContactlessPaymentApp(string deviceID)
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
        private void StartContactPaymentApp(string deviceID)
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
            EMVApproverResponse onlineResponse = GoOnlineForContactEMV(InterFaceType.Contact, ((OnlineEventArgs)e).data, ((OnlineEventArgs)e).discretionaryData);
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
                UpdateView(ViewState.Pin);
            });

            pinEntryTCS = new TaskCompletionSource<string>();
            if (!pinEntryTCS.Task.Wait(60000))
            {
                UpdateView(ViewState.Step2TapCard);
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
                UpdateView(ViewState.AppList);
               
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
                UpdateView(ViewState.Step2TapCard);
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

            UpdateView(ViewState.Step2TapCard);

            string selectedApp = ((CardAppVM)viewAppList.SelectedItem).AID;
            appSelectionTCS.SetResult(selectedApp);
        }
        private void CmdCancel_ApppList_Clicked(object sender, EventArgs e)
        {
            string selectedApp = "";

            UpdateView(ViewState.Step2TapCard);

            appSelectionTCS.SetResult(selectedApp);
        }
        private void CmdOk_Pin_Clicked(object sender, EventArgs e)
        {
            UpdateView(ViewState.Step2TapCard);
            pinEntryTCS.SetResult(txtPin.Text);
        }
        private void CmdCancel_Pin_Clicked(object sender, EventArgs e)
        {
            UpdateView(ViewState.Step2TapCard);
            pinEntryTCS.SetResult("");
        }
        #endregion

        #region Shared
        private EMVApproverResponse GoOnlineForContactEMV(InterFaceType interfaceType, TLV data, TLV discretionary)
        {
            try
            {
                #region Merge EMV Lists
                if (discretionary != null)
                    data.Children.AddListToList(discretionary.Children);
                #endregion

                EMVApproverResponse onlineResponse = (EMVApproverResponse)onlineContactEMVApprover.DoAuth(
                                        new EMVApproverRequest()
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
            QRDEList qrData = null;

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
                    qrData = ((EMVTerminalProcessingOutcome)tpo).QRData;

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
                                if (cvmr.Value.GetCVMPerformed() == CVMCode.EncipheredPINVerifiedOnline)
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
                    else if (qrData != null)
                    {
                        if (interFaceType == InterFaceType.QRCodeScanned)
                        {
                            txResult = TxResult.QRCodeScanned;
                        }
                        else if (interFaceType == InterFaceType.QRCodeToPoll)
                        {
                            txResult = TxResult.QRCodeToPoll;
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
                StopQRCodePollPaymentApp();

                #region Merge EMV Lists
                if (discretionaryData!=null)
                    if(data!=null)
                        data.Children.AddListToList(discretionaryData.Children);
                #endregion

                TxCompleted?.Invoke(this, new TxCompletedEventArgs(txResult, interFaceType, Optional<TLV>.Create(data), Optional<QRDEList>.Create(qrData)));
            }
        }
        #endregion

        #region UI Code
        private void Start()
        {
            txResult = TxResult.Error;
            cancellationTokenForStatusMessage = new CancellationTokenSource();

            //txtPin.Text = "";
            txtPin.Text = "4315";

            StartStatusMessageProcessor();
            StartContactPaymentApp(contactDeviceId);
            StartContactlessPaymentApp(contactlessDeviceId);

            UpdateView(ViewState.Step2TapCard);//call after TransactionRequest is created
        }
        private void CmdNextToPaymentApp_Clicked(object sender, EventArgs e)
        {
            long amount;
            if (!Int64.TryParse(totalAmount.Total, out amount))
            {
                lblStatusAskAmount.Text = "Enter the amount below without decimals";
                return;
            }

            long amountOther = 0;
            tr = new TransactionRequest(amount + amountOther, amountOther, TransactionTypeEnum.PurchaseGoodsAndServices);
            totalAmount.Total = Convert.ToString(tr.GetAmountAuthorized_9F02());

            Start();
        }
        private void UpdateView(ViewState viewState)
        {
            lblStatusTapPaymentCard.Text =
                "Complete the transaction by:\n" +
                "Tapping or Inserting their card on/in your card reader or\n" +
                "Tapping their phone on your card reader";

            switch (viewState)
            {
                case ViewState.Step1TransactDetails:
                    gridTransactDetails.IsVisible = true;
                    gridTapInsertCard.IsVisible = false;
                    gridAppList.IsVisible = false;
                    gridPin.IsVisible = false;
                    break;

                case ViewState.AppList:
                    gridAppList.IsVisible = true;
                    gridPin.IsVisible = false;
                    gridTransactDetails.IsVisible = false;
                    gridTapInsertCard.IsVisible = false;
                    break;

                case ViewState.Pin:
                    gridPin.IsVisible = true;
                    gridAppList.IsVisible = false;
                    gridTransactDetails.IsVisible = false;
                    gridTapInsertCard.IsVisible = false;
                    break;

                case ViewState.Step2TapCard:
                    gridTransactDetails.IsVisible = false;
                    gridTapInsertCard.IsVisible = true;
                    gridAppList.IsVisible = false;
                    gridPin.IsVisible = false;
                    break;
            }

            switch (qrCodeMode)
            {
                case QRCodeMode.PresentAndPoll:
                    lblStatusTapPaymentCard.Text = lblStatusTapPaymentCard.Text + " or\nHaving them scan this QR Code";
                    gridBarcode.IsVisible = true;
                    barcodeValue = PresentBarcode(accountNumberInUse);
                    cmdPollForQRCodeResult.IsVisible = true;
                    cmdScanQRCode.IsVisible = false;
                    break;

                case QRCodeMode.ScanAndProcess:
                    if (qrCodeScanned)
                    {
                        lblStatusTapPaymentCard.Text = "";
                        SetStatusLabel(new UIMessageEventArgs(MessageIdentifiersEnum.Authorizing, StatusEnum.EndProcessing), InterFaceType.QRCodeScanned);
                    }
                    gridBarcode.IsVisible = false;
                    cmdPollForQRCodeResult.IsVisible = false;
                    cmdScanQRCode.IsVisible = true;
                    break;

                case QRCodeMode.None:
                    gridBarcode.IsVisible = false;
                    cmdPollForQRCodeResult.IsVisible = false;
                    cmdScanQRCode.IsVisible = false;
                    break;
            }
        }
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
            Logger.Log("SetStatusLabel: " + interFaceType + " - " + message.MessageIdentifiers + " additional mesage:" + message.AdditionalMessage);
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
                            lblStatusPaymentApp.Text = MapMessageIdentifier(e.MessageIdentifiers) + (String.IsNullOrEmpty(e.AdditionalMessage)? "" : " - " + e.AdditionalMessage);
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
        public void Init(ICardInterfaceManger contactCardInterfaceManger, string contactDeviceId,
            ICardInterfaceManger contactlessCardInterfaceManger, string contactlessDeviceId,
            QRCodeMode qrCodeMode, string accountNumberInUse,
            IConfigurationProvider configProvider, IOnlineApprover onlineContactEMVApprover, 
            TCPClientStream tcpClientStream, TransactionRequest tr = null)
        {
            qrCodeScanned = false;

            this.contactDeviceId = contactDeviceId;
            this.contactlessDeviceId = contactlessDeviceId;
            this.qrCodeMode = qrCodeMode;
            this.contactCardInterfaceManger = contactCardInterfaceManger;
            this.contactlessCardInterfaceManger = contactlessCardInterfaceManger;
            this.configProvider = configProvider;
            this.onlineContactEMVApprover = onlineContactEMVApprover;
            this.tcpClientStream = tcpClientStream;
            this.accountNumberInUse = accountNumberInUse;

            if (tr != null)
            {
                this.tr = tr;
                totalAmount.Total = Convert.ToString(tr.GetAmountAuthorized_9F02());
                Start();
            }
            else
                UpdateView(ViewState.Step1TransactDetails);
        }

        public void SetASkAmountInstruction(string message)
        {
            lblStatusAskAmount.Text = message;
        }

        public void SetHeaderInstruction(string message)
        {
            lblStatusTapPaymentCard.Text = message;
        }

        
        private async void cmdScanQRCode_Clicked(object sender, EventArgs e)
        {
            //StartQRCodeScanPaymentApp(BuildQRCodeValue(2000, "5906374433f04eb5b67d25c3e50487dc", GuidBuilder.Create()));
            //qrCodeScanned = true;
            barcodeScannerPage = new ZXingScannerPage();
            barcodeScannerPage.OnScanResult += (result) =>
            {
                barcodeScannerPage.IsScanning = false;

                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopAsync();
                    StartQRCodeScanPaymentApp(result.Text);
                    qrCodeScanned = true;
                });
            };
            await Navigation.PushAsync(barcodeScannerPage);
        }

        private void cmdPollForQRCodeResult_Clicked(object sender, EventArgs e)
        {
            StartQRCodePollPaymentApp();
        }

        public void Stop()
        {
            StopContactPaymentApp();
            StopContactlessPaymentApp();
            StopQRCodePollPaymentApp();

            if (cancellationTokenForStatusMessage != null && !cancellationTokenForStatusMessage.IsCancellationRequested)
                cancellationTokenForStatusMessage.Cancel();

            qrCodeScanned = false;
            UpdateView(ViewState.Step1TransactDetails);
        }
        private void cmdCancelTx_Clicked(object sender, EventArgs e)
        {
            Stop();
            txResult = TxResult.Cancelled;
            SetTxFinalResultLabel("");
            SetStatusLabel(new UIMessageEventArgs(MessageIdentifiersEnum.ClearDisplay, StatusEnum.EndProcessing), InterFaceType.Cancelled);
            TxCompleted?.Invoke(this, new TxCompletedEventArgs(txResult, InterFaceType.Cancelled, Optional<TLV>.CreateEmpty(), Optional<QRDEList>.CreateEmpty()));
        }
        #endregion
    }
}

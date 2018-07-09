using Common;
using EMVProtocol;
using FormattingUtils;
using ISO7816Protocol;
using SPDHProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPIPDriver;
using TLVProtocol;
using Xamarin.Forms;

namespace XTerminal
{
    public partial class EMVMain : TabbedPage
    {
        public static Logger Logger = new Logger(typeof(EMVMain));

        private TransactionVM transactionVM;
        private EMVTerminalApplication ta;
        private ICardInterfaceManger cardInterfaceManger;
        private IOnlineApprover onlineApprover;
        private IConfigurationProvider configProvider;
        private TCPClientStream tcpClientStream;

        public EMVMain()
        {
            InitializeComponent();
        }
        public EMVMain(ICardInterfaceManger cardInterfaceManger, IOnlineApprover onlineApprover, IConfigurationProvider configProvider, TCPClientStream tcpClientStream)
        {
            InitializeComponent();

            dlgPin.IsVisible = false;
            this.cardInterfaceManger = cardInterfaceManger;
            this.onlineApprover = onlineApprover;
            this.tcpClientStream = tcpClientStream;
            this.configProvider = configProvider;
            transactionVM = new TransactionVM { Amount = 1000 };
            gridMain.BindingContext = transactionVM;
            cmbTransactionType.SelectedItem = transactionVM.TransactionTypes.First();
        }

        private async void CmdProcess_Click(object sender, EventArgs e)
        {
            try
            {
                SetProcessButtonEnabled(false);
                TransactionRequest tr = new TransactionRequest(transactionVM.Amount + transactionVM.AmountOther, transactionVM.AmountOther, (TransactionTypeEnum)cmbTransactionType.SelectedItem);
                List<string> ids = (await cardInterfaceManger.GetCardReaders()).Where(x=>x.ToLower().Contains("contactless")).ToList();
                if (ids.Count == 0)
                {
                    await App.Current.MainPage.DisplayAlert("Error", "No contactless reader found", "OK");
                    return;
                }
                ta = new EMVTerminalApplication(new CardQProcessor(cardInterfaceManger, ids[0]), configProvider);
                ta.UserInterfaceRequest += Ta_UserInterfaceRequest;
                ta.ProcessCompleted += Ta_ProcessCompleted;
                ta.ExceptionOccured += Ta_ExceptionOccured;
                ta.StartTransactionRequest(tr);
            }
            catch (Exception ex)
            {
                SetStatusLabel(ex.Message);
            }
        }

        private void CmdCancel_Click(object sender, EventArgs e)
        {
            try
            {
                if (ta != null)
                    ta.CancelTransactionRequest();
            }
            catch (Exception ex)
            {
                SetStatusLabel(ex.Message);
            }
        }

        private void Ta_ExceptionOccured(object sender, EventArgs e)
        {
            try
            {
                SetStatusLabel((e as ExceptionEventArgs).Exception.Message);
            }
            catch
            {
                SetStatusLabel("Terminal Error Occurred");
            }
            finally
            {
                SetProcessButtonEnabled(true);
            }
        }
        
        public enum ContentDialogResult
        {
            None,
            Primary,
            Secondary
        }

        ContentDialogResult result = ContentDialogResult.None;
        private void CmdPinOK_Click(object sender, EventArgs e)
        {
            result = ContentDialogResult.Primary;
        }

        private void CmdPinCancel_Click(object sender, EventArgs e)
        {
            result = ContentDialogResult.Secondary;
        }

        private async Task<ContentDialogResult> CapturePin()
        {
            result = ContentDialogResult.None;
            await Task.Run(
                () =>
                {
                    Device.BeginInvokeOnMainThread(() => dlgPin.IsVisible = true);
                    cmdPinOK.Clicked += CmdPinOK_Click;
                    cmdPinCancel.Clicked += CmdPinCancel_Click;
                    while (result == ContentDialogResult.None) { }
                })
                .ContinueWith(
                (x)=> 
                {
                    cmdPinOK.Clicked -= CmdPinOK_Click;
                    cmdPinCancel.Clicked -= CmdPinCancel_Click;
                    Device.BeginInvokeOnMainThread(() => dlgPin.IsVisible = false);
                });
            return result;
        }

        private void Ta_ProcessCompleted(object sender, EventArgs e)
        {
            try
            {
                EMVTerminalProcessingOutcome tpo = (EMVTerminalProcessingOutcome)(e as TerminalProcessingOutcomeEventArgs).TerminalProcessingOutcome;
                if (tpo == null)//error occurred, error displayed via Ta_ExceptionOccured
                    return;
                

                if (tpo.UIRequestOnOutcomePresent)
                    SetStatusLabel(string.Format("{0}\n{1}", tpo.UserInterfaceRequest.MessageIdentifier, tpo.UserInterfaceRequest.Status));

                TLV dataRecord = tpo.DataRecord;
                TLV discretionaryData = tpo.DiscretionaryData;

                if (dataRecord != null)
                {
                    int depth = 0;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(dataRecord.ToPrintString(ref depth));
                    depth = 0;
                    if(discretionaryData!=null) sb.AppendLine(discretionaryData.ToPrintString(ref depth));
                    SetCardDetailsLabel(sb.ToString());

                    if (dataRecord.Children.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN.Tag).Value[0] == (byte)TransactionTypeEnum.Refund) 
                    {
                        SetStatusLabel("Refund Processed");
                    }
                    else
                    {
                        if (((dataRecord.Children.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag).Value[0] & 0xC0) >> 6) == (byte)ACTypeEnum.ARQC) 
                        {
                            if (tpo.CVM == KernelCVMEnum.ONLINE_PIN)
                            {
                                ContentDialogResult pinResult = ContentDialogResult.None;
                                Task.Run(async () => pinResult = await CapturePin()).Wait();
                                if (pinResult != ContentDialogResult.Primary)
                                {
                                    SetStatusLabel("Pin Capture Cancelled - Cancelling transaction");
                                    SetProcessButtonEnabled(true);
                                    return;
                                }
                            }
                            SetCancelButtonEnabled(false);//need to change online request to run in a thread than can be cancelled, else we have to disable the button
                            ApproverResponse onlineResponse = onlineApprover.DoOnlineAuth(
                                new ApproverRequest()
                                {
                                    DataRecord = dataRecord,
                                    DiscretionaryData = discretionaryData,
                                    DefaultCurrency = ta.GetDefaultTLV(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN.Tag),
                                    TCPClientStream = tcpClientStream
                                });
                            SetStatusLabel(onlineResponse.ResponseMessage);
                        }
                        if (((dataRecord.Children.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag).Value[0] & 0xC0) >> 6) == (byte)ACTypeEnum.TC) 
                        {
                            SetCancelButtonEnabled(false);//need to change online request to run in a thread than can be cancelled, else we have to disable the button
                            ApproverResponse onlineResponse = onlineApprover.DoAdvice(
                                new ApproverRequest()
                                {
                                    DataRecord = dataRecord,
                                    DiscretionaryData = discretionaryData,
                                    DefaultCurrency = ta.GetDefaultTLV(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN.Tag),
                                    TCPClientStream = tcpClientStream
                                }, true);
                            SetStatusLabel(onlineResponse.ResponseMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ta.CancelTransactionRequest();
                SetStatusLabel(ex.Message);
            }
            finally
            {
                SetProcessButtonEnabled(true);
                SetCancelButtonEnabled(true);
            }
        }

        private void Ta_UserInterfaceRequest(object sender, EventArgs e)
        {
            SetStatusLabel((e as UIMessageEventArgs).Message);
            Task.Run(async () => await Task.Delay((int)(e as UIMessageEventArgs).HoldTime)).Wait();
        }

        private void SetStatusLabel(string text)
        {
            Device.BeginInvokeOnMainThread(() => { lblStatus.Text = text; });
        }
        private void SetCardDetailsLabel(string text)
        {
            Device.BeginInvokeOnMainThread(() => { lblCardDetails.Text = text; });
        }
        private void SetProcessButtonEnabled(bool enabled)
        {
            Device.BeginInvokeOnMainThread(() => { cmdProcess.IsEnabled = enabled; });
        }
        private void SetCancelButtonEnabled(bool enabled)
        {
            Device.BeginInvokeOnMainThread(() => { cmdCancel.IsEnabled = enabled; });
        }
    }
}

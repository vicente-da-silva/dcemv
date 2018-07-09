using Common;
using DesFireProtocol;
using FormattingUtils;
using ISO7816Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XServerCommon.Models;
using XTerminal.Proxies;

namespace XTerminal
{
   
   
    public partial class TransactViewDesfire : ModalPage
    {
        private ICardInterfaceManger cardInterfaceManger;
        private DesfireTerminalApplicationBase cardApp;
        private FlowType flowType;
        private TotalAmountViewModel totalAmount;

        public TransactViewDesfire(FlowType flowType, ICardInterfaceManger cardInterfaceManger)
        {
            InitializeComponent();
            totalAmount = new TotalAmountViewModel();
            gridProgress.IsVisible = false;

            this.cardInterfaceManger = cardInterfaceManger;
            this.flowType = flowType;

            lblTotal.BindingContext = totalAmount;
            txtAmount.BindingContext = totalAmount;

            UpdateView(ViewState.Step1TransactDetails);
        }

        private void UpdateView(ViewState viewState)
        {
            switch (flowType)
            {
                case FlowType.SendMoneyFromCardToApp:
                    lblHeaderTransact.Text = "Send money from their card to your account";
                    lblHeaderTapCard.Text = "Send money from their card to your account";
                    this.Title = "Receive Money";
                    lblStatusTapCard.Text = "Tap Card";
                    break;
                case FlowType.SendMoneyFromAppToCard:
                    lblHeaderTransact.Text = "Send money from your account to their card";
                    lblHeaderTapCard.Text = "Send money from your account to their card";
                    this.Title = "Send Money";
                    lblStatusTapCard.Text = "Tap Card";
                    break;
            }

            switch (viewState)
            {
                case ViewState.Step1TransactDetails:
                    gridTransactDetails.IsVisible = true;
                    gridTapCard.IsVisible = false;
                    gridTransactSummary.IsVisible = false;
                    break;

                case ViewState.Step2TapCard:
                    gridTransactDetails.IsVisible = false;
                    gridTapCard.IsVisible = true;
                    gridTransactSummary.IsVisible = false;
                    break;

                case ViewState.Step3Summary:
                    gridTransactDetails.IsVisible = false;
                    gridTapCard.IsVisible = false;
                    gridTransactSummary.IsVisible = true;
                    break;
            }
        }
        
        private void cmdCompletedTransact_Clicked(object sender, EventArgs e)
        {
            UpdateView(ViewState.Step2TapCard);
            StartCardScanner();
        }

        private void StartCardScanner()
        {
            try
            {
                cardApp = new DesfireTerminalApplicationBase(new CardQProcessor(cardInterfaceManger, SessionSingleton.DeviceId));
                cardApp.UserInterfaceRequest += Ta_UserInterfaceRequest;
                cardApp.ProcessCompleted += Ta_ProcessCompleted;
                cardApp.ExceptionOccured += Ta_ExceptionOccured;
                cardApp.StartTransactionRequest(DesFireTransactionTypeEnum.ProcessTransaction);
            }
            catch (Exception ex)
            {
                SetStatusLabel(ex.Message);
            }
        }
        
        private async Task CallTransactWebService(string fromAccountNumber, string toAccountNumber, string cardSerialNumberFrom, string cardSerialNumberTo, int? amount, TransactionType transactionType)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                gridProgress.IsVisible = true;
            });
            try
            {
                XServerApiClient client = SessionSingleton.GenXServerApiClient();
                using (SessionSingleton.HttpClient)
                {
                    Transaction tx = new Transaction()
                    {
                        Amount = amount.Value,
                        TransactionType = transactionType,
                        AccountFrom = fromAccountNumber,
                        AccountTo = toAccountNumber,
                        CardSerialFrom = cardSerialNumberFrom,
                        CardSerialTo = cardSerialNumberTo
                    };
                    await client.TransactionTransferPostAsync(tx.ToJsonString());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    gridProgress.IsVisible = false;
                });
            }
        }

        private async void Ta_ProcessCompleted(object sender, EventArgs e)
        {
            try
            {
                int? amount = Validate.AmountToCents(totalAmount.Total);
                TransactionType transactionType;
                string fromAccountNumber = "";
                string cardSerialNumberFrom = "";
                string toAccountNumber = "";
                string cardSerialNumberTo = "";
                
                SetStatusLabel("Remove card");
                TerminalProcessingOutcome tpo = (e as TerminalProcessingOutcomeEventArgs).TerminalProcessingOutcome;
                if (tpo == null)//error occurred, error displayed via Ta_ExceptionOccured
                    return;

                if (tpo is DesfireTerminalProcessingOutcome)
                {
                    string uid = Formatting.ByteArrayToHexString(((DesfireTerminalProcessingOutcome)tpo).CardDetails.UID);

                    switch (flowType)
                    {
                        case FlowType.SendMoneyFromCardToApp:
                            toAccountNumber = SessionSingleton.Account.AccountNumberId;
                            cardSerialNumberFrom = uid;
                            transactionType = TransactionType.SendMoneyFromCardToApp;
                            break;
                        case FlowType.SendMoneyFromAppToCard:
                            fromAccountNumber = SessionSingleton.Account.AccountNumberId; 
                            cardSerialNumberTo = uid;
                            transactionType = TransactionType.SendMoneyFromAppToCard;
                            break;

                        default:
                            throw new Exception("Unknown flow type:" + flowType);
                    }

                    await CallTransactWebService(fromAccountNumber, toAccountNumber, cardSerialNumberFrom, cardSerialNumberTo, amount, transactionType);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        lblTransactSummary.Text = "Transaction Completed Succesfully";
                        UpdateView(ViewState.Step3Summary);
                    });
                }
                else
                {
                    SetStatusLabel(string.Format("{0}\n{1}", tpo.UserInterfaceRequest.MessageIdentifier, tpo.UserInterfaceRequest.Status));
                }
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    lblTransactSummary.Text = ex.Message;
                    UpdateView(ViewState.Step3Summary);
                });
            }
        }

        private void Ta_UserInterfaceRequest(object sender, EventArgs e)
        {
            SetStatusLabel((e as UIMessageEventArgs).Message);
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
        }
        
        private void SetStatusLabel(string text)
        {
            Device.BeginInvokeOnMainThread(() => { lblStatusTapCard.Text = text; });
        }

        private void cmdCancel_Clicked(object sender, EventArgs e)
        {
            if (cardApp != null)
                cardApp.CancelTransactionRequest();
            ClosePage();
        }

        private void cmdCompletedTransactSummary_Clicked(object sender, EventArgs e)
        {
            if (cardApp != null)
                cardApp.CancelTransactionRequest();
            ClosePage();
        }
    }
}

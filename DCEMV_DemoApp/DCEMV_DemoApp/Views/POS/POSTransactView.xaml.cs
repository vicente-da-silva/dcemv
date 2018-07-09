using Common;
using DesFireProtocol;
using EMVProtocol;
using FormattingUtils;
using ISO7816Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLVProtocol;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XServerCommon.Models;
using XTerminal.Proxies;

namespace XTerminal
{
    public partial class POSTransactView : ModalPage
    {
        private IConfigurationProvider configProvider;
        private ICardInterfaceManger cardInterfaceManger;
        private EMVTerminalApplication cardApp;
        private FlowType flowType = FlowType.SendMoneyFromCardToApp;
        private TotalAmountViewModel totalAmount;
        private List<POSTransactionItem> posTransactionItems;
        private TransactionRequest tr;

        public POSTransactView(ICardInterfaceManger cardInterfaceManger, int amount, List<POSTransactionItem> posTransactionItems, IConfigurationProvider configProvider)
        {
            InitializeComponent();
            gridProgress.IsVisible = false;
            this.configProvider = configProvider;
            totalAmount = new TotalAmountViewModel();

            this.posTransactionItems = posTransactionItems;
            this.cardInterfaceManger = cardInterfaceManger;

            lblTotal.BindingContext = totalAmount;
            totalAmount.Total = Validate.CentsToAmount(amount);

            StartCardScanner();
            UpdateView(ViewState.Step2TapCard);
        }

        private void UpdateView(ViewState viewState)
        {
            switch (flowType)
            {
                case FlowType.SendMoneyFromCardToApp:
                    lblHeaderTapCard.Text = "Send money from their card to your account";
                    this.Title = "Receive Money";
                    lblStatusTapCard.Text = "Tap Card";
                    break;

                default:
                    throw new Exception("Unsupported FlowType");
            }

            switch (viewState)
            {
                case ViewState.Step2TapCard:
                    gridTapCard.IsVisible = true;
                    gridTransactSummary.IsVisible = false;
                    break;

                case ViewState.Step3Summary:
                    gridTapCard.IsVisible = false;
                    gridTransactSummary.IsVisible = true;
                    break;

                default:
                    throw new Exception("Unsupported ViewState");
            }
        }
        
        private void StartCardScanner()
        {
            try
            {
                long amount = (long)(Convert.ToDouble(totalAmount.Total) * 100);
                long amountOther = 0;
                tr = new TransactionRequest(amount + amountOther, amountOther, TransactionTypeEnum.PurchaseGoodsAndServices);

                cardApp = new EMVTerminalApplication(new CardQProcessor(cardInterfaceManger, SessionSingleton.DeviceId), configProvider);
                cardApp.UserInterfaceRequest += Ta_UserInterfaceRequest;
                cardApp.ProcessCompleted += Ta_ProcessCompleted;
                cardApp.ExceptionOccured += Ta_ExceptionOccured;
                cardApp.StartTransactionRequest(tr);
            }
            catch (Exception ex)
            {
                SetStatusLabel(ex.Message);
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

                TerminalProcessingOutcome tpo = (e as TerminalProcessingOutcomeEventArgs).TerminalProcessingOutcome;
                if (tpo == null)//error occurred, error displayed via Ta_ExceptionOccured
                    return;

                if (tpo is EMVTerminalProcessingOutcome)
                {
                    TLV dataRecord = ((EMVTerminalProcessingOutcome)tpo).DataRecord;
                    TLV discretionaryData = ((EMVTerminalProcessingOutcome)tpo).DiscretionaryData;

                    if (dataRecord != null) //error or decline
                    {
                        SetStatusLabel("Remove card");

                        if (discretionaryData != null)
                            dataRecord.Children.AddListToList(discretionaryData.Children);

                        string emvData = TLVasJSON.ToJSON(dataRecord);

                        byte[] panBCD;
                        TLV _5A = dataRecord.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag);
                        if (_5A != null)
                            panBCD = _5A.Value;
                        else
                        {
                            TLV _57 = dataRecord.Children.Get(EMVTagsEnum.TRACK_2_EQUIVALENT_DATA_57_KRN.Tag);
                            if (_57 == null)
                                throw new Exception("No PAN found");
                            String panString = Formatting.ByteArrayToHexString(_57.Value);
                            panBCD = Formatting.StringToBcd(panString.Split('D')[0], false);
                        }

                        switch (flowType)
                        {
                            case FlowType.SendMoneyFromCardToApp:
                                toAccountNumber = SessionSingleton.Account.AccountNumberId;
                                cardSerialNumberFrom = Formatting.BcdToString(panBCD);
                                transactionType = TransactionType.SendMoneyFromCardToApp;
                                break;

                            default:
                                throw new Exception("Unknown flow type:" + flowType);
                        }

                        try
                        {
                            await CallPosTransactWebService(fromAccountNumber, toAccountNumber, cardSerialNumberFrom, cardSerialNumberTo, amount, transactionType, emvData);
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                lblTransactSummary.Text = "Transaction Completed Succesfully";
                                UpdateView(ViewState.Step3Summary);
                            });
                        }
                        catch (Exception ex)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                lblTransactSummary.Text = "Declined, could not go online.";
                                UpdateView(ViewState.Step3Summary);
                            });
                        }
                    }
                    else
                    {
                        SetStatusLabel(string.Format("{0}\n{1}", tpo.UserInterfaceRequest.MessageIdentifier, tpo.UserInterfaceRequest.Status));
                    }
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

        private async Task CallPosTransactWebService(string fromAccountNumber, string toAccountNumber, string cardSerialNumberFrom, string cardSerialNumberTo, int? amount, TransactionType transactionType, String emvData)
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
                    POSTransaction posTx = new POSTransaction();
                    posTx.InvItems = posTransactionItems;

                    Transaction tx = new Transaction()
                    {
                       Amount = Validate.AmountToCents(totalAmount.Total),
                       AccountFrom = fromAccountNumber,
                       AccountTo = toAccountNumber,
                       CardSerialFrom = cardSerialNumberFrom,
                       CardSerialTo = cardSerialNumberTo,
                       CardFromEMVData = emvData,
                    };
                    await client.StoreSalePostAsync(tx.ToJsonString(), posTx.ToJsonString());
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

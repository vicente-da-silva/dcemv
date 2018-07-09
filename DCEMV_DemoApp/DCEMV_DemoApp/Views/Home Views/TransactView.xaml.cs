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
using DCEMV.FormattingUtils;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using DCEMV.TLVProtocol;
using Xamarin.Forms;
using DCEMV.ServerShared;
using DCEMV.TerminalCommon;

namespace DCEMV.DemoApp
{
    
    public enum ViewState
    {
        Step1TransactDetails,
        Step2TapCard,
        Step3Summary
    }
    public enum FlowType
    {
        SendMoneyFromAppToCard,
        SendMoneyFromCardToApp
    }

    public partial class TransactView : ModalPage
    {
        private TransactionRequest tr;
        private FlowType flowType;
        private TotalAmountViewModel totalAmount;

        //for EMVTxCtl
        private IConfigurationProvider configProvider;
        private ICardInterfaceManger contactCardInterfaceManger;
        private ICardInterfaceManger contactlessCardInterfaceManger;
        private IOnlineApprover onlineApprover;
        private TCPClientStream tcpClientStream;

        public TransactView(FlowType flowType, ICardInterfaceManger contactCardInterfaceManger, ICardInterfaceManger contactlessCardInterfaceManger, IConfigurationProvider configProvider, IOnlineApprover onlineApprover, TCPClientStream tcpClientStream)
        {
            InitializeComponent();

            this.contactCardInterfaceManger = contactCardInterfaceManger;
            this.contactlessCardInterfaceManger = contactlessCardInterfaceManger;
            this.configProvider = configProvider;
            this.onlineApprover = onlineApprover;
            this.tcpClientStream = tcpClientStream;
            this.flowType = flowType;

            emvTxCtl.TxCompleted += EmvTxCtl_TxCompleted;

            totalAmount = new TotalAmountViewModel();
            gridProgress.IsVisible = false;
            txtAmount.BindingContext = totalAmount;

            UpdateView(ViewState.Step1TransactDetails);
        }

        private void UpdateView(ViewState viewState)
        {
            switch (flowType)
            {
                case FlowType.SendMoneyFromCardToApp:
                    lblHeaderTransact.Text = "Send money from their card to your account";
                    emvTxCtl.SetTxStartLabel("Send money from their card to your account");
                    this.Title = "Receive Money";
                    break;
                case FlowType.SendMoneyFromAppToCard:
                    lblHeaderTransact.Text = "Send money from your account to their card";
                    emvTxCtl.SetTxStartLabel("Send money from your account to their card");
                    this.Title = "Send Money";
                    break;
            }

            switch (viewState)
            {
                case ViewState.Step1TransactDetails:
                    gridTransactDetails.IsVisible = true;
                    emvTxCtl.IsVisible = false;
                    gridTransactSummary.IsVisible = false;
                    break;

                case ViewState.Step2TapCard:
                    gridTransactDetails.IsVisible = false;
                    emvTxCtl.IsVisible = true;
                    gridTransactSummary.IsVisible = false;
                    break;

                case ViewState.Step3Summary:
                    gridTransactDetails.IsVisible = false;
                    emvTxCtl.IsVisible = false;
                    gridTransactSummary.IsVisible = true;
                    break;
            }
        }
        
        private void cmdCompletedTransact_Clicked(object sender, EventArgs e)
        {
            UpdateView(ViewState.Step2TapCard);

            long amount = Convert.ToInt64(totalAmount.Total);
            long amountOther = 0;
            tr = new TransactionRequest(amount + amountOther, amountOther, TransactionTypeEnum.PurchaseGoodsAndServices);

            //cannot use contact interface with DC EMV Cards
            emvTxCtl.Start(tr, null, "", contactlessCardInterfaceManger, SessionSingleton.ContactlessDeviceId ,configProvider, onlineApprover, tcpClientStream);
        }

        private async void EmvTxCtl_TxCompleted(object sender, EventArgs e)
        {
            try
            {
                long? amount = Convert.ToInt64(totalAmount.Total);

                TransactionType transactionType;
                string fromAccountNumber = "";
                string cardSerialNumberFrom = "";
                string toAccountNumber = "";
                string cardSerialNumberTo = "";

                if ((e as TxCompletedEventArgs).EMV_Data.IsPresent())
                {
                    if ((e as TxCompletedEventArgs).TxResult == TxResult.Approved || (e as TxCompletedEventArgs).TxResult == TxResult.ContactlessOnline)
                    {
                        TLV data = (e as TxCompletedEventArgs).EMV_Data.Get();

                        byte[] panBCD;
                        TLV _5A = data.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag);
                        if (_5A != null)
                            panBCD = _5A.Value;
                        else
                        {
                            TLV _57 = data.Children.Get(EMVTagsEnum.TRACK_2_EQUIVALENT_DATA_57_KRN.Tag);
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
                            case FlowType.SendMoneyFromAppToCard:
                                fromAccountNumber = SessionSingleton.Account.AccountNumberId;
                                cardSerialNumberTo = Formatting.BcdToString(panBCD);
                                transactionType = TransactionType.SendMoneyFromAppToCard;
                                break;

                            default:
                                throw new Exception("Unknown flow type:" + flowType);
                        }

                        try
                        {
                            await CallTransactWebService(fromAccountNumber, toAccountNumber, cardSerialNumberFrom, cardSerialNumberTo, amount, transactionType, data);
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                lblTransactSummary.Text = "Transaction Completed Succesfully";
                                UpdateView(ViewState.Step3Summary);
                            });
                        }
                        catch
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
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            lblTransactSummary.Text = (e as TxCompletedEventArgs).TxResult.ToString();
                            UpdateView(ViewState.Step3Summary);
                        });
                    }
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        lblTransactSummary.Text = "Declined";
                        UpdateView(ViewState.Step3Summary);
                    });
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

        private async Task CallTransactWebService(string fromAccountNumber, string toAccountNumber, string cardSerialNumberFrom, string cardSerialNumberTo, long? amount, TransactionType transactionType, TLV emvData)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                gridProgress.IsVisible = true;
            });
            try
            {
                Proxies.DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                using (SessionSingleton.HttpClient)
                {
                    TransferTransaction tx = new TransferTransaction()
                    {
                        Amount = amount.Value,
                        TransactionType = transactionType,
                        AccountFrom = fromAccountNumber,
                        AccountTo = toAccountNumber,
                        CardSerialFrom = cardSerialNumberFrom,
                        CardSerialTo = cardSerialNumberTo,
                        CardFromEMVData = TLVasJSON.ToJSON(emvData),
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
        
        protected override void OnDisappearing()
        {
            emvTxCtl.Stop();

            base.OnDisappearing();
        }
    }
}

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
using DCEMV_QRDEProtocol;
using DCEMV.EMVProtocol.EMVQRCode;

namespace DCEMV.DemoApp
{
    public enum ViewState
    {
        StepTxCtl,
        StepSummary
    }
    public enum FlowType
    {
        SendMoneyFromAppToCard,
        SendMoneyFromCardToApp
    }

    public partial class TransactView : ModalPage
    {
        private FlowType flowType;
        private IOnlineApprover onlineApprover;
       
        public TransactView(FlowType flowType, ICardInterfaceManger contactCardInterfaceManger, ICardInterfaceManger contactlessCardInterfaceManger, IConfigurationProvider configProvider, IOnlineApprover onlineApprover, TCPClientStream tcpClientStream)
        {
            InitializeComponent();

            this.onlineApprover = onlineApprover;
            this.flowType = flowType;

            QRCodeMode mode = QRCodeMode.None;
            switch (flowType)
            {
                case FlowType.SendMoneyFromCardToApp:
                    mode = QRCodeMode.PresentAndPoll;
                    emvTxCtl.SetHeaderInstruction("Send money from their card to your account");
                    emvTxCtl.SetTxStartLabel("Send money from their card to your account");
                    this.Title = "Receive Money";
                    break;
                case FlowType.SendMoneyFromAppToCard:
                    mode = QRCodeMode.ScanAndProcess;
                    emvTxCtl.SetHeaderInstruction("Send money from your account to their card");
                    emvTxCtl.SetTxStartLabel("Send money from your account to their card");
                    this.Title = "Send Money";
                    break;
            }

            emvTxCtl.Init(contactCardInterfaceManger, SessionSingleton.ContactDeviceId,
                contactlessCardInterfaceManger, SessionSingleton.ContactlessDeviceId,
                mode, SessionSingleton.Account.AccountNumberId,
                configProvider, onlineApprover, tcpClientStream);

            emvTxCtl.TxCompleted += EmvTxCtl_TxCompleted;

            gridProgress.IsVisible = false;

            UpdateView(ViewState.StepTxCtl);
        }

        private void UpdateView(ViewState viewState)
        {
            switch (viewState)
            {
                case ViewState.StepTxCtl:
                    emvTxCtl.IsVisible = true;
                    gridTransactSummary.IsVisible = false;
                    break;

                case ViewState.StepSummary:
                    emvTxCtl.IsVisible = false;
                    gridTransactSummary.IsVisible = true;
                    break;
            }
        }
        
        private async void EmvTxCtl_TxCompleted(object sender, EventArgs e)
        {
            try
            {
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

                        TLV _9F02 = data.Children.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag);
                        if (_9F02 == null)
                            throw new Exception("No Amount found");

                        long amount = Formatting.BcdToLong(_9F02.Value);

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
                            await CallTransactByCardWebService(fromAccountNumber, toAccountNumber, cardSerialNumberFrom, cardSerialNumberTo, amount, transactionType, data);
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                lblTransactSummary.Text = "Transaction Completed Succesfully";
                                UpdateView(ViewState.StepSummary);
                            });
                        }
                        catch
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                lblTransactSummary.Text = "Declined, could not go online.";
                                UpdateView(ViewState.StepSummary);
                            });
                        }
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            lblTransactSummary.Text = (e as TxCompletedEventArgs).TxResult.ToString();
                            UpdateView(ViewState.StepSummary);
                        });
                    }
                }
                else if ((e as TxCompletedEventArgs).QR_Data.IsPresent())
                {
                    QRDEList data = (e as TxCompletedEventArgs).QR_Data.Get();
                    QRDE _26 = data.Get(EMVQRTagsEnum.MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_26.Tag);
                    QRDE _54 = data.Get(EMVQRTagsEnum.TRANSACTION_AMOUNT_54.Tag);
                    QRDE gui = _26.Children.Get(EMVQRTagsEnum.GLOBALLY_UNIQUE_IDENTIFIER_00.Tag);
                    QRDE tracking = _26.Children.Get(TagId._05);

                    long amount = Convert.ToInt64(_54.Value);

                    if ((e as TxCompletedEventArgs).TxResult == TxResult.QRCodeScanned)
                    {
                    
                        switch (flowType)
                        {
                            case FlowType.SendMoneyFromCardToApp:
                                throw new Exception("Invalid flow type for Scanned QR code");
                            case FlowType.SendMoneyFromAppToCard:
                                fromAccountNumber = SessionSingleton.Account.AccountNumberId;
                                toAccountNumber = gui.Value;
                                transactionType = TransactionType.SendMoneyFromAppToCard;
                                break;

                            default:
                                throw new Exception("Unknown flow type:" + flowType);
                        }
                        try
                        {
                            await CallTransactByQRCodeWebService(fromAccountNumber, toAccountNumber, amount, tracking.Value);
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                lblTransactSummary.Text = "Transaction Completed Succesfully";
                                UpdateView(ViewState.StepSummary);
                            });
                        }
                        catch (Exception ex)
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                lblTransactSummary.Text = "Declined, could not go online.";
                                UpdateView(ViewState.StepSummary);
                            });
                        }
                    }
                    else if ((e as TxCompletedEventArgs).TxResult == TxResult.QRCodeToPoll)
                    {
                        switch (flowType)
                        {
                            case FlowType.SendMoneyFromCardToApp:
                                break;
                            case FlowType.SendMoneyFromAppToCard:
                                throw new Exception("Invalid flow type for QR code");

                            default:
                                throw new Exception("Unknown flow type:" + flowType);
                        }

                        try
                        {
                            bool success = await CallTransactGetStateWebService(tracking.Value);
                            //bool success = await CallTransactGetStateWebService("929fa290b20647988f72f2ca762bc3e7");
                            if (success)
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    lblTransactSummary.Text = "Transaction Approved";
                                    UpdateView(ViewState.StepSummary);
                                });
                            }
                            else
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    App.Current.MainPage.DisplayAlert("Info", "Transaction not found, try again if you believe the transaction was approved", "OK");
                                });
                            }
                        }
                        catch
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                App.Current.MainPage.DisplayAlert("Info", "Unknown, there is no connectivity to the server, try again if you believe the tx was approved", "OK");
                            });
                        }
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            lblTransactSummary.Text = (e as TxCompletedEventArgs).TxResult.ToString();
                            UpdateView(ViewState.StepSummary);
                        });
                    }
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        lblTransactSummary.Text = "Declined";
                        UpdateView(ViewState.StepSummary);
                    });
                }
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    lblTransactSummary.Text = ex.Message;
                    UpdateView(ViewState.StepSummary);
                });
            }
        }

        private async Task CallTransactByCardWebService(string fromAccountNumber, string toAccountNumber, string cardSerialNumberFrom, string cardSerialNumberTo, long? amount, TransactionType transactionType, TLV emvData)
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
                    CardTransferTransaction tx = new CardTransferTransaction()
                    {
                        Amount = amount.Value,
                        TransactionType = transactionType,
                        AccountFrom = fromAccountNumber,
                        AccountTo = toAccountNumber,
                        CardSerialFrom = cardSerialNumberFrom,
                        CardSerialTo = cardSerialNumberTo,
                        CardFromEMVData = TLVasJSON.ToJSON(emvData),
                };
                    await client.TransactionCardtransferPostAsync(tx.ToJsonString());
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
        private async Task CallTransactByQRCodeWebService(string fromAccountNumber, string toAccountNumber, long? amount, string trackingId)
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
                    QRCodeTransferTransaction tx = new QRCodeTransferTransaction()
                    {
                        Amount = amount.Value,
                        AccountFrom = fromAccountNumber,
                        AccountTo = toAccountNumber,
                        TrackingId = trackingId,
                    };
                    await client.TransactionQrcodetransferPostAsync(tx.ToJsonString());
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
        private async Task<bool> CallTransactGetStateWebService(string trackingId)
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
                    return await client.TransactionGettransactionstateGetAsync(trackingId);
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

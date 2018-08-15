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
using DCEMV.ISO7816Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DCEMV.TLVProtocol;
using Xamarin.Forms;
using DCEMV.ServerShared;
using DCEMV.TerminalCommon;

namespace DCEMV.DemoApp
{
    public partial class POSView : ModalPage
    {
        public enum POSViewState
        {
            POS,
            Search,
            Payment,
            PaymentSummary,
        }
        public class POSButton : Button
        {
            public int UniqueId { get; set; }
        }
        public class POSTransactionLineItem : INotifyPropertyChanged
        {
            private int quantity;
            public int Quantity
            {
                get { return quantity; }
                set
                {
                    quantity = value;
                    OnPropertyChanged("Quantity");
                    OnPropertyChanged("Total");
                }
            }

            public string Name { get; set; }
            public int InventoryItemId { get; set; }
            public long Amount { get; set; }
            public long Total { get { return Amount * Quantity; } }
            public string Description { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        private ObservableCollection<InventoryGroup> groups;
        private ObservableCollection<InventoryItemDetailViewModel> items;
        private ObservableCollection<POSTransactionLineItem> basketItems;
        private TotalAmountViewModel totalAmount;

        private FlowType flowType = FlowType.SendMoneyFromCardToApp;
        private TransactionRequest tr;

        //for EMVTxCtl
        private IConfigurationProvider configProvider;
        private ICardInterfaceManger contactCardInterfaceManger;
        private ICardInterfaceManger contactlessCardInterfaceManger;
        private IOnlineApprover onlineApprover;
        private TCPClientStream tcpClientStream;

        public POSView(ICardInterfaceManger contactCardInterfaceManger, ICardInterfaceManger contactlessCardInterfaceManger, IConfigurationProvider configProvider, IOnlineApprover onlineApprover, TCPClientStream tcpClientStream)
        {
            InitializeComponent();

            this.contactCardInterfaceManger = contactCardInterfaceManger;
            this.contactlessCardInterfaceManger = contactlessCardInterfaceManger;
            this.configProvider = configProvider;
            this.onlineApprover = onlineApprover;
            this.tcpClientStream = tcpClientStream;

            emvTxCtl.TxCompleted += EmvTxCtl_TxCompleted;

            UpdateView(POSViewState.POS);

            basketItems = new ObservableCollection<POSTransactionLineItem>();
            totalAmount = new TotalAmountViewModel();
            lblTotal.BindingContext = totalAmount;

            try
            {
                Task.Run(async () =>
                {
                    groups = await CacheProvider.Instance.GetInventoryGroups();
                    items = await CacheProvider.Instance.GetinventoryItems();
                })
                .ContinueWith((x) =>
                {
                    UpdataItemNavigator();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (lstBasketItems.ItemsSource == null)
                            lstBasketItems.ItemsSource = basketItems;
                        gridProgress.IsVisible = false;
                    });
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            }
            catch (Exception ex)
            {
                Task.Run(async () =>
                {
                    await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                });
            }
        }

        private Button CreateButton(string text, int id, Action<int> action)
        {
            POSButton b = new POSButton();
            b.UniqueId = id;
            b.Clicked += (sender, e) => { action.Invoke((sender as POSButton).UniqueId); };
            b.Text = text;
            b.Style = (Style)Application.Current.Resources["styleButtonPOS"];
            return b;
        }

        private void UpdataItemNavigator()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                gridItemNavigator.Children.Clear();

                groups.ToList().ForEach(x =>
                {
                    gridItemNavigator.Children.Add(CreateButton(x.Name, x.InventoryGroupId, GroupButtonClicked));
                });
            });
        }

        private void GroupButtonClicked(int id)
        {
            if (id == 0)
                UpdataItemNavigator();
            else
            {
                gridItemNavigator.Children.Clear();
                gridItemNavigator.Children.Add(CreateButton("Back", 0, GroupButtonClicked));

                items.Where((x) => x.InventoryGroupIdRef == id).ToList().ForEach(x =>
                {
                    gridItemNavigator.Children.Add(CreateButton(x.Name, x.InventoryItemId, ItemButtonClicked));
                });
            }
        }

        private void ItemButtonClicked(int id)
        {
            AddItemToBasket(items.Where((x) => x.InventoryItemId == id).First());
            UpdataItemNavigator();
        }

        private void AddItemToBasket(InventoryItem item)
        {
            basketItems.Add(CreatePOSTxItem(item, 1));
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            long totalCents = basketItems.ToList().Sum(x => { return x.Total; });
            totalAmount.Total = Convert.ToString(totalCents);
        }

        private POSTransactionLineItem CreatePOSTxItem(InventoryItem item, int quantity)
        {
            return new POSTransactionLineItem()
            {
                InventoryItemId = item.InventoryItemId,
                Name = item.Name,
                Quantity = quantity,
                Amount = item.Price * quantity,
                Description = item.Description,
            };
        }

        private void cmdClear_Clicked(object sender, EventArgs e)
        {
            basketItems.Clear();
            UpdateTotal();
        }

        private async void cmdUpQ_Clicked(object sender, EventArgs e)
        {
            if (lstBasketItems.SelectedItem == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No item selected", "OK");
                return;
            }

            (lstBasketItems.SelectedItem as POSTransactionLineItem).Quantity = (lstBasketItems.SelectedItem as POSTransactionLineItem).Quantity + 1;

            UpdateTotal();
        }

        private async void cmdDownQ_Clicked(object sender, EventArgs e)
        {
            if (lstBasketItems.SelectedItem == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No item selected", "OK");
                return;
            }

            if ((lstBasketItems.SelectedItem as POSTransactionLineItem).Quantity > 1)
                (lstBasketItems.SelectedItem as POSTransactionLineItem).Quantity = (lstBasketItems.SelectedItem as POSTransactionLineItem).Quantity - 1;

            UpdateTotal();
        }

        private async void txtBarcode_Completed(object sender, EventArgs e)
        {
            string barcode = txtBarcode.Text.Trim();
            List<InventoryItemDetailViewModel> foundItems = items.Where(x => { return x.Barcode.ToLower() == barcode; }).ToList();

            if (foundItems.Count > 1)
            {
                await App.Current.MainPage.DisplayAlert("Error", "More than one item found", "OK");
                return;
            }
            if (foundItems.Count == 0)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No item found", "OK");
                return;
            }

            AddItemToBasket(foundItems.First());
        }

        private void cmdSearch_Clicked(object sender, EventArgs e)
        {
            UpdateView(POSViewState.Search);
        }

        private async void cmdRemove_Clicked(object sender, EventArgs e)
        {
            if (lstBasketItems.SelectedItem == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No item selected", "OK");
                return;
            }

            basketItems.Remove((POSTransactionLineItem)lstBasketItems.SelectedItem);
            UpdateTotal();
        }

        private void cmdTakePayment_Clicked(object sender, EventArgs e)
        {
            gridPOS.IsVisible = false;
            emvTxCtl.IsVisible = true;
            gridSearch.IsVisible = false;

            long amount = Convert.ToInt64(totalAmount.Total);
            long amountOther = 0;
            tr = new TransactionRequest(amount + amountOther, amountOther, TransactionTypeEnum.PurchaseGoodsAndServices);

            //cannot use contact interface with DC EMV Cards
            emvTxCtl.Init(null, "", contactlessCardInterfaceManger, SessionSingleton.ContactlessDeviceId, 
                QRCodeMode.PresentAndPoll , SessionSingleton.Account.AccountNumberId ,configProvider, 
                onlineApprover, tcpClientStream, tr);
        }

        private async void EmvTxCtl_TxCompleted(object sender, EventArgs e)
        {
            try
            {
                if ((e as TxCompletedEventArgs).TxResult == TxResult.Approved ||
                    (e as TxCompletedEventArgs).TxResult == TxResult.ContactlessOnline)
                {
                    long? amount = Convert.ToInt64(totalAmount.Total);

                    TransactionType transactionType;
                    string fromAccountNumber = "";
                    string cardSerialNumberFrom = "";
                    string toAccountNumber = "";
                    string cardSerialNumberTo = "";

                    if ((e as TxCompletedEventArgs).EMV_Data.IsPresent())
                    {
                        if ((e as TxCompletedEventArgs).TxResult == TxResult.Approved ||
                            (e as TxCompletedEventArgs).TxResult == TxResult.ContactlessOnline)
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

                                default:
                                    throw new Exception("Unknown flow type:" + flowType);
                            }

                            try
                            {
                                await CallPosTransactWebService(fromAccountNumber, toAccountNumber, cardSerialNumberFrom, cardSerialNumberTo, amount, transactionType, data);
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    lblTransactSummary.Text = "Transaction Completed Succesfully";
                                    UpdateView(POSViewState.PaymentSummary);
                                });
                            }
                            catch
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    lblTransactSummary.Text = "Declined, could not go online.";
                                    UpdateView(POSViewState.PaymentSummary);
                                });
                            }
                        }
                        else
                        {
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                lblTransactSummary.Text = (e as TxCompletedEventArgs).TxResult.ToString();
                                UpdateView(POSViewState.PaymentSummary);
                            });
                        }
                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            lblTransactSummary.Text = "Declined";
                            UpdateView(POSViewState.PaymentSummary);
                        });
                    }
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        lblTransactSummary.Text = (e as TxCompletedEventArgs).TxResult.ToString();
                        UpdateView(POSViewState.PaymentSummary);
                    });
                }

            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    lblTransactSummary.Text = ex.Message;
                    UpdateView(POSViewState.PaymentSummary);
                });
            }
        }

        private List<POSTransactionItem> ConvertLineItems(List<POSTransactionLineItem> input)
        {
            List<POSTransactionItem> ret = new List<POSTransactionItem>();
            input.ForEach(m =>
            {
                ret.Add(new POSTransactionItem()
                {
                    Amount = m.Amount,
                    InventoryItemId = m.InventoryItemId,
                    Name = m.Name,
                    Quantity = m.Quantity,
                });
            });
            return ret;
        }

        private async Task CallPosTransactWebService(string fromAccountNumber, string toAccountNumber, string cardSerialNumberFrom, string cardSerialNumberTo, long? amount, TransactionType transactionType, TLV emvData)
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
                    POSTransaction posTx = new POSTransaction();
                    posTx.InvItems = ConvertLineItems(basketItems.ToList());

                    CardTransferTransaction tx = new CardTransferTransaction()
                    {
                        Amount = amount.Value,
                        AccountFrom = fromAccountNumber,
                        AccountTo = toAccountNumber,
                        CardSerialFrom = cardSerialNumberFrom,
                        CardSerialTo = cardSerialNumberTo,
                        CardFromEMVData = TLVasJSON.ToJSON(emvData),
                    };
                    await client.StoreSalebycardPostAsync(tx.ToJsonString(), posTx.ToJsonString());
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

        private void UpdateView(POSViewState viewState)
        {
            switch (viewState)
            {
                case POSViewState.POS:
                    gridPOS.IsVisible = true;
                    emvTxCtl.IsVisible = false;
                    gridTransactSummary.IsVisible = false;
                    gridSearch.IsVisible = false;
                    break;

                case POSViewState.Payment:
                    gridPOS.IsVisible = false;
                    emvTxCtl.IsVisible = true;
                    gridTransactSummary.IsVisible = false;
                    gridSearch.IsVisible = false;
                    break;

                case POSViewState.PaymentSummary:
                    gridPOS.IsVisible = false;
                    emvTxCtl.IsVisible = false;
                    gridTransactSummary.IsVisible = true;
                    gridSearch.IsVisible = false;
                    break;

                case POSViewState.Search:
                    gridPOS.IsVisible = false;
                    emvTxCtl.IsVisible = false;
                    gridTransactSummary.IsVisible = false;
                    gridSearch.IsVisible = true;
                    break;
            }
        }

        private void cmdCompletedTransactSummary_Clicked(object sender, EventArgs e)
        {
            emvTxCtl.Stop();

            UpdateView(POSViewState.POS);
        }

        protected override void OnDisappearing()
        {
            emvTxCtl.Stop();

            cmdClear_Clicked(null, null);

            UpdateView(POSViewState.POS);

            base.OnDisappearing();
        }

        #region Search
        public void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string search = txtSearch.Text.Trim().ToLower();
            items.ToList().ForEach(x =>
            {
                x.Group = groups.ToList().Find(y => y.InventoryGroupId == x.InventoryGroupIdRef);
            });

            if (search.Length < 3)
            {
                lstInventoryItems.ItemsSource = items;
                return;
            }

            lstInventoryItems.ItemsSource = items.Where(x => { return x.Name.ToLower().Contains(search) || x.Description.ToLower().Contains(search) || x.Barcode.Contains(search); }).ToList();
        }

        private void lstInventoryItems_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            AddItemToBasket((InventoryItemDetailViewModel)e.SelectedItem);

            gridPOS.IsVisible = true;
            emvTxCtl.IsVisible = false;
            gridSearch.IsVisible = false;
        }

        private void cmdCancel_Clicked(object sender, EventArgs e)
        {
            UpdateView(POSViewState.POS);
        }
        #endregion
    }
}

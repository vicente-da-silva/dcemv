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
using System.Linq;
using System.Threading.Tasks;
using DCEMV.TLVProtocol;
using Xamarin.Forms;
using DCEMV.ServerShared;
using DCEMV.TerminalCommon;

namespace DCEMV.DemoApp
{
    public class CardViewModel
    {
        public string FreindlyName { get; set; }
        public string DailySpendLimit { get; set; }
        public string MonthlySpendLimit { get; set; }
        public string CardSerialNumberId { get; set; }
    }

    public partial class CardAdminView : ModalPage
    {
        public enum CardAdminViewState
        {
            CardList,
            AddCard,
            EditCard,
            CardAddStatus,
        }

        public CardViewModel card { get; private set; }

        //for EMVTxCtl
        private IConfigurationProvider configProvider;
        private ICardInterfaceManger contactCardInterfaceManger;
        private ICardInterfaceManger contactlessCardInterfaceManger;
        private IOnlineApprover onlineApprover;
        private TCPClientStream tcpClientStream;

        public CardAdminView(ICardInterfaceManger contactCardInterfaceManger, ICardInterfaceManger contactlessCardInterfaceManger, IConfigurationProvider configProvider, IOnlineApprover onlineApprover, TCPClientStream tcpClientStream)
        {
            InitializeComponent();

            this.contactCardInterfaceManger = contactCardInterfaceManger;
            this.contactlessCardInterfaceManger = contactlessCardInterfaceManger;
            this.configProvider = configProvider;
            this.onlineApprover = onlineApprover;
            this.tcpClientStream = tcpClientStream;

            emvTxCtl.TxCompleted += EmvTxCtl_TxCompleted;

            gridProgress.IsVisible = false;
            viewCardList.ItemsSource = SessionSingleton.Account.Cards;

            UpdateView(CardAdminViewState.CardList);
        }

        private void UpdateView(CardAdminViewState viewState)
        {
            switch (viewState)
            {
                case CardAdminViewState.CardList:
                    emvTxCtl.IsVisible = false;
                    gridCardList.IsVisible = true;
                    gridEditCard.IsVisible = false;
                    gridCardStatus.IsVisible = false;
                    gridEditCard.IsVisible = false;
                    break;

                case CardAdminViewState.AddCard:
                    emvTxCtl.IsVisible = true;
                    gridCardList.IsVisible = false;
                    gridEditCard.IsVisible = false;
                    gridCardStatus.IsVisible = false;
                    gridEditCard.IsVisible = false;
                    break;

                case CardAdminViewState.CardAddStatus:
                    emvTxCtl.IsVisible = false;
                    gridCardList.IsVisible = false;
                    gridEditCard.IsVisible = false;
                    gridCardStatus.IsVisible = true;
                    gridEditCard.IsVisible = false;
                    break;

                case CardAdminViewState.EditCard:
                    emvTxCtl.IsVisible = false;
                    gridCardList.IsVisible = false;
                    gridEditCard.IsVisible = false;
                    gridCardStatus.IsVisible = false;
                    gridEditCard.IsVisible = true;
                    break;
            }
        }

        private async void cmdUnlink_Clicked(object sender, EventArgs e)
        {
            if (viewCardList.SelectedItem == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No card selected", "OK");
                return;
            }

            try
            {
                gridProgress.IsVisible = true;
                await Task.Run(async () =>
                {
                    Proxies.DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                    using (SessionSingleton.HttpClient)
                    {
                        await client.CardCancelcardPostAsync(((Card)viewCardList.SelectedItem).CardSerialNumberId);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            ((Card)viewCardList.SelectedItem).CardState = CardState.Cancelled;
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                gridProgress.IsVisible = false;
            }
        }

        protected override void OnDisappearing()
        {
            emvTxCtl.Stop();

            base.OnDisappearing();
        }

        #region Add Card
        private void cmdAddCard_Clicked(object sender, EventArgs e)
        {
            emvTxCtl.SetTxStartLabel("Please tap the card you would like to add.");
            UpdateView(CardAdminViewState.AddCard);

            try
            {
                TransactionRequest tr = new TransactionRequest(0, 0, TransactionTypeEnum.PurchaseGoodsAndServices);
                //cannot use contact interface to add a DC EMV card
                emvTxCtl.Start(tr, null, "", contactlessCardInterfaceManger,SessionSingleton.ContactlessDeviceId, configProvider, onlineApprover, tcpClientStream);
            }
            catch (Exception ex)
            {
                UpdateView(CardAdminViewState.CardAddStatus);
                lblStatusCard.Text = ex.Message;
            }
        }

        private async void EmvTxCtl_TxCompleted(object sender, EventArgs e)
        {
            try
            {
                if ((e as TxCompletedEventArgs).EMV_Data.IsPresent())
                {
                    TLV data = (e as TxCompletedEventArgs).EMV_Data.Get();

                    string emvData = TLVasJSON.ToJSON(data);

                    string uid = Formatting.BcdToString(data.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag).Value);

                    await RegisterCard(SessionSingleton.Account.AccountNumberId, uid);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        SessionSingleton.Account.Cards.Add(new Card()
                        {
                            CardSerialNumberId = uid,
                        });
                        lblStatusCard.Text = string.Format("Card {0} linked", uid);
                        UpdateView(CardAdminViewState.CardAddStatus);
                    });

                }
                else
                {
                    lblStatusCard.Text = "Card not linked";
                    UpdateView(CardAdminViewState.CardAddStatus);
                }
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    UpdateView(CardAdminViewState.CardAddStatus);
                    lblStatusCard.Text = ex.Message;
                });
            }
        }

        private async Task RegisterCard(string accountNumberId, string cardSerialNumber)
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
                    await client.CardRegistercardPostAsync(cardSerialNumber);
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

        private void cmdTapCardCancelled_Clicked(object sender, EventArgs e)
        {
            emvTxCtl.Stop();
            UpdateView(CardAdminViewState.CardList);
        }

        private void cmdCardStatusCompleted_Clicked(object sender, EventArgs e)
        {
            emvTxCtl.Stop();
            UpdateView(CardAdminViewState.CardList);
        }
        #endregion

        #region Edit Card
        private async void cmdEdit_Clicked(object sender, EventArgs e)
        {
            if (viewCardList.SelectedItem == null)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No card selected", "OK");
                return;
            }

            UpdateView(CardAdminViewState.EditCard);
            EditCard((Card)viewCardList.SelectedItem);
        }
        private void EditCard(Card cardToEdit)
        {
            this.card = new CardViewModel()
            {
                DailySpendLimit = Validate.CentsToAmount(cardToEdit.DailySpendLimit),
                MonthlySpendLimit = Validate.CentsToAmount(cardToEdit.MonthlySpendLimit),
                CardSerialNumberId = cardToEdit.CardSerialNumberId,
                FreindlyName = cardToEdit.FreindlyName,
            };
            gridEditCard.BindingContext = this.card;
        }

        private async void cmdEditCardOk_Clicked(object sender, EventArgs e)
        {
            try
            {
                gridProgress.IsVisible = true;
                Card cardDto = new Card()
                {
                    CardSerialNumberId = card.CardSerialNumberId,
                    FreindlyName = card.FreindlyName,
                    DailySpendLimit = Convert.ToInt64(card.DailySpendLimit),
                    MonthlySpendLimit = Convert.ToInt64(card.MonthlySpendLimit)
                };
                await Task.Run(async () =>
                {
                    Proxies.DCEMVDemoServerClient client = SessionSingleton.GenDCEMVServerApiClient();
                    using (SessionSingleton.HttpClient)
                    {
                        await client.CardUpdatecarddetailsPostAsync(cardDto.ToJsonString());

                        Card cardRepo = SessionSingleton.Account.Cards.ToList().Find(x => x.CardSerialNumberId == card.CardSerialNumberId);
                        cardRepo.FreindlyName = card.FreindlyName;
                        cardRepo.DailySpendLimit = Convert.ToInt64(card.DailySpendLimit);
                        cardRepo.MonthlySpendLimit = Convert.ToInt64(card.MonthlySpendLimit);
                    }
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        viewCardList.ItemsSource = null;
                        viewCardList.ItemsSource = SessionSingleton.Account.Cards;
                        UpdateView(CardAdminViewState.CardList);
                    });
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                gridProgress.IsVisible = false;
            }
        }

        private void cmdEditCardCancel_Clicked(object sender, EventArgs e)
        {
            UpdateView(CardAdminViewState.CardList);
        }
        #endregion
    }
}

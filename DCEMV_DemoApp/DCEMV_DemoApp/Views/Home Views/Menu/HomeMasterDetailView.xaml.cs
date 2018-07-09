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
using System;
using System.Threading.Tasks;
using DCEMV.TLVProtocol;
using Xamarin.Forms;
using DCEMV.DemoApp.Proxies;

namespace DCEMV.DemoApp
{

    public partial class HomeMasterDetailView : MasterDetailPage
    {
        private ICardInterfaceManger contactlessCardInterfaceManger;
        private ICardInterfaceManger contactCardInterfaceManger;
        private IConfigurationProvider configProvider;
        private IOnlineApprover onlineApprover;
        private TCPClientStream tcpClientStream;
       
        public HomeMasterDetailView(ICardInterfaceManger contactCardInterfaceManger, ICardInterfaceManger contactlessCardInterfaceManger, IOnlineApprover onlineApprover, IConfigurationProvider configProvider, TCPClientStream tcpClientStream)
        {
            InitializeComponent();

            this.contactlessCardInterfaceManger = contactlessCardInterfaceManger;
            this.contactCardInterfaceManger = contactCardInterfaceManger;
            this.onlineApprover = onlineApprover;
            this.configProvider = configProvider;
            this.tcpClientStream = tcpClientStream;

            masterPage.ListViewAccount.ItemSelected += OnItemSelected;
            masterPage.ListViewCredentials.ItemSelected += OnItemSelected;
            masterPage.ListViewTransact.ItemSelected += OnItemSelected;
            masterPage.ListViewPOS.ItemSelected += OnItemSelected;
            masterPage.ListViewOthers.ItemSelected += OnItemSelected;

            //if (Device.RuntimePlatform == Device.UWP)
            //{
            MasterBehavior = MasterBehavior.Popover;
            //}
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            MasterPageItem masterPageItem = e.SelectedItem as MasterPageItem;
            ModalPage selected;
            
            if (masterPageItem != null)
            {
                ViewTypes viewSelected = masterPageItem.View;
                try
                {
                    if (SessionSingleton.Account.AccountState == ServerShared.AccountState.PendingUpdate
                        &&
                        viewSelected != ViewTypes.AccountAdminView &&
                        viewSelected != ViewTypes.Settings &&
                        viewSelected != ViewTypes.LogoffView)
                    {
                        App.Current.MainPage.DisplayAlert("Validation", "Account details must be completed before the account is enabled", "OK");
                        viewSelected = ViewTypes.AccountAdminView;
                    }

                    if (String.IsNullOrEmpty(SessionSingleton.ContactlessDeviceId) && String.IsNullOrEmpty(SessionSingleton.ContactDeviceId)
                        &&
                        viewSelected != ViewTypes.Settings &&
                        viewSelected != ViewTypes.AccountAdminView &&
                        viewSelected != ViewTypes.LogoffView)
                    {
                        App.Current.MainPage.DisplayAlert("Validation", "No card reader selected in settings", "OK");
                        viewSelected = ViewTypes.Settings;
                    }

                    switch (viewSelected)
                    {
                        case ViewTypes.TransactSendMoneyView:
                            selected = new TransactView(FlowType.SendMoneyFromAppToCard, null, contactlessCardInterfaceManger, configProvider, onlineApprover, tcpClientStream);
                            break;
                        case ViewTypes.TransactReceiveMoneyView:
                            selected = new TransactView(FlowType.SendMoneyFromCardToApp, null, contactlessCardInterfaceManger, configProvider, onlineApprover, tcpClientStream);
                            break;
                        case ViewTypes.SellView:
                            selected = new POSView(null, contactlessCardInterfaceManger, configProvider, onlineApprover, tcpClientStream);
                            break;
                        case ViewTypes.TopUpView:
                            selected = new TopUpView(contactCardInterfaceManger, contactlessCardInterfaceManger, configProvider, onlineApprover, tcpClientStream);
                            break;
                        case ViewTypes.AccountCardAdminView:
                            selected = new CardAdminView(null, contactlessCardInterfaceManger, configProvider, new ContactlessDummyOnlineApprover(), tcpClientStream);
                            break;

                        case ViewTypes.Settings:
                            selected = new SettingsView(contactCardInterfaceManger, contactlessCardInterfaceManger);
                            break;
                        case ViewTypes.CredentialsPhoneNumberAdminView:
                            selected = new PhoneNumberAdminView();
                            break;
                        case ViewTypes.CredentialsChangePasswordView:
                            selected = new ChangePasswordView();
                            break;
                        case ViewTypes.AccountAdminView:
                            selected = new UpdateAccountDetailsView();
                            break;
                        case ViewTypes.AccountTransactions:
                            selected = new AccountTransactionsView();
                            break;
                        case ViewTypes.InventoryGroupView:
                            selected = new InventoryGroupAdminView();
                            break;
                        case ViewTypes.InventoryItemView:
                            selected = new InventoryItemAdminView();
                            break;
                        case ViewTypes.LogoffView:
                            selected = new LogoffView();
                            break;

                        default:
                            throw new Exception("Unknown Form Type:" + viewSelected);
                    }
                    selected.PageClosing += (sender2, e2) =>
                    {
                        naviDetailPage.PopAsync();
                        //naviDetailPage.PushAsync(new IntroView());
                        if (String.IsNullOrEmpty(SessionSingleton.AccessToken))
                            Navigation.PushModalAsync(new AuthOptionsView(), true);
                    };
                    masterPage.ClearSelectedItems();
                    IsPresented = false;

                    if (naviDetailPage.Navigation.NavigationStack.Count > 1)
                        naviDetailPage.Navigation.PopAsync();
                    naviDetailPage.PushAsync(selected);
                }
                catch (Exception ex)
                {
                    App.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                    naviDetailPage.PushAsync(new IntroView());
                    masterPage.ClearSelectedItems();
                    IsPresented = false;
                    return;
                }
            }
        }

        protected override void OnAppearing()
        {
            string accessToken = SessionSingleton.AccessToken;
            if (String.IsNullOrEmpty(accessToken))
            {
                Navigation.PushModalAsync(new AuthOptionsView(), true);
            }
            else
            {
                try
                {
                    if (SessionSingleton.Account == null)
                    {
                        (masterPage as HomeMasterView).SetWait(true);
                        Task.Run(async () => await SessionSingleton.LoadAccount()).Wait();
                    }
                }
                catch
                {
                    Navigation.PushModalAsync(new AuthOptionsView(), true);
                }
                finally
                {
                    (masterPage as HomeMasterView).SetWait(false);
                }
            }
            base.OnAppearing();
        }
    }
}

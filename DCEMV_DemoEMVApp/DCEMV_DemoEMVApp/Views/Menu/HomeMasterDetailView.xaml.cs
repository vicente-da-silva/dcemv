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
using DCEMV.TLVProtocol;
using Xamarin.Forms;

namespace DCEMV.DemoEMVApp
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

            masterPage.ListViewTransact.ItemSelected += OnItemSelected;
            masterPage.ListViewOthers.ItemSelected += OnItemSelected;

            //if (Device.RuntimePlatform == Device.UWP)
            //{
            MasterBehavior = MasterBehavior.Popover;
            //}

            (masterPage as HomeMasterView).SetWait(false);
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
                    if (String.IsNullOrEmpty(SessionSingleton.ContactlessDeviceId) && String.IsNullOrEmpty(SessionSingleton.ContactDeviceId)
                        &&
                        viewSelected != ViewTypes.Settings)
                    {
                        App.Current.MainPage.DisplayAlert("Validation", "No card reader selected in settings", "OK");
                        viewSelected = ViewTypes.Settings;
                    }

                    switch (viewSelected)
                    {
                        case ViewTypes.Transact:
                            selected = new EMVTx(contactCardInterfaceManger, contactlessCardInterfaceManger, configProvider, onlineApprover, tcpClientStream);
                            break;

                        case ViewTypes.QRCode:
                            selected = new EMVQRCodeView(contactCardInterfaceManger, contactlessCardInterfaceManger, configProvider, onlineApprover, tcpClientStream);
                            break;

                        case ViewTypes.Settings:
                            selected = new SettingsView(contactCardInterfaceManger, contactlessCardInterfaceManger);
                            break;

                        default:
                            throw new Exception("Unknown Form Type:" + viewSelected);
                    }
                    selected.PageClosing += (sender2, e2) =>
                    {
                        naviDetailPage.PopAsync();
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
            base.OnAppearing();
        }
    }
}
